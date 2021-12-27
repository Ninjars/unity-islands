using System.Collections.Generic;
using UnityEngine;

public class AmbienceController : MonoBehaviour {
    public List<AudioSource> birdSongSources;
    public AudioSource springMusic;
    public AudioSource winterMusic;
    public AudioSource windAmbience;
    
    public void setToSpring() {
        Debug.Log("setToSpring()");
        springMusic.mute = false;
        winterMusic.mute = true;
        foreach (var source in birdSongSources) {
            source.volume = 0.8f;
        }
        windAmbience.volume = 0.1f;
    }

    public void setToWinter() {
        springMusic.mute = true;
        winterMusic.mute = false;
        foreach (var source in birdSongSources) {
            source.volume = 0f;
        }
        windAmbience.volume = 0.4f;
    }
}
