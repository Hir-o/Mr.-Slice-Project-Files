using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FruitSlicerGameManager : MonoBehaviour {
	public GameObject[] fruits;
	public GameObject[] livesObjects;
	public static FruitSlicerGameManager instance;
	public int lives = 3;
	public int score = 0;
	public Text scoreText;

	void Start () {
		instance = this;

		Physics2D.gravity = new Vector2(0, -5);
	}
	
	void Update () {
		Polygon2D cameraPolygon = Polygon2D.CreateFromCamera(Camera.main);
		cameraPolygon = cameraPolygon.ToRotation(Camera.main.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
		cameraPolygon = cameraPolygon.ToOffset(new Vector2D(Camera.main.transform.position));
	
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			if (Math2D.PolyCollidePoly(slicer.GetPolygon().ToWorldSpace(slicer.transform), cameraPolygon) == false) {
				if (slicer.enabled == true) {
					lives --;
					if (lives >= 0) {
						SpriteRenderer sr = livesObjects[lives].GetComponent<SpriteRenderer>();
						sr.color = Color.white;
					} else {
						Debug.Log("lose");
					}
				}
				Destroy(slicer.gameObject);
			}
		}

		scoreText.text = score.ToString();
	}
}
