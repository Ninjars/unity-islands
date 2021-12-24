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
		public float maxElevation { get; private set; }
		public float minElevation { get; private set; }

        public Island(List<Center> centers, Vector3 islandCenter, Rect bounds) {
			this.centers = centers;
			this.center = islandCenter;
			this.bounds = bounds;
			this.corners = new List<Corner>();
			minElevation = float.MaxValue;
			foreach(Center c in centers) {
				if (c.coord.elevation > maxElevation) {
					maxElevation = c.coord.elevation;
				}
				if (c.coord.elevation < minElevation) {
					minElevation = c.coord.elevation;
				}
				foreach (Corner corner in c.corners) {
					if (!corners.Contains(corner)) {
						corners.Add(corner);
					}
				}
			}
			List<Corner> rimCorners = corners.Where(c => c.isIslandRim).ToList();
            Coord[] cornerCoords = rimCorners.Select(c => new Coord(c.coord.x, c.coord.elevation, c.coord.y)).ToArray();
			float lowestRimCorner = cornerCoords.Aggregate((prev, next) => prev.elevation < next.elevation ? prev : next).elevation;
			Coord[] coords = centers.Select(c => new Coord(c.coord.x, -lowestRimCorner, c.coord.y)).ToArray();
            undersideCoords = new List<ConnectedCoord>(centers.Count);
			for (int i = 0; i < centers.Count; i++) {
				Center center = centers[i];
				CoordUnderside centerCoord = new CoordUnderside(coords[i], false);
				List<CoordUnderside> neighbouringCoords = center.neighbours
									.Where(c => centers.Contains(c))
									.Select(c => new CoordUnderside(coords[centers.IndexOf(c)], false))
									.ToList();
				IEnumerable<CoordUnderside> neighbouringRimCoords = center.corners
									.Where(c => c.isIslandRim)
									.Select(c => new CoordUnderside(cornerCoords[rimCorners.IndexOf(c)], true));
				neighbouringCoords.AddRange(neighbouringRimCoords);
				undersideCoords.Add(new ConnectedCoord(centerCoord, neighbouringCoords));
			}
		}
	}
}
