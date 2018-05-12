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
            elevate(initial, coords, radius, x, y, verticalScale, power);
        }

        internal static void elevate(Coord initial, List<Coord> coords, float radius, float x, float y, float verticalScale, float power) {
            radius = Mathf.Max(radius, 1);
            foreach (Coord current in coords) {
                float distanceFromCenter = initial == current ? 0 : Vector3.Distance(initial.toVector3(), current.toVector3());
                if (distanceFromCenter > radius) {
                    continue;
                }
                float distanceFactor = Mathf.Pow(distanceFromCenter / radius, 1);
                current.setElevation(current.elevation + verticalScale * (1f - distanceFactor));
            }
        }

        internal static List<Center> elevate(Center initial, float width, float verticalScale, Center current, List<Center> processed, float falloffPower) {
            processed.Add(current);
            float distanceFromCenter = initial == current ? 0 : Vector3.Distance(initial.coord.toVector3(), current.coord.toVector3());
            if (distanceFromCenter > width) {
                return processed;
            }
            float distanceFactor = Mathf.Pow(distanceFromCenter / width, falloffPower);
            float elevation = verticalScale * (1f - distanceFactor);
            current.coord.setElevation(current.coord.elevation + elevation);
            foreach (Center center in current.neighbours) {
                if (!processed.Contains(center)) {
                    processed = elevate(initial, width, verticalScale, center, processed, falloffPower);
                }
            }
            return processed;
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
                coord.setElevation(coord.elevation - coord.elevation);
            }
        }

        internal static void offsetElevation(List<Coord> coords, float elevation) {
            foreach (Coord coord in coords) {
                coord.setElevation(coord.elevation + elevation);
            }
        }
	}
}
