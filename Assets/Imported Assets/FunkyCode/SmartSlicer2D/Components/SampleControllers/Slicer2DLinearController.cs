using System.Collections.Generic;
using UnityEngine;

public class Slicer2DLinearController : MonoBehaviour {
	// Physics Force
	public bool addForce = true;
	public float addForceAmount = 5f;

	// Controller Visuals
	public bool drawSlicer = true;
	public float lineWidth = 1.0f;
	public float zPosition = 0f;
	public Color slicerColor = Color.black;

	// Mouse Events
	private Pair2D pair = Pair2D.Zero();

	private bool mouseDown = false;

	public void OnRenderObject() {
		if (drawSlicer == false) {
			return;
		}
		
		if (mouseDown) {
			Max2D.SetBorder (true);
			Max2D.SetLineMode(Max2D.LineMode.Smooth);
			Max2D.SetLineWidth (lineWidth * .5f);
			Max2D.SetColor (slicerColor);

			Max2DLegacy.DrawLineSquare (pair.A, 0.5f, zPosition);
			Max2DLegacy.DrawLineSquare (pair.B, 0.5f, zPosition);
			Max2DLegacy.DrawLine (pair.A, pair.B, zPosition);
		}
	}

	// Checking mouse press and release events to get linear slices based on input
	public void LateUpdate()  {
		Vector2D pos = new Vector2D (Camera.main.ScreenToWorldPoint (Input.mousePosition));

		if (Input.GetMouseButtonDown (0)) {
			pair.A.Set (pos);
		}
		
		if (Input.GetMouseButton (0)) {
			pair.B.Set (pos);
			mouseDown = true;
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;
			LinearSlice (pair);
		}
	}

	private void LinearSlice(Pair2D slice) {
		List<Slice2D> results = Slicer2D.LinearSliceAll (slice, null);

		// Adding Physics Forces
		if (addForce == true) {
			float sliceRotation = (float)Vector2D.Atan2 (slice.B, slice.A);
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						foreach (Vector2D p in id.collisions) {
							Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount, Mathf.Sin (sliceRotation) * addForceAmount), p.ToVector2());
						}
					}
				}
			}
		}
	}
}