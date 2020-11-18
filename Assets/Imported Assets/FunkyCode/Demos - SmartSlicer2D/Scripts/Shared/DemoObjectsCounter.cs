using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoObjectsCounter : MonoBehaviour {

	void OnRenderObject() {
		Text text = GetComponent<Text> ();
		text.text = "objects " + Slicer2D.GetListCount();
	}
}
