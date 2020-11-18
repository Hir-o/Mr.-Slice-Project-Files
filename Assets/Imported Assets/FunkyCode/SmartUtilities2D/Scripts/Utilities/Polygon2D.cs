using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Polygon2D  {
	public enum ColliderType {Polygon, Box, Circle, Capsule, Edge, None}
	public enum PolygonType {Rectangle, Circle, Pentagon, Hexagon, Octagon};
	static public int defaultCircleVerticesCount = 25;

	public List<Vector2D> pointsList = new List<Vector2D>();
	public List<Polygon2D> holesList = new List<Polygon2D>();

	//public bool InCamera(Camera camera) {
	//	return(false);
	//}

	static public Polygon2D CreateFromCamera(Camera camera) {
		float sizeY = Camera.main.orthographicSize;
		Polygon2D polygon = Polygon2D.CreateFromRect(new Vector2(sizeY * (((float)Screen.width) / Screen.height), sizeY));
		return(polygon);
	}

	public void AddPoint(Vector2D point) {
		pointsList.Add (point);
	}

	public void AddPoint(Vector2 point) {
		pointsList.Add (new Vector2D(point));
	}

	public void AddPoint(float pointX, float pointY) {
		pointsList.Add (new Vector2D(pointX, pointY));
	}

	public void AddPoints(List<Vector2D> points) { 
		foreach (Vector2D point in points) {
			AddPoint (point);
		}
	}

	public Polygon2D() {}
	public Polygon2D(List<Vector2D> polygonPointsList, List<Polygon2D> holes = null)  {
		pointsList = polygonPointsList; 
		if (holes != null) {
			holesList = holes;
		}
	}

	public Polygon2D(Polygon2D polygon) {
		pointsList = new List<Vector2D>(polygon.pointsList);
		holesList = new List<Polygon2D>(polygon.holesList);
	}

	public void AddHole(Polygon2D poly) {
		holesList.Add (poly);
	}
		
	public bool PointInPoly(Vector2D point) {
		if (PointInHole (point) != null) {
			return(false);
		}
		
		return(Math2D.PointInPoly(point, this));
	}

	public bool PolyInPoly(Polygon2D poly) { // Not Finished? 
		foreach (Polygon2D holes in holesList) {
			if (Math2D.PolyIntersectPoly (poly, holes) == true) { 
				return(false);
			}
		}
		
		return(Math2D.PolyInPoly(this, poly));
	}

	public Polygon2D PointInHole(Vector2D point) {
		foreach (Polygon2D p in holesList) {
			if (p.PointInPoly (point) == true) {
				return(p);
			}
		}

		return(null);
	}

	public Polygon2D ToLocalSpace(Transform transform) {
		Polygon2D newPolygon = new Polygon2D();

		foreach (Vector2D id in pointsList) {
			newPolygon.AddPoint (transform.InverseTransformPoint (id.ToVector2()));
		}

		foreach (Polygon2D p in holesList) {
			newPolygon.AddHole (p.ToLocalSpace (transform));
		}

		return(newPolygon);
	}

	public Polygon2D ToWorldSpace(Transform transform) {
		Polygon2D newPolygon = new Polygon2D();

		foreach (Vector2D id in pointsList) {
			newPolygon.AddPoint (transform.TransformPoint (id.ToVector2()));
		}

		foreach (Polygon2D p in holesList) {
			newPolygon.AddHole (p.ToWorldSpace (transform));
		}

		return(newPolygon);
	}

	// ToTransform?
	public Polygon2D ToScale(Vector2 scale, Vector2D center = null) {
		Polygon2D newPolygon = new Polygon2D();
		
		if (center == null) {
			center = new Vector2D(Vector2.zero);
		}

		foreach(Vector2D pos in pointsList) {
			float dist = (float)Vector2D.Distance(pos, center);
			float rot = (float)Vector2D.Atan2(pos, center);
			newPolygon.AddPoint((float)center.x + Mathf.Cos(rot) * dist * scale.x, (float)center.y + Mathf.Sin(rot) * dist * scale.y);
		}

		return(newPolygon);
	}
	
	// ToTransform?
	public Polygon2D ToRotation(float rotation, Vector2D center = null) {
		Polygon2D newPolygon = new Polygon2D();
		
		if (center == null) {
			center = new Vector2D(Vector2.zero);
		}

		foreach(Vector2D pos in pointsList) {
			float dist = (float)Vector2D.Distance(pos, center);
			float rot = (float)Vector2D.Atan2(pos, center) + rotation;
			newPolygon.AddPoint((float)center.x + Mathf.Cos(rot) * dist, (float)center.y + Mathf.Sin(rot) * dist);
		}

		return(newPolygon);
	}

	// ToTransform?
	public Polygon2D ToOffset(Vector2D pos) {
		Polygon2D newPolygon = new Polygon2D (pointsList);
		foreach (Vector2D p in newPolygon.pointsList) {
			p.Inc (pos);
		}

		foreach (Polygon2D p in holesList) {
			newPolygon.AddHole (p.ToOffset(pos));
		}

		return(newPolygon);
	}
	
	public void Normalize() {
		if (IsClockwise () == false) {
			pointsList.Reverse ();
		}

		foreach (Polygon2D p in holesList) {
			p.Normalize ();
		}
	}
	
	public bool IsClockwise() {
		double sum = 0;
		foreach (Pair2D id in Pair2D.GetList(pointsList)) {
			sum += (id.B.x - id.A.x) * (id.B.y + id.A.y);
		}

		return(sum > 0);
	}

	public double GetArea() {
		double area = 0f;
		foreach (Pair2D id in Pair2D.GetList(pointsList)) {
			area += ((id.B.x - id.A.x) * (id.B.y + id.A.y)) / 2.0f;
		}

		foreach (Polygon2D p in holesList) {
			area -= p.GetArea ();
		}

		return(System.Math.Abs(area)); 
	}

	public Rect GetBounds() {
		return(Math2D.GetBounds(pointsList)); 
	}
 
	public List<Polygon2D> LineIntersectHoles(Pair2D pair) {
		List<Polygon2D> resultList = new List<Polygon2D>();
		foreach (Polygon2D poly in holesList) {
			if (Math2D.LineIntersectPoly(pair, poly) == true) {
				resultList.Add (poly);
			}
		}

		return(resultList);
	}

	public bool SliceIntersectPoly(List <Vector2D> slice) {
		if (Math2D.SliceIntersectPoly (slice, this)) {
			return(true);
		}
		
		foreach (Polygon2D poly in holesList) {
			if (Math2D.SliceIntersectPoly (slice, poly)) {
				return(true);
			}
		}

		return(false);
	}
		
	public List<Polygon2D> GetListSliceIntersectHoles(List <Vector2D> slice) {
		List<Polygon2D> resultList = new List<Polygon2D> ();
		foreach (Polygon2D poly in holesList) {
			if (Math2D.SliceIntersectPoly(slice, poly) == true) {
				resultList.Add (poly);
			}
		}

		return(resultList);
	}

	public List<Vector2D> GetListLineIntersectPoly(Pair2D line) {
		List<Vector2D> intersections = Math2D.GetListLineIntersectPoly(line, this);
		foreach (Polygon2D poly in holesList) {
			foreach (Vector2D p in Math2D.GetListLineIntersectPoly(line, poly)) {
				intersections.Add (p);
			}
		}
		
		return(intersections);
	}

	public static ColliderType GetColliderType(GameObject gameObject) {
		EdgeCollider2D edgeCollider2D = gameObject.GetComponent<EdgeCollider2D> ();
		if (edgeCollider2D != null) {
			return(ColliderType.Edge);
		}

		PolygonCollider2D polygonCollider2D = gameObject.GetComponent<PolygonCollider2D> ();
		if (polygonCollider2D != null) {
			return(ColliderType.Polygon);
		}

		BoxCollider2D boxCollider2D = gameObject.GetComponent<BoxCollider2D> ();
		if (boxCollider2D != null) {
			return(ColliderType.Box);
		}

		CircleCollider2D circleCollider2D = gameObject.GetComponent<CircleCollider2D> ();
		if (circleCollider2D != null) {
			return(ColliderType.Circle);
		}

		CapsuleCollider2D capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D> ();
		if (capsuleCollider2D != null) {
			return(ColliderType.Capsule);
		}

		return(ColliderType.None);
	}

	static public Polygon2D CreateFromRect(Vector2 size) {
		Polygon2D polygon = new Polygon2D();
		polygon.AddPoint(-size.x, -size.y);
		polygon.AddPoint(size.x, -size.y);
		polygon.AddPoint(size.x, size.y);
		polygon.AddPoint(-size.x, size.y);
		return(polygon);
	}

	static public Polygon2D CreateFromEdgeCollider(EdgeCollider2D edgeCollider) {
		Polygon2D newPolygon = new Polygon2D ();
		if (edgeCollider != null) {
			foreach (Vector2 p in edgeCollider.points) {
				newPolygon.AddPoint (p + edgeCollider.offset);
			}
			//newPolygon.AddPoint (edgeCollider.points[0] + edgeCollider.offset);
		}
		return(newPolygon);
	}

	static public Polygon2D CreateFromCircleCollider(CircleCollider2D circleCollider, int pointsCount = -1) {
		if (pointsCount < 1) {
			pointsCount = defaultCircleVerticesCount;
		}

		Polygon2D newPolygon = new Polygon2D ();

		float size = circleCollider.radius;
		float i = 0;

		while (i < 360) {
			newPolygon.AddPoint (new Vector2(Mathf.Cos (i * Mathf.Deg2Rad) * size, Mathf.Sin (i * Mathf.Deg2Rad) * size) + circleCollider.offset);
			i += 360f / (float)pointsCount;
		}

		return(newPolygon);
	}

	static public Polygon2D CreateFromBoxCollider(BoxCollider2D boxCollider) {
		Polygon2D newPolygon = new Polygon2D();

		Vector2 size = new Vector2(boxCollider.size.x / 2, boxCollider.size.y / 2);

		newPolygon.AddPoint (new Vector2(-size.x, -size.y) + boxCollider.offset);
		newPolygon.AddPoint (new Vector2(-size.x, size.y) + boxCollider.offset);
		newPolygon.AddPoint (new Vector2(size.x, size.y) + boxCollider.offset);
		newPolygon.AddPoint (new Vector2(size.x, -size.y) + boxCollider.offset);

		return(newPolygon);
	}

	static public Polygon2D CreateFromCapsuleCollider(CapsuleCollider2D capsuleCollider, int pointsCount = -1) {
		if (pointsCount < 1) {
			pointsCount = defaultCircleVerticesCount;
		}

		Polygon2D newPolygon = new Polygon2D();

		Vector2 size = new Vector2(capsuleCollider.size.x / 2, capsuleCollider.size.y / 2);
		float offset = 0;
		float i = 0;

		switch (capsuleCollider.direction) {
			case CapsuleDirection2D.Vertical:
				float sizeXY = (capsuleCollider.transform.localScale.x / capsuleCollider.transform.localScale.y);
				size.x *= sizeXY;
				i = 0;

				if (capsuleCollider.size.x < capsuleCollider.size.y) 
					offset = (capsuleCollider.size.y - capsuleCollider.size.x) / 2;

				while (i < 180) {
					Vector2 v = new Vector2 (Mathf.Cos (i * Mathf.Deg2Rad) * size.x, offset + Mathf.Sin (i * Mathf.Deg2Rad) * size.x);
					newPolygon.AddPoint (v + capsuleCollider.offset);
					i += 360f / (float)pointsCount;
				}

				while (i < 360) {
					Vector2 v = new Vector2 (Mathf.Cos (i * Mathf.Deg2Rad) * size.x, -offset + Mathf.Sin (i * Mathf.Deg2Rad) * size.x);
					newPolygon.AddPoint (v + capsuleCollider.offset);
					i += 360f / (float)pointsCount;
				}
				break;

			case CapsuleDirection2D.Horizontal:
				float sizeYX = (capsuleCollider.transform.localScale.y / capsuleCollider.transform.localScale.x);
				size.x *= sizeYX; // not size.y?
				i = -90;

				if (capsuleCollider.size.y < capsuleCollider.size.x) 
					offset = (capsuleCollider.size.x - capsuleCollider.size.y) / 2;

				while (i < 90) {
					Vector2 v = new Vector2 (offset + Mathf.Cos (i * Mathf.Deg2Rad) * size.y, Mathf.Sin (i * Mathf.Deg2Rad) * size.y);
					newPolygon.AddPoint (v + capsuleCollider.offset);
					i += 360f / (float)pointsCount;
				}

				while (i < 270) {
					Vector2 v = new Vector2 (-offset + Mathf.Cos (i * Mathf.Deg2Rad) * size.y, Mathf.Sin (i * Mathf.Deg2Rad) * size.y);
					newPolygon.AddPoint (v + capsuleCollider.offset);
					i += 360f / (float)pointsCount;
				}
				break;
		}

		return(newPolygon);
	}

	// Capsule Missing
	static public Polygon2D Create(PolygonType type, float size = 1f) {
		Polygon2D newPolygon = new Polygon2D();

		switch (type) {

			case PolygonType.Pentagon:
				newPolygon.AddPoint (0f * size, 1f * size);
				newPolygon.AddPoint (-0.9510565f * size, 0.309017f * size);
				newPolygon.AddPoint (-0.5877852f * size, -0.8090171f * size);
				newPolygon.AddPoint (0.5877854f * size, -0.8090169f * size);
				newPolygon.AddPoint (0.9510565f * size, 0.3090171f * size);
				break;

			case PolygonType.Rectangle:
				newPolygon.AddPoint (-size, -size);
				newPolygon.AddPoint (size, -size);
				newPolygon.AddPoint (size, size);
				newPolygon.AddPoint (-size, size);
				break;

			case PolygonType.Circle:
				float i = 0;

				float cycle = 360f / (float)defaultCircleVerticesCount;

				while (i < 360 ) {
					newPolygon.AddPoint (Mathf.Cos (i * Mathf.Deg2Rad) * size, Mathf.Sin (i * Mathf.Deg2Rad) * size);
					i += cycle;
				}
				break;

			case PolygonType.Hexagon:
				for (int s = 1; s < 360; s = s + 60)
					newPolygon.AddPoint (Mathf.Cos (s * Mathf.Deg2Rad) * size, Mathf.Sin (s * Mathf.Deg2Rad) * size);

				break;

			case PolygonType.Octagon:
				for (int s = 1; s < 360; s = s + 40)
					newPolygon.AddPoint (Mathf.Cos (s * Mathf.Deg2Rad) * size, Mathf.Sin (s * Mathf.Deg2Rad) * size);

				break;
		}

		return(newPolygon);
	}

	public PolygonCollider2D CreatePolygonCollider(GameObject gameObject) {
		PolygonCollider2D collider = gameObject.GetComponent<PolygonCollider2D> ();

		if (collider == null) {
			collider = gameObject.AddComponent<PolygonCollider2D> ();
		}

		List<Vector2> points = new List<Vector2> ();

		foreach (Vector2D p in pointsList) {
			points.Add(p.ToVector2());
		}

		collider.pathCount = (1 + holesList.Count);

		collider.enabled = false;

		collider.SetPath(0, points.ToArray());

		if (holesList.Count > 0) {
			int pathID = 1;
			List<Vector2> pointList = null;

			foreach (Polygon2D poly in holesList) {
				pointList = new List<Vector2> ();

				foreach (Vector2D p in poly.pointsList) {
					pointList.Add (p.ToVector2());
				}

				collider.SetPath (pathID, pointList.ToArray ());
				pathID += 1;
			}
		}

		collider.enabled = true;

		return(collider);
	}

	public EdgeCollider2D CreateEdgeCollider(GameObject gameObject) {
		EdgeCollider2D collider = gameObject.GetComponent<EdgeCollider2D> ();

		if (collider == null) {
			collider = gameObject.AddComponent<EdgeCollider2D> ();
		}

		List<Vector2> points = new List<Vector2> ();

		foreach (Vector2D p in pointsList) {
			points.Add(p.ToVector2());
		}

		collider.points = points.ToArray();

		return(collider);
	}
	
	public Mesh CreateMesh(Vector2 UVScale, Vector2 UVOffset, PolygonTriangulator2D.Triangulation triangulation = PolygonTriangulator2D.Triangulation.Advanced) {        
        return(PolygonTriangulator2D.Triangulate (this, UVScale, UVOffset, triangulation));
    }

	public Mesh CreateMesh(GameObject gameObject, Vector2 UVScale, Vector2 UVOffset, PolygonTriangulator2D.Triangulation triangulation = PolygonTriangulator2D.Triangulation.Advanced) {		
		if (gameObject.GetComponent<MeshRenderer>() == null) {
			gameObject.AddComponent<MeshRenderer>();
		}

		MeshFilter filter = gameObject.GetComponent<MeshFilter> ();
		if (filter == null) {
			filter = gameObject.AddComponent<MeshFilter>() as MeshFilter;
		}
		
		filter.sharedMesh = PolygonTriangulator2D.Triangulate (this, UVScale, UVOffset, triangulation);
		if (filter.sharedMesh == null) {
			UnityEngine.Object.Destroy(gameObject);
		}

		return(filter.sharedMesh);
	}

	public Mesh CreateMesh3D(GameObject gameObject, float z, Vector2 UVScale, Vector2 UVOffset, PolygonTriangulator2D.Triangulation triangulation) {		
		if (gameObject.GetComponent<MeshRenderer>() == null) {
			gameObject.AddComponent<MeshRenderer>();
		}

		MeshFilter filter = gameObject.GetComponent<MeshFilter> ();
		if (filter == null) {
			filter = gameObject.AddComponent<MeshFilter>() as MeshFilter;
		}
		
		filter.sharedMesh = PolygonTriangulator2D.Triangulate3D (this, z, UVScale, UVOffset, triangulation);
		if (filter.sharedMesh == null) {
			UnityEngine.Object.Destroy(gameObject);
		}

		return(filter.sharedMesh);
	}

	public static void SpriteToMesh(GameObject gameObject, VirtualSpriteRenderer spriteRenderer, PolygonTriangulator2D.Triangulation triangulation = PolygonTriangulator2D.Triangulation.Advanced) {
		Texture2D texture = null;
		Sprite sprite = null;

		if (spriteRenderer.sprite != null) {
			sprite = spriteRenderer.sprite;
			texture = sprite.texture;
		}
		
		float spriteSheetU = (float)(texture.width) / sprite.rect.width;
		float spriteSheetV = (float)(texture.height) / sprite.rect.height;

		Rect rect = sprite.rect;
		Rect uvRect = new Rect((float)rect.x / texture.width, (float)rect.y / texture.height, (float)rect.width / texture.width, (float)rect.height / texture.height);

		Vector2 scale = new Vector2(spriteSheetU * rect.width / sprite.pixelsPerUnit, spriteSheetV * rect.height / spriteRenderer.sprite.pixelsPerUnit);
		
		if (spriteRenderer.flipX) {
			scale.x = -scale.x;
		}

		if (spriteRenderer.flipY) {
			scale.y = -scale.y;
		}

		float pivotX = sprite.pivot.x / sprite.rect.width - 0.5f;
		float pivotY = sprite.pivot.y / sprite.rect.height - 0.5f;

		float ix = -0.5f + pivotX / spriteSheetU;
		float iy = -0.5f + pivotY / spriteSheetV;

		Vector2 uvOffset = new Vector2(uvRect.center.x + ix, uvRect.center.y + iy);
		
		Polygon2D polygon2D = Polygon2DList.CreateFromGameObject (gameObject)[0];
		polygon2D.CreateMesh (gameObject, scale, uvOffset, triangulation);

		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer> ();
		if (meshRenderer == null) {
			meshRenderer = gameObject.AddComponent<MeshRenderer> ();
		}
		
		meshRenderer.sharedMaterial = spriteRenderer.material;
		meshRenderer.sharedMaterial.mainTexture = texture;
		meshRenderer.sharedMaterial.color = spriteRenderer.color;

		meshRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
		meshRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
		meshRenderer.sortingOrder = spriteRenderer.sortingOrder;
	}



	// Lighting 2D
	static public Polygon2D GenerateShadow(Polygon2D poly, float sunDirection, float height) {
		Polygon2D convexHull = new Polygon2D ();
		foreach (Vector2D p in poly.pointsList) {
			Vector2D vA = p.Copy();
			vA.Push (sunDirection, height);
			
			convexHull.pointsList.Add (vA);
			convexHull.pointsList.Add (p);
		}

		convexHull.pointsList = Math2D.GetConvexHull (convexHull.pointsList);
		return(convexHull);
	}
}