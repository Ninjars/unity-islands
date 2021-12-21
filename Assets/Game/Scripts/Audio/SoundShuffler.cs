using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundShuffler : MonoBehaviour {

    public SoundBank soundBank;

    public float minInterval;
    public float maxInterval;

    private float timer;
    private AudioSource audioSource;

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        timer -= Time.deltaTime;
        if (timer < 0) {
            audioSource.clip = soundBank.getRandomSound();
            audioSource.Play();
            timer = UnityEngine.Random.Range(minInterval, maxInterval);
        }
    }
}
