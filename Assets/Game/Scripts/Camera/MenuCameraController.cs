using UnityEngine;

public class MenuCameraController : MonoBehaviour {

    public Transform target;
    public float speed = 0.05f;

    private float menuDistance;
    private Quaternion yAxisOffset;
    private Quaternion targetRotation;
    private Quaternion baseRotation;

    private void Start() {
        configure();
    }

    private void OnEnable() {
        configure();
    }

    private void configure() {
        baseRotation = transform.rotation;
        targetRotation = transform.rotation * yAxisOffset;
    }

    private void Awake() {
        menuDistance = Vector3.Magnitude(transform.position - target.transform.position);
        var rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        yAxisOffset = Quaternion.Euler(0, -rotation.eulerAngles.y, 0);
        Debug.Log($"y offset { rotation.eulerAngles.y}");
    }

    void LateUpdate() {
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -menuDistance);
        Vector3 position = baseRotation * negDistance + target.position;

        transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, targetRotation, speed);
        transform.position = Vector3.Slerp(transform.position, position, speed);
    }
}
