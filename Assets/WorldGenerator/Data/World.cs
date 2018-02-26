using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class World {
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

		public Coord center {
			get;
			private set;
		}
        public World(int seed, float size, List<Center> centers, List<Corner> corners, List<Edge> edges)
        {
            this.seed = seed;
			this.size = size;
            this.centers = centers;
            this.corners = corners;
            this.edges = edges;
			this.center = new Coord(size / 2d, size / 2d);
        }
	}
}
