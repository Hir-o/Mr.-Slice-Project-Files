using System.Collections.Generic;
using UnityEngine;

public class DemoBackgroundManager : MonoBehaviour {
	public enum DayState {day, night};

	public DayState dayState = DayState.day;

	public GameObject[] dayCloudPrefabs;
	public GameObject dayPrefab;
	public GameObject nightPrefab;
	public GameObject sunPrefab;
	public GameObject starsPrefab;
	public GameObject nightCloudPrefab;

	private GameObject[] dayClouds;
	private float[] dayCloudSpeed;
	private float[] dayCloudPos;

	private GameObject[] stars;
	private GameObject[] nightClouds;

	private GameObject sun;

	private float nightState = 0f;
	private Material nightMaterial;

	static private DemoBackgroundManager instance;

	public static void SetDayState(DayState state) {
		instance.dayState = state;
	}

	public static DayState GetDayState() {
		if (instance == null) {
			return(DayState.day);
		}
		return(instance.dayState);
	}

	void Start () {
		CreateSkybox ();
		CreateDayClouds ();
		CreateSun ();
		CreateStars ();
		CreateNightClouds ();

		instance = this;
	}

	void Update () {
		DayCloudsUpdate ();
		SkyBoxUpdate ();
		UpdateSun ();
		UpdateStars ();
		UpdateNightClouds ();
	}

	void CreateNightClouds() {
		nightClouds = new GameObject[2];

		GameObject nightCloudsParent = new GameObject();
		nightCloudsParent.transform.name = "NightClouds";
		nightCloudsParent.transform.parent = transform;

		for (int i = 0; i < 2; i++) {
			nightClouds[i] = InstantiateObject (nightCloudPrefab);
			nightClouds[i].transform.parent = nightCloudsParent.transform;
		}
	}

	void UpdateNightClouds() {
		for (int i = 0; i < 2; i++) {
			Vector3 pos = nightClouds[i].transform.localPosition;
			pos.x = (-Camera.main.transform.position.x + i * 100f - 500f + Time.realtimeSinceStartup) % 200 + 50f;
			pos.y = (-Camera.main.transform.position.y + (1f - nightState) * 40f) + 7f; 
			nightClouds[i].transform.localPosition = pos;
			if (dayState == DayState.night) {
				nightClouds[i].SetActive (true);
			} else if (pos.y > 25) {
				nightClouds[i].SetActive (false);
			}
		}
	}
		
	void CreateStars() {
		stars = new GameObject[3];

		GameObject starsParent = new GameObject();
		starsParent.name = "Stars";
		starsParent.transform.parent = transform;

		for (int i = 0; i < 3; i++) {
			stars[i] = InstantiateObject (starsPrefab) as GameObject;
			stars[i].transform.parent = starsParent.transform;
		}
	}

	void UpdateStars() {
		for (int i = 0; i < 3; i++) {
			Vector3 pos = stars[i].transform.localPosition;

			pos.x = (-Camera.main.transform.position.x + i * 30f - 5000 - Time.realtimeSinceStartup / 4f) % 100 + 50f;
			pos.y = (-Camera.main.transform.position.y + (1f - nightState) * 40f) + 7;
			pos.z = 12f;

			stars[i].transform.localPosition = pos;

			if (dayState == DayState.night) {
				stars[i].SetActive (true);
			} else if (pos.y > 35) {
				stars[i].SetActive (false);
			}
		}
	}

	void CreateSun() {
		sun = InstantiateObject (sunPrefab) as GameObject;
		sun.transform.localPosition = new Vector3 (-20, 7, 15);
	}

	void UpdateSun() {
		sun.transform.localPosition = new Vector3 (35 * nightState - 20, 7 - nightState * 5, 20);
	}

	void CreateSkybox() {
		GameObject day = InstantiateObject (dayPrefab)  as GameObject;
		GameObject night = InstantiateObject (nightPrefab) as GameObject;

		GameObject skybox = new GameObject();
		skybox.name = "Skybox";
		skybox.transform.parent = transform;

		nightMaterial = night.GetComponent<SpriteRenderer>().material;

		day.transform.parent = skybox.transform;
		night.transform.parent = skybox.transform;
	}

	void SkyBoxUpdate() {
		switch (dayState){
			case DayState.day:
				nightState = nightState * .95f;
				break;

			case DayState.night:
				nightState = nightState * .95f+ 1f * 0.05f;
				break;
			}

		nightMaterial.SetColor("_TintColor", new Color(.5f, .5f, .5f, nightState * .5f));
	}

	void CreateDayClouds() {
		dayClouds = new GameObject[11];
		dayCloudSpeed = new float[11];
		dayCloudPos = new float[11];
		bool[] state = new bool[4];

		GameObject cloudsParent = new GameObject();
		cloudsParent.name = "Day Clouds";
		cloudsParent.transform.parent = transform;

		for (int i = 0; i < 10; i++) {
			dayClouds[i] = InstantiateObject (dayCloudPrefabs[i % 4]) as GameObject;
			dayClouds [i].transform.localScale = new Vector3 (2, 2, 1);
			state [i % 4] = !state [i % 4];
			
			if (state[i % 4]) {
				dayClouds [i].transform.Rotate(new Vector3 (0, 0, 180));
			}

			dayCloudSpeed [i] = (float)Random.Range (2f, 8f) / 7f;
			dayClouds [i].transform.parent = cloudsParent.transform;
		}
	}

	void DayCloudsUpdate() {
		for (int i = 0; i < 10; i++) {
			Vector3 pos = dayClouds[i].transform.localPosition;
			pos.x = (-Camera.main.transform.position.x + i * 10f - 5000f + dayCloudPos[i]) % 100 + 50f;
			pos.y = (-Camera.main.transform.position.y + 5 + (i % 2) * 4) + nightState * 40;

			dayCloudPos [i] += dayCloudSpeed [i] * Time.deltaTime;
			dayClouds[i].transform.localPosition = pos;

			if (dayState == DayState.day) {
				dayClouds[i].SetActive (true);
			} else if (pos.y > 40) {
				dayClouds[i].SetActive (false);
			}
		}
	}

	public GameObject InstantiateObject(GameObject obj) {
		return(Instantiate(obj, transform) as GameObject);
	}
}
