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

        public List<Center> neighbours {
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
        public TerrainType terrainType { get; internal set; }

        public Center(int index, Coord coord) {
            this.index = index;
            this.coord = coord;
            neighbours = new List<Center>();
            corners = new List<Corner>();
            edges = new List<Edge>();
            terrainType = TerrainType.LAND;
        }

        internal void AddNeighbour(Center center) {
            if (!neighbours.Contains(center)) {
                neighbours.Add(center);
            }
        }

        internal void AddCorner(Corner corner) {
            if (!corners.Contains(corner)) {
                corners.Add(corner);
            }
        }

        internal void AddEdge(Edge edge) {
            if (!edges.Contains(edge)) {
                edges.Add(edge);
            }
        }

        internal bool isLand() {
            return terrainType == TerrainType.LAND || terrainType == TerrainType.COAST;
        }

        internal bool isOcean() {
            return terrainType == TerrainType.OCEAN;
        }
    }
}
