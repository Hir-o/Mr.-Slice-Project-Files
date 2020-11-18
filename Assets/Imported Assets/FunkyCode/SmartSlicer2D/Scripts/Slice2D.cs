using System.Collections.Generic;
using UnityEngine;

public class Slice2D {
	public Slice2DType sliceType = Slice2DType.Undefined;
	public List<Vector2D> slice = new List<Vector2D>();
	public List<List<Vector2D>> slices = new List<List<Vector2D>>();
	public List<Vector2D> collisions = new List<Vector2D>();
	public List<Polygon2D> polygons = new List<Polygon2D>();
	public List<GameObject> gameObjects = new List<GameObject>();

	// Linear Slice
	public void AddSlice(Pair2D slice) {
		List<Vector2D> list = new List<Vector2D>();
		list.Add(slice.A);
		list.Add(slice.B);
		slices.Add(list);
	}

	// Complex Slicer
	public void AddSlice(List<Vector2D> list) {
		slices.Add(list);
	}

	public void AddCollision(Vector2D point)
	{
		collisions.Add (point);
	}
	
	// Private
	private void AddGameObject(GameObject gameObject)
	{
		gameObjects.Add (gameObject);
	}

	public void AddGameObjects(List<GameObject> newGameObjects)
	{
		if (newGameObjects.Count < 1) {
			return;
		}

		foreach (GameObject gameObject in new List<GameObject>(newGameObjects)) {
			AddGameObject (gameObject);
		}
	}

	public void AddPolygon(Polygon2D polygon)
	{
		polygons.Add (polygon);
	}

	public void RemovePolygon(Polygon2D polygon)
	{
		polygons.Remove (polygon);
	}

	// Complex Slice
	public static Slice2D Create(List<Vector2D> newSlice)
	{
		Slice2D slice2D = Create(Slice2DType.Complex);
		slice2D.slice = new List<Vector2D>(newSlice);
		return(slice2D);
	}

	// Linear Slice
	public static Slice2D Create(Pair2D newSlice)
	{
		Slice2D slice2D = Create(Slice2DType.Linear);
		slice2D.slice = new List<Vector2D>();
		slice2D.slice.Add(newSlice.A);
		slice2D.slice.Add(newSlice.B);
		return(slice2D);
	}

	// Linear Cut Slice
	public static Slice2D Create(LinearCut newSlice)
	{
		Slice2D slice2D = Create(Slice2DType.LinearCut);
		slice2D.slice = new List<Vector2D>();
		
		if (newSlice.pairCut != null) {
			slice2D.slice.Add(newSlice.pairCut.A);
			slice2D.slice.Add(newSlice.pairCut.B);
		} else {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("SmartUtilities2D: Null Linear Cut Slice");
			}
		}
		
		return(slice2D);
	}

	//Complex Cut Slice
	public static Slice2D Create(ComplexCut newSlice)
	{
		Slice2D slice2D = Create(Slice2DType.ComplexCut);
		slice2D.slice = new List<Vector2D>();
		
		if (newSlice.pointsList != null) {
			slice2D.slice = new List<Vector2D>(newSlice.pointsList);
		} else {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("SmartUtilities2D: Null Linear Cut Slice");
			}
		}
		
		return(slice2D);
	}

	// Point Slice
	public static Slice2D Create(Vector2D point, float rotation)
	{
		Slice2D slice2D = Create(Slice2DType.Point);
		slice2D.slice = new List<Vector2D>();
		slice2D.slice.Add(point);
		return(slice2D);
	}

	// Polygon Slice
	public static Slice2D Create(Polygon2D slice)
	{
		Slice2D slice2D = Create(Slice2DType.Polygon);
		slice2D.slice = new List<Vector2D>(slice.pointsList);
		return(slice2D);
	}

	// Exploding Point Slice
	public static Slice2D Create(Vector2D point)
	{
		Slice2D slice2D = Create(Slice2DType.ExplodeByPoint);
		slice2D.slice = new List<Vector2D>();
		slice2D.slice.Add(point);
		return(slice2D);
	}

	public static Slice2D Create(Slice2DType sliceType)
	{
		Slice2D slice2D = new Slice2D ();
		slice2D.sliceType = sliceType;
		return(slice2D);
	}
}


