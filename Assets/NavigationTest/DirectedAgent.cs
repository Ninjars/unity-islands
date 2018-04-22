using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Agents {
	public class DirectedAgent : MonoBehaviour {

 		private NavMeshAgent agent;

		void Awake() {
			agent = GetComponent<NavMeshAgent> ();
		}

		public void MoveToLocation(Vector3 targetPoint) {
			agent.destination = targetPoint;
			agent.isStopped = false;
		}
	}
}
