using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

	public class Flock : MonoBehaviour {
		private static List<Flock> flocks = new List<Flock>();
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
			flocks.Add(this);
			gameObject.name = "Flock " + flocks.Count;
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
				moveToNewNode();
			}
		}

		void moveToNewNode() {
			List<TerrainNode> neighbours = currentNode.neighbouringNodes;
			int next = random.Next(neighbours.Count);
			currentNode = neighbours[next];
			foreach (Flock flock in flocks) {
				if (flock != this && flock.currentNode == currentNode) {
					mergeIntoFlock(flock);
					return;
				}
			}
			gameObject.transform.position = currentNode.position;
			foreach (FlockMember member in members) {
				member.travelTo(currentNode);
			}
		}

		void mergeIntoFlock(Flock flock) {
			Debug.Log("mergeIntoFlock: " + gameObject.name + " to " + flock.gameObject.name);
			foreach (FlockMember member in members) {
				flock.addMember(member);
			}
			members.Clear();
			flocks.Remove(this);
			Destroy(gameObject);
		}

		private void addMember(FlockMember member) {
			members.Add(member);
			member.travelTo(currentNode);
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
