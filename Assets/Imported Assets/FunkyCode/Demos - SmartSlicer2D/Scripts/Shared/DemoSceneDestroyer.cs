using System.Collections.Generic;
using UnityEngine;

public class DemoSceneDestroyer : MonoBehaviour {
	public void DestroyScene() {
		Destroy(transform.parent.gameObject);
	}
}


