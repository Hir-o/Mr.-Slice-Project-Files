using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ComplexCollision {
	public List<Point> collisionSlice = new List<Point>();
	public int collisionCount = 0; // Change?
	
	public bool error = false;

	Pair2D outside = Pair2D.Zero();
	Pair2D inside = Pair2D.Zero();
	
	private bool calculated = false;
	private List<Vector2D> points;

	public List<Pair2D> polygonCollisionPairs = new List<Pair2D>();

	public ComplexCollision(Polygon2D polygon, List<Vector2D> slice) {
		bool enterCollision = false;

		// Generate correct intersected slice

		foreach (Pair2D pair in Pair2D.GetList(slice, false)) {
			List<Vector2D> intersections = polygon.GetListLineIntersectPoly(pair);

			if (intersections.Count == 1) {
				//intersectionType = IntersectionType.DifferentPairs;
				collisionCount += 1;

				collisionSlice.Add (new Point(intersections[0], Point.Type.Intersection));

				enterCollision = !enterCollision;

			} else if (intersections.Count == 2) {
				//intersectionType = IntersectionType.SamePair;
				collisionCount += intersections.Count; // Check if it's okay

				if (Vector2D.Distance (intersections[0], pair.A) < Vector2D.Distance (intersections[1], pair.A)) {
					collisionSlice.Add (new Point(intersections[0], Point.Type.Intersection));
					collisionSlice.Add (new Point(intersections[1], Point.Type.Intersection));
				} else {
					collisionSlice.Add (new Point(intersections[1], Point.Type.Intersection));
					collisionSlice.Add (new Point(intersections[0], Point.Type.Intersection));
				}

			} else if (intersections.Count > 2) {
				error = true;
			}

			if (enterCollision == true) {
				collisionSlice.Add (new Point(pair.B, Point.Type.Inside));
			}
		}

		List<Vector2D> insidePoints = GetPointsInside();

		// Complex Points Generating
		if (insidePoints.Count > 0) {
			///// Outside Points
			Vector2D first = insidePoints.First();
			Vector2D last = insidePoints.Last();

			Vector2D firstOutside = First();
			Vector2D lastOutside = Last();

			outside.A = firstOutside.Copy();
			outside.B = lastOutside.Copy();

			outside.A.Push(Vector2D.Atan2(firstOutside, first), 0.001f);
			outside.B.Push(Vector2D.Atan2(lastOutside, last),  0.001f);

			///// Inside Points
			
			Vector2D firstInside = First();
			Vector2D lastInside = Last();

			inside.A = firstInside.Copy();
			inside.B = lastInside.Copy();

			inside.A.Push(Vector2D.Atan2(first, firstInside), 0.001f);
			inside.B.Push(Vector2D.Atan2(last, lastInside),  0.001f);
			
		// Linear Points Generating
		} else { 
			Vector2D first = slice.Last();
			Vector2D last = slice.First();

			Vector2D firstOutside = slice.First();
			Vector2D lastOutside = slice.Last();

			outside.A = firstOutside.Copy();
			outside.B = lastOutside.Copy();

			outside.A.Push(Vector2D.Atan2(firstOutside, first), 0.001f);
			outside.B.Push(Vector2D.Atan2(lastOutside, last),  0.001f);

			///// Inside Points
			Vector2D firstInside = slice.First();
			Vector2D lastInside = slice.Last();

			inside.A = firstInside.Copy();
			inside.B = lastInside.Copy();

			inside.A.Push(Vector2D.Atan2(first, firstInside), 0.001f);
			inside.B.Push(Vector2D.Atan2(last, lastInside),  0.001f);
		}

		///// Pairs Collided
		List<Vector2D> slicePoints = GetSlicePoints();
		foreach (Pair2D pair in Pair2D.GetList (polygon.pointsList)) {
			if (Math2D.LineIntersectSlice (pair, slicePoints) == true) {
				polygonCollisionPairs.Add(pair);
			}
		}

		foreach (Polygon2D hole in polygon.holesList) {
			foreach (Pair2D pair in Pair2D.GetList (hole.pointsList)) {
				if (Math2D.LineIntersectSlice (pair, slicePoints) == true) {
					polygonCollisionPairs.Add(pair);
				}
			}	
		}
	}

	public Vector2D First() {
		return(collisionSlice.First().vector);
	}

	public Vector2D Last() {
		return(collisionSlice.Last().vector);
	}

	public void Reverse() {
		collisionSlice.Reverse();

		Vector2D temp = outside.A;
		outside.A = outside.B;
		outside.B = temp;

		temp = inside.A;
		inside.A = inside.B;
		inside.B = temp;

		calculated = false;
	}

	public List<Vector2D> GetPoints() {
		List<Vector2D> points = new List<Vector2D>();
		foreach(Point point in collisionSlice) {
			points.Add(point.vector);
		}
		return(points);
	}

	public List<Vector2D> GetPointsInside() {
		if (calculated == false) {
			points = new List<Vector2D>();
			foreach(Point point in collisionSlice) {
				if (point.collision != Point.Type.Inside) {
					continue;
				}
				points.Add(point.vector);
			}
			calculated = true;
		}
		return(points);
	}

	public List<Vector2D> GetPointsInsidePlus() {
		List<Vector2D> points = GetPointsInside();
		points.Insert(0, inside.A);
		points.Add(inside.B);

		Vector2D lastPoint = null;
		foreach(Vector2D point in new List<Vector2D>(points)) {
			if (lastPoint != null) {
				if (Vector2D.Distance(point, lastPoint) < 0.01f) {
					points.Remove(lastPoint);
				}
			}

			lastPoint = point;
		}
		return(points);
	}

	public List<Vector2D> GetSlicePoints() {
		List<Vector2D> points = new List<Vector2D>(GetPointsInside());
		points.Insert(0, outside.A);
		points.Add(outside.B);


		return(points);
	}
	
	//public void Test() {
	//	Debug.Log("Test");
	//	foreach(Point point in collisionSlice) {
	//		Debug.Log(point.collision);
	//	}
	//}

	public class Point {
		public enum Type {Intersection, Inside, Outside};
		public Vector2D vector;
		public Type collision;
		
		public Point(Vector2D pos, Type col) {
			vector = pos;
			collision = col;
		}
	}
}