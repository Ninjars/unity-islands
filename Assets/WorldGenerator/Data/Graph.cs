using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGenerator {
	public class Graph {
        public int seed {
			get;
			private set;
		}
        public float size {
			get;
			private set;
		}

        public List<Center> centers {
			get;
			private set;
		}
		public List<Corner> corners {
			get;
			private set;
		}
		public List<Edge> edges {
			get;
			private set;
		}

		public Vector3 center {
			get;
			private set;
		}

		private List<Coord> centerPoints;

        public Graph(int seed, float size, List<Center> centers, List<Corner> corners, List<Edge> edges) {
            this.seed = seed;
			this.size = size;
            this.centers = centers;
            this.corners = corners;
            this.edges = edges;
			this.center = new Vector3(size / 2f, 0, size / 2f);
        }

		public List<Coord> getCenterPoints() {
			if (centerPoints == null) {
				centerPoints = centers.Select(c => c.coord).ToList();
			}
			return centerPoints;
		}
	}
}
