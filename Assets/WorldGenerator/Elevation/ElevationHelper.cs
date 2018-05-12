using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldGenerator;
using System.Linq;

namespace Elevation {
	public class ElevationHelper {
        private readonly System.Random random;
        private readonly Vector3 centerPos;
        private readonly List<Coord>  coords;
        private bool cornersUpToDate = false;
		private float size;

        public ElevationHelper(System.Random random, Vector3 center, float size, List<Coord> centerCoords) {
            this.random = random;
			this.centerPos = center;
			this.size = size;
			this.coords = centerCoords;
		}

		public ElevationHelper linearRadialGradient(float height) {
			return linearRadialGradient(0.5f, 0.5f, height, 0.5f);
		}

		public ElevationHelper linearRadialGradient(float x, float y, float height) {
			return linearRadialGradient(x, y, height, 0.5f);
		}

		/**
			radius, x and y are normalised to map size
		 */
		public ElevationHelper linearRadialGradient(float x, float y, float height, float radius) {
			ElevationFunctions.addCone(coords, radius * size, x * size, y * size, height);
			cornersUpToDate = false;
			return this;
		}

		public ElevationHelper plateau(float height, float radius) {
			return plateau((float) random.NextDouble(), (float) random.NextDouble(), height, radius);
		}

		/**
			radius, x and y are normalised to map size.
		 */
		public ElevationHelper plateau(float x, float y, float height, float radius) {
			PerlinRadialShape shape = new PerlinRadialShape(random, radius * size, 1, x * size, y * size);
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
			return mound((float) random.NextDouble(), (float) random.NextDouble(), height, radius);
		}

		public ElevationHelper mound(float x, float y, float height, float radius) {
			ElevationFunctions.addBump(centerPos, size, coords, radius, height, x, y);
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

		public ElevationHelper islandCenteredNoise(float scale, float height) {
			ElevationFunctions.addRadialWeightedNoise(centerPos, size, coords, random, scale, height);
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
			for (int i = 0; i < passes; i++ ) {
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
            while(queue.Count > 0) {
                Center next = queue[queue.Count-1];
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
