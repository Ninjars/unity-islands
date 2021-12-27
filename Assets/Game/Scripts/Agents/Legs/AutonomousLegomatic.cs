using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game {

	public class AutonomousLegomatic : MonoBehaviour {

		public List<Vector3> leftLegOrigins;
		public List<Vector3> rightLegOrigins;
		public float legOffset = 0.5f;
		public float maxFootDistance = 1f;
		public GameObject footPrefab;
		public LayerMask footLayerMask;

		private List<Foot> leftFeet;
		private List<Foot> rightFeet;
		private FootIndexer footIndexer;
		private Vector3 leftOffset;
		private Vector3 rightOffset;
		private float footMoveTimer;
		private float footMoveThreshold = 0.25f;
		private IMovingEntity entityReference;

		void Awake() {
			if (leftLegOrigins.Count != rightLegOrigins.Count) {
				Debug.LogError("need same number of legs each side!");
				GameObject.Destroy(this);
				return;
			}
			Vector3 leftOffset = transform.TransformPoint(Vector3.left * legOffset);
			leftFeet = new List<Foot>(leftLegOrigins.Count);
			foreach (var position in leftLegOrigins) {
				var foot = GameObject.Instantiate(footPrefab, position + leftOffset, transform.rotation);
				leftFeet.Add(foot.GetComponent<Foot>());
			}
			Vector3 rightOffset = transform.TransformPoint(Vector3.right * legOffset);
			rightFeet = new List<Foot>(rightLegOrigins.Count);
			foreach (var position in rightLegOrigins) {
				var foot =GameObject.Instantiate(footPrefab, position + rightOffset, transform.rotation);
				rightFeet.Add(foot.GetComponent<Foot>());
			}
			footIndexer = new FootIndexer(leftFeet.Count);
			leftOffset = Vector3.left * legOffset;
			rightOffset = Vector3.right * legOffset;
			entityReference = gameObject.GetComponent<IMovingEntity>();
		}

		void Start () {
			positionAllFeet();
		}
		
		void Update () {
			checkForFootPositionUpdate(Time.deltaTime);
		}

        private void checkForFootPositionUpdate(float deltaTime) {
			footMoveTimer += deltaTime;
			if (footMoveTimer < footMoveThreshold) {
				return;
			}
			footMoveTimer -= footMoveThreshold;
            Foot currentFoot;
			Vector3 restPosition;
			if (footIndexer.currentSide == FootIndexer.Side.LEFT) {
				currentFoot = leftFeet[footIndexer.index];
				restPosition = transform.TransformPoint(leftLegOrigins[footIndexer.index] + leftOffset);
			} else {
				currentFoot = rightFeet[footIndexer.index];
				restPosition = transform.TransformPoint(rightLegOrigins[footIndexer.index] + rightOffset);
			}
			if (entityReference == null || entityReference.isMoving()) {
				var distance = Vector3.Distance(restPosition, currentFoot.transform.position);
				if (distance > maxFootDistance) {
					// step foot in direction of current motion
					Vector3 targetPosition = transform.forward * maxFootDistance + restPosition;
					updateFoot(currentFoot, targetPosition);
					footIndexer.increment();
					
				}
			} else {
				updateFoot(currentFoot, restPosition);
				footIndexer.increment();
			}
        }

        internal void scaleFeet(Vector3 scale) {
            foreach (var foot in leftFeet) {
				foot.transform.localScale = scale;
			}
			foreach (var foot in rightFeet) {
				foot.transform.localScale = scale;
			}
        }

        void OnDestroy() {
			try {
				foreach (var foot in leftFeet) {
					GameObject.Destroy(foot.gameObject);
				}
				foreach (var foot in rightFeet) {
					GameObject.Destroy(foot.gameObject);
				}
			} catch (Exception e) {
				// this silences annoying error logging when stopping the app, probably due to how objects are being destroyed
			}
		}

		private void positionAllFeet() {
			for (int i = 0; i < leftFeet.Count; i++) {
				updateFoot(leftFeet[i], transform.TransformPoint(leftLegOrigins[i] + leftOffset));
				updateFoot(rightFeet[i], transform.TransformPoint(rightLegOrigins[i] + rightOffset));
			}
		}

		private bool updateFoot(Foot foot, Vector3 origin) {
			if (Mathf.Approximately(origin.x, foot.targetPosition.x) && Mathf.Approximately(origin.z, foot.targetPosition.z)) {
				return false;
			}
			RaycastHit hit;
			if (Physics.Raycast(origin + Vector3.up, Vector3.down, out hit, Mathf.Infinity, footLayerMask)) {
				foot.setTarget(hit.point, Quaternion.LookRotation(transform.forward, hit.normal));
			}
			return true;
		}
		
		private class FootIndexer {
			public enum Side {
				LEFT, RIGHT
			}

			public Side currentSide { get; private set; }
			public int index { get; private set; }

			private readonly int maxIndex;

			public FootIndexer(int footCount) {
				maxIndex = footCount;
				currentSide = Side.LEFT;
				index = 0;
			}

			public void increment() {
				if (currentSide == Side.LEFT) {
					currentSide = Side.RIGHT;
				} else {
					currentSide = Side.LEFT;
					index = (index + 1) % maxIndex;
				}
			}
		}
	}
}
