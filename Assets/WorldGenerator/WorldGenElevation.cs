﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Elevation;
using UnityEngine;

namespace WorldGenerator {
    public class WorldGenElevation {
        private readonly System.Random random;
        private readonly Graph graph;
        private readonly float clippingPlaneHeight;
        private ElevationHelper elevationHelper;

        public WorldGenElevation(Graph graph, float clippingPlaneHeight) {
            this.graph = graph;
            this.clippingPlaneHeight = clippingPlaneHeight;
            random = new System.Random(graph.seed);
        }
        
        public void generateElevations() {
            List<Coord> graphCenterCoords = graph.centers.Select(c => c.coord).ToList();
            elevationHelper = new ElevationHelper(random, graph.center, graph.size, graphCenterCoords);
            applyNoise();
            applyPlateauCluster();
            applyPlateauCluster();
            applyCraterCluster(radius: 0.1f);
            applyCraterCluster(radius: 0.08f);
            applyCraterCluster(radius: 0.05f);
            complete();
        }

        private void applyNoise() {
            elevationHelper
                    .islandCenteredNoise(2f, 0.1f)
                    .islandCenteredNoise(5f, 0.1f)
                    .noise(0.005f, 0.05f)
                    .noise(10f, 0.01f);
        }

        private void applyMoundCluster(float height = 0.1f, float radius = 0.1f, int count = 3) {
            float originX = (float) random.NextDouble();
            float originY = (float) random.NextDouble();
            float moundRadius = radius * (0.75f + (float) random.NextDouble() * 0.5f);
            float originHeight = height * (0.9f + (float) random.NextDouble() * 0.2f);
            elevationHelper.mound(originX, originY, originHeight, moundRadius);
            for (int i = 1; i < count; i++) {
                float offset = moundRadius * (1f + (float) random.NextDouble());
                float angle = (float) random.NextDouble() * 360;
                float moundHeight = originHeight * (0.9f + (float) random.NextDouble()  * 0.2f);
                elevationHelper.mound(offsetX(originX, angle, offset), offsetY(originY, angle, offset), moundHeight, moundRadius);
            }
        }

        private void applyCraterCluster(float radius = 0.1f, int count = 3) {
            float originX = (float) random.NextDouble();
            float originY = (float) random.NextDouble();
            float craterRadius = radius * (0.75f + (float) random.NextDouble() * 0.5f);
            elevationHelper.crater(originX, originY, craterRadius);
            for (int i = 1; i < count; i++) {
                float offset = craterRadius * (0.5f + (float) random.NextDouble());
                float angle = (float) random.NextDouble() * 360;
                elevationHelper.crater(offsetX(originX, angle, offset), offsetY(originY, angle, offset), craterRadius);
            }
        }

        private void applyPlateauCluster(float height = 0.05f, float radius = 0.4f, int count = 2) {
            float x = (float) random.NextDouble();
            float y = (float) random.NextDouble();
            float plateauRadius = radius * (0.75f + (float) random.NextDouble() * 0.5f);
            float originHeight = height * (0.9f + (float) random.NextDouble() * 0.2f);
            elevationHelper.plateau(x, y, originHeight, plateauRadius);
            for (int i = 1; i < count; i++) {
                float offset = plateauRadius * (0.5f * (float) random.NextDouble());
                float angle = (float) random.NextDouble() * 360;
                plateauRadius = plateauRadius * (0.3f + (float) random.NextDouble() * 0.3f);
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
                    .normalise()
                    .smooth(graph.centers, 1)
                    .erode(graph.corners)
                    .updateCornerElevations(graph.corners)
                    .clip(graph.centers, graph.corners, clippingPlaneHeight);
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
