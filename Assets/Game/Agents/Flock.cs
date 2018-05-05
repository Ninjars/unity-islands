using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

	public class Flock : MonoBehaviour {
		public GameObject flockAgentPrefab;
		public int initialFlockSize = 10;
		public int roamTimeMin = 10;
		public int roamTimeMax = 20;
		public bool drawDebug;

		private float currentRoamTime;
		private float currentRoamTimer;
        private System.Random random;
        private TerrainNode currentNode;
		private List<FlockMember> members = new List<FlockMember>();


        void Awake() {
			random = WorldManager.instance.GetRandom();
			currentNode = WorldManager.instance.getClosestTerrainNode(gameObject.transform.position);
		}

		void Start () {
			for (int i = 0; i < initialFlockSize; i++) {
				GameObject newMember = Instantiate(flockAgentPrefab, currentNode.getRandomPoint3D(), UnityEngine.Random.rotation);
				FlockMember flockMember = newMember.GetComponent<FlockMember>();
				flockMember.travelTo(currentNode);
				members.Add(flockMember);
			}
		}
		
		void Update () {
			currentRoamTime += Time.deltaTime;
			if (currentRoamTime > currentRoamTimer) {
				currentRoamTime = 0;
				currentRoamTimer = roamTimeMin + (float)random.NextDouble() * roamTimeMax;

				List<TerrainNode> neighbours = currentNode.neighbouringNodes;
				int next = random.Next(neighbours.Count);
				currentNode = neighbours[next];
				gameObject.transform.position = currentNode.position;
				foreach (FlockMember member in members) {
					member.travelTo(currentNode);
				}
			}
		}

		private void OnDrawGizmos() {
			if (!drawDebug) {
				return;
			}
			Gizmos.color = Color.green;
			foreach (FlockMember member in members) {
				Gizmos.DrawLine(gameObject.transform.position, member.getPosition());
			}
		}
	}

	public interface FlockMember {
		void travelTo(TerrainNode node);
		Vector3 getPosition();
		void regroup();
	}
}
