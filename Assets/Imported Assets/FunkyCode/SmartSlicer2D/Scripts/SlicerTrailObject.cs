using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerTrailObject {
	public Slicer2D slicer;
	public Vector2D lastPosition;
	public List<TrailPoint> pointsList = new List<TrailPoint>();
}

public class TrailPoint {
	public Vector2D position;
	public float time = 1;
	public TimerHelper timer;

	public TrailPoint(Vector2D vector, float _time) {
		position = vector;
		time = _time;
		timer = TimerHelper.Create();
	}

	public bool Update() {
		if (timer.Get() >= time) {
			return(false);
		} else {
			return(true);
		}
	}
}

