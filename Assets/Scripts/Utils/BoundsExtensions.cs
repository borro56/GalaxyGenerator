using UnityEngine;

namespace Framework.Extensions
{
	public static class BoundsExtensions
	{
		public static float Distance(this Bounds bounds, Vector3 p)
		{
			var dx = Mathf.Max(bounds.min.x - p.x, Mathf.Max(0, p.x - bounds.max.x));
			var dy = Mathf.Max(bounds.min.y - p.y, Mathf.Max(0, p.y - bounds.max.y));
			var dz = Mathf.Max(bounds.min.z - p.z, Mathf.Max(0, p.z - bounds.max.z));
			return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static float RoundDistance(this Bounds bounds, Vector3 p)
		{
			var dx = Mathf.Max(bounds.min.x - p.x, Mathf.Max(0, p.x - bounds.max.x));
			var dy = Mathf.Max(bounds.min.y - p.y, Mathf.Max(0, p.y - bounds.max.y));
			var dz = Mathf.Max(bounds.min.z - p.z, Mathf.Max(0, p.z - bounds.max.z));
			var max = Mathf.Max(dx, Mathf.Max(dy, dz));
			return Mathf.Min(Mathf.Sqrt(dx * dx + dy * dy + dz * dz), max);
		}

		public static void DebugDraw(this Bounds area)
		{
			area.DebugDraw(Color.white);
		}
		public static void DebugDraw(this Bounds area, Color color)
		{
			area.min += Vector3.one * 0.1f;
			area.max -= Vector3.one * 0.1f;

			//Base
			Debug.DrawLine(new Vector3(area.min.x, area.min.y, area.min.z),
						   new Vector3(area.min.x, area.min.y, area.max.z), color);
			Debug.DrawLine(new Vector3(area.max.x, area.min.y, area.min.z),
						   new Vector3(area.max.x, area.min.y, area.max.z), color);
			Debug.DrawLine(new Vector3(area.min.x, area.min.y, area.min.z),
						   new Vector3(area.max.x, area.min.y, area.min.z), color);
			Debug.DrawLine(new Vector3(area.min.x, area.min.y, area.max.z),
						   new Vector3(area.max.x, area.min.y, area.max.z), color);

			//Top
			Debug.DrawLine(new Vector3(area.min.x, area.max.y, area.min.z),
						   new Vector3(area.min.x, area.max.y, area.max.z), color);
			Debug.DrawLine(new Vector3(area.max.x, area.max.y, area.min.z),
						   new Vector3(area.max.x, area.max.y, area.max.z), color);
			Debug.DrawLine(new Vector3(area.min.x, area.max.y, area.min.z),
						   new Vector3(area.max.x, area.max.y, area.min.z), color);
			Debug.DrawLine(new Vector3(area.min.x, area.max.y, area.max.z),
						   new Vector3(area.max.x, area.max.y, area.max.z), color);

			//Sides
			Debug.DrawLine(new Vector3(area.min.x, area.min.y, area.min.z),
						   new Vector3(area.min.x, area.max.y, area.min.z), color);
			Debug.DrawLine(new Vector3(area.max.x, area.min.y, area.min.z),
						   new Vector3(area.max.x, area.max.y, area.min.z), color);
			Debug.DrawLine(new Vector3(area.min.x, area.min.y, area.max.z),
						   new Vector3(area.min.x, area.max.y, area.max.z), color);
			Debug.DrawLine(new Vector3(area.max.x, area.min.y, area.max.z),
						   new Vector3(area.max.x, area.max.y, area.max.z), color);
		}
	}
}