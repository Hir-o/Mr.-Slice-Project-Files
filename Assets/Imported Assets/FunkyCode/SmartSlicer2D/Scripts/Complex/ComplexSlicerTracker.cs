using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ComplexSlicerTracker {
	public List<ComplexSlicerTrackerObject> trackerList = new List<ComplexSlicerTrackerObject>();

	public void Update(Vector2 position, float minVertexDistance = 1f) {
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			ComplexSlicerTrackerObject tracker = GetSlicerTracker(slicer);
			if (tracker == null) {
				tracker = new ComplexSlicerTrackerObject();
				tracker.slicer = slicer;
				trackerList.Add(tracker);
			}

			Vector2D trackedPos = new Vector2D(slicer.transform.transform.InverseTransformPoint(position));
			if (tracker.lastPosition != null) {
				if (slicer.GetPolygon().PointInPoly(trackedPos)) {
					if (tracker.tracking == false) {
						tracker.pointsList.Add(tracker.lastPosition);
					}

					tracker.tracking = true;

					if (tracker.pointsList.Count < 1 || (Vector2D.Distance (trackedPos, tracker.pointsList.Last ()) > minVertexDistance / 4f)) {
						tracker.pointsList.Add(trackedPos);
					}

				} else if (tracker.tracking == true) {
					tracker.tracking = false;
					tracker.pointsList.Add(trackedPos);

					List<Vector2D> slicePoints = new List<Vector2D>();
					foreach(Vector2D point in tracker.pointsList) {
						slicePoints.Add(new Vector2D(slicer.transform.TransformPoint(point.ToVector2())));
					}

					Slice2D slice = slicer.ComplexSlice(slicePoints);
					if (slice.gameObjects.Count > 0) {
						CopyTracker(slice, slicer);
					};

					trackerList.Remove(tracker);
				}
			}

			if (tracker != null) {
				tracker.lastPosition = trackedPos;
			}
		}
	}

	public void CopyTracker(Slice2D slice, Slicer2D slicer) {
		foreach(Demo10ComplexTrackedSlicer trackerComponent in Object.FindObjectsOfType<Demo10ComplexTrackedSlicer>()) {
			if (trackerComponent.trackerObject == this) {
				continue;
			}
			foreach(ComplexSlicerTrackerObject trackerObject in new List<ComplexSlicerTrackerObject>(trackerComponent.trackerObject.trackerList)) {
				if (trackerObject.slicer != slicer) {
					continue;
				}
				foreach(GameObject g in slice.gameObjects){
					ComplexSlicerTrackerObject t = trackerComponent.trackerObject.GetSlicerTracker(g.GetComponent<Slicer2D>());
					if (t == null) {
						t = new ComplexSlicerTrackerObject();
						t.slicer = g.GetComponent<Slicer2D>();
						t.pointsList = new List<Vector2D>(trackerObject.pointsList);
						t.tracking = true;
						trackerComponent.trackerObject.trackerList.Add(t);
					}
				}
			}
		}
	}

	public ComplexSlicerTrackerObject GetSlicerTracker(Slicer2D slicer) {
		foreach(ComplexSlicerTrackerObject tracker in trackerList) {
			if (tracker.slicer == slicer) {
				return(tracker);
			}
		}
		return(null);
	}
}

public class ComplexSlicerTrackerObject {
	public Slicer2D slicer;
	public Vector2D lastPosition;
	public List<Vector2D> pointsList = new List<Vector2D>();
	public bool tracking = false;
}