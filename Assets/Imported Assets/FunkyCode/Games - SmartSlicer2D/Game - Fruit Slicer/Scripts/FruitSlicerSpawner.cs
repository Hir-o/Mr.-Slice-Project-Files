using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSlicerSpawner : MonoBehaviour {

	Pair2D pair;

	TimerHelper timer;
	float timerRequired = 1f;

	void Start () {
		Polygon2D edge = Polygon2DList.CreateFromGameObject(gameObject)[0];

		pair = Pair2D.GetList(edge.pointsList)[0];

		timer = TimerHelper.Create();
	}
	
	void Update () {
		if (timer.Get() > timerRequired) {
			GameObject fruit = Instantiate(FruitSlicerGameManager.instance.fruits[Random.Range(0, FruitSlicerGameManager.instance.fruits.Length)]);

			fruit.transform.parent = FruitSlicerGameManager.instance.transform;

			double rotation = Vector2D.Atan2(pair.B, pair.A);
			float distance = (float)Vector2D.Distance(pair.A, pair.B);

			Vector3 newPosition = pair.A.ToVector2() + Vector2D.RotToVec(rotation).ToVector2() * Random.Range(0, distance);
			newPosition.z = Random.Range(0, 30);

			fruit.transform.position = newPosition;
			fruit.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

			Rigidbody2D rb = fruit.GetComponent<Rigidbody2D>();
			rb.AddForce(new Vector2(0, 500));
			rb.AddTorque(Random.Range(-100, 100));

			timer = TimerHelper.Create();
			timerRequired = Random.Range(0.2f, 1);
		}
	}
}
