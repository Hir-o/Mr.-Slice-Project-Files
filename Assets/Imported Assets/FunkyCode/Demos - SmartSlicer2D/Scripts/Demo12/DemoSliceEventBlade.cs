using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSliceEventBlade : MonoBehaviour {

	void Start () {
		Slicer2D slicer = GetComponent<Slicer2D>();
		if (slicer != null) {
			slicer.AddAnchorResultEvent(sliceEvent);
		}
	}
	
	void sliceEvent (Slice2D slice) {
		foreach(GameObject g in slice.gameObjects) {
			Destroy(g.GetComponent<DemoSpinBlade>());
			Destroy(g.GetComponent<DemoSliceEventBlade>());
			g.transform.parent = null;
		}
	}
}
