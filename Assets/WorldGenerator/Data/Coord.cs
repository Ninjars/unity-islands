using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class Coord {
		public double x {
			get;
			private set;
		}
		public double y {
			get;
			private set;
		}

		public Coord(double x, double y) {
			this.x = x;
			this.y = y;
		}

        internal void set(double x, double y)
        {
            this.x = x;
			this.y = y;
        }
    }
}
