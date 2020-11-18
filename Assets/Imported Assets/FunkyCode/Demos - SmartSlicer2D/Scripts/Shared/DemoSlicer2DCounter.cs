using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSlicer2DCounter : MonoBehaviour {
	public static int sliceCount = 0;

	void Start () {
		Slicer2D slicer = GetComponent<Slicer2D>();
		slicer.AddResultEvent(SliceEvent);
	}
	
	void SliceEvent (Slice2D slice) {
		sliceCount ++;
		Debug.Log("Slice Count: " + sliceCount);
	}
}
