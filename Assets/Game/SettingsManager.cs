using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {


	private static SettingsManager _instance;

	public static SettingsManager instance {get { return _instance; }}

    public float scrollSpeed;

    private void Awake() {
		_instance = this;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
