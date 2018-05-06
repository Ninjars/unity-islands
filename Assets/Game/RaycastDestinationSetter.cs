using UnityEngine;
using System.Collections;
using System;

namespace Game {
	public class RaycastDestinationSetter : MonoBehaviour {
		public Camera camera;
		public GameObject agentPrefab;
		private BaseAgent directedAgent;
		private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
		private LineRenderer laserLine;                                     // Reference to the LineRenderer component which will display our laserline

		void Start () {
			// Get and store a reference to our LineRenderer component
			laserLine = GetComponent<LineRenderer>();
		}

		void Update () {
			// Check if the player has pressed the fire button and if enough time has elapsed since they last fired
			if (Input.GetButtonDown("Fire1")) {
				// Check if our raycast has hit anything
				Ray ray = camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit)) {
					// Set the end position for our laser line 
					laserLine.SetPosition (0, camera.transform.position - Vector3.down * 3);
					laserLine.SetPosition (1, hit.point);

					// Start our ShotEffect coroutine to turn our laser line on and off
					StartCoroutine (ShotEffect());

					if (directedAgent == null) {
						instantiateAgent(hit.point);
					} else {
						Debug.Log("MoveToLocation @ " + hit.point);
						directedAgent.MoveToLocation (hit.point);
					}
				} else {
					Debug.Log("No hit from  @ " + ray.origin);
				}
			}
		}

        private void instantiateAgent(Vector3 point) {
			Debug.Log("instantiateAgent @ " + point);
            GameObject agent = Instantiate(agentPrefab, point, new Quaternion(0, 0, 0, 1));
			directedAgent = agent.GetComponent<BaseAgent>();
        }

        private IEnumerator ShotEffect() {
			// Turn on our line renderer
			laserLine.enabled = true;

			//Wait for .07 seconds
			yield return shotDuration;

			// Deactivate our line renderer after waiting
			laserLine.enabled = false;
		}
	}
}