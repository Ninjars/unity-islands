using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class Coord {

		public float x;
		public float y;
		public float elevation;

		public Coord(float x, float elevation, float y) {
			this.x = x;
			this.y = y;
			this.elevation = elevation;
		}

		public Vector3 toVector3() {
			return new Vector3(x, elevation, y);
		}

		public Vector3 toVector3(float verticalScale) {
			return new Vector3(x, elevation * verticalScale, y);
		}

		public void setXY(float x, float y) {
			this.x = x;
			this.y = y;
		}

		public float sqrDistance(float w, float z) {
			return (w-x) * (w-x) + (z-y) * (z-w);
		}

		public float sqrDistance(Coord other) {
			return sqrDistance(other.x, other.y);
		}

		public override string ToString() {
			return toVector3().ToString();
		} 
	}
}
