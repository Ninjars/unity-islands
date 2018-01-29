using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class PerlinIslandShape {
		private float size;
        private float offset;

        public PerlinIslandShape(int seed, float size) {
			this.size = size;
            System.Random pointRandom = new System.Random(seed);
			offset = (float) pointRandom.NextDouble();
		}

		public bool isInside(float x, float y) {
			float perlinVal = getPerlin(x, y);
			double lineLength = Mathf.Sqrt(x * x + y * y);
			return perlinVal > 0.3 + 0.7 * lineLength * lineLength;
		}

		private float getPerlin(float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(offset + x/size, offset + y/size));
		}
	}
}
