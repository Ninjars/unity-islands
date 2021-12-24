using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using WorldGenerator;

namespace Elevation {
	public class ElevationFunctions {
        
        internal static void addBump(Vector3 center, float graphSize, List<Coord> coords, float radius, float verticalScale, float x, float y) {
            Coord initial = ElevationUtils.findClosestCoord(x, y, coords);
            float radialFactor = 1f - Vector3.Distance(center, initial.toVector3()) * 2 / graphSize;
            ElevationUtils.elevate(initial, coords, radius, verticalScale * radialFactor, 2);
        }

        internal static void addCone(List<Coord> coords, float radius, float x, float y, float verticalScale) {
            ElevationUtils.elevate(coords, radius, x, y, verticalScale, 1);
        }

        internal static void addNoise(List<Coord> coords, RandomProvider random, float horizontalScale, float verticalScale) {
            float offset = random.getFloat();
            foreach (Coord coord in coords) {
                var perlin = ElevationUtils.getPerlin(offset, 
                                        horizontalScale, 
                                        coord.x, 
                                        coord.y);
                coord.setElevation(coord.elevation + perlin * verticalScale);
            }
        }

        /**
            Applys perlin noise across the map with a radial bias, maximum strength at the center, 0 at map width.
            Changing the horizonal scale should change the frequency of the effect.  Smaller numbers = less detailed.
            Change the vertical scale to change the strength of the effect.
         */
        internal static void addRadialWeightedNoise(Vector3 center, float radius, List<Coord> coords, RandomProvider random, float horizontalScale, float verticalScale) {
			float offset = random.getFloat();
            foreach (Coord coord in coords) {
                var perlin = ElevationUtils.getPerlin(offset, 
                                        horizontalScale, 
                                        coord.x / radius, 
                                        coord.y / radius);
                float radialFactor = 1f - Mathf.Pow(Vector3.Distance(center, coord.toVector3()) * 2 / radius, 2);
                coord.setElevation(coord.elevation + perlin * verticalScale * radialFactor);
            }
        }

        /**
            Scale positive elevations to lie between 0 and 1, and flatten all negative elevations to 0
         */
        internal static void normalise(List<Coord> coords) {
            float maxElevation = 0;
            foreach (Coord coord in coords) {
                maxElevation = Math.Max(coord.elevation, maxElevation);
            }
            if (maxElevation <= 0) {
                return;
            }
            foreach (Coord coord in coords) {
                coord.setElevation(coord.elevation / maxElevation);
                if (coord.elevation < 0) {
                    coord.setElevation(0);
                }
            }
        }

        /**
            averages heights between neighbouring centers to smooth the terrain
        */
        internal static void smooth(List<Center> centers) {
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
                center.coord.setElevation(totalElevation / (float) neighbours.Count);
            }
        }

        internal static void performWaterErosion(List<Corner> corners) {
            foreach (Corner corner in corners) {
                corner.coord.setElevation(corner.coord.elevation - 0.1f * corner.moisture);
            }
        }
	}
}
