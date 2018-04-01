﻿using System.Collections;
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

		public List<Island> islands {
			get;
			private set;
		}

        public World(int seed, float size, Graph graph, List<Island> islands) {
            this.seed = seed;
			this.size = size;
            this.centers = graph.centers;
            this.corners = graph.corners;
            this.edges = graph.edges;
			this.center = new Coord(size / 2d, size / 2d);
			this.islands = islands;
        }
	}
}
