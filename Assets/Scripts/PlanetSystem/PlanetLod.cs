using Framework.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.PCG.PlanetGen
{
    public class PlanetLod
    {
        #region Const
        public const int SideCellsLength = 2;
        #endregion

        #region State

        bool divided;
        public PlanetPart part;

        public PlanetLod parent;
        public PlanetLod up;
        public PlanetLod down;
        public PlanetLod left;
        public PlanetLod right;
        public PlanetLod front;
        public PlanetLod back;

        public Bounds bounds;
        public bool hasBounds;
        #endregion

        #region Properties
        public int level { get; }
        public PlanetLod[,,] childs { get; private set; }
        public Bounds area { get; }
        public float SideLength => area.size.x;
        public Planet planet { get; }

        public int LeftLevel => left == null ? level : left.level;
        public int RightLevel => right == null ? level : right.level;
        public int UpLevel => up == null ? level : up.level;
        public int DownLevel => down == null ? level : down.level;
        public int FrontLevel => front == null ? level : front.level;
        public int BackLevel => back == null ? level : back.level;

        public int LeftDownsample => Mathf.RoundToInt(Mathf.Pow(2, Mathf.Max(0, level - LeftLevel)));
        public int RightDownsample => Mathf.RoundToInt(Mathf.Pow(2, Mathf.Max(0, level - RightLevel)));
        public int UpDownsample => Mathf.RoundToInt(Mathf.Pow(2, Mathf.Max(0, level - UpLevel)));
        public int DownDownsample => Mathf.RoundToInt(Mathf.Pow(2, Mathf.Max(0, level - DownLevel)));
        public int FrontDownsample => Mathf.RoundToInt(Mathf.Pow(2, Mathf.Max(0, level - FrontLevel)));
        public int BackDownsample => Mathf.RoundToInt(Mathf.Pow(2, Mathf.Max(0, level - BackLevel)));

        public bool AnyDownsample => LeftDownsample + RightDownsample + DownDownsample + UpDownsample + FrontDownsample + BackDownsample > 6;

        #endregion

        #region Constructors

        public PlanetLod(Planet planet, Bounds area) : this(planet, area, 0)
        {
        }

        PlanetLod(Planet planet, Bounds area, int level)
        {
            this.planet = planet;
            this.area = area;
            this.level = level;
        }

        #endregion

        #region Public
        public void RefreshDFS()
        {
            ClearNeighbours();
            RefreshDFS(planet.LookPosition);
        }

        public void RefreshBFS()
        {
            RecursiveClear();
            RefreshBFS(planet.LookPosition);
        }

        #endregion

        #region Private

        void RefreshDFS(Vector3 viewPos)
        {
            if (Divide(viewPos))
                for (var x = 0; x < SideCellsLength; x++)
                    for (var y = 0; y < SideCellsLength; y++)
                        for (var z = 0; z < SideCellsLength; z++)
                            childs[x, y, z].RefreshDFS(viewPos);
        }

        void RefreshBFS(Vector3 viewPos)
        {
            var currentLevel = new List<PlanetLod>();
            var nextLevel = new List<PlanetLod>();
            currentLevel.Add(this);

            while (currentLevel.Count > 0)
            {
                for (var i = 0; i < currentLevel.Count; i++)
                    if (currentLevel[i].Divide(viewPos))
                        for (var x = 0; x < SideCellsLength; x++)
                            for (var y = 0; y < SideCellsLength; y++)
                                for (var z = 0; z < SideCellsLength; z++)
                                    nextLevel.Add(currentLevel[i].childs[x, y, z]);

                for (var i = 0; i < currentLevel.Count; i++)
                    currentLevel[i].ReconnectChilds();

                var aux = currentLevel;
                currentLevel = nextLevel;
                nextLevel = aux;
                nextLevel.Clear();
            }
        }

        bool Divide(Vector3 viewPos)
        {
            var distance = area.RoundDistance(viewPos) / SideLength;
            divided = distance < planet.LodDistance && SideLength > planet.LodMinSize;

            if (!divided && part == null)
            {
                ReturnPartRecursive();
                part = planet.GetPart();
                part.position = area.min;
                part.size = area.size;
                part.lodNode = this;
            }
            else if (divided)
            {
                InitChilds();
                ReturnPart();
            }

            return divided;

            //TODO: Instead of generating one by one join them togheter in one call
            //TODO: Evaluate Pool of VoxelNode (calculate bounds constantly, less memory)
        }

        void ReconnectChilds()
        {
            if (childs == null || !divided) return;

            for (var x = 0; x < SideCellsLength; x++)
                for (var y = 0; y < SideCellsLength; y++)
                    for (var z = 0; z < SideCellsLength; z++)
                    {
                        var child = childs[x, y, z];

                        //Set inner childs
                        var r = x == 0 ? childs[x + 1, y + 0, z + 0] : null;
                        var u = y == 0 ? childs[x + 0, y + 1, z + 0] : null;
                        var f = z == 0 ? childs[x + 0, y + 0, z + 1] : null;
                        child.SetNeighbours(r, u, f);

                        //Set outer childs
                        if (x == 0) child.left = left != null && left.divided ? left.childs[1, y, z] : left;
                        if (y == 0) child.down = down != null && down.divided ? down.childs[x, 1, z] : down;
                        if (z == 0) child.back = back != null && back.divided ? back.childs[x, y, 1] : back;

                        if (x == 1) child.right = right != null && right.divided ? right.childs[0, y, z] : right;
                        if (y == 1) child.up = up != null && up.divided ? up.childs[x, 0, z] : up;
                        if (z == 1) child.front = front != null && front.divided ? front.childs[x, y, 0] : front;
                    }
        }

        void InitChilds()
        {
            if (childs != null) return;
            childs = new PlanetLod[SideCellsLength, SideCellsLength, SideCellsLength];

            for (var x = 0; x < SideCellsLength; x++)
                for (var y = 0; y < SideCellsLength; y++)
                    for (var z = 0; z < SideCellsLength; z++)
                    {
                        //Calculate new child node bounds
                        var newArea = new Bounds();
                        var offsetX = x == 0 ? -1 : 1;
                        var offsetY = y == 0 ? -1 : 1;
                        var offsetZ = z == 0 ? -1 : 1;
                        newArea.center = area.center + new Vector3(offsetX * (area.extents.x / 2),
                                                                    offsetY * (area.extents.y / 2),
                                                                    offsetZ * (area.extents.z / 2));
                        newArea.size = area.extents;

                        //Assign to parent
                        childs[x, y, z] = new PlanetLod(planet, newArea, level + 1) { parent = this };
                    }
        }

        void ReturnPart()
        {
            if (part == null) return;
            planet.ReturnPart(part);
            part = null;
            hasBounds = false;
            bounds = new Bounds();
        }

        void ClearNeighbours()
        {
            up = down = right = left = front = back = null;
        }

        void RecursiveClear()
        {
            ClearNeighbours();
            divided = false;
            if (childs != null)
                for (var x = 0; x < SideCellsLength; x++)
                    for (var y = 0; y < SideCellsLength; y++)
                        for (var z = 0; z < SideCellsLength; z++)
                            childs[x, y, z].RecursiveClear();
        }

        void SetNeighbours(PlanetLod right, PlanetLod up, PlanetLod front)
        {
            if (right != null)
            {
                this.right = right;
                right.left = this;
            }

            if (up != null)
            {
                this.up = up;
                up.down = this;
            }

            if (front != null)
            {
                this.front = front;
                front.back = this;
            }
        }

        void ReturnPartRecursive()
        {
            divided = false;
            if (part == null)
            {
                if (childs == null) return;
                for (var x = 0; x < SideCellsLength; x++)
                    for (var y = 0; y < SideCellsLength; y++)
                        for (var z = 0; z < SideCellsLength; z++)
                            childs[x, y, z].ReturnPartRecursive();
            }
            else
            {
                planet.ReturnPart(part);
                part = null;
            }
        }
        #endregion
    }
}