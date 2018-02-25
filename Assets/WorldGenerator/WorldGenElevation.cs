using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
    public static class WorldGenElevation {
        
        public static void createVolcanicIsland(World world) {
            addCone(world, world.size / 2f, world.size / 2f, 100f, 0.5f);
            updateCorners(world.corners);
        }

        private static void addCone(World world, float x, float y, float verticalScale, float horizonalScale) {
            Center initial = findClosestCenter(x, y, world.centers);
            float width = world.size * horizonalScale;
            elevate(initial, width, verticalScale, initial, new List<Center>());
        }

        private static List<Center> elevate(Center initial, float width, float verticalScale, Center current, List<Center> processed) {
            processed.Add(current);
            double distanceFromCenter = initial == current ? 0 : Coord.distanceBetween(initial.coord, current.coord);
            if (distanceFromCenter > width) {
                return processed;
            }
            double elevation = verticalScale * (1.0 - (distanceFromCenter / width));
            current.elevation += elevation;
            foreach (Center center in current.neighbours) {
                if (!processed.Contains(center)) {
                    processed = elevate(initial, width, verticalScale, center, processed);
                }
            }
            return processed;
        }

        private static Center findClosestCenter(float x, float y, List<Center> centers) {
            Center closestCenter = null;
            float dx = 0;
            float dy = 0;
            foreach (Center center in centers) {
                float cx = Mathf.Abs(x - (float) center.coord.x);
                float cy = Mathf.Abs(y - (float) center.coord.y);
                if (closestCenter == null || cx * cx + cy * cy < dx * dx + dy * dy) {
                    closestCenter = center;
                    dx = cx;
                    dy = cy;
                }
            }
            return closestCenter;
        }

        private static void updateCorners(List<Corner> corners) {
            foreach (Corner corner in corners) {
                double elevation = 0;
                List<Center> touchesCenters = corner.GetTouches();
                foreach (Center center in touchesCenters) {
                    elevation += center.elevation;
                }
                corner.elevation = elevation / touchesCenters.Count;
            }
        }
    }
}
