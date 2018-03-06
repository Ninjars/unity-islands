using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
    public static class WorldGenElevation {
        
        public static void createIsland(World world, float clippingPlaneHeight) {
            System.Random random = new System.Random(world.seed);
            addCone(world, world.size / 2f, world.size / 2f, 10f, 0.4f);
            applyNoise(world, random, 15f, 25f);
            applyNoise(world, random, 8f, 10f);
            applyNoise(world, random, 1f, 5f);
            for (int i = 0; i < 5; i++) {
                var x = random.NextDouble() * world.size;
                var y = random.NextDouble() * world.size;
                addBump(world, 20f, x, y);
            }
            for (int i = 0; i < 3; i++) {
                var x = random.NextDouble() * world.size;
                var y = random.NextDouble() * world.size;
                addBump(world, -10f, x, y);
            }
            for (int i = 0; i < 3; i++) {
                smooth(world.centers);
            }
            normalise(world.centers);

			calculateDownslopes(world.centers);
            calculateMoisture(world.centers);
            performWaterErosion(world.centers);
            normalise(world.centers);

            assignCornerElevations(world.corners);
            
            calculateClipping(world, clippingPlaneHeight);
        }

        private static void calculateClipping(World world, float clippingPlaneHeight) {
            foreach (Center center in world.centers) {
			    center.isClipped = center.elevation < clippingPlaneHeight;
            }
            foreach (Corner corner in world.corners) {
                corner.isClipped = corner.elevation < clippingPlaneHeight;
            }
            foreach (Center center in world.centers) {
                if (!center.isClipped) {
                    foreach (Center neigh in center.neighbours) {
                        if (neigh.isClipped) {
                            center.isOnRim = true;
                            break;
                        }
                    }
                }
            }
            foreach (Corner corner in world.corners) {
                if (!corner.isClipped) {
                    foreach (Corner adj in corner.GetAdjacents()) {
                        if (adj.isClipped) {
                            corner.isIslandRim = true;
                            break;
                        }
                    }
                }
            }
        }

        /**
            Applys perlin noise across the map with a radial bias, maximum strength at the center, 0 at map width.!--
            Changing the horizonal scale should change the frequency of the effect.  Smaller numbers = less detailed.
            Change the vertical scale to change the strength of the effect.
         */
        private static void applyNoise(World world, System.Random random, float horizontalScale, float verticalScale) {
			float offset = (float) random.NextDouble();
            foreach (Center center in world.centers) {
                var perlin = getPerlin(offset, 
                                        horizontalScale, 
                                        (float) center.coord.x / world.size, 
                                        (float) center.coord.y / world.size);
                var radialFactor = 1.0 - (Coord.distanceBetween(world.center, center.coord) * 2 / world.size);
                center.elevation += perlin * verticalScale * radialFactor;
            }
        }

		private static float getPerlin(float offset, float scale, float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x), scale * (offset + y)));
		}

        private static void addBump(World world, float verticalScale, double x, double y) {
            Center initial = findClosestCenter(x, y, world.centers);
            float width = world.size / 10f;
            float radialFactor = 1.0f - (float) (Coord.distanceBetween(world.center, initial.coord) * 2 / world.size);
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

        private static void addCone(World world, double x, double y, float verticalScale, float horizonalScale) {
            Center initial = findClosestCenter(x, y, world.centers);
            float width = world.size * horizonalScale;
            elevate(initial, width, verticalScale, initial, new List<Center>(), 1);
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
