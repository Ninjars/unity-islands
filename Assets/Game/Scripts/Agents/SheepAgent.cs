using System;
using UnityEngine;

namespace Game {
    [RequireComponent(typeof(AudioSource))]
    public class SheepAgent : BaseAgent {
        private float currentDelayTime;
        private float nextActionDelay;

        public float wanderDistance = 8;
        public float minActionDelaySeconds = 3;
        public float maxActionDelaySeconds = 10;
        public float breathCycleTime = 2;
        public float minBaaInterval = 15;
        public float maxBaaInterval = 90;
        private float baaInterval;
        public Transform body;
        public float breathHeight = 0.025f;
        public LayerMask foodMask;
        private Collider[] foodHits;
        private Food foodTarget;
        private Vector3 initialObjPos;
        private float breathTimer = -1;
        private float breathCycleCurrent;
        public int foodEaten;
        public int foodCountToReproduce = 5;
        public int babyFoodCount = -5;
        private float baseBabyScale = 0.33f;
        private AutonomousLegomatic legController;
        public GameObject babySpawnParticleEffect;
        public GameObject happySheepParticleEffect;
        public SoundBank sheepSounds;
        private AudioSource audioSource;
        private AudioClip voice;
        private int voiceIndex = -1;

        void Awake() {
            base.init();
            audioSource = GetComponent<AudioSource>();
            legController = GetComponent<AutonomousLegomatic>();
            initialObjPos = body.localPosition;
            if (voiceIndex < 0) {
                setVoice(UnityEngine.Random.Range(0, sheepSounds.sounds.Count));
            }
            baaInterval = UnityEngine.Random.Range(minBaaInterval, maxBaaInterval) * 0.5f;
            foodHits = new Collider[5];
        }

        void Start() {
            moveToRandomPoint();
            updateScale();
        }

        void Update() {
            updateBreathing();
            updateBaa();

            if (foodTarget == null) {
                currentDelayTime += Time.deltaTime;
                if (currentDelayTime > nextActionDelay) {
                    if (!checkForFood()) {
                        moveToRandomPoint();
                        currentDelayTime = 0;
                    }
                }
            }
        }

        internal void setVoice(int voiceIndex) {
            if (voiceIndex < 0 || voiceIndex >= sheepSounds.sounds.Count) {
                Debug.Log($"invalid voice index {voiceIndex}");
                voiceIndex = UnityEngine.Random.Range(0, sheepSounds.sounds.Count);
            }
            this.voiceIndex = voiceIndex;
            voice = sheepSounds.sounds[voiceIndex];
            audioSource.clip = voice;
        }

        internal int getVoice() {
            return voiceIndex;
        }

        public void baa() {
            if (!audioSource.isPlaying) {
                audioSource.Play();
            }
        }

        public bool setFoodTarget(Food target) {
            if (foodTarget == null) {
                currentDelayTime = 0;
                foodTarget = target;
                MoveToLocation(target.transform.position);
                return true;
            } else {
                return false;
            }
        }

        public void onFoodEaten(Food food) {
            if (food == foodTarget) {
                foodTarget = null;
            }

            foodEaten++;
            if (foodEaten > foodCountToReproduce / 1.5) {
                GameObject.Instantiate(happySheepParticleEffect, transform.position + Vector3.up, Quaternion.identity);
            }

            if (foodEaten >= foodCountToReproduce) {
                foodEaten -= foodCountToReproduce;

                for (int i = 0; i < UnityEngine.Random.Range(1, 4); i++) {
                    var position = Game.Utils.RandomNavSphere(transform.position, 3f, -1);
                    if (position.x == Mathf.Infinity) continue;
                    GameObject.Instantiate(babySpawnParticleEffect, position, Quaternion.identity);
                    var newSheep = GameObject.Instantiate(this, position, UnityEngine.Random.rotation);
                    newSheep.foodEaten = babyFoodCount;
                }

            } else if (foodEaten <= 0) {
                updateScale();
            }
        }

        private bool checkForFood() {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, 10, foodHits, foodMask);
            Food closest = null;
            float distance = float.PositiveInfinity;
            for (int i = 0; i < hits && i < foodHits.Length; i++) {
                var coll = foodHits[i];
                var dist = (coll.transform.position - transform.position).sqrMagnitude;
                if (closest == null || dist < distance) {
                    closest = coll.gameObject.GetComponent<Food>();
                    if (closest != null) {
                        distance = dist;
                    }
                }
            }
            if (closest == null) {
                return false;
            } else {
                setFoodTarget(closest);
                return true;
            }
        }

        private void updateScale() {
            float scale = 1 - (foodEaten / (float)babyFoodCount);
            scale = Mathf.Min(1, baseBabyScale + (1 - baseBabyScale) * scale);
            transform.localScale = new Vector3(scale, scale, scale);
            legController.scaleFeet(transform.localScale);

            audioSource.pitch = 1 + (1-scale) * 0.5f;
        }

        private void updateBreathing() {
            breathTimer -= Time.deltaTime;
            if (breathTimer < 0) {
                breathCycleCurrent = breathCycleTime * UnityEngine.Random.Range(0.8f, 1.2f);
                breathTimer = breathCycleCurrent;
            }
            var fraction = 1 - breathTimer / breathCycleCurrent;
            var value = fraction * (2 * Mathf.PI);
            body.localPosition = new Vector3(initialObjPos.x, initialObjPos.y + breathHeight * Mathf.Sin(value), initialObjPos.z);
        }

        private void updateBaa() {
            baaInterval -= Time.deltaTime;
            if (baaInterval < 0) {
                baaInterval = UnityEngine.Random.Range(minBaaInterval, maxBaaInterval);
                baa();
            }
        }

        private void moveToRandomPoint() {
            Vector3 targetPosition = Game.Utils.RandomNavSphere(transform.position, wanderDistance, -1);
            nextActionDelay = UnityEngine.Random.Range(minActionDelaySeconds, maxActionDelaySeconds);
            MoveToLocation(targetPosition);
        }
    }
}
