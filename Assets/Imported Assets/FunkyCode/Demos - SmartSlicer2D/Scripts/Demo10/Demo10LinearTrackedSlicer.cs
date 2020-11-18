using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo10LinearTrackedSlicer : MonoBehaviour {
	public LinearSlicerTracker trackerObject = new LinearSlicerTracker();
	public float lineWidth = 0.25f;

	private Mesh mesh;

	void Update () {
		trackerObject.Update(transform.position);

		mesh = Max2DMesh.GenerateLinearTrackerMesh(trackerObject.trackerList, transform, lineWidth, transform.position.z + 0.001f);

		Max2D.SetColor (Color.black);
		Max2DMesh.Draw(mesh, Max2D.lineMaterial);
	}
}
