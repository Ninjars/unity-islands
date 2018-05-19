using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game {
	public interface IMovingEntity {
		bool isMoving();
	}	public class BaseAgent : MonoBehaviour, IMovingEntity {

		public int maxHealth = 10;
		private int currentHealth;

		private NavMeshAgent agent;

		protected virtual void init() {
			agent = GetComponent<NavMeshAgent> ();
			currentHealth = maxHealth;
		}

		public void MoveToLocation(Vector3 targetPoint) {
			agent.destination = targetPoint;
			agent.isStopped = false;
		}

        internal void debugNavigationDraw() {
			Debug.DrawLine(
				gameObject.transform.position,
				agent.destination,
				Color.red);
        }

        internal NavMeshAgent getNavAgent() {
            return agent;
        }

		public virtual void receiveDamage(int damage) {
			currentHealth -= damage;
			if (currentHealth <= 0) {
				GameObject.Destroy(gameObject);
			}
		}

        public bool isMoving() {
            return agent.velocity.sqrMagnitude > 0;
        }
    }
}
