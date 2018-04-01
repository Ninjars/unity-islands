using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
public class Island {

			public List<Center> centers {
				get;
				private set;
			}
        public Coord center { get; private set; }
        public Rect bounds { get; private set; }

        public Island(List<Center> centers, Coord islandCenter, Rect bounds) {
				this.centers = centers;
				this.center = islandCenter;
				this.bounds = bounds;
			}
	}
}
