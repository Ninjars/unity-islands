using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class PerlinRadialShape {
		private readonly float size;
        private readonly float centerPolarX;
        private readonly float centerPolarY;
        private readonly float offset;
		private readonly float scale;

        public PerlinRadialShape(System.Random random, float radius, float scale, float centerX, float centerY) {
			this.size = radius;
			this.centerPolarX = ((centerX / size) - 0.5f) * 2f;
			this.centerPolarY = ((centerY / size) - 0.5f) * 2f;
			this.scale = scale;
			offset = (float) random.NextDouble();
		}

		public bool isInside(float x, float y) {
			float normalisedX = x / size;
			float normalisedY = y / size;
			float perlinVal = getPerlin(normalisedX, normalisedY);
			float polarX = (normalisedX - 0.5f) * 2 - centerPolarX;
			float polarY = (normalisedY - 0.5f) * 2 - centerPolarY;
			float lineLength = (polarX * polarX + polarY * polarY);
			return perlinVal > 0.2 + 0.8 * lineLength;
		}

		private float getPerlin(float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x), scale * (offset + y)));
		}
	}
}
