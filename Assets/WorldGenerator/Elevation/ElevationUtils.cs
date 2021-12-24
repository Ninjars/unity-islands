using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGenerator;

namespace Elevation {
	public class ElevationUtils {

		internal static float getPerlin(float offset, float scale, float x, float y) {
			return Mathf.Clamp01(Mathf.PerlinNoise(scale * (offset + x), scale * (offset + y)));
		}

        internal static void elevate(List<Coord> coords, float radius, float x, float y, float verticalScale, float power) {
            Coord initial = findClosestCoord(x, y, coords);
            elevate(initial, coords, radius, verticalScale, power);
        }

        internal static void elevate(Coord initial, List<Coord> coords, float radius, float verticalScale, float power) {
            radius = Mathf.Max(radius, 1);
            foreach (Coord current in coords) {
                float distanceFromCenter = initial == current ? 0 : Vector3.Distance(initial.toVector3(), current.toVector3());
                if (distanceFromCenter > radius) {
                    continue;
                }
                float distanceFactor = Mathf.Pow(distanceFromCenter / radius, power);
                current.changeElevationBy(verticalScale * (1f - distanceFactor));
            }
        }

        internal static Coord findClosestCoord(float x, float y, List<Coord> coords) {
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

        internal static Center findClosestCenter(float x, float y, List<Center> centers) {
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

        internal static void invert(List<Coord> coords) {
            foreach (Coord coord in coords) {
                coord.setElevation(-coord.elevation);
            }
        }

        internal static void offsetElevation(List<Coord> coords, float elevation) {
            foreach (Coord coord in coords) {
                coord.setElevation(coord.elevation + elevation);
            }
        }

        internal static void assignCornerElevations(List<Corner> corners) {
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
