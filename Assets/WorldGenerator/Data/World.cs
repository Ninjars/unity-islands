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
        public float verticalScale { get; private set; }
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

		public List<Island> islands {
			get;
			private set;
		}

        public World(int seed, float size, float verticalScale, Graph graph, List<Island> islands) {
            this.seed = seed;
			this.size = size;
			this.verticalScale = verticalScale;
            this.centers = graph.centers;
            this.corners = graph.corners;
            this.edges = graph.edges;
			this.center = new Vector3(size / 2f, 0, size / 2f);
			this.islands = islands;
        }

		public int indexOfClosestCenter(int startIndex, float x, float y, bool allowUnclipped) {
			Center c = centers[startIndex];
			Vector3 point = new Vector3(x, 0, y);
			return indexOfClosestCenter(c, point, allowUnclipped);
		}

		private int indexOfClosestCenter(Center startCenter, Vector3 point, bool allowUnclipped) {
			Center closest = startCenter.findClosestNeighbour(point, allowUnclipped);
			if (closest == startCenter) {
				return startCenter.index;
			} else {
				return indexOfClosestCenter(closest, point, allowUnclipped);
			}
		}
	}
}
