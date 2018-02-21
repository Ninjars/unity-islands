using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator {
    public class WorldGenBiomes {

        private const float LAKE_THRESHOLD = 0.3f;

        internal static void separateTheLandFromTheWater(World world, PerlinIslandShape perlinIslandShape) {
            // assign coarse water/land separation to corners
            foreach (Corner corner in world.corners) {
                bool isWater = !isInsideShape(perlinIslandShape, corner.coord);
                if (corner.isBorder) {
                    corner.terrainType = TerrainType.OCEAN;
                } else if (isWater) {
                    corner.terrainType = TerrainType.LAKE;
                } else {
                    corner.terrainType = TerrainType.LAND;
                }
            }

            // assign coarse water/land separation to centers
            List<Center> borderCenters = new List<Center>();
            foreach (Center center in world.centers) {
                int waterCornerCount = 0;
                foreach (Corner corner in center.corners) {
                    if (corner.isBorder) {
                        center.terrainType = TerrainType.OCEAN;
                        borderCenters.Add(center);
                        continue;
                    }
                    if (corner.isWater()) {
                        waterCornerCount++;
                    }
                }
                if (center.terrainType == TerrainType.OCEAN) {
                    continue;
                } else if (waterCornerCount >= center.corners.Count * LAKE_THRESHOLD) {
                    center.terrainType = TerrainType.LAKE;
                } else {
                    center.terrainType = TerrainType.LAND;
                }
            }

            floodFillOceanCenters(borderCenters);

            markOceanCenters(world.centers);

            // TODO: coast and shallows
        }

        private static bool isInsideShape(PerlinIslandShape shape, Coord coordinate) {
            return shape.isInside((float)coordinate.x, (float)coordinate.y);
        }

        private static void markOceanCenters(List<Center> centers) {
            foreach (Center center in centers) {
                int oceans = 0;
                int lands = 0;
                foreach (Center c in center.neighbours) {
                    if (c.isLand()) {
                        lands++;
                    } else if (c.isOcean()) {
                        oceans++;
                    }
                    if (oceans > 0 && lands > 0) {
                        break;
                    }
                }
                if (oceans > 0 && lands > 0) {
                    if (center.isLand()) {
                        center.terrainType = TerrainType.COAST;
                    } else {
                        center.terrainType = TerrainType.OCEAN;
                    }
                }
            }
        }

        private static void floodFillOceanCenters(List<Center> borderCenters) {
            int i = 0;
            while (i < borderCenters.Count) {
                Center c = borderCenters[i];
                foreach (Center other in c.neighbours) {
                    if (other.terrainType == TerrainType.LAKE) {
                        other.terrainType = TerrainType.OCEAN;
                        borderCenters.Add(other);
                    }
                }
                i++;
            }
        }
    }
}
