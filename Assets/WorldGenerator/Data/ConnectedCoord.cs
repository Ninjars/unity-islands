using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class ConnectedCoord {

		public CoordUnderside coord {get; private set; }
		public List<CoordUnderside> neighbours {get; private set; }
		
		public ConnectedCoord(CoordUnderside coord, List<CoordUnderside> neighbours) {
			this.coord = coord;
			this.neighbours = neighbours;
		}

        internal Vector3 toVector3() {
            return coord.toVector3();
        }
    }
}
