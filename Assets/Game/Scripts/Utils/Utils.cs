using UnityEngine;
using UnityEngine.AI;

namespace Utils {
    public class Utils {
        public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask) {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
            randomDirection += origin;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

            return navHit.position;
        }

        public static Vector2 RandomRadial2DUnitVector(RandomProvider random, float distance) {
            float angle = random.getFloat(0, 2 * Mathf.PI);
            float radius = Mathf.Sqrt(distance);
            return new Vector2(
                radius * Mathf.Cos(angle),
                radius * Mathf.Sin(angle)
            );
        }
    }
}
