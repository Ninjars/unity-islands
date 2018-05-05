using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
	public class WanderingAgent : BaseAgent {
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
			currentNode = WorldManager.instance.getClosestTerrainNode(gameObject.transform.position);
		}

		// Use this for initialization
		void Start () {
			wander();
		}
		
		// Update is called once per frame
		void Update () {
			currentDelayTime += Time.deltaTime;
			if (currentDelayTime > nextActionDelay) {
				if (random.Next(2) == 0) {
					wander();
				} else {
					roam();
				}
			}
		}

		private void roam() {
			List<TerrainNode> neighbours = currentNode.neighbouringNodes;
			int next = random.Next(neighbours.Count);
			currentNode = neighbours[next];
			moveToRandomPoint();
		}

        private void wander()  {
			moveToRandomPoint();
        }

		private void moveToRandomPoint() {
            Vector3 targetPosition = currentNode.getRandomPoint3D();
			currentDelayTime = 0;
			nextActionDelay = minActionDelaySeconds + (float)random.NextDouble() * maxActionDelaySeconds;
			MoveToLocation(targetPosition);
		}

		void OnDrawGizmos() {
			if (debugDraw) {
				if (currentNode == null) {
					return;
				}
				Debug.DrawLine(
					gameObject.transform.position,
					currentNode.position,
					Color.green);
				base.debugNavigationDraw();
			}
		}
    }
}
