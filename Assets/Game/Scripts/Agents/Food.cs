using System;
using UnityEngine;

namespace Game {
    [RequireComponent(typeof(AudioSource))]
    public class Food : MonoBehaviour {

        public GameObject onDestroyEffect;
        public Transform bouncingObject;
        public float bounceHeight = 0.5f;
        public float life = 30f;
        public SoundBank placementSounds;
        private AudioSource audioSource;

        private Vector3 initialObjPos;
        private float bounceTimer;

        void Start() {
            audioSource = GetComponent<AudioSource>();
            initialObjPos = bouncingObject.position;
            bounceTimer = 0;
            SheepAgent[] sheep = FindObjectsOfType<SheepAgent>();

            if (sheep.Length == 0) {
                GameObject.Destroy(gameObject);
                return;
            }

            Array.Sort(sheep, new Comparison<SheepAgent>((a, b) =>
                (transform.position - a.transform.position).sqrMagnitude
                    .CompareTo((transform.position - b.transform.position).sqrMagnitude))
            );
            bool wasSet = false;
            for (int i = 0; i < sheep.Length; i++) {
                wasSet = sheep[i].setFoodTarget(this);
                if (wasSet) break;
            }

            audioSource.clip = placementSounds.getRandomSound();
            audioSource.Play();
        }

        private void OnTriggerEnter(Collider other) {
            var sheep = other.gameObject.GetComponent<SheepAgent>();
            if (sheep == null) return;
            sheep.onFoodEaten(this);
            GameObject.Instantiate(onDestroyEffect, transform.position, Quaternion.identity);
            GameObject.Destroy(gameObject);

            FindObjectOfType<GameManager>().onSaveEvent();
        }   

        void Update() {
            life -= Time.deltaTime;
            if (life < 0) {
                GameObject.Destroy(gameObject);
                return;
            }

            bounceTimer += Time.deltaTime;
            bounceTimer = bounceTimer % (2 * Mathf.PI);
            bouncingObject.transform.position = new Vector3(initialObjPos.x, initialObjPos.y + bounceHeight * Mathf.Sin(bounceTimer), initialObjPos.z);
        }
    }
}
