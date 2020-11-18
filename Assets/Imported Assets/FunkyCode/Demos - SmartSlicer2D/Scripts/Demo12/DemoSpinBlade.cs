using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoSpinBlade : MonoBehaviour {
	void Update () {
		GetComponent<Rigidbody2D>().AddTorque(15);
	}
}
