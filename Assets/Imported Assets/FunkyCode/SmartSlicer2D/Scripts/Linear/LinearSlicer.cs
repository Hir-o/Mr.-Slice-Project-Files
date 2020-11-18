using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class LinearSlicer {
	public static float precision = 0.0001f;

	// Linear Slice
	static public Slice2D Slice(Polygon2D polygon, Pair2D slice) {
		Slice2D result = Slice2D.Create(slice);

		// Optimize: Cancel slicing for polygons that are not overlapping
		Rect sliceRect = Math2D.GetBounds(slice);
		Rect polygonRect = polygon.GetBounds();

		if (sliceRect.Overlaps(polygonRect) == false) {
			return(result);
		}
		
		// Normalize into clockwise
		polygon.Normalize();

		// Getting the list of intersections
		List<Vector2D> intersections = polygon.GetListLineIntersectPoly(slice);

		// Single collision cannot make a proper slice
		if (intersections.Count < 2) {
			return(result);
		}

		// Sorting intersections from one point
		intersections = Vector2DList.GetListSortedToPoint(intersections, slice.A);

		List<Pair2D> collisionList = new List<Pair2D>();

		// Dividing intersections into single slices
		// Optimize this (polygon.PointInPoly) line
		// This method doesn't look like very reliable!!!
		foreach (Pair2D p in Pair2D.GetList(intersections, false)) {
			if (polygon.PointInPoly (new Vector2D ((p.B.x + p.A.x) / 2, (p.B.y + p.A.y) / 2)) == true) {
				collisionList.Add (p);
				intersections.Remove (p.A);
				intersections.Remove (p.B);
			}
		}

		result.AddPolygon(polygon);

		// Slice line points generated from intersections list
		foreach (Pair2D id in collisionList) {
			result.AddCollision (id.A);
			result.AddCollision (id.B);

			Vector2D vec0 = new Vector2D(id.A);
			Vector2D vec1 = new Vector2D(id.B);

			double rot = Vector2D.Atan2 (vec0, vec1);

			// Slightly pushing slice line so it intersect in all cases
			vec0.Push (rot, precision);
			vec1.Push (rot, -precision);

			Pair2D line = new Pair2D(vec0, vec1);

			// For each in polygons list attempt convex split
			foreach (Polygon2D poly in (new List<Polygon2D>(result.polygons))) {
				Slice2D resultList = SingleSlice(poly, line);

				if (resultList.slices.Count > 0) {
					foreach (List<Vector2D> i in resultList.slices) {
						result.AddSlice(i);
					}
				}

				if (resultList.polygons.Count > 0) {
					result.AddSlice(line);

					foreach (Polygon2D i in resultList.polygons) {
						result.AddPolygon(i);
					}

					// If it's possible to perform splice, remove currently sliced polygon from result list
					result.RemovePolygon(poly);
				}
			}
		}

		result.RemovePolygon (polygon);

		return(result);
	}

	static private Slice2D SingleSlice(Polygon2D polygon, Pair2D slice) {
		Slice2D result = Slice2D.Create(slice);

		if (Vector2D.Distance(slice.A, slice.B) < 0.001f) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("Slicer2D: Slice is too short");
			}
			return(result);
		}

		if ((polygon.PointInPoly (slice.A) || polygon.PointInPoly (slice.B))) { //  && pointsInHoles == 1
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning ("Slicer2D: Incorrect Split: Code 1");
			}
			// Slicing through hole cut-out
			return(result);
		}

		Polygon2D holeA = polygon.PointInHole (slice.A);
		Polygon2D holeB = polygon.PointInHole (slice.B);

		int pointsInHoles = Convert.ToInt32 (holeA != null) + Convert.ToInt32 (holeB != null);

		if (pointsInHoles == 2 && holeA == holeB) {
			pointsInHoles = 1;
		}

		switch (pointsInHoles) {
			case 0:
				return(SliceWithoutHoles(polygon, slice));

			case 1:
				return(SliceWithOneHole(polygon, slice, holeA, holeB));

			case 2:
				return(SliceWithTwoHoles(polygon, slice, holeA, holeB));

			default:
				break;
		}

		return(result);
	}

	static private Slice2D SliceWithoutHoles(Polygon2D polygon, Pair2D slice) {
		Slice2D result = Slice2D.Create(slice);

		if (polygon.LineIntersectHoles (slice).Count > 0) {
			// When does this happen? - Only when using point slicer!!!
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning ("Slicer2D: Slice Intersect Holes (Point Slicer?)"); 
			}
			return(result);
		}

		Polygon2D polyA = new Polygon2D();
		Polygon2D polyB = new Polygon2D();

		Polygon2D currentPoly = polyA;

		int collisionCount = 0;

		foreach (Pair2D p in Pair2D.GetList(polygon.pointsList)) {
			Vector2D intersection = Math2D.GetPointLineIntersectLine (p, slice);
			
			if (intersection != null) {
				polyA.AddPoint (intersection);
				polyB.AddPoint (intersection);

				currentPoly = (currentPoly == polyA) ? polyB : polyA;

				collisionCount++;
			}
			currentPoly.AddPoint (p.B);
		}

		if (collisionCount == 2) {  // ' Is it convex or concave split?
			if (polyA.pointsList.Count () >= 3) {
				result.AddPolygon (polyA);
			}

			if (polyB.pointsList.Count () >= 3) {
				result.AddPolygon (polyB);
			}

			foreach (Polygon2D poly in result.polygons) {
				foreach (Polygon2D hole in polygon.holesList) {
					if (poly.PolyInPoly (hole) == true) {
						poly.AddHole (hole);	
					}
				}
			}

			return(result);
		}

		if (collisionCount == 3) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("Slicer2D: Linear Slice with 3 points");
			}
		}

		if (collisionCount == 333) {  // Complicated Slices With Holes
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("Slicer2D: Slice " + collisionCount);
			}

			if (polyA.pointsList.Count () >= 3) {
				result.AddPolygon (polyA);
			}

			if (polyB.pointsList.Count () >= 3) {
				result.AddPolygon (polyB);
			}

			foreach (Polygon2D poly in result.polygons) {
				foreach (Polygon2D hole in polygon.holesList) {
					if (poly.PolyInPoly (hole) == true) {
						poly.AddHole (hole);	
					}
				}
			}	

			return(result);
		}
		return(result);
	}

	static private Slice2D SliceWithTwoHoles(Polygon2D polygon, Pair2D slice, Polygon2D holeA, Polygon2D holeB) {
		Slice2D result = Slice2D.Create(slice);

		if (holeA == holeB) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning ("Slicer2D: Incorrect Split 2: Cannot Split Into Same Hole");
			}
			return(result);
		}

		Polygon2D polyA = new Polygon2D ();
		Polygon2D polyB = new Polygon2D (polygon.pointsList);

		polyA.AddPoints (Vector2DList.GetListStartingIntersectLine (holeA.pointsList, slice));
		polyA.AddPoints (Vector2DList.GetListStartingIntersectLine (holeB.pointsList, slice));

		foreach (Polygon2D poly in polygon.holesList) {
			if (poly != holeA && poly != holeB) {
				polyB.AddHole (poly);
			}
		}

		polyB.AddHole (polyA);

		result.AddPolygon (polyB);
		return(result);
	}

	static private Slice2D SliceWithOneHole(Polygon2D polygon, Pair2D slice, Polygon2D holeA, Polygon2D holeB) {
		Slice2D result = Slice2D.Create(slice);

		if (holeA == holeB) {
			Polygon2D polyA = new Polygon2D (polygon.pointsList);
			Polygon2D polyB = new Polygon2D ();
			Polygon2D polyC = new Polygon2D ();

			Polygon2D currentPoly = polyB;

			foreach (Pair2D pair in Pair2D.GetList (holeA.pointsList)) {
				Vector2D point = Math2D.GetPointLineIntersectLine(slice, pair);
				if (point != null) { 
					polyB.AddPoint (point);
					polyC.AddPoint (point);
					currentPoly = (currentPoly == polyB) ? polyC : polyB;
				}
				currentPoly.AddPoint (pair.B);
			}

			if (polyB.pointsList.Count > 2 && polyC.pointsList.Count > 2) {
				if (polyB.GetArea() > polyC.GetArea()) {
					polyA.AddHole (polyB);
					result.AddPolygon (polyC);
				} else {
					result.AddPolygon (polyB);
					polyA.AddHole (polyC);
				}

				result.AddPolygon (polyA);
			}

			return(result);
			// Cross From Side To Polygon
		} else if (polygon.PointInPoly (slice.A) == false || polygon.PointInPoly (slice.B) == false) {
			Polygon2D holePoly = (holeA != null) ? holeA : holeB;

			if (holePoly != null) {
				Polygon2D polyA = new Polygon2D ();
				Polygon2D polyB = new Polygon2D (holePoly.pointsList);

				polyB.pointsList.Reverse ();

				polyA.AddPoints (Vector2DList.GetListStartingIntersectLine (polygon.pointsList, slice));
				polyA.AddPoints (Vector2DList.GetListStartingIntersectLine (polyB.pointsList, slice));

				foreach (Polygon2D poly in polygon.holesList) {
					if (poly != holePoly) {
						polyA.AddHole (poly);
					}
				}

				result.AddPolygon (polyA);
				return(result);
			}
		}
		return(result);
	}
}