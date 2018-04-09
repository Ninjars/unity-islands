using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Topology.DCEL;
using UnityEngine;


namespace WorldGenerator {
    public class Corner {
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
        public bool isClipped { get; internal set; }
        public bool isIslandRim { get; internal set; }

        public Corner(Coord coord) {
            this.coord = coord;
            vertexIndices = new List<int>();
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

        internal List<Corner> GetAdjacents() {
            return adjacent;
        }

        internal List<Center> GetTouches()
        {
            return centers;
        }

        internal void setPosition(float x, float y)
        {
            coord.setXY(x, y);
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

        public float scaledElevation(float factor) {
            return coord.y * factor;
        }

		public override string ToString() {
			return "Corner @ " + coord.ToString() + " isClipped? " + isClipped + " isRim? " + isIslandRim; 
		} 
    }
}
