using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo11ColliderSlice : MonoBehaviour {

	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			float timer = Time.realtimeSinceStartup;
			Polygon2D poly = Polygon2DList.CreateFromGameObject(gameObject)[0].ToWorldSpace(transform);

			Slicer2D.ComplexSliceAll(poly.pointsList, Slice2DLayer.Create());
		
			Destroy(gameObject);

			Debug.Log(name + " in " + (Time.realtimeSinceStartup - timer) * 1000 + "ms");
		}
	}
}
