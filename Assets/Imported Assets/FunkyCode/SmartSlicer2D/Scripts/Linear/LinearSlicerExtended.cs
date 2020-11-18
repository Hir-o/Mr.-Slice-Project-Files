using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class LinearSlicerExtended {

	static public Slice2D ExplodeInPoint(Polygon2D polygon, Vector2D point)
	{
		Slice2D result = Slice2D.Create(point);

		if (polygon.PointInPoly (point) == false) {
			return(result);
		}

        return(Explode(polygon));
	}

	static public Slice2D ExplodeByPoint(Polygon2D polygon, Vector2D point)
	{
		Slice2D result = Slice2D.Create(point);

		if (polygon.PointInPoly (point) == false) {
			return(result);
		}

		float sliceRot = UnityEngine.Random.Range(0, 360);

		Polygon2D hole = Polygon2D.Create(Polygon2D.PolygonType.Rectangle, 0.1f);
		hole = hole.ToOffset(point);
		polygon.AddHole(hole);

		result.AddPolygon (polygon);

		int tries = 0;
		while (result.polygons.Count < Slicer2D.explosionPieces) {
			foreach (Polygon2D p in new List<Polygon2D>(result.polygons)) {
				sliceRot += UnityEngine.Random.Range(15, 45) * Mathf.Deg2Rad;

				Vector2D v = new Vector2D(point);
				v.Push(sliceRot, 0.4f);

				Slice2D newResult = SliceFromPoint (p, v, sliceRot);

				if (newResult.slices.Count > 0) {
					foreach (List<Vector2D> i in newResult.slices) {
						result.AddSlice(i);
					}
				}

				if (newResult.polygons.Count > 0) {
					foreach (Polygon2D poly in newResult.polygons) {
						result.AddPolygon (poly);
					}
					result.RemovePolygon (p);
				}
			}
			
			tries += 1;
			if (tries > 20) {
				return(result);
			}
		}


		return(result);
	}

	static public Slice2D Explode(Polygon2D polygon)
	{
		Slice2D result = Slice2D.Create(Slice2DType.Explode);

		Rect polyRect = polygon.GetBounds ();

		result.AddPolygon (polygon);

		int tries = 0;
		while (result.polygons.Count < Slicer2D.explosionPieces) {
			foreach (Polygon2D p in new List<Polygon2D>(result.polygons)) {
				Slice2D newResult = SliceFromPoint (p, new Vector2D(polyRect.x + UnityEngine.Random.Range(0, polyRect.width), polyRect.y + UnityEngine.Random.Range(0, polyRect.height)), UnityEngine.Random.Range(0, Mathf.PI * 2));
				
				
				if (newResult.slices.Count > 0) {
					foreach (List<Vector2D> i in newResult.slices) {
						result.AddSlice(i);
					}
				}
				
				if (newResult.polygons.Count > 0) {
					foreach (Polygon2D poly in newResult.polygons) {
						result.AddPolygon (poly);
					}
					result.RemovePolygon (p);
				}
			}
			
			tries += 1;
			if (tries > 20) {
				return(result);
			}
		}

		return(result);
	}

	// Slice From Point
	static public Slice2D SliceFromPoint(Polygon2D polygon, Vector2D point, float rotation)
	{
		Slice2D result = Slice2D.Create(point, rotation);
	
		// Normalize into clockwise
		polygon.Normalize();

		Vector2D sliceA = new Vector2D (point);
		Vector2D sliceB = new Vector2D (point);

		sliceA.Push (rotation, 1e+10f / 2);
		sliceB.Push (rotation, -1e+10f / 2);

		if (polygon.PointInPoly (point) == false) {
			return(result);
		}
			
		// Getting the list of intersections
		List<Vector2D> intersectionsA = polygon.GetListLineIntersectPoly(new Pair2D(point, sliceA));
		List<Vector2D> intersectionsB = polygon.GetListLineIntersectPoly(new Pair2D(point, sliceB));

		// Sorting intersections from one point
		if (intersectionsA.Count > 0 && intersectionsB.Count > 0) {
			sliceA = Vector2DList.GetListSortedToPoint (intersectionsA, point) [0];
			sliceB = Vector2DList.GetListSortedToPoint (intersectionsB, point) [0];
		} else {
			return(result);
		}
			
		List<Pair2D> collisionList = new List<Pair2D>();
		collisionList.Add (new Pair2D (sliceA, sliceB));

		result.AddPolygon(polygon);

		foreach (Pair2D id in collisionList) {
			result.AddCollision (id.A);
			result.AddCollision (id.B);

			// Sclice line points generated from intersections list
			Vector2D vec0 = new Vector2D(id.A);
			Vector2D vec1 = new Vector2D(id.B);

			double rot = Vector2D.Atan2 (vec0, vec1);

			// Slightly pushing slice line so it intersect in all cases
			vec0.Push (rot, LinearSlicer.precision);
			vec1.Push (rot, -LinearSlicer.precision);

			// For each in polygons list attempt convex split
			List<Polygon2D> temp = new List<Polygon2D>(result.polygons); // necessary?
			foreach (Polygon2D poly in temp) {
				// NO, that's the problem
				Slice2D resultList = LinearSlicer.Slice(poly, new Pair2D(vec0, vec1));

				if (resultList.slices.Count > 0) {
					foreach (List<Vector2D> i in resultList.slices) {
						result.AddSlice(i);
					}
				}

				if (resultList.polygons.Count > 0) {
					foreach (Polygon2D i in resultList.polygons) {
						result.AddPolygon(i);
					}					

					// If it's possible to perform splice, remove parent polygon from result list
					result.RemovePolygon(poly);
				}
			}
		}
		result.RemovePolygon (polygon);
		return(result);
	}
}
