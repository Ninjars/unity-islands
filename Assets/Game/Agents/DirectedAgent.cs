using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game {
    public class DirectedAgent : BaseAgent, ThreatProvider {

		public int threat = 1;

        private void Awake() {
			base.init();
        }

        public int getThreat() {
            return threat;
        }

        void OnTriggerEnter(Collider other) {
            var baseAgent = other.gameObject.GetComponentInParent<BaseAgent>();
            if (baseAgent != null) {
                baseAgent.receiveDamage(1);
            }
        }

        public Vector3 getPosition() {
            return gameObject.transform.position;
        }
    }
}
