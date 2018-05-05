using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
	public class FlockingAgent : BaseAgent, FlockMember {
        private System.Random random;
        private TerrainNode currentNode;

		private float currentDelayTime;
		private float nextActionDelay;

		public float minActionDelaySeconds = 5;
		public float maxActionDelaySeconds = 10;
		public bool debugDraw = false;

		void Awake() {
			base.init();
			random = WorldManager.instance.GetRandom();
		}

		void Start () {
			moveToRandomPoint();
		}
		
		void Update () {
			currentDelayTime += Time.deltaTime;
			if (currentDelayTime > nextActionDelay) {
				moveToRandomPoint();
			}
		}

		private void moveToRandomPoint() {
			if (currentNode == null) {
				Debug.LogError("Flocking agent has no current node!");
				return;
			}
			Vector3 targetPosition = currentNode.getRandomPoint3D();
			currentDelayTime = 0;
			nextActionDelay = minActionDelaySeconds + (float)random.NextDouble() * maxActionDelaySeconds;
			MoveToLocation(targetPosition);
		}

		void OnDrawGizmos() {
			if (debugDraw) {
				base.debugNavigationDraw();
			}
		}

        public void travelTo(TerrainNode node) {
			currentNode = node;
			moveToRandomPoint();
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public void regroup() {
			Vector3 targetPosition = currentNode.getRandomPoint3D(0.25f);
			currentDelayTime = 0;
			nextActionDelay = maxActionDelaySeconds;
			MoveToLocation(targetPosition);
        }
    }
}
