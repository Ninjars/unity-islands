using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
	public class Foot : MonoBehaviour {
		public float movementDurationSeconds = 0.2f;
		public float stepHeight = 1f;

		private Vector3 targetPosition;
		private Quaternion targetRotation;
		private Vector3 initialPosition;
		private Quaternion initialRotation;
		private float startTime = -1;
		
		void Update () {
			var elapsed = Time.time - startTime;
			var rawFraction = elapsed / movementDurationSeconds;
			var horizontalFraction = Mathf.SmoothStep(0.0f, 1.0f, rawFraction);
			var verticalFraction =  Mathf.SmoothStep(0.0f, 1.0f, 1 - Mathf.Abs(rawFraction - 0.5f) * 2);
			transform.position = Vector3.Lerp(initialPosition, targetPosition, horizontalFraction);
			transform.position +=  Vector3.Lerp(Vector3.zero, Vector3.up * stepHeight, verticalFraction);
			transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, horizontalFraction);
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
