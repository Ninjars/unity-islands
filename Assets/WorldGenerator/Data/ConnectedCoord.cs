using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
	public class ConnectedCoord {

		public Coord coord {get; private set; }
		public List<Coord> neighbours {get; private set; }
		
		public ConnectedCoord(Coord coord, List<Coord> neighbours) {
			this.coord = coord;
			this.neighbours = neighbours;
		}

        internal Vector3 toVector3() {
            return coord.toVector3();
        }
    }
}
