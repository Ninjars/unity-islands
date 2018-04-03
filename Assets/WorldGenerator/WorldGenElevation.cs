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
                addBump(graph, 20f, (float) x, (float) y);
            }
            for (int i = 0; i < 3; i++) {
                var x = random.NextDouble() * graph.size;
                var y = random.NextDouble() * graph.size;
                addBump(graph, -10f, (float) x, (float) y);
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

        public static void generateIslandUndersideElevations(int seed, Island island) {
            // System.Random random = new System.Random(seed);
            //addConeUnderside(island.centers, island.undersideCoords, island.bounds.width / 2f, island.center.x, island.center.y, -200);
        }

        public static void applyClipping(Graph graph, float clippingPlaneHeight) {
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
                                        center.coord.x / graph.size, 
                                        center.coord.y / graph.size);
                float radialFactor = 1f - Vector3.Distance(graph.center, center.coord.toVector3()) * 2 / graph.size;
                center.coord.elevation += perlin * verticalScale * radialFactor;
            }
        }

		private static float getPerlin(float offset, float scale, float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x), scale * (offset + y)));
		}

        private static void addBump(Graph graph, float verticalScale, float x, float y) {
            Center initial = findClosestCenter(x, y, graph.centers);
            float width = graph.size / 10f;
            float radialFactor = 1f - Vector3.Distance(graph.center, initial.coord.toVector3()) * 2 / graph.size;
            elevate(initial, width, verticalScale * radialFactor, initial, new List<Center>(), 2);
        }

        private static void addConeUnderside(List<Center> centers, List<Coord> mutableCoords, float radius, float x, float y, int verticalScale) {
            Center initial = findClosestCenter(x, y, centers);
            int initialIndex = centers.IndexOf(initial);
            elevateUnderside(initialIndex, mutableCoords, centers, radius, verticalScale, initialIndex, new List<Center>(), 1);
        }

        private static List<Center> elevateUnderside(int initialIndex, List<Coord> coords, List<Center> centers, float width, float verticalScale, int currentIndex, List<Center> processed, float falloffPower) {
            Coord initial = coords[initialIndex];
            Coord current = coords[currentIndex];
            processed.Add(centers[currentIndex]);
            float distanceFromCenter = initial == current ? 0 : Vector3.Distance(initial.toVector3(), current.toVector3());
            if (distanceFromCenter > width) {
                return processed;
            }
            float distanceFactor = Mathf.Pow(distanceFromCenter / width, falloffPower);
            float elevation = verticalScale * (1f - distanceFactor);
            current.elevation += elevation;
            foreach (Center center in centers[currentIndex].neighbours) {
                if (!processed.Contains(center)) {
                    int index = centers.IndexOf(center);
                    if (index >= 0) {
                        processed = elevateUnderside(initialIndex,coords, centers, width, verticalScale, index, processed, falloffPower);
                    } else {
                        processed.Add(center);
                    }
                }
            }
            return processed;
        }

        private static List<Center> elevate(Center initial, float width, float verticalScale, Center current, List<Center> processed, float falloffPower) {
            processed.Add(current);
            float distanceFromCenter = initial == current ? 0 : Vector3.Distance(initial.coord.toVector3(), current.coord.toVector3());
            if (distanceFromCenter > width) {
                return processed;
            }
            float distanceFactor = Mathf.Pow(distanceFromCenter / width, falloffPower);
            float elevation = verticalScale * (1f - distanceFactor);
            current.coord.elevation += elevation;
            foreach (Center center in current.neighbours) {
                if (!processed.Contains(center)) {
                    processed = elevate(initial, width, verticalScale, center, processed, falloffPower);
                }
            }
            return processed;
        }

        private static void addCone(List<Center> centers, float radius, float x, float y, float verticalScale) {
            Center initial = findClosestCenter(x, y, centers);
            elevate(initial, radius, verticalScale, initial, new List<Center>(), 1);
        }

        private static Center findClosestCenter(float x, float y, List<Center> centers) {
            Center closestCenter = null;
            float dx = 0;
            float dy = 0;
            foreach (Center center in centers) {
                float cx = Math.Abs(x - center.coord.x);
                float cy = Math.Abs(y - center.coord.y);
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
                float elevation = 0;
                List<Center> touchesCenters = corner.GetTouches();
                foreach (Center center in touchesCenters) {
                    elevation += center.coord.elevation;
                }
                corner.coord.elevation += elevation / (float) touchesCenters.Count;
            }
        }

        /**
            Scale positive elevations to lie between 0 and 1, and flatten all negative elevations to 0
         */
        private static void normalise(List<Center> centers) {
            float maxElevation = 0;
            foreach (Center center in centers) {
                maxElevation = Math.Max(center.coord.elevation, maxElevation);
            }
            foreach (Center center in centers) {
                center.coord.elevation /= maxElevation;
                if (center.coord.elevation < 0) {
                    center.coord.elevation = 0;
                }
            }
        }

        /**
            averages heights between neighbouring centers to smooth the terrain
        */
        private static void smooth(List<Center> centers) {
            List<Center> sorted = new List<Center>(centers);
            sorted.Sort((a, b) => b.coord.elevation.CompareTo(a.coord.elevation));
            foreach (Center center in sorted) {
                if (center.coord.elevation == 0) {
                    continue;
                }
                float elevation = 0;
                List<Center> neighbours = center.neighbours;
                foreach (Center neighbour in neighbours) {
                    elevation += neighbour.coord.elevation;
                }
                center.coord.elevation = elevation / (float) neighbours.Count;
            }
        }

		internal static void calculateDownslopes(List<Center> centers) {
			foreach (Center center  in centers) {
				Center lowest = null;
				foreach (Center neigh in center.neighbours) {
					if (lowest == null) {
						lowest = neigh;
					} else {
						if (neigh.coord.elevation < lowest.coord.elevation) {
							lowest = neigh;
						}
					}
				}
				if (center.coord.elevation > lowest.coord.elevation) {
					center.downslope = lowest;
				}
			}
		}

        /**
            Assignes normalised moisture values to all centers
        */
        internal static void calculateMoisture(List<Center> centers) {
            List<Center> descendingCenters = new List<Center>(centers);
            descendingCenters.Sort((a, b) => b.coord.elevation.CompareTo(a.coord.elevation));
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
                if (center.coord.elevation <= 0) {
                    center.moisture = 1;
                } else {
                    center.moisture /= highestMoisture;
                }
            }
        }

        internal static void performWaterErosion(List<Center> centers) {
            foreach (Center center in centers) {
                center.coord.elevation *= 1f - (float) (center.moisture * center.moisture);
            }
        }
    }
}
