using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoMainManager : MonoBehaviour {
	public RectTransform UIObject;
	public RectTransform UIBack;
	public GameObject[] demoScenes;
	public int currentSceneID = 0;

	public GameObject[] gameScenes;

	public GameObject demoCamera;
	public GameObject demoUI;

	private GameObject lastScene;
	private Vector3 startPosition;
	private GameObject currentScene = null;

	public void SetGame(int id) {
		Instantiate(gameScenes[id]);
		demoCamera.SetActive(false);
		demoUI.SetActive(false);
	}

	public void NextScene()
	{
		if (demoScenes.Length < currentSceneID + 2) {
			Destroy (currentScene);
			SetScene(0);
		} else {
			Destroy (currentScene);
			SetScene(currentSceneID+1);
		}
	}

	public void PrevScene()
	{
		if (currentSceneID < 1) {
			Destroy (currentScene);
			SetScene(demoScenes.Length - 1);
		} else {
			Destroy (currentScene);
			SetScene(currentSceneID-1);
		}
	}

	public void ResetScene()
	{
		Destroy (currentScene);
		SetScene (currentSceneID);
	}

	public void SetScene(int id)
	{
		currentSceneID = id;
		currentScene = Instantiate(demoScenes[id]) as GameObject;
		currentScene.transform.position = new Vector3 ((id + 1) * 50f, 0f, 0f);

		switch (DemoBackgroundManager.GetDayState ()) {
			case DemoBackgroundManager.DayState.day:
					DemoBackgroundManager.SetDayState (DemoBackgroundManager.DayState.night);
				break;
			case DemoBackgroundManager.DayState.night:
				DemoBackgroundManager.SetDayState (DemoBackgroundManager.DayState.day);
				break;
		}

	}

	public void SetMainMenu()
	{
		lastScene = currentScene;
		currentScene = null;
	}

	void Start()
	{
		startPosition = UIObject.anchoredPosition;

		Application.targetFrameRate = 60;
	}

	void Update () {
		Camera mainCamera = Camera.main;
		if (currentScene != null) {
			UIBack.gameObject.SetActive (true);
			if (currentScene.activeSelf == false) 
				currentScene.SetActive (true);

			Vector3 position = mainCamera.transform.position;
			position.x = position.x * 0.95f + currentScene.transform.position.x * 0.05f;
			mainCamera.transform.position = position;

			position = UIObject.position;
			position.y = position.y * 0.95f + -500f * 0.05f;
			UIObject.position = position;
			if (UIObject.position.y < -350f)
				UIObject.gameObject.SetActive (false);

		} else {
			UIBack.gameObject.SetActive (false);
			if (lastScene != null) {
				if (Vector2.Distance (lastScene.transform.position, mainCamera.transform.position) > 40) {
					Destroy (lastScene);
					lastScene = null;
				}
			}

			Vector3 position = mainCamera.transform.position;
			position.x = position.x * 0.95f;
			mainCamera.transform.position = position;

			position = UIObject.anchoredPosition;
			position.x = position.x * 0.95f + startPosition.x * 0.05f;
			position.y = position.y * 0.95f + startPosition.y * 0.05f;
			UIObject.anchoredPosition = position;

			UIObject.gameObject.SetActive (true);
		}
	}
}
