using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Elevation;
using UnityEngine;
using Utils;

namespace WorldGenerator {

    public class WorldGenElevation {
        private readonly RandomProvider random;
        private readonly Graph graph;
        private readonly float clippingPlaneHeight;
        private ElevationHelper elevationHelper;

        public WorldGenElevation(Graph graph, float clippingPlaneHeight) {
            this.graph = graph;
            this.clippingPlaneHeight = clippingPlaneHeight;
            random = new SeededRandomProvider(graph.seed);
        }
        
        public void generateElevations() {
            elevationHelper = new ElevationHelper(random, graph.center, graph.size, graph.centers);
            elevationHelper.applyFeatures(new List<ElevationFeature>{
                new ElevationFeature(
                    feature: ElevationFeature.FeatureType.NOISE,
                    minHeight: 0.5f, maxHeight: 2f,
                    noiseScale: 0.5f,
                    minIterations: 3, maxIterations: 3
                ),
                new ElevationFeature(
                    feature: ElevationFeature.FeatureType.NOISE,
                    minHeight: 2f, maxHeight: 8f,
                    noiseScale: 0.05f,
                    minIterations: 1, maxIterations: 2
                ),
                new ElevationFeature(
                    feature: ElevationFeature.FeatureType.POSITION_BIASED_NOISE,
                    minHeight: 2f, maxHeight: 8f,
                    minFeatureRadius: graph.size * 0.25f, maxFeatureRadius: graph.size * 0.75f,
                    noiseScale: 0.5f,
                    minOffset: 0, maxOffset: 0.6f,
                    minIterations: 1, maxIterations: 2
                ),
                new ElevationFeature(
                    feature: ElevationFeature.FeatureType.MOUND,
                    minHeight: 8f, maxHeight: 20f,
                    minFeatureRadius: graph.size * 0.25f, maxFeatureRadius: graph.size * 0.6f,
                    minOffset: 0, maxOffset: 0.8f,
                    minIterations: 1, maxIterations: 6
                ),
                new ElevationFeature(
                    feature: ElevationFeature.FeatureType.PLATEAU,
                    minHeight: 5f, maxHeight: 20f,
                    minFeatureRadius: graph.size * 0.05f, maxFeatureRadius: graph.size * 0.4f,
                    minOffset: 0, maxOffset: 0.8f,
                    minIterations: 1, maxIterations: 6
                ),
            });
            // applyNoise(3);
            // applyPlateauCluster(height: 20, radius: 0.1f, count: 4);
            // applyPlateauCluster();
            // applyCraterCluster(radius: 0.1f);
            // applyCraterCluster(radius: 0.08f);
            // applyCraterCluster(radius: 0.05f);
            complete();
        }

        private void applyNoise(float scale) {
            elevationHelper
                    .radialNoise(0.5f, 0.5f, graph.size * 0.5f, 2f, scale)
                    .radialNoise(0.5f, 0.5f, graph.size * 0.5f, 5f, scale)
                    .noise(0.005f, scale * 0.5f)
                    .noise(10f, scale * 0.1f);
        }

        private void applyMoundCluster(float height = 0.1f, float radius = 0.1f, int count = 3) {
            float originX = random.getFloat();
            float originY = random.getFloat();
            float moundRadius = radius * (0.75f + random.getFloat() * 0.5f);
            float originHeight = height * (0.9f + random.getFloat() * 0.2f);
            elevationHelper.mound(originX, originY, originHeight, moundRadius);
            for (int i = 1; i < count; i++) {
                float offset = moundRadius * (1f + random.getFloat());
                float angle = random.getFloat() * 360;
                float moundHeight = originHeight * (0.9f + random.getFloat()  * 0.2f);
                elevationHelper.mound(offsetX(originX, angle, offset), offsetY(originY, angle, offset), moundHeight, moundRadius);
            }
        }

        private void applyCraterCluster(float radius = 0.1f, int count = 3) {
            float originX = random.getFloat();
            float originY = random.getFloat();
            float craterRadius = radius * (0.75f + random.getFloat() * 0.5f);
            elevationHelper.crater(originX, originY, craterRadius);
            for (int i = 1; i < count; i++) {
                float offset = craterRadius * (0.5f + random.getFloat());
                float angle = random.getFloat() * 360;
                elevationHelper.crater(offsetX(originX, angle, offset), offsetY(originY, angle, offset), craterRadius);
            }
        }

        private void applyPlateauCluster(float height = 0.05f, float radius = 0.4f, int count = 2) {
            float x = random.getFloat();
            float y = random.getFloat();
            float plateauRadius = radius * (0.75f + random.getFloat() * 0.5f);
            float originHeight = height * (0.9f + random.getFloat() * 0.2f);
            elevationHelper.plateau(x, y, originHeight, plateauRadius);
            for (int i = 1; i < count; i++) {
                float offset = plateauRadius * (0.5f * random.getFloat());
                float angle = random.getFloat() * 360;
                plateauRadius = plateauRadius * (0.3f + random.getFloat() * 0.3f);
                x = offsetX(x, angle, offset);
                y = offsetY(y, angle, offset);
                elevationHelper.plateau(x, y, originHeight, plateauRadius);
            }
        }

        private static float offsetX(float x, float angle, float distance) {
            return x + Mathf.Cos(angle) * distance;
        }

        private static float offsetY(float y, float angle, float distance) {
            return y + Mathf.Sin(angle) * distance;
        }

        private void complete() {
            elevationHelper
                    .smooth(graph.centers, 2)
                    .updateCornerElevations(graph.corners)
                    .clip(graph.centers, graph.corners, clippingPlaneHeight);
        }

        public static void generateIslandUndersideElevations(int seed, Island island) {
            RandomProvider random = new SeededRandomProvider(seed);
            List<Coord> islandCoords = island.undersideCoords.Where(c => !c.coord.isFixed).Select(c => c.coord.coord).ToList();
            float islandMinDim = island.topsideBounds.min.magnitude;
            
            ElevationFunctions.addRadialWeightedNoise(island.center, islandMinDim * 0.5f, islandCoords, random, 7f, 100f);
            ElevationFunctions.addRadialWeightedNoise(island.center, islandMinDim * 0.8f, islandCoords, random, 15f, 30f);
            ElevationFunctions.addNoise(islandCoords, random, 30f, 5f);
            ElevationUtils.invert(islandCoords);
        }
    }
}
