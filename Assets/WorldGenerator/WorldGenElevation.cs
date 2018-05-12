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

            new ElevationHelper(random, graph.center, graph.size, graphCenterCoords)
                    .islandCenteredNoise(2f, 0.1f)
                    .islandCenteredNoise(5f, 0.1f)
                    .noise(0.005f, 0.05f)
                    .noise(10f, 0.01f)
                    .mound(height: 0.01f, radius: 0.4f)
                    .mound(height: 0.01f, radius: 0.4f)
                    .mound(height: 0.05f, radius: 0.3f)
                    .mound(height: 0.05f, radius: 0.3f)
                    .mound(height: 0.05f, radius: 0.3f)
                    .plateau(height: 0.05f, radius: 0.4f)
                    .plateau(height: 0.05f, radius: 0.2f)
                    .plateau(height: 0.05f, radius: 0.3f)
                    .normalise()
                    .smooth(graph.centers, 1)
                    .erode(graph.corners)
                    .updateCornerElevations(graph.corners)
                    .clip(graph.centers, graph.corners, clippingPlaneHeight)
                    ;
        }

        public static void generateIslandUndersideElevations(int seed, Island island) {
            System.Random random = new System.Random(seed);
            List<Coord> islandCoords = island.undersideCoords.Where(c => !c.coord.isFixed).Select(c => c.coord.coord).ToList();
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

    }
}
