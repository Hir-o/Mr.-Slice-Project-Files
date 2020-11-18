using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Slicer2DComplexController : MonoBehaviour {
	// Physics Force
	public bool addForce = true;
	public float addForceAmount = 5f;

	// Controller Visuals
	public bool drawSlicer = true;
	public float lineWidth = 1.0f;
	public float zPosition = 0f;
	public Color slicerColor = Color.black;

	// Mouse Events
	private static List<Vector2D> points = new List<Vector2D>();
	private float minVertexDistance = 1f;

	private bool mouseDown = false;

	// Complex Slice Type
	public Slicer2D.SliceType complexSliceType = Slicer2D.SliceType.SliceHole;

	public void OnRenderObject() {
		if (drawSlicer == false) {
			return;
		}	

		if (mouseDown) {
			Max2D.SetBorder(true);
			Max2D.SetLineMode(Max2D.LineMode.Smooth);
			Max2D.SetLineWidth (lineWidth * .5f);
			Max2D.SetColor (slicerColor);

			if (points.Count > 0) {
				Max2DLegacy.DrawStrippedLine (points, minVertexDistance, zPosition);
				Max2DLegacy.DrawLineSquare (points.Last(), 0.5f, zPosition);
				Max2DLegacy.DrawLineSquare (points.First (), 0.5f, zPosition);
			}
		}
	}

	// Checking mouse press and release events to get linear slices based on input
	public void LateUpdate() {
		Vector2D pos = new Vector2D (Camera.main.ScreenToWorldPoint (Input.mousePosition));

		if (Input.GetMouseButtonDown (0)) {
			points.Clear ();
			points.Add (pos);
		}

		if (Input.GetMouseButton (0)) {
			Vector2D posMove = new Vector2D (points.Last ());
			while ((Vector2D.Distance (posMove, pos) > minVertexDistance)) {
				float direction = (float)Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertexDistance);
				points.Add (new Vector2D (posMove));
			}
			mouseDown = true;
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;
			Slicer2D.complexSliceType = complexSliceType;
			ComplexSlice (points);
		}
	}

	private void ComplexSlice(List <Vector2D> slice) {
		List<Slice2D> results = Slicer2D.ComplexSliceAll (slice, null);
		if (addForce == true) {
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						foreach (Pair2D p in Pair2D.GetList(id.collisions)) {
							float sliceRotation = (float)Vector2D.Atan2 (p.B, p.A);
							Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount, Mathf.Sin (sliceRotation) * addForceAmount), (p.A + p.B).ToVector2() / 2f);
						}
					}
				}
			}
		}
	}
}