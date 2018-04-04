using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGenerator {
public class Island {
        public List<ConnectedCoord> undersideCoords {
			get; private set;
		}

        public List<Center> centers {
			get;
			private set;
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
				List<Coord> coords = centers.Select(c => {
					return new Coord(c.coord.x, 0, c.coord.y);
				}).ToList();
				undersideCoords = new List<ConnectedCoord>(centers.Count);
				for (int i = 0; i < centers.Count; i++) {
					Center center = centers[i];
					Coord centerCoord = coords[i];
					List<Coord> neighbouringCoords = center.neighbours
															.Where(c => centers.Contains(c))
															.Select(c => coords[centers.IndexOf(c)])
															.ToList();
					undersideCoords.Add(new ConnectedCoord(centerCoord, neighbouringCoords));
				}
			}
	}
}
