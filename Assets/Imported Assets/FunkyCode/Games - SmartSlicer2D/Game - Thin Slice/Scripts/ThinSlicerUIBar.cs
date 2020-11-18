using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThinSlicerUIBar : MonoBehaviour {
	UICanvasScale scaler;

	float fullHeight;
	// Use this for initialization
	void Start () {
		scaler = GetComponent<UICanvasScale>();
		fullHeight = scaler.rect.height;
	}
	
	// Update is called once per frame
	void Update () {
		float newHeight = fullHeight * (float)(ThinSliceGameManager.instance.leftArea / 100);
		scaler.rect.height = scaler.rect.height * 0.95f + newHeight * 0.05f;
	}
}
