using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFade : MonoBehaviour {
	UICanvasScale scaler;
	public bool state = true;

	float startingY; 

	static public UIFade instance;

	public GameObject[] menuObjects;
	

	void Start () {
		scaler = GetComponent<UICanvasScale>();

		startingY = scaler.rect.y;

		instance = this;
	}
	
	void Update () {
		if (state) {
			scaler.rect.y = scaler.rect.y * 0.95f + startingY * 0.05f;
		} else {
			scaler.rect.y = scaler.rect.y * 0.95f + 200 * 0.05f;
		}
	}
}
