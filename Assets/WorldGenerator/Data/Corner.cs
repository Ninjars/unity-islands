using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Topology.DCEL;
using UnityEngine;


namespace WorldGenerator {
    public class Corner {
        private int index;
        public Coord coord {
            get;
            private set;
        }

        private List<Edge> edges = new List<Edge>();
        private List<Center> centers = new List<Center>();
        private List<Corner> adjacent = new List<Corner>();

        public Corner(int index, Coord coord)
        {
            this.index = index;
            this.coord = coord;
        }
        internal bool matchesId(int id)
        {
            return index.Equals(id);
        }

        internal void AddEdge(Edge edge)
        {
            edges.Add(edge);
        }

        internal void AddCenter(Center center)
        {
            centers.Add(center);
        }

        internal void AddAdjacent(Corner corner) {
            adjacent.Add(corner);
        }
    }
}
