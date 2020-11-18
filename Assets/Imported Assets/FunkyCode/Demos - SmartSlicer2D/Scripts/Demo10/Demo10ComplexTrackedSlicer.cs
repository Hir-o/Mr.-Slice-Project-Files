using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo10ComplexTrackedSlicer : MonoBehaviour {
	public ComplexSlicerTracker trackerObject = new ComplexSlicerTracker();
	public float lineWidth = 0.25f;

	private Mesh mesh;

	void Update () {
		trackerObject.Update(transform.position);

		mesh = Max2DMesh.GenerateComplexTrackerMesh(trackerObject.trackerList, transform, lineWidth, transform.position.z + 0.001f);

		Max2D.SetColor (Color.black);
		Max2DMesh.Draw(mesh, Max2D.lineMaterial);
	}
}