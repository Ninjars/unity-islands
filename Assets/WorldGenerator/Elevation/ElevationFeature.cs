using UnityEngine;
using Utils;

namespace Elevation {
    public class ElevationFeature {

        public enum FeatureType {
            RADIAL_GRADIENT, NOISE, POSITION_BIASED_NOISE, SMOOTH, MOUND, PLATEAU, CRATER
        }

        public FeatureType feature;

        // vertical scale of feature, in world units
        public float minHeight, maxHeight;

        // used for noise functions. Smaller values produces corser noise.
        public float noiseScale;

        // range of radius for discrete features, in world units
        public float minFeatureRadius, maxFeatureRadius;

        // distance allowed from center of world to origin of feature, in fraction of world radius. <0 means no restriction.
        public float minOffset, maxOffset;

        // number of times to apply the feature with the same parameters (though different random values)
        public int minIterations, maxIterations;

        public ElevationFeature(
            FeatureType feature,
            float minHeight,
            float maxHeight,
            float noiseScale = 0,
            float minFeatureRadius = -1,
            float maxFeatureRadius = -1,
            float minOffset = -1,
            float maxOffset = -1,
            int minIterations = -1,
            int maxIterations = -1
        ) {
            this.feature = feature;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;
            this.noiseScale = noiseScale;
            this.minFeatureRadius = minFeatureRadius;
            this.maxFeatureRadius = maxFeatureRadius;
            this.minOffset = minOffset;
            this.maxOffset = maxOffset;
            this.minIterations = minIterations;
            this.maxIterations = maxIterations;
        }

        public Vector2 getOffsetVector(RandomProvider random) {
            Vector2 normalisedOffset;
            if (minOffset < 0 || maxOffset < 0) {
                normalisedOffset = Utils.Utils.RandomRadial2DUnitVector(random,  random.getFloat());
            } else {
                normalisedOffset = Utils.Utils.RandomRadial2DUnitVector(random,  random.getFloat(minOffset, maxOffset));
            }
            return normalisedOffset * 0.5f + Vector2.one * 0.5f;
        }

        public float getRadius(RandomProvider random) {
            return random.getFloat(minFeatureRadius, maxFeatureRadius);
        }

        public float getHeight(RandomProvider random) {
            return random.getFloat(minHeight, maxHeight);
        }

        public int getIterations(RandomProvider random) {
            if (minIterations < 0 || maxIterations < 0) {
                return 1;
            } else {
                return random.getInt(minIterations, maxIterations);
            }
        }
    }
}
