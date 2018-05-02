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
            Vector2 targetXY = getRandomPointWithinNode(currentNode);
			Vector3 targetPosition = WorldManager.instance.findSurfacePosition(targetXY);
			currentDelayTime = 0;
			nextActionDelay = minActionDelaySeconds + (float)random.NextDouble() * maxActionDelaySeconds;
			MoveToLocation(targetPosition);
		}

		private Vector2 getRandomPointWithinNode(TerrainNode node) {
			var radius = node.radius;
			float x = radius * 2 * (float)random.NextDouble() - radius;
			float y = radius * 2 * (float)random.NextDouble() - radius;
			return new Vector2(node.position.x + x, node.position.z + y);
		}
    }
}
