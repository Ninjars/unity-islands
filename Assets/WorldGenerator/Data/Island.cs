using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
public class Island {

		public List<Center> centers {
			get;
			private set;
		}
		public List<Center> undersideCenters {
			get;
			set;
		}
		public List<Corner> corners {
			get;
			private set;
		}
        public Vector3 center { get; private set; }
        public Rect bounds { get; private set; }

        public Island(List<Center> centers, Vector3 islandCenter, Rect bounds) {
				this.centers = centers;
				this.center = islandCenter;
				this.bounds = bounds;
				this.corners = new List<Corner>();
				foreach(Center c in centers) {
					foreach (Corner corner in c.corners) {
						if (!corners.Contains(corner)) {
							corners.Add(corner);
						}
					}
				}
			}
	}
}
