using UnityEngine;

namespace Game {
    public class InteractionController : MonoBehaviour {
        public LayerMask groundLayerMask;
        public LayerMask sheepLayerMask;
        public GameObject placedObject;

        void Update() {
            if (Input.GetButtonDown("Fire1")) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, sheepLayerMask)) {
                    var sheep = hit.transform.gameObject.GetComponent<SheepAgent>();
                    if (sheep != null) {
                        sheep.baa();
                    }

                } else if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask)) {
                    var position = hit.point;
                    Instantiate(placedObject, position, Quaternion.identity);
                }
            }
        }
    }
}
