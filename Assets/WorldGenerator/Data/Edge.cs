using System.Collections;
using System.Collections.Generic;
using TriangleNet.Topology.DCEL;
using UnityEngine;


namespace WorldGenerator {
    public class Edge
    {
        private int index;

         // Voronoi corner
        public Corner v0 {
            get;
            private set;
        }
        public Corner v1 {
            get;
            private set;
        }
        private Center d0; // Delunay center
        private Center d1;

        public bool isBorder {
            get;
            private set;
        }

        public Edge(int index, bool isBorder, Corner v0, Corner v1, Center d0, Center d1) {
            this.index = index;
            this.isBorder = isBorder;
            this.v0 = v0;
            this.v1 = v1;
            this.d0 = d0;
            this.d1 = d1;
        }
    }
}
