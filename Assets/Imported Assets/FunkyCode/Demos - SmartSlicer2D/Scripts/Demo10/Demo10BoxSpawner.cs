using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo10BoxSpawner : MonoBehaviour {
	public GameObject spawnObject;
	public TimerHelper time;
	
	void SpawnBox() {
		GameObject box = Instantiate(spawnObject, transform) as GameObject;
		box.transform.parent = transform;
		box.transform.localPosition = new Vector3(0, 10, -5);
	}

	void Start () {
		 SpawnBox();
		 time = TimerHelper.Create();
	}
	
	void Update () {
		if (time.GetMillisecs() > 750) {
			SpawnBox();
			time = TimerHelper.Create();
		}
	}
}
