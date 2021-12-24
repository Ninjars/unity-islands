using UnityEngine;

namespace Utils {
    public interface RandomProvider : RandomIntProvider, RandomFloatProvider {}

    public interface RandomIntProvider {
        int getInt(int max);
        int getInt(int min, int max);
    }

    public interface RandomFloatProvider {
        float getFloat(float max = 1);
        float getFloat(float min, float max);
    }

    public class UnityRandomProvider : RandomProvider {
        int RandomIntProvider.getInt(int max) {
            return Mathf.FloorToInt(UnityEngine.Random.value * max);
        }
        int RandomIntProvider.getInt(int min, int max) {
            return Mathf.FloorToInt(UnityEngine.Random.Range(min, max));
        }

        float RandomFloatProvider.getFloat(float max) {
            return UnityEngine.Random.value * max;
        }

        float RandomFloatProvider.getFloat(float min, float max) {
            return UnityEngine.Random.Range(min, max);
        }
    }

    public class SeededRandomProvider : RandomProvider {

        private readonly System.Random random;

        public SeededRandomProvider(int seed) {
            random = new System.Random(seed);
        }

        public SeededRandomProvider(string seed) {
            if (seed.Length == 0) {
                random = new System.Random();
            } else {
                random = new System.Random(seed.GetHashCode());
            }
        }

        float RandomFloatProvider.getFloat(float max) {
            return (float) random.NextDouble() * max;
        }

        float RandomFloatProvider.getFloat(float min, float max) {
            return min + (float) random.NextDouble() * (max - min);
        }

        int RandomIntProvider.getInt(int max) {
            return random.Next(max);
        }

        int RandomIntProvider.getInt(int min, int max) {
            return random.Next(min, max);
        }
    }
}
