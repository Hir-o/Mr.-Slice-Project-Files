using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Slicer2D : MonoBehaviour {
	//public enum SlicerType {Collider}; // Mesh?
	//public SlicerType slicerType = SlicerType.Collider; // Not Finished
	
	public enum SliceType {Regular, SliceHole, FillSlicedHole}; // Customize More!
	public enum TextureType {Sprite, Mesh2D, Mesh3D, SpriteAnimation, ImageUI, None};
	public enum CenterOfMass {Default, RigidbodyOnly, TransformOnly};
	public enum AnchorType {AttachRigidbody, RemoveConstraints, CancelSlice, Nothing}; // DestroySlice
	public enum ColliderType {PolygonCollider2D, EdgeCollider2D};

	// Complex Slicer Options
	public static SliceType complexSliceType = SliceType.Regular;
	public static int explosionPieces = 15; // In Use?

	//[Tooltip("Type of texture to generate")]
	public TextureType textureType = TextureType.Sprite;

	public Slicing2DLayer slicingLayer = Slicing2DLayer.Layer1;

	public ColliderType colliderType = ColliderType.PolygonCollider2D;

	// Polygon and Mesh Fields
	public PolygonTriangulator2D.Triangulation triangulation = PolygonTriangulator2D.Triangulation.Advanced;
	public Material material;
	public Vector2 materialScale = new Vector2(1, 1);
	public Vector2 materialOffset = Vector2.zero;
	private Polygon2D polygon = null;

	// Slicing Limit
	public bool slicingLimit = false;
	public int sliceCounter = 0;
	public int maxSlices = 10;

	public bool supportJoints = false;
	
	// Mass
	public CenterOfMass centerOfMass = CenterOfMass.Default;
	public bool recalculateMass = false;

	// Anchors
	public bool anchors = false;

	public Collider2D[] anchorsList = new Collider2D[1];
	public AnchorType anchorType = AnchorType.AttachRigidbody;

	private List<Polygon2D> anchorPolygons = new List<Polygon2D>();
	private List<Collider2D> anchorColliders = new List<Collider2D>();

	// Sprite Information
	public VirtualSpriteRenderer spriteRenderer = null;
	static private Material spriteMaterial;

	// Joints Support
	private Rigidbody2D body;
	private List<Joint2D> joints = new List<Joint2D>();

	// Event Handling
	public delegate bool Slice2DEvent(Slice2D slice);
	public delegate void Slice2DResultEvent(Slice2D slice);

	private event Slice2DEvent sliceEvent;
	private event Slice2DResultEvent sliceResultEvent;

	static private event Slice2DEvent globalSliceEvent;
	static private event Slice2DResultEvent globalSliceResultEvent;

	private event Slice2DEvent anchorSliceEvent;
	private event Slice2DResultEvent anchorSliceResultEvent;

	static private event Slice2DEvent anchorGlobalSliceEvent;
	static private event Slice2DResultEvent anchorGlobalSliceResultEvent;

	static private List<Slicer2D> slicer2DList = new List<Slicer2D>();

	private Rigidbody2D GetRigibody() {
		if (body == null) {
			body = GetComponent<Rigidbody2D>();
		}
		return(body); 
	}

	public void AddAnchorEvent(Slice2DEvent slicerEvent) {
		anchorSliceEvent += slicerEvent;
	}
	
	public void AddAnchorResultEvent(Slice2DResultEvent slicerEvent) {
		anchorSliceResultEvent += slicerEvent;
	}

	static public void AddGlobalAnchorEvent(Slice2DEvent slicerEvent) {
		anchorGlobalSliceEvent += slicerEvent;
	}
	
	static public void AddGlobalResultAnchorEvent(Slice2DResultEvent slicerEvent) {
		anchorGlobalSliceResultEvent += slicerEvent;
	}

	public void AddEvent(Slice2DEvent slicerEvent) {
		sliceEvent += slicerEvent;
	}
	public void AddResultEvent(Slice2DResultEvent slicerEvent) {
		sliceResultEvent += slicerEvent;
	}

	static public void AddGlobalEvent(Slice2DEvent slicerEvent) {
		globalSliceEvent += slicerEvent;
	}
	static public void AddGlobalResultEvent(Slice2DResultEvent slicerEvent) {
		globalSliceResultEvent += slicerEvent;
	}

	static public int GetListCount() {
		return(slicer2DList.Count);
	}

	static public List<Slicer2D> GetList() {
		return(new List<Slicer2D>(slicer2DList));
	}

	static public List<Slicer2D> GetListLayer(Slice2DLayer layer) {
		List<Slicer2D> result = new List<Slicer2D> ();

		foreach (Slicer2D id in slicer2DList) {
			if (id.MatchLayers (layer)) {
				result.Add(id);
			}
		}
		
		return(result);
	}
		
	public int GetLayerID() {
		return((int)slicingLayer);
	}

	public bool MatchLayers(Slice2DLayer sliceLayer) {
		return((sliceLayer == null || sliceLayer.GetLayerType() == Slice2DLayerType.All) || sliceLayer.GetLayerState(GetLayerID ()));
	}
	
	public Polygon2D GetPolygon() {
		if (polygon == null) {
			polygon = Polygon2DList.CreateFromGameObject(gameObject)[0];
		}
		return(polygon);
	}

	void OnEnable() {
		slicer2DList.Add (this);
	}
	void OnDisable() {
		slicer2DList.Remove (this);
	}

	void Start() {
		Initialize ();

		if (supportJoints == true) {
			RecalculateJoints();
		}

		StartAnchor ();
	}

	// Check Before Each Function - Then This Could Be Private
	public void Initialize() {
		if (spriteMaterial == null) {
			spriteMaterial = new Material (Shader.Find ("Sprites/Default"));
		}

		List<Polygon2D> result = Polygon2DList.CreateFromGameObject (gameObject);

		// Split collider if there are more polygons than 1
		if (result.Count > 1) {
			PerformResult(result, new Slice2D());
		}

		MeshRenderer meshRenderer;

		switch (textureType) {
			case TextureType.Mesh2D:
				// Needs Mesh UV Options
				GetPolygon().CreateMesh (gameObject, materialScale, materialOffset, triangulation);
				
				meshRenderer = GetComponent<MeshRenderer> ();
				meshRenderer.material = material;

				break;

			case TextureType.Mesh3D:
				GetPolygon().CreateMesh3D (gameObject, 1, new Vector2 (1, 1), Vector2.zero, triangulation);
				
				meshRenderer = GetComponent<MeshRenderer> ();
				meshRenderer.material = material;

				break;

			case TextureType.Sprite: case TextureType.SpriteAnimation:
				if (spriteRenderer == null) {
					spriteRenderer = new VirtualSpriteRenderer(GetComponent<SpriteRenderer>());
				}
				break;

			default:
				break;
			}
	}
		
	public Slice2D LinearSlice(Pair2D slice, bool perform = true) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();

		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.LinearSlice (colliderPolygon, slice);

			if (perform) {
				PerformResult (sliceResult.polygons, sliceResult);
			}

			return(sliceResult);
		}
			
		return(slice2D);
	}

	public Slice2D LinearCutSlice(LinearCut slice) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Polygon2D slicePoly = new Polygon2D(slice.GetPointsList(1.01f));
			
			if (Math2D.PolyInPoly(slicePoly, colliderPolygon) == true) {
				Destroy (gameObject);
				return(slice2D);

			} else {
				Slice2D sliceResult = Slicer2D.API.LinearCutSlice (colliderPolygon, slice);
				
				foreach(Polygon2D poly in new List<Polygon2D> (sliceResult.polygons)) {
					if (Math2D.PolyInPoly(slicePoly, poly)) {
						sliceResult.RemovePolygon(poly);
					}
				}

				PerformResult (sliceResult.polygons, sliceResult);

				return(sliceResult);
			}
		}
		
		return(Slice2D.Create (slice));
	}
			
	public Slice2D ComplexSlice(List<Vector2D> slice) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.ComplexSlice (colliderPolygon, slice);
			PerformResult (sliceResult.polygons, sliceResult);

			return(sliceResult);
		}
		
		return(slice2D);
	}

	public Slice2D ComplexCutSlice(ComplexCut slice) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Polygon2D slicePoly = new Polygon2D(slice.GetPointsList(1.01f));
			
			if (Math2D.PolyInPoly(slicePoly, colliderPolygon) == true) {
				Destroy (gameObject);
				return(slice2D);

			} else {
				Slice2D sliceResult = Slicer2D.API.ComplexCutSlice (colliderPolygon, slice);
				
				foreach(Polygon2D poly in new List<Polygon2D> (sliceResult.polygons)) {
					if (Math2D.PolyInPoly(slicePoly, poly)) {
						sliceResult.RemovePolygon(poly);
					}
				}

				PerformResult (sliceResult.polygons, sliceResult);

				return(sliceResult);
			}
		}
		
		return(Slice2D.Create (slice));
	}

	public Slice2D PointSlice(Vector2D point, float rotation) {
		Slice2D slice2D = Slice2D.Create (point, rotation);

		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.PointSlice (colliderPolygon, point, rotation);

			PerformResult (sliceResult.polygons, sliceResult);
			
			return(sliceResult);
		}

		return(slice2D);
	}

	public Slice2D PolygonSlice(Polygon2D slice, Polygon2D slicePolygonDestroy) {
		Slice2D slice2D = Slice2D.Create (slice);

		if (isActiveAndEnabled == false) {
			return(slice2D);
		}
		
		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.PolygonSlice (colliderPolygon, slice);

			if (sliceResult.polygons.Count > 0) {
				if (slicePolygonDestroy != null) {
					foreach (Polygon2D p in new List<Polygon2D>(sliceResult.polygons)) {
						if (slicePolygonDestroy.PolyInPoly (p) == true) {
							sliceResult.RemovePolygon (p);
						}
					}
				}
				// Check If Slice Result Is Correct
				if (sliceResult.polygons.Count > 0) {
					sliceResult.AddGameObjects (PerformResult (sliceResult.polygons, slice2D));
				} else if (slicePolygonDestroy != null) {
					Destroy (gameObject);
				}
	
				return(sliceResult);
			}
		}

		return(slice2D);
	}

	public Slice2D ExplodeByPoint(Vector2D point) {
		Slice2D slice2D = Slice2D.Create (point);
		
		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.ExplodeByPoint (colliderPolygon, point);
			PerformResult (sliceResult.polygons, sliceResult);
			
			return(sliceResult);
		}

		return(slice2D);
	}

	public Slice2D ExplodeInPoint(Vector2D point) {
		Slice2D slice2D = Slice2D.Create (point);
		
		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.ExplodeInPoint (colliderPolygon, point);
			PerformResult (sliceResult.polygons, sliceResult);
			
			return(sliceResult);
		}

		return(slice2D);
	}


	public Slice2D Explode() {
		Slice2D slice2D = Slice2D.Create (Slice2DType.Explode);

		if (isActiveAndEnabled == false) {
			return(slice2D);
		}

		Polygon2D colliderPolygon = GetPolygonToSlice ();
		if (colliderPolygon != null) {
			Slice2D sliceResult = Slicer2D.API.Explode (colliderPolygon);
			PerformResult (sliceResult.polygons, sliceResult);
			
			return(sliceResult);
		}

		return(slice2D);
	}

	public List<GameObject> PerformResult(List<Polygon2D> result, Slice2D slice) {
		List<GameObject> resultGameObjects = new List<GameObject> ();

		if (result.Count < 1) {
			return(resultGameObjects);
		}

		slice.polygons = result;

		if (sliceEvent != null && sliceEvent (slice) == false) {
			return(resultGameObjects);
		}

		if (anchorSliceEvent != null && anchorSliceEvent (slice) == false) {
			return(resultGameObjects);
		}

		if (globalSliceEvent != null && globalSliceEvent (slice) == false) {
			return(resultGameObjects);
		}

		if (anchorGlobalSliceEvent != null && anchorGlobalSliceEvent (slice) == false) {
			return(resultGameObjects);
		}

		double originArea = 1f;

		if (recalculateMass) {
			originArea = GetPolygon().GetArea();
		}

		Rigidbody2D originalRigidBody = GetComponent<Rigidbody2D>();

		int name_id = 1;
		foreach (Polygon2D id in result) {
			GameObject gObject = new GameObject();
			resultGameObjects.Add (gObject);

			CopyComponents(this, gObject);

			Slicer2D slicer = gObject.GetComponent<Slicer2D> ();
			slicer.sliceCounter = sliceCounter + 1;
			slicer.maxSlices = maxSlices;

			gObject.name = name + " (" + name_id + ")";
			gObject.transform.parent = transform.parent;
			gObject.transform.position = transform.position;
			gObject.transform.rotation = transform.rotation;
			gObject.transform.localScale = transform.localScale;

			if (centerOfMass == CenterOfMass.TransformOnly) {
				Polygon2D localPoly = id.ToLocalSpace (gObject.transform);
				Rect bounds = localPoly.GetBounds();
				Vector2 centerOffset = new Vector2(bounds.center.x * transform.lossyScale.x, bounds.center.y * transform.lossyScale.y);
				gObject.transform.Translate(centerOffset, 0);
			}

			if (originalRigidBody) {
				Rigidbody2D newRigidBody = gObject.GetComponent<Rigidbody2D> ();

				newRigidBody.isKinematic = originalRigidBody.isKinematic;
				newRigidBody.velocity = originalRigidBody.velocity;
				newRigidBody.angularVelocity = originalRigidBody.angularVelocity;
				newRigidBody.angularDrag = originalRigidBody.angularDrag;
				newRigidBody.constraints = originalRigidBody.constraints;
				newRigidBody.gravityScale = originalRigidBody.gravityScale;
				newRigidBody.collisionDetectionMode = originalRigidBody.collisionDetectionMode;
				//newRigidBody.sleepMode = originalRigidBody.sleepMode;
				//newRigidBody.inertia = originalRigidBody.inertia;

				// Center of Mass : Auto / Center
				if (centerOfMass == CenterOfMass.RigidbodyOnly) {
					newRigidBody.centerOfMass = Vector2.zero;
				}
				
				if (recalculateMass) {
					float newArea = (float)id.ToLocalSpace(transform).GetArea ();
					newRigidBody.mass = originalRigidBody.mass * (float) (newArea / originArea);
				}
			}

			PhysicsMaterial2D material = gameObject.GetComponent<Collider2D> ().sharedMaterial;
			bool isTrigger = gameObject.GetComponent<Collider2D>().isTrigger;	

			Collider2D collider = null;
			switch (colliderType) {
				case ColliderType.PolygonCollider2D:
					collider = (Collider2D)id.ToLocalSpace (gObject.transform).CreatePolygonCollider (gObject);
					break;
				case ColliderType.EdgeCollider2D:
					collider = (Collider2D)id.ToLocalSpace (gObject.transform).CreateEdgeCollider (gObject);
					break;
			}

			collider.sharedMaterial = material;
			collider.isTrigger = isTrigger;

			switch (textureType) {
				case TextureType.Sprite:
					Polygon2D.SpriteToMesh(gObject, spriteRenderer);
					break;

				case TextureType.SpriteAnimation:
					gObject.GetComponent<Slicer2D>().textureType = TextureType.Sprite;
					Polygon2D.SpriteToMesh(gObject, spriteRenderer);
					break;
					
				default:
					break;
				}

			name_id += 1;
		}
			
		Destroy (gameObject);

		if (resultGameObjects.Count > 0) {
			slice.gameObjects = resultGameObjects;

			if (supportJoints == true) {
				SliceJointEvent (slice);
			}

			if ((sliceResultEvent != null)) {
				sliceResultEvent (slice);
			}

			if ((globalSliceResultEvent != null)) {
				globalSliceResultEvent (slice);
			}

			if ((anchorSliceResultEvent != null)) {
				anchorSliceResultEvent (slice);
			}

			if ((anchorGlobalSliceResultEvent != null)) {
				anchorGlobalSliceResultEvent (slice);
			}
		}

		return(resultGameObjects);
	}
	
	static public List<Slice2D> LinearSliceAll(Pair2D slice, Slice2DLayer layer = null, bool perform = true) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.LinearSlice (slice, perform);

			if (perform) {
				if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
				}
			} else {
				if (sliceResult.polygons.Count > 0) {
					result.Add (sliceResult);
				}
			}
			
		}

		return(result);
	}
	
	static public List<Slice2D> LinearCutSliceAll(LinearCut linearCut, Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.LinearCutSlice (linearCut);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}
				
		return(result);
	}

	static public List<Slice2D> ComplexSliceAll(List<Vector2D> slice, Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ComplexSlice (slice);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}
				
		return(result);
	}

	static public List<Slice2D> ComplexCutSliceAll(ComplexCut complexCut, Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ComplexCutSlice (complexCut);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}
				
		return(result);
	}

	static public List<Slice2D> PointSliceAll(Vector2D slice, float rotation, Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.PointSlice (slice, rotation);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}

	// Remove Position
	static public List<Slice2D> PolygonSliceAll(Vector2D position, Polygon2D slicePolygon, bool destroy, Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		Polygon2D slicePolygonDestroy = null;
		if (destroy) {
			slicePolygonDestroy = slicePolygon.ToScale(new Vector2(1.1f, 1.1f));
			slicePolygonDestroy = slicePolygonDestroy.ToOffset (position);
		}
		
		slicePolygon = slicePolygon.ToOffset (position);
		
		foreach (Slicer2D id in GetListLayer(layer)) {
			result.Add (id.PolygonSlice (slicePolygon, slicePolygonDestroy));
		}
		
		return(result);
	}
	
	static public List<Slice2D> ExplodeByPointAll(Vector2D point, Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ExplodeByPoint (point);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}

	static public List<Slice2D> ExplodeInPointAll(Vector2D point, Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}
		
		foreach (Slicer2D id in GetListLayer(layer)) {
			Slice2D sliceResult = id.ExplodeInPoint (point);
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}

	static public List<Slice2D> ExplodeAll(Slice2DLayer layer = null) {
		List<Slice2D> result = new List<Slice2D> ();

		if (layer == null) {
			layer = Slice2DLayer.Create();
		}

		foreach (Slicer2D id in	GetListLayer(layer)) {
			Slice2D sliceResult = id.Explode ();
			if (sliceResult.gameObjects.Count > 0) {
				result.Add (sliceResult);
			}
		}

		return(result);
	}
		
	private Polygon2D GetPolygonToSlice() {
		if (sliceCounter >= maxSlices && slicingLimit) {
			return(null);
		}

	    return(GetPolygon().ToWorldSpace (transform));
	}

	public void RecalculateJoints() {
		Rigidbody2D body = GetRigibody();
		if (body) {
			joints = Joint2D.GetJointsConnected (body);
		}
	}

	void SliceJointEvent(Slice2D sliceResult) {
		RecalculateJoints() ;

		// Remove Slicer Component Duplicated From Sliced Components
		foreach (GameObject g in sliceResult.gameObjects) {
			List<Joint2D> joints = Joint2D.GetJoints(g);
			foreach(Joint2D joint in joints) {
				if (Polygon2DList.CreateFromGameObject (g)[0].PointInPoly (new Vector2D (joint.anchoredJoint2D.anchor)) == false) {
					Destroy (joint.anchoredJoint2D);
				} else {
					if (joint.anchoredJoint2D != null && joint.anchoredJoint2D.connectedBody != null) {
						Slicer2D slicer2D = joint.anchoredJoint2D.connectedBody.gameObject.GetComponent<Slicer2D>();
						if (slicer2D != null) {
							slicer2D.RecalculateJoints();
						}
					}
				}
			}
		}
	
		if (GetRigibody() == null) {
			return;
		}

		// Reconnect Joints To Sliced Bodies
		foreach(Joint2D joint in joints) {
			if (joint.anchoredJoint2D == null) {
				continue;
			}
			
			foreach (GameObject g in sliceResult.gameObjects) {
				Polygon2D poly = Polygon2DList.CreateFromGameObject (g)[0];

				switch (joint.jointType) {
					case Joint2D.Type.HingeJoint2D:
						if (poly.PointInPoly (new Vector2D (joint.anchoredJoint2D.connectedAnchor))) {
							joint.anchoredJoint2D.connectedBody = g.GetComponent<Rigidbody2D> ();
						}
						break;

					default:
						if (poly.PointInPoly (new Vector2D (joint.anchoredJoint2D.connectedAnchor))) {
							joint.anchoredJoint2D.connectedBody = g.GetComponent<Rigidbody2D> ();
						} else {
							
						}
						break;
				}
			}
		}
	}

		
	void StartAnchor () {
		bool addEvents = false;

		foreach(Collider2D collider in anchorsList) {
			if (collider != null) {
				addEvents = true;
			}
		}

		if (addEvents == false) {
			return;
		}

		Slicer2D slicer = GetComponent<Slicer2D> ();
		if (slicer != null) {
			slicer.AddResultEvent (OnAnchorSliceResult);
			slicer.AddEvent (OnAnchorSlice);
		}

		foreach(Collider2D collider in anchorsList) {
			anchorPolygons.Add(Polygon2DList.CreateFromGameObject (collider.gameObject)[0]);
			anchorColliders.Add(collider);
		}
	}

	bool OnAnchorSlice(Slice2D sliceResult) {
		if (anchorSliceEvent != null) {
			if (sliceEvent (sliceResult) == false) {
				return(false);
			}
		}

		if (anchorGlobalSliceEvent != null) {
			if (globalSliceEvent (sliceResult) == false) {
				return(false);
			}
		}

		switch (anchorType) {
			case AnchorType.CancelSlice:
				foreach (Polygon2D polyA in new List<Polygon2D>(sliceResult.polygons)) {
					bool perform = true;
					foreach(Polygon2D polyB in anchorPolygons) {
						if (Math2D.PolyCollidePoly (polyA, GetPolygonInWorldSpace(polyB)) ) {
							perform = false;
						}
					}
					if (perform) {
						return(false);
					}
				}
				break;
			/* 
			case AnchorType.DestroySlice:
				foreach (Polygon2D polyA in new List<Polygon2D>(sliceResult.polygons)) {
					bool perform = true;
					foreach(Polygon2D polyB in polygons) {
						if (Math2D.PolyCollidePoly (polyA.pointsList, GetPolygonInWorldSpace(polyB).pointsList) ) {
							sliceResult.polygons.Remove(polyB);
						}
					}
					
				}
				break;
			*/

			default:
				break;
		}
		return(true);
	}

	void OnAnchorSliceResult(Slice2D sliceResult) {
		if (anchorPolygons.Count < 1) {
			return;
		}

		List<GameObject> gameObjects = new List<GameObject>();

		foreach (GameObject p in sliceResult.gameObjects) {
			Polygon2D polyA = Polygon2DList.CreateFromGameObject (p)[0].ToWorldSpace (p.transform);
			bool perform = true;

			foreach(Polygon2D polyB in anchorPolygons) {
				if (Math2D.PolyCollidePoly (polyA, GetPolygonInWorldSpace(polyB))) {
					perform = false;
				}
			}

			if (perform) {
				gameObjects.Add(p);
			}
		}

		foreach(GameObject p in gameObjects) {
			switch (anchorType) {
				case AnchorType.AttachRigidbody:
					if (p.GetComponent<Rigidbody2D> () == null) {
						p.AddComponent<Rigidbody2D> ();
					}

					p.GetComponent<Rigidbody2D> ().isKinematic = false;
					break;

				case AnchorType.RemoveConstraints:
					if (p.GetComponent<Rigidbody2D> () != null) {
						p.GetComponent<Rigidbody2D> ().constraints = 0;
						p.GetComponent<Rigidbody2D>().useAutoMass = true;
					}
					break;

				default:
					break;
			}
			Slicer2D slicer = p.GetComponent<Slicer2D>();
			slicer.anchors = false;
			slicer.anchorColliders = new List<Collider2D>();
		}

		if (gameObjects.Count > 0) {
			Slice2D newSlice = Slice2D.Create(sliceResult.sliceType);
			newSlice.gameObjects = gameObjects;

			if ((anchorSliceResultEvent != null)) {
				anchorSliceResultEvent (newSlice);
			}

			if ((anchorGlobalSliceResultEvent != null)) {
				anchorGlobalSliceResultEvent (newSlice);
			}
		}
	}

	private Polygon2D GetPolygonInWorldSpace(Polygon2D poly) {
		return(poly.ToWorldSpace(anchorColliders[anchorPolygons.IndexOf(poly)].transform));
	}

	static public void CopyComponents(Slicer2D slicer, GameObject gObject) {
		Component[] scriptList = slicer.gameObject.GetComponents<Component>();	
		foreach (Component script in scriptList) {
			if (script == null) {
				continue;
			}
			// Do not copy Colliders
			if (script.GetType().ToString() == "UnityEngine.PolygonCollider2D" || script.GetType().ToString() == "UnityEngine.EdgeCollider2D" || script.GetType().ToString() == "UnityEngine.BoxCollider2D" || script.GetType().ToString() == "UnityEngine.CircleCollider2D" || script.GetType().ToString() == "UnityEngine.CapsuleCollider2D") {
				continue;
			}

			switch (slicer.textureType) {
				case TextureType.SpriteAnimation:
					if (script.GetType().ToString() == "UnityEngine.SpriteRenderer" || script.GetType().ToString() == "UnityEngine.Animator") {
						continue;
					}
					break;
				
				case TextureType.Sprite:
					if (script.GetType().ToString() == "UnityEngine.SpriteRenderer") {
						continue;
					}
					break;

				default:
					break;
				}

			if (script.GetType().ToString() != "UnityEngine.Transform") {
				gObject.AddComponent(script.GetType());
				System.Reflection.FieldInfo[] fields = script.GetType().GetFields();

				foreach (System.Reflection.FieldInfo field in fields) {
					field.SetValue(gObject.GetComponent(script.GetType()), field.GetValue(script));
				}
			}
		}

		foreach (Behaviour childCompnent in gObject.GetComponentsInChildren<Behaviour>()) {
			foreach (Behaviour child in slicer.GetComponentsInChildren<Behaviour>()) {
				if (child.GetType() == childCompnent.GetType()) {
					childCompnent.enabled = child.enabled;
					break;
				}
			}
		}
	}

	public class API {
		static public Slice2D LinearSlice(Polygon2D polygon, Pair2D slice) {
			return(LinearSlicer.Slice (polygon, slice));
		}
		static public Slice2D LinearCutSlice(Polygon2D polygon, LinearCut linearCut) {
			return(ComplexSlicerExtended.LinearCutSlice (polygon, linearCut));
		}
		static public Slice2D ComplexSlice(Polygon2D polygon, List<Vector2D> slice) {
			return(ComplexSlicer.Slice (polygon, slice));
		}
		static public Slice2D ComplexCutSlice(Polygon2D polygon, ComplexCut complexCut) {
			return(ComplexSlicerExtended.ComplexCutSlice (polygon, complexCut));
		}
		static public Slice2D PointSlice(Polygon2D polygon, Vector2D point, float rotation) {
			return(LinearSlicerExtended.SliceFromPoint (polygon, point, rotation));
		}
		static public Slice2D PolygonSlice(Polygon2D polygon, Polygon2D polygonB) {
			return(ComplexSlicerExtended.Slice (polygon, polygonB)); 
		}
		static public Slice2D ExplodeByPoint(Polygon2D polygon, Vector2D point) {
			return(LinearSlicerExtended.ExplodeByPoint (polygon, point));
		}
		static public Slice2D ExplodeInPoint(Polygon2D polygon, Vector2D point) {
			return(LinearSlicerExtended.ExplodeInPoint (polygon, point));
		}
		static public Slice2D Explode(Polygon2D polygon) {
			return(LinearSlicerExtended.Explode (polygon));
		}
		static public Polygon2D CreatorSlice(List<Vector2D> slice) {
			return(ComplexSlicerExtended.CreateSlice (slice));
		}
	}

	public class Debug {
		static public bool enabled = true;
	}
}