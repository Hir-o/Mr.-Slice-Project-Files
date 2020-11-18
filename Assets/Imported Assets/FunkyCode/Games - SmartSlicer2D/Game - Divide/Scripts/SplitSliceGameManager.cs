using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SplitSliceObject {
	public GameObject level;
	public int allowedSlices;
	public int allowedPieces;
} 

public class SplitSliceGameManager : MonoBehaviour {
	public int currentLevelID = 0;
	public SplitSliceObject[] Levels;
	
	public Text slicesText;
	public Text piecesText;

	int slicesCount = 0;
	int piecesCount = 0;

	TimerHelper cooldown;

	List<GameObject> garbage = new List<GameObject>();

	public GameObject particlePrefab;

	public void StartScene(int sceneID) {
		foreach(GameObject g in garbage) {
			Destroy(g);
		}
		garbage.Clear();

		currentLevelID = sceneID;
		Instantiate(Levels[sceneID].level);
		slicesCount = 0;

		UIFade.instance.state = false;
	}

	public void ReplayScene() {
		StartScene(currentLevelID);
	}

	public void NextScene() {
		StartScene(currentLevelID + 1);
	}

	public void GoToLevels() {
		UIFade.instance.menuObjects[0].SetActive(true);
		UIFade.instance.menuObjects[1].SetActive(false);
		UIFade.instance.menuObjects[2].SetActive(false);
	}

	void Start () {
		cooldown = TimerHelper.Create();

		piecesCount = Slicer2D.GetListCount();

		Slicer2D.AddGlobalResultEvent(SliceEvent);

		UIFade.instance.menuObjects[0].SetActive(true);
		UIFade.instance.menuObjects[1].SetActive(false);
		UIFade.instance.menuObjects[2].SetActive(false);

		// LVL 1 : SQUARE
		// 1 Slices
		// 2 Pieces

		// LVL 2 : Triangle
		// 3 Slices
		// 3 Pieces

		// LVL 3 : U Latter
		// 2 Slices
		// 5 Pieces

		// LVL 4 : T Latter
		// 2 Slices
		// 6 Pieces

		// LVL 6 : OMN Latter
		// 3 Slices
		// 8 Pieces

		// LVL 7 : +
		// 4 Slices
		// 10 Pieces

		Slicer2D.AddGlobalResultEvent(GlobalSliceEvent);
	}

	void GlobalSliceEvent(Slice2D slice) { 
		Vector3 position = slice.slice[0].ToVector2();
		position.z = -5;

		GameObject particleGameObject = Instantiate(particlePrefab, position, Quaternion.Euler(0, 0, (float)Vector2D.Atan2(slice.slice[0], slice.slice[1]) * Mathf.Rad2Deg));

		SlashParticle particle = particleGameObject.GetComponent<SlashParticle>();
		particle.moveTo = slice.slice[1];
	}
	
	void Update() {
		piecesCount = Slicer2D.GetListCount();

		slicesText.text = "Slices: " + slicesCount + " / " + Levels[currentLevelID].allowedSlices;
		piecesText.text = "Pieces: " + piecesCount + " / " + Levels[currentLevelID].allowedPieces;

		Vector3 pos = Camera.main.transform.position;
		pos.x = pos.x * 0.95f + (float)(currentLevelID * 50f) * 0.05f;
		Camera.main.transform.position = pos;

		if (Slicer2D.GetList().Count < 1) {
			return;
		}
		if (slicesCount <= Levels[currentLevelID].allowedSlices && piecesCount == Levels[currentLevelID].allowedPieces) {
			foreach(Slicer2D slicer in Slicer2D.GetList()) {
				slicer.gameObject.AddComponent<GreenFade>();
				DestroyObject(slicer);
				garbage.Add(slicer.gameObject);
			}
			UIFade.instance.state = true;
			UIFade.instance.menuObjects[0].SetActive(false);
			UIFade.instance.menuObjects[1].SetActive(false);
			UIFade.instance.menuObjects[2].SetActive(true);
			//currentLevelID++;
			//Levels[currentLevelID].level.SetActive(true);
			//slicesCount = 0;
		} else if (slicesCount > Levels[currentLevelID].allowedSlices) {
			foreach(Slicer2D slicer in Slicer2D.GetList()) {
				GreenFade fade = slicer.gameObject.AddComponent<GreenFade>();
				fade.fadeTyp = GreenFade.FadeType.Red;
				DestroyObject(slicer);
				garbage.Add(slicer.gameObject);
			}
			UIFade.instance.menuObjects[0].SetActive(false);
			UIFade.instance.menuObjects[1].SetActive(true);
			UIFade.instance.menuObjects[2].SetActive(false);
			UIFade.instance.state = true;
		}
	}

	void SliceEvent(Slice2D slice) {
		if (cooldown.GetMillisecs() < 30) {
			return;
		} else {
			cooldown = TimerHelper.Create();
		}

		slicesCount++;
	}
}
