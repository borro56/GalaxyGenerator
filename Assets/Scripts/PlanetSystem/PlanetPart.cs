using UnityEngine;

namespace Framework.PCG.PlanetGen
{
    public class PlanetPart
    {
        #region State

        readonly int[] boundsData = new int[6];

        public Vector3 position;
        public Vector3 size;
        public PlanetLod lodNode;
        public bool dirty;
        #endregion

        #region Properties
        public ComputeBuffer boundsBuffer { get; }
        public ComputeBuffer cellsBuffer { get; }
        public ComputeBuffer vertexesBuffer { get; }

        public Bounds bounds
        {
            get => lodNode.bounds;
            set => lodNode.bounds = value;
        }

        public bool hasBounds
        {
            get => lodNode.hasBounds;
            set => lodNode.hasBounds = value;
        }
        #endregion

        #region Public
        public PlanetPart(int cellsFieldCount, int vertexCount)
        {
            boundsBuffer = new ComputeBuffer(6, sizeof(int));
            cellsBuffer = new ComputeBuffer(cellsFieldCount, 4 * sizeof(float));
            vertexesBuffer = new ComputeBuffer(vertexCount, 3 * sizeof(float));
        }

        public void DestroyBuffers()
        {
            cellsBuffer.Release();
            cellsBuffer.Dispose();

            vertexesBuffer.Release();
            vertexesBuffer.Dispose();

            boundsBuffer.Release();
            boundsBuffer.Dispose();
        }

        public void ResetBounds()
        {
            if (hasBounds || bounds.size.sqrMagnitude != 0) return;
            bounds = new Bounds(Vector3.zero, Vector3.one * float.MaxValue);
            boundsData[0] = boundsData[1] = boundsData[2] = int.MaxValue;
            boundsData[3] = boundsData[4] = boundsData[5] = int.MinValue;
            boundsBuffer.SetData(boundsData);
        }

        public void Update(Planet planet, bool force = false)
        {
            UpdateMesh(planet, force);
            UpdateBounds();
        }

        public void UpdateBounds()
        {
            //Bounds has already calculated
            if (hasBounds) return;

            //Check if bounds calculation are ready
            var status = AsyncTextureReader.RetrieveBufferData(boundsBuffer, boundsData);
            if (status != AsyncTextureReader.Status.Succeeded) return;

            //Retrieve bounds
            hasBounds = true;
            var minBounds = new Vector3(boundsData[0] / 10000f, boundsData[1] / 10000f, boundsData[2] / 10000f);
            var maxBounds = new Vector3(boundsData[3] / 10000f, boundsData[4] / 10000f, boundsData[5] / 10000f);
            var size = maxBounds - minBounds;

            //If bounds are empty theres no triangles in the part
            if (size.x < 0 || size.y < 0 || size.z < 0)
                bounds = new Bounds(Vector3.zero, Vector3.zero);
            else
                bounds = new Bounds((minBounds + maxBounds) / 2, maxBounds - minBounds);
        }

        public void UpdateMesh(Planet planet, bool force = false)
        {
            //Check if the generation is needed
            if (!dirty && !force) return;
            dirty = false;

            //Reset Bounds
            ResetBounds();

            //Set part custom parameters
            planet.Shader.SetVector("offset", position);
            planet.Shader.SetVector("size", size);
            planet.Shader.SetInt("leftDown", lodNode.LeftDownsample);
            planet.Shader.SetInt("rightDown", lodNode.RightDownsample);
            planet.Shader.SetInt("upDown", lodNode.UpDownsample);
            planet.Shader.SetInt("downDown", lodNode.DownDownsample);
            planet.Shader.SetInt("frontDown", lodNode.FrontDownsample);
            planet.Shader.SetInt("backDown", lodNode.BackDownsample);
            planet.Shader.SetBuffer(planet.FieldBaseKernel, "field", cellsBuffer);
            planet.Shader.SetBuffer(planet.FieldVertexesKernel, "field", cellsBuffer);
            planet.Shader.SetBuffer(planet.FieldVertexesKernel, "vertexes", vertexesBuffer);
            planet.Shader.SetBuffer(planet.FieldVertexesKernel, "bounds", boundsBuffer);

            //Dispatch shaders
            planet.Shader.Dispatch(planet.FieldBaseKernel, planet.GroupCount, planet.GroupCount, planet.GroupCount);
            planet.Shader.Dispatch(planet.FieldVertexesKernel, planet.GroupCount, planet.GroupCount, planet.GroupCount);

            //Enqueue a wait request for the bounds data
            AsyncTextureReader.RequestBufferData(boundsBuffer);

            //TODO: Move all properties to a buffer to prevent so many calls
            //TODO: Replace Dispatch with DispatchIndirect to prevent send x,y,z constantly
        }
        #endregion
    }
}