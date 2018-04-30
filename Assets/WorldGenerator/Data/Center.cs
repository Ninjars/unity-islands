using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Topology.DCEL;
using UnityEngine;

namespace WorldGenerator {
	public class Center {
        internal double moisture {
            get; set;
        }

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
        public bool isClipped { get; internal set; }
        public bool isOnRim { get; internal set; }

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

        public float scaledElevation(float factor) {
            return coord.elevation * factor;
        }

        public Center findClosestNeighbour(float x, float y, bool allowUnclipped) {
            Center closest = this;
            float dx = Math.Abs(x - closest.coord.x);
            float dy = Math.Abs(y - closest.coord.y);
            foreach(Center neighbour in neighbours) {
                if (!allowUnclipped && neighbour.isClipped) {
                    continue;
                }
                var otherDx = Math.Abs(x - neighbour.coord.x);
                var otherDy = Math.Abs(y - neighbour.coord.y);
                if (otherDx < dx && otherDy < dy) {
                    closest = neighbour;
                    dx = otherDx;
                    dy = otherDy;
                }
            }
            return closest;
        }
    }
}
