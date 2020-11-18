using System.Collections.Generic;
using UnityEngine;

public class Demo6Manager : MonoBehaviour {
	public GameObject bombPrefab;
	public GameObject bouncerPrefab;
	public Transform parent;

	void Update () {
		Vector2D pos = Slicer2DController.GetMousePosition ();

		if (Input.GetMouseButtonDown (0)) {
			GameObject g = Instantiate (bombPrefab) as GameObject;
			g.transform.position = new Vector3 ((float)pos.x, (float)pos.y, -4.75f);
			g.transform.parent = transform;
		}

		if (Input.GetMouseButtonDown (1)) {
			GameObject g = Instantiate (bouncerPrefab) as GameObject;
			g.transform.position = new Vector3 ((float)pos.x, (float)pos.y, -4.75f);
			g.transform.parent = transform;
		}
	}
}
