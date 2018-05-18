using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
	public class Foot : MonoBehaviour {
		public float movementDurationSeconds = 0.1f;

		private Vector3 targetPosition;
		private Quaternion targetRotation;
		private Vector3 initialPosition;
		private Quaternion initialRotation;
		private float startTime = -1;
		
		void Update () {
			var elapsed = Time.time - startTime;
			if (elapsed > movementDurationSeconds) {
				if (targetPosition != null) {
					transform.position = targetPosition;
				}
				if (targetRotation != null) {
					transform.rotation = targetRotation;
				}
				return;
			}
			var fraction = Mathf.SmoothStep(0.0f, 1.0f, elapsed / movementDurationSeconds);
			transform.position = Vector3.Lerp(initialPosition, targetPosition, fraction);
			transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, fraction);
		}

		public void setTarget(Vector3 position, Quaternion rotation) {
			startTime = Time.time;
			initialPosition = transform.position;
			initialRotation = transform.rotation;
			targetPosition = position;
			targetRotation = rotation;
		}
	}
}
