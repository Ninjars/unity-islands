using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGenerator {
    public static class WorldGenElevation {
        
        public static void generateElevations(Graph graph, float clippingPlaneHeight) {
            System.Random random = new System.Random(graph.seed);
            List<Coord> graphCenterCoords = graph.centers.Select(c => c.coord).ToList();
            
            float radius = graph.size * 0.2f;
            addCone(graphCenterCoords, radius, graph.size / 2f, graph.size / 2f, 0.2f);
            applyRadialWeightedNoise(graph.center, graph.size, graphCenterCoords, random, 15f, 25f);
            applyRadialWeightedNoise(graph.center, graph.size, graphCenterCoords, random, 8f, 10f);
            applyRadialWeightedNoise(graph.center, graph.size, graphCenterCoords, random, 1f, 5f);
            for (int i = 0; i < 5; i++) {
                var x = random.NextDouble() * graph.size;
                var y = random.NextDouble() * graph.size;
                addBump(graph.center, graph.size, graphCenterCoords, 20f, graph.size / 10f, (float) x, (float) y);
            }
            for (int i = 0; i < 3; i++) {
                var x = random.NextDouble() * graph.size;
                var y = random.NextDouble() * graph.size;
                addBump(graph.center, graph.size, graphCenterCoords, -10f, graph.size / 10f, (float) x, (float) y);
            }
            for (int i = 0; i < 3; i++) {
                smooth(graph.centers);
            }
            normalise(graphCenterCoords);

			calculateDownslopes(graph.centers);
            calculateMoisture(graph.centers);
            performWaterErosion(graph.centers);
            normalise(graphCenterCoords);

            assignCornerElevations(graph.corners);
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
                addBump(island.center, islandMaxDim, islandCoords, (float) random.NextDouble() * islandMinDim, 0.5f, (float) x, (float) y);
            }
            
            addCone(islandCoords, islandMaxDim / 2f, island.center.x, island.center.z, 0.1f);
            applyRadialWeightedNoise(island.center, islandMinDim / 2f, islandCoords, random, 3f, 0.2f);
            applyNoise(islandCoords, random, 10f, 0.2f);

            normalise(islandCoords);
            invert(islandCoords);
            offsetElevation(islandCoords, island.minElevation);
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
        private static void applyRadialWeightedNoise(Vector3 center, float radius, List<Coord> coords, System.Random random, float horizontalScale, float verticalScale) {
			float offset = (float) random.NextDouble();
            foreach (Coord coord in coords) {
                var perlin = getPerlin(offset, 
                                        horizontalScale, 
                                        coord.x / radius, 
                                        coord.y / radius);
                float radialFactor = 1f - Vector3.Distance(center, coord.toVector3()) * 2 / radius;
                coord.elevation += perlin * verticalScale * radialFactor;
            }
        }

        private static void applyNoise(List<Coord> coords, System.Random random, float horizontalScale, float verticalScale) {
            float offset = (float) random.NextDouble();
            foreach (Coord coord in coords) {
                var perlin = getPerlin(offset, 
                                        horizontalScale, 
                                        coord.x, 
                                        coord.y);
                coord.elevation += perlin * verticalScale;
            }
        }

		private static float getPerlin(float offset, float scale, float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x), scale * (offset + y)));
		}

        private static void addBump(Vector3 center, float graphSize, List<Coord> coords, float radius, float verticalScale, float x, float y) {
            Coord initial = findClosestCoord(x, y, coords);
            float radialFactor = 1f - Vector3.Distance(center, initial.toVector3()) * 2 / graphSize;
            elevate(initial, coords, radius, x, y, verticalScale * radialFactor, 2);
        }

        private static void addCone(List<Coord> coords, float radius, float x, float y, float verticalScale) {
            elevate(coords, radius, x, y, verticalScale, 1);
        }

        private static void elevate(List<Coord> coords, float radius, float x, float y, float verticalScale, float power) {
            Coord initial = findClosestCoord(x, y, coords);
            elevate(initial, coords, radius, x, y, verticalScale, power);
        }

        private static void elevate(Coord initial, List<Coord> coords, float radius, float x, float y, float verticalScale, float power) {
            radius = Mathf.Max(radius, 1);
            foreach (Coord current in coords) {
                float distanceFromCenter = initial == current ? 0 : Vector3.Distance(initial.toVector3(), current.toVector3());
                if (distanceFromCenter > radius) {
                    continue;
                }
                float distanceFactor = Mathf.Pow(distanceFromCenter / radius, 1);
                current.elevation += verticalScale * (1f - distanceFactor);
            }
        }

        private static Coord findClosestCoord(float x, float y, List<Coord> coords) {
            Coord closestCoord = null;
            float dx = 0;
            float dy = 0;
            foreach (Coord coord in coords) {
                float cx = Math.Abs(x - coord.x);
                float cy = Math.Abs(y - coord.y);
                if (closestCoord == null || cx * cx + cy * cy < dx * dx + dy * dy) {
                    closestCoord = coord;
                    dx = cx;
                    dy = cy;
                }
            }
            return closestCoord;
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
        private static void normalise(List<Coord> coords) {
            float maxElevation = 0;
            foreach (Coord coord in coords) {
                maxElevation = Math.Max(coord.elevation, maxElevation);
            }
            if (maxElevation <= 0) {
                return;
            }
            foreach (Coord coord in coords) {
                coord.elevation /= maxElevation;
                if (coord.elevation < 0) {
                    coord.elevation = 0;
                }
            }
        }

        private static void invert(List<Coord> coords) {
            foreach (Coord coord in coords) {
                coord.elevation = -coord.elevation;
            }
        }

        private static void offsetElevation(List<Coord> coords, float elevation) {
            foreach (Coord coord in coords) {
                coord.elevation += elevation;
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
                float totalElevation = 0;
                List<Center> neighbours = center.neighbours;
                foreach (Center neighbour in neighbours) {
                    totalElevation += neighbour.coord.elevation;
                }
                center.coord.elevation = totalElevation / (float) neighbours.Count;
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
