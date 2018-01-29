using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class World {

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
        public World(List<Center> centers, List<Corner> corners, List<Edge> edges)
        {
            this.centers = centers;
            this.corners = corners;
            this.edges = edges;
        }
	}
}
