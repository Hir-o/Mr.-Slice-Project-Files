using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenFade : MonoBehaviour {
	public enum FadeType {Green, Red};
	public FadeType fadeTyp = FadeType.Green;
	MeshRenderer meshRenderer;
	float fade = 0;


	void Start () {
		meshRenderer = GetComponent<MeshRenderer>();
	}
	
	void Update () {
		switch(fadeTyp) {
			case FadeType.Green:
				meshRenderer.material.SetColor ("_TintColor", new Color(0.5f, 0.5f + fade, 0.5f, 0.5f));

			break;

			case FadeType.Red:
				meshRenderer.material.SetColor ("_TintColor", new Color(0.5f + fade, 0.5f - fade * 0.25f, 0.5f - fade * 0.25f, 0.5f));

			break;
		}

		if (fade < 0.35f) {
			fade += Time.deltaTime * 2;
		}
	}
}
