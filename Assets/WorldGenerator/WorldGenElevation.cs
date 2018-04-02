using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGenerator {
    public static class WorldGenElevation {
        
        public static void generateElevations(Graph graph, float clippingPlaneHeight) {
            System.Random random = new System.Random(graph.seed);
            
            float radius = graph.size * 0.2f;
            addCone(graph.centers, radius, graph.size / 2f, 10f, 0.4f);
            applyNoise(graph, random, 15f, 25f);
            applyNoise(graph, random, 8f, 10f);
            applyNoise(graph, random, 1f, 5f);
            for (int i = 0; i < 5; i++) {
                var x = random.NextDouble() * graph.size;
                var y = random.NextDouble() * graph.size;
                addBump(graph, 20f, x, y);
            }
            for (int i = 0; i < 3; i++) {
                var x = random.NextDouble() * graph.size;
                var y = random.NextDouble() * graph.size;
                addBump(graph, -10f, x, y);
            }
            for (int i = 0; i < 3; i++) {
                smooth(graph.centers);
            }
            normalise(graph.centers);

			calculateDownslopes(graph.centers);
            calculateMoisture(graph.centers);
            performWaterErosion(graph.centers);
            normalise(graph.centers);

            assignCornerElevations(graph.corners);
        }

        public static void generateIslandUndersideElevations(int seed, List<Vector3> points, Rect bounds, Coord center) {
            // System.Random random = new System.Random(seed);
            // addCone(graph.centers, bounds.width / 2f, center.x, center.y, -200);
        }

        public static void applyClipping(Graph graph, float clippingPlaneHeight) {
            List<Center> clippedCenters = graph.centers.Where(center => center.elevation < clippingPlaneHeight).ToList();
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

        /**
            Applys perlin noise across the map with a radial bias, maximum strength at the center, 0 at map width.!--
            Changing the horizonal scale should change the frequency of the effect.  Smaller numbers = less detailed.
            Change the vertical scale to change the strength of the effect.
         */
        private static void applyNoise(Graph graph, System.Random random, float horizontalScale, float verticalScale) {
			float offset = (float) random.NextDouble();
            foreach (Center center in graph.centers) {
                var perlin = getPerlin(offset, 
                                        horizontalScale, 
                                        (float) center.coord.x / graph.size, 
                                        (float) center.coord.y / graph.size);
                var radialFactor = 1.0 - (Coord.distanceBetween(graph.center, center.coord) * 2 / graph.size);
                center.elevation += perlin * verticalScale * radialFactor;
            }
        }

		private static float getPerlin(float offset, float scale, float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x), scale * (offset + y)));
		}

        private static void addBump(Graph graph, float verticalScale, double x, double y) {
            Center initial = findClosestCenter(x, y, graph.centers);
            float width = graph.size / 10f;
            float radialFactor = 1.0f - (float) (Coord.distanceBetween(graph.center, initial.coord) * 2 / graph.size);
            elevate(initial, width, verticalScale * radialFactor, initial, new List<Center>(), 2);
        }

        private static List<Center> elevate(Center initial, float width, float verticalScale, Center current, List<Center> processed, double falloffPower) {
            processed.Add(current);
            double distanceFromCenter = initial == current ? 0 : Coord.distanceBetween(initial.coord, current.coord);
            if (distanceFromCenter > width) {
                return processed;
            }
            var distanceFactor = Math.Pow(distanceFromCenter / width, falloffPower);
            double elevation = verticalScale * (1.0 - distanceFactor);
            current.elevation += elevation;
            foreach (Center center in current.neighbours) {
                if (!processed.Contains(center)) {
                    processed = elevate(initial, width, verticalScale, center, processed, falloffPower);
                }
            }
            return processed;
        }

        private static void addCone(List<Center> centers, float radius, double x, double y, float verticalScale) {
            Center initial = findClosestCenter(x, y, centers);
            elevate(initial, radius, verticalScale, initial, new List<Center>(), 1);
        }

        private static Center findClosestCenter(double x, double y, List<Center> centers) {
            Center closestCenter = null;
            double dx = 0;
            double dy = 0;
            foreach (Center center in centers) {
                double cx = Math.Abs(x - center.coord.x);
                double cy = Math.Abs(y - center.coord.y);
                if (closestCenter == null || cx * cx + cy * cy < dx * dx + dy * dy) {
                    closestCenter = center;
                    dx = cx;
                    dy = cy;
                }
            }
            return closestCenter;
        }

        private static void assignCornerElevations(List<Corner> corners) {
            foreach (Corner corner in corners) {
                double elevation = 0;
                List<Center> touchesCenters = corner.GetTouches();
                foreach (Center center in touchesCenters) {
                    elevation += center.elevation;
                }
                corner.elevation = elevation / touchesCenters.Count;
            }
        }

        /**
            Scale positive elevations to lie between 0 and 1, and flatten all negative elevations to 0
         */
        private static void normalise(List<Center> centers) {
            double maxElevation = 0;
            foreach (Center center in centers) {
                maxElevation = Math.Max(center.elevation, maxElevation);
            }
            foreach (Center center in centers) {
                center.elevation /= maxElevation;
                if (center.elevation < 0) {
                    center.elevation = 0;
                }
            }
        }

        /**
            averages heights between neighbouring centers to smooth the terrain
        */
        private static void smooth(List<Center> centers) {
            List<Center> sorted = new List<Center>(centers);
            sorted.Sort((a, b) => b.elevation.CompareTo(a.elevation));
            foreach (Center center in sorted) {
                if (center.elevation == 0) {
                    continue;
                }
                double elevation = 0;
                List<Center> neighbours = center.neighbours;
                foreach (Center neighbour in neighbours) {
                    elevation += neighbour.elevation;
                }
                center.elevation = elevation / neighbours.Count;
            }
        }

		internal static void calculateDownslopes(List<Center> centers) {
			foreach (Center center  in centers) {
				Center lowest = null;
				foreach (Center neigh in center.neighbours) {
					if (lowest == null) {
						lowest = neigh;
					} else {
						if (neigh.elevation < lowest.elevation) {
							lowest = neigh;
						}
					}
				}
				if (center.elevation > lowest.elevation) {
					center.downslope = lowest;
				}
			}
		}

        /**
            Assignes normalised moisture values to all centers
        */
        internal static void calculateMoisture(List<Center> centers) {
            List<Center> descendingCenters = new List<Center>(centers);
            descendingCenters.Sort((a, b) => b.elevation.CompareTo(a.elevation));
            double highestMoisture = 1;
            foreach (Center center in descendingCenters) {
                center.moisture += 1;
                if (center.downslope != null) {
                    center.downslope.moisture += center.moisture;
                } else {
                    highestMoisture = Math.Max(highestMoisture, center.moisture);
                }
            }
            // Normalise
            foreach (Center center in descendingCenters) {
                if (center.elevation <= 0) {
                    center.moisture = 1;
                } else {
                    center.moisture /= highestMoisture;
                }
            }
        }

        internal static void performWaterErosion(List<Center> centers) {
            foreach (Center center in centers) {
                center.elevation *= 1 - (center.moisture * center.moisture);
            }
        }
    }
}
