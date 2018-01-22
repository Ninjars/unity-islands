using System.Collections;
using System.Collections.Generic;
using TriangleNet.Topology.DCEL;
using UnityEngine;


namespace WorldGenerator {
    public class Edge
    {
        private int index;
        private Corner v0; // Voronoi corner
        private Corner v1;
        private Center d0; // Delunay center
        private Center d1;

        public Edge(int index, Corner v0, Corner v1, Center d0, Center d1) {
            this.index = index;
            this.v0 = v0;
            this.v1 = v1;
            this.d0 = d0;
            this.d1 = d1;
        }
    }
}
