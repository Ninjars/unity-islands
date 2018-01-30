using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class PerlinIslandShape {
		private float size;
        private float offset;

		private float scale = 100f;

        public PerlinIslandShape(int seed, float size) {
			this.size = size;
            System.Random pointRandom = new System.Random(seed);
			offset = (float) pointRandom.NextDouble();
		}

		public bool isInside(float x, float y) {
			float perlinVal = getPerlin(x, y);
			float polarX = (x/size - 0.5f) * 2;
			float polarY = (y/size - 0.5f) * 2;
			float lineLength = (polarX * polarX + polarY * polarY);
			return perlinVal > 0.3 + 0.7 * lineLength;
		}

		private float getPerlin(float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x/size), scale * (offset + y/size)));
		}
	}
}
