using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Elevation;
using UnityEngine;

namespace WorldGenerator {
    public static class WorldGenElevation {
        
        public static void generateElevations(Graph graph, float clippingPlaneHeight) {
            System.Random random = new System.Random(graph.seed);
            List<Coord> graphCenterCoords = graph.centers.Select(c => c.coord).ToList();
            
            float radius = graph.size * 0.2f;
            ElevationFunctions.addCone(graphCenterCoords, radius, graph.size / 2f, graph.size / 2f, 0.2f);
            ElevationFunctions.addRadialWeightedNoise(graph.center, graph.size, graphCenterCoords, random, 15f, 25f);
            ElevationFunctions.addRadialWeightedNoise(graph.center, graph.size, graphCenterCoords, random, 8f, 10f);
            ElevationFunctions.addRadialWeightedNoise(graph.center, graph.size, graphCenterCoords, random, 1f, 5f);
            for (int i = 0; i < 5; i++) {
                var x = random.NextDouble() * graph.size;
                var y = random.NextDouble() * graph.size;
                ElevationFunctions.addBump(graph.center, graph.size, graphCenterCoords, 20f, graph.size / 10f, (float) x, (float) y);
            }
            for (int i = 0; i < 3; i++) {
                var x = random.NextDouble() * graph.size;
                var y = random.NextDouble() * graph.size;
                ElevationFunctions.addBump(graph.center, graph.size, graphCenterCoords, -10f, graph.size / 10f, (float) x, (float) y);
            }
            for (int i = 0; i < 3; i++) {
                ElevationFunctions.smooth(graph.centers);
            }
            ElevationFunctions.normalise(graphCenterCoords);

            ElevationFunctions.normalise(graphCenterCoords);
            assignCornerElevations(graph.corners);
            calculateDownslopes(graph.corners);
            calculateMoisture(graph.corners);
            ElevationFunctions.performWaterErosion(graph.corners);
        }

        public static void generateIslandUndersideElevations(int seed, Island island) {
            System.Random random = new System.Random(seed);
            List<CoordUnderside> undersideCoords = island.undersideCoords.Where(c => !c.coord.isFixed).Select(c => c.coord).ToList();
            List<Coord> islandCoords = undersideCoords.Select(c => c.coord).ToList();
            float islandMinDim = Mathf.Min(island.bounds.width, island.bounds.height);
            float islandMaxDim = Mathf.Max(island.bounds.width, island.bounds.height);
            
            for (int i = 0; i < 5; i++) {
                var x = random.NextDouble() * islandMinDim;
                var y = random.NextDouble() * islandMinDim;
                ElevationFunctions.addBump(island.center, islandMaxDim, islandCoords, (float) random.NextDouble() * islandMinDim, 0.5f, (float) x, (float) y);
            }
            
            ElevationFunctions.addCone(islandCoords, islandMaxDim / 2f, island.center.x, island.center.z, 0.1f);
            ElevationFunctions.addRadialWeightedNoise(island.center, islandMinDim / 2f, islandCoords, random, 3f, 0.2f);
            ElevationFunctions.addNoise(islandCoords, random, 10f, 0.2f);

            ElevationFunctions.normalise(islandCoords);
            ElevationUtils.invert(islandCoords);
            ElevationUtils.offsetElevation(islandCoords, island.minElevation);
        }

        public static void applyClippingPlane(Graph graph, float clippingPlaneHeight) {
            List<Center> clippedCenters = graph.centers.Where(center => center.coord.elevation < clippingPlaneHeight).ToList();
            List<Center> borderCenters = graph.centers.Where(center => center.isBorder).ToList();
            List<Center> queue = new List<Center>(borderCenters);
            while(queue.Count > 0) {
                Center next = queue[queue.Count-1];
                queue.Remove(next);
                List<Center> neighbours = next.neighbours;
                foreach (var neigh in neighbours) {
                    if (clippedCenters.Contains(neigh) && !borderCenters.Contains(neigh)) {
                        borderCenters.Add(neigh);
                        queue.Add(neigh);
                    }
                }
            }

            foreach (Center c in borderCenters) {
                c.isClipped = true;
            }

            foreach (Corner corner in graph.corners) {
                int clippedCenterCount = corner.GetTouches().Where(center => center.isClipped).ToList().Count;
                corner.isClipped = clippedCenterCount == corner.GetTouches().Count;
                corner.isIslandRim = !corner.isClipped && clippedCenterCount > 0;
             }
        }

        private static void assignCornerElevations(List<Corner> corners) {
            foreach (Corner corner in corners) {
                float elevation = 0;
                List<Center> touchesCenters = corner.GetTouches();
                foreach (Center center in touchesCenters) {
                    elevation += center.coord.elevation;
                }
                corner.coord.setElevation(corner.coord.elevation + elevation / (float) touchesCenters.Count);
            }
        }

		internal static void calculateDownslopes(List<Corner> corners) {
			foreach (Corner corner  in corners) {
				Corner lowest = null;
				foreach (Corner neigh in corner.GetAdjacents()) {
					if (lowest == null) {
						lowest = neigh;
					} else {
						if (neigh.coord.elevation < lowest.coord.elevation) {
							lowest = neigh;
						}
					}
				}
				if (corner.coord.elevation > lowest.coord.elevation) {
					corner.downslope = lowest;
				}
			}
		}

        /**
            Assignes normalised moisture values to all centers
        */
        internal static void calculateMoisture(List<Corner> corners) {
            float maxValue = 0;
            foreach (Corner corner in corners) {
                maxValue = Mathf.Max(maxValue, recursivelyApplyMoisture(corner));
            }
            foreach (Corner corner in corners) {
                corner.moisture /= maxValue;
            }
        }

        private static float recursivelyApplyMoisture(Corner corner) {
            corner.moisture += 1;
            if (corner.downslope != null) {
                return recursivelyApplyMoisture(corner.downslope);
            } else {
                return corner.moisture;
            }
        }
    }
}
