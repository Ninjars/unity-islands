using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

	enum ThreatState {
		UNTHREATENED,
		THREATENED,
		FLEEING
	}
	public class Flock : MonoBehaviour {
		private static List<Flock> flocks = new List<Flock>();
		public GameObject flockAgentPrefab;
		public int initialFlockSize = 10;
		public int roamTimeMin = 10;
		public int roamTimeMax = 20;
		[TooltipAttribute("Distance at which threatening things start getting noticed")]
		public float threatDetectionRadius = 50;
		[TooltipAttribute("Factor of detection radius at which threats start getting scarier")]
		[Range(0f,1f)]
		public float threatEscalationFactor = 0.5f;
		[TooltipAttribute("When threat reaches this level, perform threat response behaviour")]
		public float threatResponseThreshold = 1;
		[TooltipAttribute("When threat reaches this level, run away")]
		public float threatRetreatThreshold = 3;
		public bool drawDebug;

		private float currentRoamTime;
		private float currentRoamTimer;
        private System.Random random;
        private TerrainNode currentNode;
		private List<FlockMember> members = new List<FlockMember>();
		private ThreatState threatState;
		private List<ThreatProvider> threatProviders = new List<ThreatProvider>();
		private float threatEscalationVal;
        private float accumulatedEnergy = 0;

        private void configureFrom(Flock flock, float energy) {
			flockAgentPrefab = flock.flockAgentPrefab;
			initialFlockSize = 0;
			roamTimeMin = flock.roamTimeMin;
			roamTimeMax = flock.roamTimeMax;
			threatDetectionRadius = flock.threatDetectionRadius;
			threatEscalationFactor = flock.threatEscalationFactor;
			threatResponseThreshold = flock.threatResponseThreshold;
			threatRetreatThreshold = flock.threatRetreatThreshold;
			threatState = flock.threatState;
			members = new List<FlockMember>();
			accumulatedEnergy = energy;
			drawDebug = false;
		}

        void Awake() {
			random = WorldManager.instance.GetRandom();
			if (currentNode == null) currentNode = WorldManager.instance.getClosestTerrainNode(getPosition());
			flocks.Add(this);
			gameObject.name = "Flock " + flocks.Count;
			threatEscalationVal = threatDetectionRadius * threatEscalationFactor;
			threatState = ThreatState.UNTHREATENED;
		}

		void Start () {
			for (int i = 0; i < initialFlockSize; i++) {
				GameObject newMember = Instantiate(flockAgentPrefab, currentNode.getRandomPoint3D(), UnityEngine.Random.rotation);
				FlockMember flockMember = newMember.GetComponent<FlockMember>();
				addMember(flockMember);
			}
			InvokeRepeating("checkForThreats", 1, 1);
		}

        internal TerrainNode getCurrentNode() {
            return currentNode;
        }

        void checkForThreats() {
			var collisions = Physics.OverlapSphere(getPosition(), threatDetectionRadius, -1);
			float threat = 0;
			foreach (Collider collision in collisions) {
				var otherGO = collision.gameObject;
				var threatComponent = otherGO.GetComponentInParent<ThreatProvider>();
				if (threatComponent != null) {
					if (threatProviders == null) {
						threatProviders = new List<ThreatProvider>();
					}
					threatProviders.Add(threatComponent);
					threat += getWeightedThreat(threatComponent);
				}
			}

			if (threat >= threatRetreatThreshold) {
				breakAndRun(findHighestPriorityThreat(threatProviders));
			} else if (threat >= threatResponseThreshold) {
				transitionIntoState(ThreatState.THREATENED);
				setThreatPriority(findHighestPriorityThreat(threatProviders));
			} else {
				transitionIntoState(ThreatState.UNTHREATENED);
			}
		}

        private void breakAndRun(ThreatProvider threat) {
			Debug.Log("breakAndRun()");
			threatState = ThreatState.FLEEING;
            List<TerrainNode> saferNodes = new List<TerrainNode>();
			Vector3 threatVector = transform.position - threat.getPosition();
			foreach (TerrainNode node in  getCurrentNode().neighbouringNodes) {
				float threatAngle = Vector3.Angle(threatVector, transform.position - node.getPosition());
				if (threatAngle > 120) {
					saferNodes.Add(node);
				}
			}
			saferNodes.Shuffle();
			if (members.Count == 1) {
				if (saferNodes.Count > 0) {
					moveToNode(saferNodes[0]);
				}
				return;
			}
			int newFlockCount = Math.Min(members.Count, saferNodes.Count);
			int groupSize = (int) Math.Ceiling(members.Count / (float)newFlockCount);
			for (int i = 0; i < newFlockCount; i++) {
				int finalIndex;
				if (i == newFlockCount - 1) finalIndex = members.Count;
				else finalIndex = (i+1) * groupSize;
				Flock flock = createNewFlockFromMembers(i*groupSize, finalIndex);
				flock.moveToNode(saferNodes[i]);
			}
			members.Clear();
			gameObject.SetActive(false);
			GameObject.Destroy(gameObject);
        }

		private Flock createNewFlockFromMembers(int fromIndex, int endIndex) {
			GameObject newFlockObject = Instantiate(new GameObject());
			Flock newFlock = newFlockObject.AddComponent<Flock>();
			newFlock.configureFrom(this, accumulatedEnergy * (endIndex - fromIndex) / (float)members.Count);
			for (int i = fromIndex; i < endIndex; i++) {
				newFlock.addMember(members[i]);
			}
			return newFlock;
		}

		private void OnDestroy() {
			flocks.Remove(this);
		}

        /**
			The base threat of a threat provider can be magnified by closeness with the philosphy that it's scarier when you can see the teeth... 
		*/
        private float getWeightedThreat(ThreatProvider threat) {
            float distance = Vector3.Distance(threat.getPosition(), transform.position);
			int baseThreat = threat.getThreat();
			if (distance > threatEscalationVal) {
				return baseThreat;
			} else {
				float factor = (distance / threatEscalationVal) * (distance / threatEscalationVal);
				return baseThreat / factor;
			}
        }

        private ThreatProvider findHighestPriorityThreat(List<ThreatProvider> threats) {
				ThreatProvider closest = null;
				float minDistance = threatDetectionRadius;
				foreach (ThreatProvider provider in threatProviders) {
					float distance = Vector3.Distance(provider.getPosition(), gameObject.transform.position);
					if (distance < minDistance) {
						closest = provider;
						minDistance = distance;
					}
				}
				return closest;
		}

        private void setThreatPriority(ThreatProvider threat) {
            foreach (FlockMember member in members) {
				member.setPriorityThreat(threat);
			}
        }

        private void transitionIntoState(ThreatState state) {
			if (threatState == state) {
				return;
			}
			threatState = state;
			switch (threatState) {
				case ThreatState.UNTHREATENED:
					performNormalBehaviour();
					break;
				case ThreatState.THREATENED:
					performThreatResponse();
					break;
				default:
					Debug.Log("unhandled threatState transition " + threatState);
					break;
			}
		}

        private void performNormalBehaviour() {
			threatState = ThreatState.UNTHREATENED;
            foreach (var member in members) {
				member.normalPosture();
			}
        }

        private void performThreatResponse() {
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

        void Update() {
			if (threatState != ThreatState.UNTHREATENED) {
				return;
			}
			currentRoamTime += Time.deltaTime;
			if (currentRoamTime > currentRoamTimer) {
				currentRoamTime = 0;
				currentRoamTimer = roamTimeMin + (float)random.NextDouble() * roamTimeMax;
				moveToNode(getBestNeighbourNode());
			}
		}

		TerrainNode getRandomNeighbourNode() {
			List<TerrainNode> neighbours = currentNode.neighbouringNodes;
			int next = random.Next(neighbours.Count);
			return neighbours[next];
		}

		TerrainNode getBestNeighbourNode() {
			List<TerrainNode> neighbours = currentNode.neighbouringNodes;
			TerrainNode bestNode = null;
			float value = 0;
			foreach (var node in neighbours) {
				var resource = node.getResource(ResourceType.LOW_DENSITY_ENERGY);
				if (resource.getCurrentValue() > value) {
					bestNode = node;
					value = resource.getCurrentValue();
				}
			}
			return bestNode;
		}

		void moveToNode(TerrainNode node) {
			if (node == null) {
				return;
			}
			currentNode = node;
			transform.position = currentNode.getPosition();
			foreach (Flock flock in flocks) {
				if (flock != this && flock.currentNode == currentNode) {
					mergeIntoFlock(flock);
					return;
				}
			}
			foreach (FlockMember member in members) {
				member.onFlockRepositioning();
			}
		}

		void mergeIntoFlock(Flock flock) {
			foreach (FlockMember member in members) {
				flock.addMember(member);
			}
			flock.addEnergy(accumulatedEnergy);
			members.Clear();
			Destroy(gameObject);
		}

		private void addMember(FlockMember member) {
			members.Add(member);
			member.setFlock(this);
			member.onFlockRepositioning();
		}

		public void onMemberLost(FlockMember member) {
			members.Remove(member);
			if (threatState == ThreatState.THREATENED) {
				performThreatResponse();
			}
		}

		private void OnDrawGizmos() {
			if (!drawDebug) {
				return;
			}
			Gizmos.color = Color.green;
			foreach (FlockMember member in members) {
				Gizmos.DrawLine(getPosition(), member.getPosition());
			}
			DebugUtils.drawString(getPosition(), accumulatedEnergy.ToString());
		}

		private Vector3 getPosition() {
			return gameObject.transform.position;
		}

        internal void addEnergy(float energyChange) {
            this.accumulatedEnergy += energyChange;
        }
	}

	public interface FlockMember {
		void onFlockRepositioning();
		Vector3 getPosition();
		void regroup();
        void defensivePosture(Vector3 position);
        void normalPosture();
        void setFlock(Flock flock);
        void setPriorityThreat(ThreatProvider threat);
    }
}
