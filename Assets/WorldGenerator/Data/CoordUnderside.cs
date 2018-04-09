using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class CoordUnderside {

		public bool isFixed { get; private set; }

		public float x {
			get  {return coord.x;}
		}
		public float y {
			get  {return coord.y;}
		}

		public float elevation {
			get  {return coord.elevation;}
		}

		public Coord coord { get; private set; }

		public CoordUnderside(Coord coord, bool isFixed) {
			this.coord = coord;
			this.isFixed = isFixed;
		 }

		public Vector3 toVector3() {
			return coord.toVector3();
		}

		public Vector3 toVector3(float verticalScale) {
			return coord.toVector3(verticalScale);
		}

		public void setXY(float x, float y) {
			coord.setXY(x, y);
		}

		public override string ToString() {
			return coord.ToString() + " fixed? " + isFixed;
		}
	}
}
