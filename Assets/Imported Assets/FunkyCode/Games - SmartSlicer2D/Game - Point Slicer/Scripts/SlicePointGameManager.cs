using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlicePointGameManager : MonoBehaviour {
	public double startingArea = 0;
	public double currentArea = 0;

	public Text percentText;
	
	void Start () {
		
		UpdateCurrentArea();

		startingArea = currentArea;
	}
	
	void Update () {
		Polygon2D cameraPolygon = Polygon2D.CreateFromCamera(Camera.main);
		cameraPolygon = cameraPolygon.ToRotation(Camera.main.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
		cameraPolygon = cameraPolygon.ToOffset(new Vector2D(Camera.main.transform.position));
	
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			if (Math2D.PolyCollidePoly(slicer.GetPolygon().ToWorldSpace(slicer.transform), cameraPolygon) == false) {
				Destroy(slicer.gameObject);
			}
		}

		UpdateCurrentArea();
 
		int percent = (int)((currentArea / startingArea) * 100);
		percentText.text = "Left: " + percent + "%";
	}

	public void UpdateCurrentArea() {
		currentArea = 0f;
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			currentArea += slicer.GetPolygon().GetArea();
		}
	}
}
