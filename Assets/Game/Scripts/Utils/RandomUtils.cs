using System;
using UnityEngine;
using UnityEngine.AI;

namespace Utils {
    public class RandomUtils {
        public static Vector3 RandomNavSphere(RandomFloatProvider random, Vector3 origin, float distance, int layermask) {
            Vector3 randomDirection = RandomPointInSphere(random) * distance;
            randomDirection += origin;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

            return navHit.position;
        }

        //https://karthikkaranth.me/blog/generating-random-points-in-a-sphere/
        public static Vector3 RandomPointInSphere(RandomFloatProvider randomProvider) {
            var u = randomProvider.getFloat(1f);
            var v = randomProvider.getFloat(1f);
            var theta = u * 2.0 * Math.PI;
            var phi = Mathf.Acos(2f * v - 1f);
            // cube root to give even distribution, rather than clumping to origin
            var r = Mathf.Pow(randomProvider.getFloat(1f), 1f / 3f);
            var sinTheta = Math.Sin(theta);
            var cosTheta = Math.Cos(theta);
            var sinPhi = Math.Sin(phi);
            var cosPhi = Math.Cos(phi);
            var x = r * sinPhi * cosTheta;
            var y = r * sinPhi * sinTheta;
            var z = r * cosPhi;
            return new Vector3((float)x, (float)y, (float)z);
        }

        public static Vector2 RandomRadial2DUnitVector(RandomFloatProvider random, float distance) {
            float angle = random.getFloat(0, 2 * Mathf.PI);
            float radius = Mathf.Sqrt(distance);
            return new Vector2(
                radius * Mathf.Cos(angle),
                radius * Mathf.Sin(angle)
            );
        }

        public static Quaternion RandomRotation(RandomFloatProvider randomProvider) {
            return Quaternion.Euler(randomProvider.getFloat(0, 360), randomProvider.getFloat(0, 360), randomProvider.getFloat(0, 360));
        }
    }
    
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
