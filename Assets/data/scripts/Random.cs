using UnityEngine;

namespace data.scripts
{
	public class Rand
	{
		System.Random rnd;

		public float value
		{
			get { return (float)rnd.NextDouble(); }
		}

		public Rand(int seed)
		{
			rnd = new System.Random(seed);
		}

		public int Range(int a, int b)
		{
			var v = value;
			return Mathf.FloorToInt(Mathf.Lerp(a, b, v));
		}

		public float Range(float a, float b)
		{
			var v = value;
			return Mathf.Lerp(a, b, v);
		}
	}
}