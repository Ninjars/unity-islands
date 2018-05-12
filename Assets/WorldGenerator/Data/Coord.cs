using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class Coord {

		public float x {
			get;
			private set;
		}
		public float y {
			get;
			private set;
		}
		public float elevation { get; private set; }
        private Vector3 _vector;
        private Vector3 _vectorScaled;

		public Coord(float x, float elevation, float y) {
			this.x = x;
			this.y = y;
			this.elevation = elevation;
			_vector = Vector3.down;
			_vectorScaled = Vector3.down;
		}

		public Vector3 toVector3() {
			if (_vector == Vector3.down) {
				_vector = new Vector3(x, elevation, y);
			}
			return _vector;
		}

		public Vector3 toVector3(float verticalScale) {
			if (_vectorScaled == Vector3.down) {
				_vectorScaled = new Vector3(x, elevation * verticalScale, y);
			}
			return _vectorScaled;
		}

		public void setXY(float x, float y) {
			this.x = x;
			this.y = y;
			_vector = Vector3.down;
			_vectorScaled = Vector3.down;
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

        internal void setElevation(float v) {
			if (float.IsNaN(v)) {
				throw new SystemException();
			}
            elevation = v;
			_vector = Vector3.down;
			_vectorScaled = Vector3.down;
        }
    }
}
