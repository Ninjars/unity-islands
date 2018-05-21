using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Game {
	public class FlockingAgent : BaseAgent, FlockMember {
        private System.Random random;

		private float currentDelayTime;
		private float nextActionDelay;
		private ThreatState threatState;
		private float baseSpeed;
		private float baseAcceleration;
        private Flock flock;
		private ThreatProvider priorityThreat;

		public float minActionDelaySeconds = 5;
		public float maxActionDelaySeconds = 10;
		public float threatenedSpeedBoostFactor = 2f;
		public float energyHarvestRate = 1;
		public bool debugDraw = false;

        void Awake() {
			base.init();
			random = WorldManager.instance.GetRandom();
			baseSpeed = base.getNavAgent().speed;
			baseAcceleration = base.getNavAgent().acceleration;
		}
		
		void Update () {
			switch (threatState) {
				case ThreatState.THREATENED:
					currentDelayTime = nextActionDelay;
					if (priorityThreat != null && !isMoving()) {
						transform.LookAt(priorityThreat.getPosition());
					}
					break;
				case ThreatState.UNTHREATENED:
					currentDelayTime += Time.deltaTime;
					if (currentDelayTime > nextActionDelay) {
						moveToRandomPoint();
					}
					harvestEnergy(Time.deltaTime);
					break;
				default:
					Debug.LogError("unhandled threatstate " + threatState);
					break;
			}
		}

        private void harvestEnergy(float timeDelta) {
            flock.addEnergy(flock.getCurrentNode().getResource(ResourceType.LOW_DENSITY_ENERGY).harvest(energyHarvestRate / timeDelta));
        }

        void OnDestroy() {
			if (flock != null) {
				flock.onMemberLost(this);
			}
		}

		private void moveToRandomPoint() {
			if (flock == null) {
				Debug.LogError("moveToRandomPoint(): Flocking agent has no flock!");
				return;
			}
			Vector3 targetPosition = flock.getCurrentNode().getRandomPoint3D();
			currentDelayTime = 0;
			nextActionDelay = minActionDelaySeconds + (float)random.NextDouble() * maxActionDelaySeconds;
			MoveToLocation(targetPosition);
		}

		void OnDrawGizmos() {
			if (debugDraw) {
				base.debugNavigationDraw();
			}
		}

        public void onFlockRepositioning() {
			moveToRandomPoint();
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }

        public void regroup() {
			if (flock == null) {
				Debug.LogError("regroup(): Flocking agent has no flock!");
				return;
			}
			Vector3 targetPosition = flock.getCurrentNode().getRandomPoint3D(0.25f);
			currentDelayTime = 0;
			nextActionDelay = maxActionDelaySeconds;
			MoveToLocation(targetPosition);
        }

        public void defensivePosture(Vector3 position) {
            threatState = ThreatState.THREATENED;
			base.getNavAgent().speed = baseSpeed * threatenedSpeedBoostFactor;
			base.getNavAgent().acceleration = baseAcceleration * threatenedSpeedBoostFactor;
			MoveToLocation(position);
        }

        public void normalPosture() {
            threatState = ThreatState.UNTHREATENED;
			base.getNavAgent().speed = baseSpeed;
			base.getNavAgent().acceleration = baseAcceleration;
        }

        public void setFlock(Flock flock) {
            this.flock = flock;
        }

        public void setPriorityThreat(ThreatProvider threat) {
            priorityThreat = threat;
        }
    }
}
