using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Topology.DCEL;
using UnityEngine;


namespace WorldGenerator {
    public class Corner {
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
        public TerrainType terrainType { get; internal set; }

        private List<Edge> edges = new List<Edge>();
        private List<Center> centers = new List<Center>();
        private List<Corner> adjacent = new List<Corner>();
        public List<int> vertexIndices {
            get;
            private set;
        }

        public Corner(int index, Coord coord)
        {
            this.index = index;
            this.coord = coord;
            vertexIndices = new List<int>();
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
            if (center != null) {
                centers.Add(center);
            }
        }

        internal void AddAdjacent(Corner corner) {
            adjacent.Add(corner);
        }

        internal List<Center> GetTouches()
        {
            return centers;
        }

        internal void setPosition(double x, double y)
        {
            coord.set(x, y);
        }

        internal bool isWater() {
            return terrainType.Equals(TerrainType.LAKE) || terrainType.Equals(TerrainType.OCEAN);
        }

        internal bool isLand() {
            return !isWater();
        }

        internal void addVertexIndex(int index) {
            vertexIndices.Add(index);
        }
    }
}
