using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ThinSliceGameManager : MonoBehaviour {
	private double startingArea = 0; // Original Size Of The Map

	public double leftArea = 100;	// Percents Of Map Left

	static public ThinSliceGameManager instance;

	void Start () {
		foreach(Slicer2D slicer in  Slicer2D.GetList()) {
			startingArea += Polygon2DList.CreateFromGameObject(slicer.gameObject)[0].ToWorldSpace(slicer.transform).GetArea();
		}
		instance = this;
	}

	void Update() {
	}

	// Recalculate area that is left
	static public void UpdateLevelBar() {
		instance.leftArea = 0;
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			Polygon2D poly = Polygon2DList.CreateFromGameObject(slicer.gameObject)[0];

			instance.leftArea += poly.ToWorldSpace(slicer.gameObject.transform).GetArea();
		}

		instance.leftArea = ((instance.leftArea) / instance.startingArea) * 100f;
	}

	static public void CreateParticles() {
		if (Slicer2DController.instance.startedSlice == false) {
			return;
		}

		List<Vector2D> points = Slicer2DController.GetLinearVertices(Slicer2DController.GetPair(),  Slicer2DController.instance.minVertexDistance);
		
		if (points.Count < 3) {
			return;
		}

		Max2DParticles.CreateSliceParticles(points);

		float size = 0.5f;
		Vector2 f = points.First().ToVector2();
		f.x -= size / 2;
		f.y -= size / 2;

		List<Vector2D> list = new List<Vector2D>();
		list.Add( new Vector2D (f.x, f.y));
		list.Add( new Vector2D (f.x + size, f.y));
		list.Add( new Vector2D (f.x + size, f.y + size));
		list.Add( new Vector2D (f.x, f.y + size));
		list.Add( new Vector2D (f.x, f.y));

		Max2DParticles.CreateSliceParticles(list).stripped = false;

		f = points.Last().ToVector2();
		f.x -= size / 2;
		f.y -= size / 2;

		list = new List<Vector2D>();
		list.Add( new Vector2D (f.x, f.y));
		list.Add( new Vector2D (f.x + size, f.y));
		list.Add( new Vector2D (f.x + size, f.y + size));
		list.Add( new Vector2D (f.x, f.y + size));
		list.Add( new Vector2D (f.x, f.y));
		
		Max2DParticles.CreateSliceParticles(list).stripped = false;
	
	}
}

	//	if (Slicer2DController.instance.startSliceIfPossible == false || Slicer2DController.instance.startedSlice) {
	//		if (Math2D.SliceIntersectItself(Slicer2DController.complexSlicerPointsList)) {
	//			CreateParticles();
	//			Slicer2DController.complexSlicerPointsList.Clear();
	//		}
	//	}
