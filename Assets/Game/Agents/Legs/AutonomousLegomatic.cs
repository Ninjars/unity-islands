using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutonomousLegomatic : MonoBehaviour {

	public List<Vector3> leftLegOrigins;
	public List<Vector3> rightLegOrigins;
	public float legOffset = 0.5f;
	public float rangeOfMotion = 90;
	public GameObject footPrefab;

	private float maxForwardOffset;
	private List<GameObject> leftFeet;
	private List<GameObject> rightFeet;
	private LayerMask footLayerMask;
	void Awake() {
		if (leftLegOrigins.Count != rightLegOrigins.Count) {
			Debug.LogError("need same number of legs each side!");
			GameObject.Destroy(this);
			return;
		}
		maxForwardOffset = Mathf.Sin(rangeOfMotion / 2f) * legOffset;
		footLayerMask = LayerMask.GetMask("Terrain");
		Vector3 leftOffset = transform.TransformPoint(Vector3.left * legOffset);
		leftFeet = new List<GameObject>(leftLegOrigins.Count);
		foreach (var position in leftLegOrigins) {
			leftFeet.Add(GameObject.Instantiate(footPrefab, position + leftOffset, transform.rotation));
			
		}
		Vector3 rightOffset = transform.TransformPoint(Vector3.right * legOffset);
		rightFeet = new List<GameObject>(rightLegOrigins.Count);
		foreach (var position in rightLegOrigins) {
			rightFeet.Add(GameObject.Instantiate(footPrefab, position + rightOffset, transform.rotation));
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		updateFeetPositions();
	}

	private void updateFeetPositions() {
		Vector3 leftOffset = Vector3.left * legOffset;
		Vector3 rightOffset = Vector3.right * legOffset;
		for (int i = 0; i < leftFeet.Count; i++) {
			updateFoot(leftFeet[i], transform.TransformPoint(leftLegOrigins[i] + leftOffset));
			updateFoot(rightFeet[i], transform.TransformPoint(rightLegOrigins[i] + rightOffset));
		}
	}

	private void updateFoot(GameObject foot, Vector3 origin) {
		RaycastHit hit;
        if (Physics.Raycast(origin + Vector3.up, Vector3.down, out hit, Mathf.Infinity, footLayerMask)) {
			foot.transform.position = hit.point;
			foot.transform.up = hit.normal;
		}
	}
}
