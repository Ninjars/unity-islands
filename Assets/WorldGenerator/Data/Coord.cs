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

        internal bool matches(double x, double y) {
			double diffX = Math.Abs(this.x * .000001);
			double diffY = Math.Abs(this.y * .000001);
			return (Math.Abs(this.x - x) <= diffX) && (Math.Abs(this.y - y) <= diffY);
        }

		public static double distanceBetween(Coord a, Coord b) {
			return Math.Sqrt(squareDistance(a, b));
		}

        internal static double squareDistance(Coord a, Coord b) {
            double dx = a.x - b.x;
			double dy = a.y - b.y;
			return (dx * dx + dy * dy);
        }
    }
}
