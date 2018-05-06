using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game {
	public class BaseAgent : MonoBehaviour {

		private NavMeshAgent agent;

		protected virtual void init() {
			agent = GetComponent<NavMeshAgent> ();
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
    }
}
