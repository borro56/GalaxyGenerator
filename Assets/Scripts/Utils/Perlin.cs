using UnityEngine;

namespace Framework.Random
{
	public static class Perlin
	{
		#region Lookup Tables

		private const float HashMaxValue = 256;

		private static readonly uint[] _hash =
		{
			151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
			140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
			247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
			57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
			74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
			60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
			65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
			200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
			52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
			207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
			119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
			129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
			218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
			81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
			184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
			222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
		};

		private static readonly float[] _grads = new float[] { -1.0f, 1.0f, 0.5f, -0.5f };

		private static readonly Vector2[] _grads2 = new Vector2[]
		{
			new Vector2(1,1).normalized,
			new Vector2(-1,1).normalized,
			new Vector2(-1,-1).normalized,
			new Vector2(1,-1).normalized,
			new Vector2(0,1),
			new Vector2(1,0),
			new Vector2(-1,0),
			new Vector2(0,-1)
		};

		private static readonly Vector3[] _grads3 = new Vector3[]
		{
			new Vector3(1,1,0).normalized,
			new Vector3(-1,1,0).normalized,
			new Vector3(1,-1,0).normalized,
			new Vector3(-1,-1,0).normalized,
			new Vector3(1,0,1).normalized,
			new Vector3(-1,0,1).normalized,
			new Vector3(1,0,-1).normalized,
			new Vector3(-1,0,-1).normalized,
			new Vector3(0,1,1).normalized,
			new Vector3(0,-1,1).normalized,
			new Vector3(0,1,-1).normalized,
			new Vector3(0,-1,-1).normalized
		};
		#endregion

		#region Private Methods

		private static float Smooth(float t)
		{
			return t * t * t * (t * (t * 6 - 15) + 10);
		}

		private static float Grad(float i)
		{
			return _grads[(uint)i % _grads.Length];
		}

		private static Vector2 Grad2(float i)
		{
			return _grads2[(uint)i % _grads2.Length];
		}

		private static Vector3 Grad3(float i)
		{
			return _grads3[(int)i * _grads3.Length];
		}

		#endregion

		#region Public Methods

		public static float Random(float i)
		{
			var h = (uint)i;
			return (int)_hash[h % _hash.Length] / HashMaxValue;
		}

		public static float Noise(Vector2 uv)
		{
			var offsetX = uv.x % 1;
			var sample0 = Random(uv.x);
			var sample1 = Random(uv.x + 1);
			return Mathf.Lerp(sample0, sample1, Smooth(offsetX));
		}

		public static float Random2(float i, float j)
		{
			var h = (uint)i;
			h = _hash[h % _hash.Length] + (uint)j;
			h = _hash[h % _hash.Length];
			return (int)h / HashMaxValue;
		}
		public static float Noise2(Vector2 uv)
		{
			var offsetX = uv.x % 1;
			var offsetY = uv.y % 1;

			var i00 = Random2(uv.x, uv.y);
			var i01 = Random2(uv.x + 1, uv.y);
			var i10 = Random2(uv.x, uv.y + 1);
			var i11 = Random2(uv.x + 1, uv.y + 1);

			var g00 = Grad2(i00 * 8);
			var g01 = Grad2(i01 * 8);
			var g10 = Grad2(i10 * 8);
			var g11 = Grad2(i11 * 8);

			var c00 = new Vector2(offsetX, offsetY);
			var c01 = new Vector2(offsetX - 1, offsetY);
			var c10 = new Vector2(offsetX, offsetY - 1);
			var c11 = new Vector2(offsetX - 1, offsetY - 1);

			var v00 = Vector2.Dot(g00, c00) * 0.5f + 0.5f;
			var v01 = Vector2.Dot(g01, c01) * 0.5f + 0.5f;
			var v10 = Vector2.Dot(g10, c10) * 0.5f + 0.5f;
			var v11 = Vector2.Dot(g11, c11) * 0.5f + 0.5f;

			offsetX = Smooth(offsetX);
			offsetY = Smooth(offsetY);

			return Mathf.Lerp(Mathf.Lerp(v00, v01, offsetX),
							  Mathf.Lerp(v10, v11, offsetX),
							  offsetY);
		}

		public static float Random3(Vector3 pos)
		{
			return Random3(pos.x, pos.y, pos.z);
		}

		public static int Random3Int(Vector3 pos)
		{
			return Random3Int(pos.x, pos.y, pos.z);
		}

		public static float Random3(float i, float j, float k)
		{
			var h = (uint)i;
			h = _hash[h % _hash.Length] + (uint)j;
			h = _hash[h % _hash.Length] + (uint)k;
			h = _hash[h % _hash.Length];
			return (int)h / HashMaxValue;
		}

		public static int Random3Int(float i, float j, float k)
		{
			var h = (uint)i;
			h = _hash[h % _hash.Length] + (uint)j;
			h = _hash[h % _hash.Length] + (uint)k;
			h = _hash[h % _hash.Length];
			return (int)h;
		}

		public static int Random3Int(Vector3Int pos)
		{
			return Random3Int(pos.x, pos.y, pos.z);
		}

		public static int Random3Int(int i, int j, int k)
		{
			var h = (uint)i;
			h = _hash[h % _hash.Length] + (uint)j;
			h = _hash[h % _hash.Length] + (uint)k;
			h = _hash[h % _hash.Length];
			return (int)h;
		}


		public static float Noise3(Vector3 pos)
		{
			var offsetX = pos.x % 1;
			var offsetY = pos.y % 1;
			var offsetZ = pos.z % 1;

			var i000 = Random3(pos.x, pos.y, pos.z);
			var i010 = Random3(pos.x + 1, pos.y, pos.z);
			var i100 = Random3(pos.x, pos.y + 1, pos.z);
			var i110 = Random3(pos.x + 1, pos.y + 1, pos.z);
			var i001 = Random3(pos.x, pos.y, pos.z + 1);
			var i011 = Random3(pos.x + 1, pos.y, pos.z + 1);
			var i101 = Random3(pos.x, pos.y + 1, pos.z + 1);
			var i111 = Random3(pos.x + 1, pos.y + 1, pos.z + 1);

			var g000 = Grad3(i000);
			var g010 = Grad3(i010);
			var g100 = Grad3(i100);
			var g110 = Grad3(i110);
			var g001 = Grad3(i001);
			var g011 = Grad3(i011);
			var g101 = Grad3(i101);
			var g111 = Grad3(i111);

			var c000 = new Vector3(offsetX, offsetY, offsetZ);
			var c010 = new Vector3(offsetX - 1, offsetY, offsetZ);
			var c100 = new Vector3(offsetX, offsetY - 1, offsetZ);
			var c110 = new Vector3(offsetX - 1, offsetY - 1, offsetZ);
			var c001 = new Vector3(offsetX, offsetY, offsetZ - 1);
			var c011 = new Vector3(offsetX - 1, offsetY, offsetZ - 1);
			var c101 = new Vector3(offsetX, offsetY - 1, offsetZ - 1);
			var c111 = new Vector3(offsetX - 1, offsetY - 1, offsetZ - 1);

			var v000 = Vector3.Dot(g000, c000) * 0.5f + 0.5f;
			var v010 = Vector3.Dot(g010, c010) * 0.5f + 0.5f;
			var v100 = Vector3.Dot(g100, c100) * 0.5f + 0.5f;
			var v110 = Vector3.Dot(g110, c110) * 0.5f + 0.5f;
			var v001 = Vector3.Dot(g001, c001) * 0.5f + 0.5f;
			var v011 = Vector3.Dot(g011, c011) * 0.5f + 0.5f;
			var v101 = Vector3.Dot(g101, c101) * 0.5f + 0.5f;
			var v111 = Vector3.Dot(g111, c111) * 0.5f + 0.5f;

			var tx = Smooth(offsetX);
			var ty = Smooth(offsetY);
			var tz = Smooth(offsetZ);

			return Mathf.Lerp(
				Mathf.Lerp(Mathf.Lerp(v000, v010, tx), Mathf.Lerp(v100, v110, tx), ty),
				Mathf.Lerp(Mathf.Lerp(v001, v011, tx), Mathf.Lerp(v101, v111, tx), ty),
				tz);
		}
		#endregion
	}
}