using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LinearClickController : MonoBehaviour {
	public float visualScale = 1f;
	public float lineWidth = 1f;
	public float zPosition = 0f;
	public int slicesLimit = 3;
	
	public GameObject particlePrefab;
	private Color slicerColor = Color.white;

	private List<Pair2D> slicePairs = new List<Pair2D>();
	private Vector2D lastPoint = null;

	private Material lineMaterial;
	private Material lineMaterialBorder;

	private List<Pair2D> animationPairs = new List<Pair2D>();

	void Start () {
		Max2D.Check();

		lineMaterial = new Material(Max2D.lineMaterial);
		lineMaterialBorder = new Material(Max2D.lineMaterial);
	}
	
	void Update () {
		Vector2D pos = GetMousePosition();

		if (Input.GetMouseButtonDown(1)) {
			slicePairs.Clear();
			lastPoint = null;
		}

		// Puting point inside the Slice-able object is not allowed (!?!)
		if (PointInObjects(pos)) {
			slicerColor = Color.red;
		} else {
			slicerColor = Color.black;

			if (Input.GetMouseButtonDown(0)) {
				if (lastPoint != null) {
					slicePairs.Add(new Pair2D(lastPoint, pos));
				}

				if (slicePairs.Count >= slicesLimit) {
					animationPairs = new List<Pair2D>(slicePairs);

					slicePairs.Clear();
					lastPoint = null;
				} else {
					lastPoint = pos;
				}
			}
		}

		foreach(Pair2D pair in slicePairs) {
			DrawLine(pair);
		}

		if (lastPoint != null) {
			DrawLine(new Pair2D(lastPoint, pos));
		}

		UpdateSliceAnimations();
	}

	public void UpdateSliceAnimations() {
		if (animationPairs.Count < 1) {
			return;

		}

		if (SlashParticle.GetList().Count > 0) {
			return;
		}

		Pair2D animationPair = animationPairs.First();

		Slicer2D.LinearSliceAll(animationPair);

		Vector3 position = animationPair.A.ToVector2();
		position.z = -1;

		GameObject particleGameObject = Instantiate(particlePrefab, position, Quaternion.Euler(0, 0, (float)Vector2D.Atan2(animationPair.A, animationPair.B) * Mathf.Rad2Deg));
		
		SlashParticle particle = particleGameObject.GetComponent<SlashParticle>();
		particle.moveTo = animationPair.B;

		animationPairs.Remove(animationPair);
	}

	public void DrawLine(Pair2D pair) {
		Mesh meshBorder = Max2DMesh.GenerateLinearMesh(pair, transform, lineWidth * 2f * visualScale, zPosition + 0.001f,  0, lineWidth * 2f * visualScale);
		Mesh mesh = Max2DMesh.GenerateLinearMesh(pair, transform, lineWidth * visualScale, zPosition, 0, lineWidth * 2f * visualScale);

		lineMaterial.SetColor ("_Emission", Color.black);
		Max2DMesh.Draw( Max2DMesh.GenerateLinearMesh(new Pair2D(pair.A, pair.A), transform, lineWidth * 10f * visualScale, zPosition + 0.001f, 0,  lineWidth * 10f * visualScale), lineMaterialBorder);
		Max2DMesh.Draw( Max2DMesh.GenerateLinearMesh(new Pair2D(pair.B, pair.B), transform, lineWidth * 10f * visualScale, zPosition + 0.001f, 0,  lineWidth * 10f * visualScale), lineMaterialBorder);

		Max2DMesh.Draw(meshBorder, lineMaterialBorder);

		lineMaterial.SetColor ("_Emission", slicerColor);
		Max2DMesh.Draw( Max2DMesh.GenerateLinearMesh(new Pair2D(pair.A, pair.A), transform, lineWidth * 5f * visualScale, zPosition + 0.001f, 0, lineWidth * 5f * visualScale), lineMaterial);
		Max2DMesh.Draw( Max2DMesh.GenerateLinearMesh(new Pair2D(pair.B, pair.B), transform, lineWidth * 5f * visualScale, zPosition + 0.001f, 0, lineWidth * 5f * visualScale), lineMaterial);

		Max2DMesh.Draw(mesh, lineMaterial);
	}

	public static Vector2D GetMousePosition() {
		return(new Vector2D (Camera.main.ScreenToWorldPoint (Input.mousePosition)));
	}

	bool PointInObjects(Vector2D pos) {
		foreach (Slicer2D slicer in Slicer2D.GetList()) {
			if (slicer.GetPolygon().PointInPoly(pos.InverseTransformPoint(slicer.transform))) {
				return(true);
			}
		}
		return(false);
	}
}
