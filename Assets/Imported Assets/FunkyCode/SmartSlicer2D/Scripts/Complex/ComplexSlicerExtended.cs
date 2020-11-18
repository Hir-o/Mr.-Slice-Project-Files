using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class ComplexSlicerExtended {
	
	// Can Be Used In Advanced SliceInside
	static public Polygon2D CreateSlice(List<Vector2D> polygonSlice) {
		if (polygonSlice.Count < 1) {
			return(null);
		}
		polygonSlice.Add(polygonSlice.First());
		if (Math2D.SliceIntersectItself(polygonSlice) == true) {
			return(null);
		}
		polygonSlice.Remove(polygonSlice.Last());

		if (polygonSlice.Count () > 2) {
			return(new Polygon2D (polygonSlice));
		}

		return(null);
	}

	// Polygon Slice - TODO: Return No Polygon if it's eaten by polygon slice
	static public Slice2D Slice(Polygon2D polygon, Polygon2D polygonSlice) {
		Slice2D result = Slice2D.Create (polygon);

		Slicer2D.SliceType tempSliceType = Slicer2D.complexSliceType;
		Slicer2D.complexSliceType = Slicer2D.SliceType.SliceHole;

		polygonSlice.Normalize ();
		polygon.Normalize ();

		// Eat a polygon completely
		// Complex Slicer does not register slice in this case
		if (polygonSlice.PolyInPoly (polygon) == true) {
			result.AddPolygon (polygon);
			return(result);
		}

		if (polygon.PolyInPoly (polygonSlice) == true) {
			polygon.AddHole (polygonSlice);
			result.AddPolygon (polygon);
			return(result);
		}

		// Act as Regular Slice
		Vector2D startPoint = null;

		foreach (Vector2D id in polygonSlice.pointsList) {
			if (polygon.PointInPoly (id) == false) {
				startPoint = id;
				break;
			}
		}

		if (startPoint == null) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning ("Starting Point Error In PolygonSlice");
			}
			return(result);
		}

		polygonSlice.pointsList = Vector2DList.GetListStartingPoint (polygonSlice.pointsList, startPoint);
	
		/*
		List<Vector2D> s = new List<Vector2D> ();
		foreach (Pair2D pair in Pair2D.GetList(polygonSlice.pointsList, false)) {
			List<Vector2D> stackList = polygon.GetListSliceIntersectPoly(pair);
			stackList = Vector2DList.GetListSortedToPoint (stackList, pair.A);
			Vector2D old = pair.A;
			s.Add (old);

			foreach (Vector2D id in stackList) {
				s.Add (new Vector2D((old.GetX() + id.GetX()) / 2, (old.GetY() + id.GetY()) / 2));
				old = id;
			}
		}

		polygonSlice.pointsList = s;
		*/

		polygonSlice.AddPoint (startPoint);

		// Not Necessary
		if (polygon.SliceIntersectPoly (polygonSlice.pointsList) == false) {
			return(result);
		}

		// Slice More Times?
		result = ComplexSlicer.Slice (polygon, new List<Vector2D> (polygonSlice.pointsList));

		if (result.polygons.Count < 1) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning ("Slicer2D: Returns Empty Polygon Slice");
			}
		}

		Slicer2D.complexSliceType = tempSliceType;

		return(result);
	}

	static public Slice2D LinearCutSlice(Polygon2D polygon, LinearCut linearCut) {
		List<Vector2D> slice = linearCut.GetPointsList();
		Slice2D result = Slice2D.Create (linearCut);

		if (slice.Count < 1) {
			return(result);
		}

		Vector2D startPoint = null;
		foreach (Vector2D id in slice) {
			if (polygon.PointInPoly (id) == false) {
				startPoint = id;
				break;
			}
		}

		Polygon2D newPolygon = new Polygon2D(slice);

		slice = Vector2DList.GetListStartingPoint (slice, startPoint);
		slice.Add(startPoint);

		if (polygon.PolyInPoly(newPolygon)) {
			polygon.AddHole(newPolygon);
			result.AddPolygon(polygon);
			return(result);
		}

		result = ComplexSlicer.Slice (polygon, slice);
	
		return(result);
	}

	static public Slice2D ComplexCutSlice(Polygon2D polygon, ComplexCut complexCut) {
		List<Vector2D> slice = complexCut.GetPointsList();
		Slice2D result = Slice2D.Create (complexCut);

		if (slice.Count < 1) {
			return(result);
		}

		if (Math2D.SliceIntersectItself(slice)) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("Slicer2D: Complex Cut Slicer intersected with itself!");
			}
			return(result);
		}

		Vector2D startPoint = null;
		foreach (Vector2D id in slice) {
			if (polygon.PointInPoly (id) == false) {
				startPoint = id;
				break;
			}
		}

		Polygon2D newPolygon = new Polygon2D(slice);

		slice = Vector2DList.GetListStartingPoint (slice, startPoint);
		slice.Add(startPoint);

		if (polygon.PolyInPoly(newPolygon)) {
			polygon.AddHole(newPolygon);
			result.AddPolygon(polygon);
			return(result);
		}

		result = ComplexSlicer.Slice (polygon, slice);
	
		return(result);
	}
}