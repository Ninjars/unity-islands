using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class PerlinRadialShape {
		private readonly float size;
        private readonly float centerX;
        private readonly float centerY;
        private readonly float offset;
		private readonly float scale;

        public PerlinRadialShape(int seed, float radius, float scale, float centerX, float centerY) {
			this.size = radius;
			this.centerX = centerX;
			this.centerY = centerY;
			this.scale = scale;
            System.Random pointRandom = new System.Random(seed);
			offset = (float) pointRandom.NextDouble();
		}

		public bool isInside(float x, float y) {
			float perlinVal = getPerlin(x, y);
			float polarX = ((x - centerX)/size - 0.5f) * 2;
			float polarY = ((y - centerY)/size - 0.5f) * 2;
			float lineLength = (polarX * polarX + polarY * polarY);
			return perlinVal > 0.2 + 0.8 * lineLength;
		}

		private float getPerlin(float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x/size), scale * (offset + y/size)));
		}
	}
}
