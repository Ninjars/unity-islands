using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGenerator;
using System.Linq;
using Utils;

namespace Elevation {
    public class ElevationHelper {
        private readonly RandomProvider random;
        private readonly Vector3 centerPos;
        private readonly List<Center> centers;
        private readonly List<Coord> coords;
        private bool cornersUpToDate = false;
        private float size;

        public ElevationHelper(RandomProvider random, Vector3 center, float size, List<Center> centers) {
            this.random = random;
            this.centerPos = center;
            this.size = size;
            this.centers = centers;
            this.coords = centers.Select(c => c.coord).ToList();
        }

        public ElevationHelper applyFeatures(List<ElevationFeature> features) {
            foreach (var feature in features) {
                int iterations = feature.getIterations(random);
                foreach (var i in Enumerable.Range(0, iterations)) {
                	Vector2 offset = feature.getOffsetVector(random);
                    switch (feature.feature) {
                        case ElevationFeature.FeatureType.RADIAL_GRADIENT:
                            linearRadialGradient(offset.x, offset.y, feature.getHeight(random), feature.getRadius(random));
                            break;
                        case ElevationFeature.FeatureType.NOISE:
                            noise(feature.noiseScale, feature.getHeight(random));
                            break;
                        case ElevationFeature.FeatureType.POSITION_BIASED_NOISE:
                            radialNoise(offset.x, offset.y, feature.getRadius(random), feature.noiseScale, feature.getHeight(random));
                            break;
                        case ElevationFeature.FeatureType.SMOOTH:
                            ElevationFunctions.smooth(centers);
                            break;
                        case ElevationFeature.FeatureType.MOUND:
							mound(offset.x, offset.y, feature.getHeight(random), feature.getRadius(random));
                            break;
                        case ElevationFeature.FeatureType.PLATEAU:
							plateau(offset.x, offset.y, feature.getHeight(random), feature.getRadius(random));
                            break;
                        case ElevationFeature.FeatureType.CRATER:
							crater(offset.x, offset.y, feature.getRadius(random));
                            break;
                    }
                }
            }
            return this;
        }

        /**
			radius, x and y are normalised to map size
		 */
        public ElevationHelper linearRadialGradient(float x, float y, float height, float radius) {
            ElevationFunctions.addCone(coords, radius, x * size, y * size, height);
            cornersUpToDate = false;
            return this;
        }

        public ElevationHelper plateau(float height, float radius) {
            return plateau(random.getFloat(), random.getFloat(), height, radius);
        }

        /**
			radius, x and y are normalised to map size.
		 */
        public ElevationHelper plateau(float x, float y, float height, float radius) {
            PerlinRadialShape shape = new PerlinRadialShape(random, radius, 1, x * size, y * size);
            Coord centralCoord = ElevationUtils.findClosestCoord(x * size, y * size, coords);
            float plateauHeight = centralCoord.elevation + height;
            foreach (var coord in coords) {
                if (shape.isInside(coord.x, coord.y)) {
                    float heightMod = (centralCoord.elevation - coord.elevation) * 0.1f;
                    coord.setElevation(plateauHeight + heightMod);
                }
            }
            return this;
        }

        public ElevationHelper mound(float height, float radius) {
            return mound(random.getFloat(), random.getFloat(), height, radius);
        }

        public ElevationHelper mound(float x, float y, float height, float radius) {
            ElevationFunctions.addBump(centerPos, size, coords, radius, height, x * size, y * size);
            return this;
        }

        public ElevationHelper crater(float radius) {
            return crater(random.getFloat(), random.getFloat(), radius);
        }

        public ElevationHelper crater(float x, float y, float radius) {
            Coord origin = ElevationUtils.findClosestCoord(x * size, y * size, coords);
            float originDepth = radius * 0.5f;
            float wallHeight = radius * 0.25f;
            float totalHeightDelta = originDepth + wallHeight;
            float wallFalloff = radius + radius * 0.25f;
            foreach (var coord in coords) {
                float distance = Vector3.Distance(origin.toVector3(), coord.toVector3());
                if (distance < radius) {
                    float factor = distance / radius;
                    coord.changeElevationBy(totalHeightDelta * (factor * factor * factor) - originDepth);
                } else if (distance < wallFalloff) {
                    float factor = (distance - radius) / (wallFalloff - radius);
                    coord.changeElevationBy(wallHeight * (1 - (factor * factor)));
                }
            }
            return this;
        }

        /**
			Smaller scale = less detailed noise. 
		*/
        public ElevationHelper noise(float scale, float height) {
            ElevationFunctions.addNoise(coords, random, scale, height);
            cornersUpToDate = false;
            return this;
        }

        public ElevationHelper radialNoise(float x, float y, float radius, float scale, float height) {
			Debug.Log($"radialNoise({x}, {y})");
            ElevationFunctions.addRadialWeightedNoise(new Vector3(x * size, 0, y * size), radius, coords, random, scale, height);
            return this;
        }

        public ElevationHelper erode(List<Corner> corners) {
            updateCornerElevations(corners);
            ElevationUtils.calculateDownslopes(corners);
            ElevationUtils.calculateMoisture(corners);
            ElevationFunctions.performWaterErosion(corners);
            return this;
        }

        public ElevationHelper normalise() {
            ElevationFunctions.normalise(coords);
            return this;
        }

        public ElevationHelper smooth(List<Center> centers, int passes) {
            for (int i = 0; i < passes; i++) {
                ElevationFunctions.smooth(centers);
            }
            return this;
        }

        public ElevationHelper updateCornerElevations(List<Corner> corners) {
            if (!cornersUpToDate) {
                ElevationUtils.assignCornerElevations(corners);
                cornersUpToDate = true;
            }
            return this;
        }

        public void clip(List<Center> centers, List<Corner> corners, float clippingPlaneHeight) {
            List<Center> clippedCenters = centers.Where(center => center.coord.elevation < clippingPlaneHeight).ToList();
            List<Center> borderCenters = centers.Where(center => center.isBorder).ToList();
            List<Center> queue = new List<Center>(borderCenters);
            while (queue.Count > 0) {
                Center next = queue[queue.Count - 1];
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

            foreach (Corner corner in corners) {
                int clippedCenterCount = corner.GetTouches().Where(center => center.isClipped).ToList().Count;
                corner.isClipped = clippedCenterCount == corner.GetTouches().Count;
                corner.isIslandRim = !corner.isClipped && clippedCenterCount > 0;
            }
        }
    }
}
