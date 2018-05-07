using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

	enum ThreatState {
		UNTHREATENED,
		THREATENED
	}
	public class Flock : MonoBehaviour {
		private static List<Flock> flocks = new List<Flock>();
		public GameObject flockAgentPrefab;
		public int initialFlockSize = 10;
		public int roamTimeMin = 10;
		public int roamTimeMax = 20;
		public float threatDetectionRadius = 50;
		public int threatResponseThreshold = 1;
		public bool drawDebug;

		private float currentRoamTime;
		private float currentRoamTimer;
        private System.Random random;
        private TerrainNode currentNode;
		private List<FlockMember> members = new List<FlockMember>();
		private ThreatState threatState = ThreatState.UNTHREATENED;

        void Awake() {
			random = WorldManager.instance.GetRandom();
			currentNode = WorldManager.instance.getClosestTerrainNode(getPosition());
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
			InvokeRepeating("checkForThreats", 1, 1);
		}

		void checkForThreats() {
			var collisions = Physics.OverlapSphere(getPosition(), threatDetectionRadius, -1);
			List<ThreatProvider> threatProviders = null;
			int threat = 0;
			foreach (Collider collision in collisions) {
				var otherGO = collision.gameObject;
				var threatComponent = otherGO.GetComponentInParent<ThreatProvider>();
				if (threatComponent != null) {
					Debug.Log("threat component found");
					if (threatProviders == null) {
						threatProviders = new List<ThreatProvider>();
					}
					threatProviders.Add(threatComponent);
					threat += threatComponent.getThreat();
				}
			}

			if (threat >= threatResponseThreshold) {
				transitionIntoState(ThreatState.THREATENED, threatProviders);
			} else {
				transitionIntoState(ThreatState.UNTHREATENED);
			}
		}

		private void transitionIntoState(ThreatState state, List<ThreatProvider> threatProviders = null) {
			if (threatState == state) {
				return;
			}
			threatState = state;
			switch (threatState) {
				case ThreatState.UNTHREATENED:
					performNormalBehaviour();
					break;
				case ThreatState.THREATENED:
					if (threatProviders != null) {
						performThreatResponse(threatProviders);
					}
					break;
				default:
					Debug.Log("unhandled threatState " + threatState);
					break;
			}
		}

        private void performNormalBehaviour() {
			Debug.Log("performNormalBehaviour()");
			threatState = ThreatState.UNTHREATENED;
            foreach (var member in members) {
				member.normalPosture();
			}
        }

        private void performThreatResponse(List<ThreatProvider> threatProviders) {
			Debug.Log("performThreatResponse()");
			threatState = ThreatState.THREATENED;
			for (int i = 0; i < members.Count; i++) {
				var position = getDefensivePosition(getAngle(i, members.Count), members.Count/3f);
				members[i].defensivePosture(position);
			}
        }

		private float getAngle(int index, int count) {
			return 2 * Mathf.PI * index / (float) count;
		}

		private Vector3 getDefensivePosition(float angle, float radius) {
			float dy = radius * Mathf.Sin(angle);
			float dx = radius * Mathf.Cos(angle);
			var position = getPosition();
			return WorldManager.instance.findSurfacePosition(new Vector2(position.x + dx, position.z + dy));
		}

        void Update () {
			if (threatState != ThreatState.UNTHREATENED) {
				return;
			}
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
				Gizmos.DrawLine(getPosition(), member.getPosition());
			}
		}

		private Vector3 getPosition() {
			return gameObject.transform.position;
		}
	}

	public interface FlockMember {
		void travelTo(TerrainNode node);
		Vector3 getPosition();
		void regroup();
        void defensivePosture(Vector3 position);
        void normalPosture();
    }
}
