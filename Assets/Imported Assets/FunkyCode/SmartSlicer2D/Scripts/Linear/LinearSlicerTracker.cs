using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LinearSlicerTracker {
	public List<LinearSlicerTrackerObject> trackerList = new List<LinearSlicerTrackerObject>();

	public void Update(Vector2 position, float minVertexDistance = 1f) {
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			LinearSlicerTrackerObject tracker = GetSlicerTracker(slicer);
			if (tracker == null) {
				tracker = new LinearSlicerTrackerObject();
				tracker.slicer = slicer;
				trackerList.Add(tracker);
			}

			Vector2D trackedPos = new Vector2D(slicer.transform.transform.InverseTransformPoint(position));
			if (tracker.lastPosition != null) {
				if (slicer.GetPolygon().PointInPoly(trackedPos)) {
					if (tracker.tracking == false) {
						//tracker.pointsList.Add(tracker.lastPosition);
						tracker.firstPosition = tracker.lastPosition;
					}

					tracker.tracking = true;

					//if (tracker.pointsList.Count < 1 || (Vector2D.Distance (trackedPos, tracker.pointsList.Last ()) > minVertexDistance / 4f)) {
					//	tracker.pointsList.Add(trackedPos);
					//}

				} else if (tracker.tracking == true) {
					tracker.tracking = false;
					//tracker.pointsList.Add(trackedPos);
					
					if (tracker.firstPosition != null) {
						tracker.lastPosition = trackedPos;

						Pair2D slicePair = new Pair2D(new Vector2D(slicer.transform.TransformPoint(tracker.firstPosition.ToVector2())), new Vector2D(slicer.transform.TransformPoint(tracker.lastPosition.ToVector2())));


						Slice2D slice = slicer.LinearSlice(slicePair);
						if (slice.gameObjects.Count > 0) {
							CopyTracker(slice, slicer);
						};
					}

					trackerList.Remove(tracker);
				}
			}

			if (tracker != null) {
				tracker.lastPosition = trackedPos;
			}
		}
	}

	public void CopyTracker(Slice2D slice, Slicer2D slicer) {
		foreach(Demo10LinearTrackedSlicer trackerComponent in Object.FindObjectsOfType<Demo10LinearTrackedSlicer>()) {
			if (trackerComponent.trackerObject == this) {
				continue;
			}
			foreach(LinearSlicerTrackerObject trackerObject in new List<LinearSlicerTrackerObject>(trackerComponent.trackerObject.trackerList)) {
				if (trackerObject.slicer != slicer) {
					continue;
				}
				foreach(GameObject g in slice.gameObjects){
					LinearSlicerTrackerObject t = trackerComponent.trackerObject.GetSlicerTracker(g.GetComponent<Slicer2D>());
					if (t == null) {
						t = new LinearSlicerTrackerObject();
						t.slicer = g.GetComponent<Slicer2D>();
						t.firstPosition = trackerObject.firstPosition;
						t.lastPosition = trackerObject.lastPosition;
						t.tracking = true;
						trackerComponent.trackerObject.trackerList.Add(t);
					}
				}
			}
		}
	}

	public LinearSlicerTrackerObject GetSlicerTracker(Slicer2D slicer) {
		foreach(LinearSlicerTrackerObject tracker in trackerList) {
			if (tracker.slicer == slicer) {
				return(tracker);
			}
		}
		return(null);
	}
}

public class LinearSlicerTrackerObject {
	public Slicer2D slicer;
	public Vector2D lastPosition;
	public Vector2D firstPosition;
	public bool tracking = false;

	public List<Vector2D> GetPoints() {
		List<Vector2D> points = new List<Vector2D>();
		points.Add(firstPosition);
		points.Add(lastPosition);
		return(points);
	}
}
