using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game {
	public class BaseAgent : MonoBehaviour {

		private NavMeshAgent agent;

		protected virtual void init() {
			agent = GetComponent<NavMeshAgent> ();
			Debug.Log(agent);
		}

		public void MoveToLocation(Vector3 targetPoint) {
			agent.destination = targetPoint;
			agent.isStopped = false;
		}

	}
}
