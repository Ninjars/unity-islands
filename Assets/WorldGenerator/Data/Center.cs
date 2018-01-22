using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Topology.DCEL;
using UnityEngine;

namespace WorldGenerator {
	public class Center {
		public int index {
            get;
            private set;
        }
        public Coord coord {
            get;
            private set;
        }

        public bool isBorder {
            get;
            set;
        }

        private List<Center> neighbours = new List<Center>();
        public Center(int index, Coord coord) {
            this.index = index;
            this.coord = coord;
        }

        internal void AddNeighbour(Center center)
        {
            neighbours.Add(center);
        }
    }
}
