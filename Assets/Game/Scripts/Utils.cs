using UnityEngine;
using UnityEngine.AI;

namespace Game {
    public class Utils {
        public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask) {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
            randomDirection += origin;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, distance, layermask);

            return navHit.position;
        }
    }
}