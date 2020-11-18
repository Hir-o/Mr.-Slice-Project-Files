using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

// Controller
public class Slicer2DController : MonoBehaviour {
	public enum SliceType {Linear, LinearCut, LinearTracked, Complex, ComplexCut, ComplexClick, ComplexTracked, Point, Polygon, Explode, ExplodeByPoint, Create, ComplexTrail, LinearTrail};
	public enum SliceRotation {Random, Vertical, Horizontal};
	public enum CreateType {Slice, PolygonType};

	public bool addForce = true;
	public float addForceAmount = 5f;

	public bool endSliceIfPossible = false;
	public bool startSliceIfPossible = false;
	public bool startedSlice = false;
	
	// Linear Stripped
	public bool strippedLinear = false;

	[Tooltip("Slice type represents algorithm complexity")]
	public SliceType sliceType = SliceType.Complex;
	public Slice2DLayer sliceLayer = Slice2DLayer.Create();

	public Polygon2D slicePolygon = Polygon2D.Create (Polygon2D.PolygonType.Pentagon);

	[Tooltip("Minimum distance between points (SliceType: Complex/CompelxCut/Trail")]
	public float minVertexDistance = 1f;

	// Polygon Destroyer type settings
	public Polygon2D.PolygonType polygonType = Polygon2D.PolygonType.Circle;
	public float polygonSize = 1;
	public bool polygonDestroy = true;
	
	public bool sliceJoints = false;

	// Polygon Creator
	public Material material;
	public CreateType createType = CreateType.Slice;

	// Complex Slicer - Improve Options!!!!
	public Slicer2D.SliceType complexSliceType = Slicer2D.SliceType.SliceHole;
	
	// Click Slicer
	public int pointsLimit = 3;

	// Cut Slicer
	public float cutSize = 0.5f;

	// Slicer Visuals
	public Max2DMesh.LineType lineType =  Max2DMesh.LineType.Default;
	public bool drawSlicer = true;
	public float visualScale = 1f;
	public float lineWidth = 1.0f;
	public float lineEndWidth = 1.0f;
	public float zPosition = 0f;
	public Color slicerColor = Color.black;
	public bool lineBorder = true;
	public float lineEndSize = 0.5f;
	public float vertexSpace = 0.25f;
	public float borderScale = 2f;

	public bool displayCollisions = false;

	public int edgeCount = 30;

	// Point Slicer
	public SliceRotation sliceRotation = SliceRotation.Random;

	// Events Input Handler
	private static List<Vector2D> pointsList = new List<Vector2D>();
	private static Pair2D linearPair = Pair2D.Zero();


	public static ComplexSlicerTracker complexTracker = new ComplexSlicerTracker();
	public static LinearSlicerTracker linearTracker = new LinearSlicerTracker();

	// Trail Renderer Slicer
	public TrailRenderer trailRenderer = null;
	Vector3[] trailPositions = new Vector3[500];

	public static ComplexSlicerTrail complexTrail = new ComplexSlicerTrail();
	public static LinearSlicerTrail linearTrail = new LinearSlicerTrail();

	public static Slicer2DController instance;
	private bool mouseDown = false;

	public static Color[] slicerColors = {Color.black, Color.green, Color.yellow , Color.red, new Color(1f, 0.25f, 0.125f)};

	public delegate void Slice2DResultEvent(Slice2D slice);
	private event Slice2DResultEvent sliceResultEvent;
	
	private Material lineMaterial;
	private Material lineMaterialBorder;

	private Material lineLegacyMaterial;

	private Mesh mesh = null;
	private Mesh meshBorder = null;

	static public List<Vector2D> GetPoints() {
		return(pointsList);
	}

	static public void ClearPoints() {
		pointsList.Clear();
		linearPair = Pair2D.Zero();
		instance.mouseDown = false;
	}

	static public Pair2D GetPair() {
		return(linearPair);
	}

	public void AddResultEvent(Slice2DResultEvent e) {
		sliceResultEvent += e;
	}

	public void Awake() {
		instance = this;
	}
	
	public void Start() {
		Max2D.Check();

		lineMaterial = new Material(Max2D.lineMaterial);
		lineMaterialBorder = new Material(Max2D.lineMaterial);
		lineLegacyMaterial = new Material(Max2D.lineLegacyMaterial);
	}
	
	public void Draw() {
		if (lineType == Max2DMesh.LineType.Legacy) {
			lineLegacyMaterial.SetColor ("_Emission", slicerColor);
			Max2DMesh.Draw(mesh, lineLegacyMaterial);

		} else {
			if (lineBorder) {
				if (meshBorder != null) {
					//lineMaterialBorder.color = Color.black;
					lineMaterial.SetColor ("_Emission", Color.black);
					Max2DMesh.Draw(meshBorder, lineMaterialBorder);
				}
			}
			
			//lineMaterial.color = slicerColor;
			lineMaterial.SetColor ("_Emission", slicerColor);
			Max2DMesh.Draw(mesh, lineMaterial);
		}
	}

	public static Vector2D GetMousePosition() {
		return(new Vector2D (Camera.main.ScreenToWorldPoint (Input.mousePosition)));
	}
	
	public static Vector2D GetPlayerPosition() {
		return(new Vector2D (Player.Instance.transform.position));
	}

	public void SetSliceType(int type) {
		sliceType = (SliceType)type;
	}

	public void SetLayerType(int type) {
		if (type == 0) {
			sliceLayer.SetLayerType((Slice2DLayerType)0);
		} else {
			sliceLayer.SetLayerType((Slice2DLayerType)1);
			sliceLayer.DisableLayers ();
			sliceLayer.SetLayer (type - 1, true);
		}
	}

	public void SetSlicerColor(int colorInt) {
		slicerColor = slicerColors [colorInt];
	}

	static public List<Vector2D> GetLinearVertices(Pair2D pair, float minVertexDistance) {
		Vector2D startPoint = pair.A.Copy();
		Vector2D endPoint = pair.B.Copy();

		List<Vector2D> linearPoints = new List<Vector2D>();
		int loopCount = 0;
		while ((Vector2D.Distance (startPoint, endPoint) > minVertexDistance)) {
			linearPoints.Add (startPoint.Copy());
			float direction = (float)Vector2D.Atan2 (endPoint, startPoint);
			startPoint.Push (direction, minVertexDistance);

			loopCount ++;
			if (loopCount > 150) {
				break;
			}
		}

		linearPoints.Add (endPoint.Copy());
		return(linearPoints);
	}

	public void Update() {
		if (drawSlicer == false) {
			return;
		}
		float squareSize = lineEndSize;
		
		Max2DMesh.lineType = lineType;
		switch (sliceType) {
			case SliceType.Linear:
				if (mouseDown) {
					if (startSliceIfPossible == false || startedSlice == true) {
						// If Stripped Line
						if (strippedLinear) {
							List<Vector2D> linearPoints = GetLinearVertices(linearPair, minVertexDistance * visualScale);

							if (linearPoints.Count > 1) {
								meshBorder = Max2DMesh.GenerateComplexMesh(linearPoints, transform, lineWidth * visualScale * borderScale, minVertexDistance, zPosition + 0.001f, squareSize * visualScale, lineEndWidth * visualScale * borderScale, vertexSpace);
								mesh = Max2DMesh.GenerateComplexMesh(linearPoints, transform, lineWidth * visualScale, minVertexDistance, zPosition, squareSize * visualScale, lineEndWidth * visualScale, vertexSpace);
								Draw();
							}
						
						} else {
							meshBorder = Max2DMesh.GenerateLinearMesh(linearPair, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f, squareSize * visualScale, lineEndWidth * visualScale * borderScale);
							mesh = Max2DMesh.GenerateLinearMesh(linearPair, transform, lineWidth * visualScale, zPosition, squareSize * visualScale, lineEndWidth * visualScale);
							Draw();
						}

						if (displayCollisions) {
							List<Slice2D> results = Slicer2D.LinearSliceAll (linearPair, sliceLayer, false);
							foreach(Slice2D slice in results) {
								foreach(Vector2D collision in slice.collisions) {
									Pair2D p = new Pair2D(collision, collision);
									meshBorder = Max2DMesh.GenerateLinearMesh(p, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f, 0, 1.5f *lineEndWidth * visualScale * borderScale);
									mesh = Max2DMesh.GenerateLinearMesh(p, transform, lineWidth * visualScale, zPosition, 0, 2f * lineEndWidth * visualScale);
									Draw();
								}
							}
						}
					}
				}
				break;

			case SliceType.Complex:
				if (mouseDown) {
					if (pointsList.Count > 0) {
						if (startSliceIfPossible == false || startedSlice == true) {
							meshBorder = Max2DMesh.GenerateComplexMesh(pointsList, transform, lineWidth * visualScale * borderScale, minVertexDistance, zPosition + 0.001f, squareSize * visualScale,  lineEndWidth * visualScale * borderScale, vertexSpace);
							mesh = Max2DMesh.GenerateComplexMesh(pointsList, transform, lineWidth * visualScale, minVertexDistance, zPosition, squareSize * visualScale, lineEndWidth * visualScale, vertexSpace);
							Draw();
						}
					}
				}
				break;

			case SliceType.ComplexTracked:
				if (pointsList.Count > 0) {
					meshBorder = Max2DMesh.GenerateComplexTrackerMesh(GetMousePosition (), complexTracker.trackerList, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f, squareSize * visualScale);
					mesh = Max2DMesh.GenerateComplexTrackerMesh(GetMousePosition (), complexTracker.trackerList, transform, lineWidth * visualScale, zPosition, squareSize * visualScale);
					Draw();
				}
				break;

			case SliceType.LinearTracked:
				if (pointsList.Count > 0) {
					meshBorder = Max2DMesh.GenerateLinearTrackerMesh(GetMousePosition (), linearTracker.trackerList, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f, squareSize * visualScale);
					mesh = Max2DMesh.GenerateLinearTrackerMesh(GetMousePosition (), linearTracker.trackerList, transform, lineWidth * visualScale, zPosition, squareSize * visualScale);
					Draw();
				}
				break;


			case SliceType.LinearCut:
				if (mouseDown) {
					meshBorder = Max2DMesh.GenerateLinearCutMesh(linearPair, cutSize * visualScale, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f);
					mesh = Max2DMesh.GenerateLinearCutMesh(linearPair, cutSize * visualScale, transform, lineWidth * visualScale, zPosition);
					Draw();
				}
			
				break;
				
			case SliceType.ComplexCut:
				if (mouseDown) {
					meshBorder = Max2DMesh.GenerateComplexCutMesh(pointsList, cutSize * visualScale, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f);
					mesh = Max2DMesh.GenerateComplexCutMesh(pointsList, cutSize * visualScale, transform, lineWidth * visualScale, zPosition);
					Draw();
				}
					
				break;
				
			case SliceType.Polygon:
				meshBorder = Max2DMesh.GeneratePolygonMesh(GetMousePosition (), polygonType, polygonSize * visualScale, minVertexDistance, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f);
				mesh = Max2DMesh.GeneratePolygonMesh(GetMousePosition (), polygonType, polygonSize * visualScale, minVertexDistance, transform, lineWidth * visualScale, zPosition);
				Draw();
				
				break;

			case SliceType.Create:
				if (mouseDown) {
					meshBorder =  Max2DMesh.GenerateCreateMesh(GetMousePosition (), polygonType, polygonSize, createType, pointsList, linearPair, minVertexDistance, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f, squareSize);
					mesh = Max2DMesh.GenerateCreateMesh(GetMousePosition (), polygonType, polygonSize, createType, pointsList, linearPair, minVertexDistance, transform, lineWidth * visualScale, zPosition, squareSize);
					Draw();
				}

				break;

			case SliceType.ComplexClick:
				if (pointsList.Count > 0) {
					List<Vector2D> points = new List<Vector2D>(pointsList);
					Vector2D posA = GetMousePosition ();
					points.Add(posA);
					meshBorder = Max2DMesh.GenerateComplexMesh(points, transform, lineWidth * visualScale * borderScale, minVertexDistance, zPosition + 0.001f, squareSize * visualScale, lineEndWidth * visualScale * borderScale, vertexSpace * visualScale);
					mesh = Max2DMesh.GenerateComplexMesh(points, transform, lineWidth * visualScale, minVertexDistance, zPosition, squareSize * visualScale,  lineEndWidth * visualScale, vertexSpace * visualScale);
					Draw();
				}

				break;

			case SliceType.ComplexTrail:

				meshBorder = Max2DMesh.GenerateComplexTrailMesh(GetMousePosition (), complexTrail.trailList, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f, squareSize * visualScale);
				mesh = Max2DMesh.GenerateComplexTrailMesh(GetMousePosition (), complexTrail.trailList, transform, lineWidth * visualScale, zPosition, squareSize * visualScale);
				Draw();

				break;

			case SliceType.LinearTrail:

				meshBorder = Max2DMesh.GenerateComplexTrailMesh(GetMousePosition (), linearTrail.trailList, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f, squareSize * visualScale);
				mesh = Max2DMesh.GenerateComplexTrailMesh(GetMousePosition (), linearTrail.trailList, transform, lineWidth * visualScale, zPosition, squareSize * visualScale);
				Draw();

				break;


			case SliceType.Point:
			case SliceType.Explode:
			case SliceType.ExplodeByPoint:
				Vector2D pos = GetMousePosition ();
				Pair2D pair = new Pair2D(pos, pos);
				meshBorder = Max2DMesh.GeneratePointMesh(pair, transform, lineWidth * visualScale * borderScale, zPosition + 0.001f);
				mesh = Max2DMesh.GeneratePointMesh(pair, transform, lineWidth * visualScale, zPosition);
				Draw();

				break;
		}

		Max2DMesh.lineType = Max2DMesh.LineType.Default;
	}

	public void LateUpdate() {
		//if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
        //    return;
        //}

//		Vector2D pos = GetMousePosition ();
		Vector2D pos = GetPlayerPosition();

		float scroll = Input.GetAxis("Mouse ScrollWheel");
		switch (sliceType) {
			case SliceType.LinearCut:
			case SliceType.ComplexCut:
				float newCutSize = cutSize + scroll;
				if (newCutSize > 0.05f) {
					cutSize = newCutSize;
				}

				break;

			case SliceType.Polygon:
			case SliceType.Create:
				float newPolygonSize = polygonSize + scroll;
				if (newPolygonSize > 0.05f) {
					polygonSize = newPolygonSize;
				}
				break;
		}


		switch (sliceType) {	
			case SliceType.Linear:
				UpdateLinear (pos);
				break;

			case SliceType.LinearCut:
				UpdateLinearCut (pos);
				break;

			case SliceType.ComplexCut:
				UpdateComplexCut (pos);
				break;

			case SliceType.Complex:
				UpdateComplex (pos);
				break;

			case SliceType.ComplexTracked:
				UpdateComplexTracked(pos);
				break;

			
			case SliceType.LinearTracked:
				UpdateLinearTracked(pos);
				break;

			case SliceType.Point:
				UpdatePoint (pos);
				break;
				
			case SliceType.Explode:			
				UpdateExplode (pos);
				break;

			case SliceType.ExplodeByPoint:			
				UpdateExplodeByPoint (pos);
				break;

			case SliceType.Create:
				UpdateCreate (pos);
				break;

			case SliceType.ComplexTrail:
				UpdateComplexTrail();
				break;

			case SliceType.Polygon:
				UpdatePolygon (pos);
				break;

			case SliceType.ComplexClick:
				UpdateComplexClick (pos);
				break;

			case SliceType.LinearTrail:
				UpdateLinearTrail();
				break;

			default:
				break; 
		}
	}

	private void UpdateLinear(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			linearPair = new Pair2D(pos.Copy(), pos.Copy());
			mouseDown = true;
			startedSlice = false;
		}
		
		if (startSliceIfPossible) {
			if (startedSlice == true) { 
				if (InSlicerComponents(pos.Copy()) == false) {
					startedSlice = false;
				}
			} else if (startedSlice == false) {
				if (InSlicerComponents(pos.Copy())) {
					startedSlice = true;
				} else {
					linearPair.A.Set (pos.Copy());
				}
			}
		}

		if (mouseDown && Player.Instance.isDashing) {
			linearPair.B.Set (pos);
		
			if (endSliceIfPossible) {
				if (LinearSlice (linearPair)) {
					mouseDown = false;
					linearPair.A.Set (pos);

					if (startSliceIfPossible) {
						mouseDown = true;
						linearPair = new Pair2D(pos.Copy(), pos.Copy());
						startedSlice = false;
					}
				}
			}
		}

		if (mouseDown == true && Player.Instance.isDashing == false) {
			mouseDown = false;

			LinearSlice (linearPair);
		}
	}

	private void UpdateComplex(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			pointsList.Clear ();
			pointsList.Add (pos);
			mouseDown = true;
			startedSlice = false;
		}

		if (pointsList.Count < 1) {
			return;
		}
		
		if (Input.GetMouseButton (0)) {
			Vector2D posMove = pointsList.Last ().Copy();
			bool added = false;
			int loopCount = 0;
			while ((Vector2D.Distance (posMove, pos) > minVertexDistance * visualScale)) {
				float direction = (float)Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertexDistance * visualScale);

				if (startSliceIfPossible == true && startedSlice == false) {
					if (InSlicerComponents(posMove.Copy())) {
						while (pointsList.Count > 2) {
							pointsList.RemoveAt(0);
						}

						startedSlice = true;
					}
				}

				pointsList.Add (posMove.Copy());

				added = true;

				loopCount ++;
				if (loopCount > 150) {
					break;
				}
			}

			if (endSliceIfPossible == true && added) {
				if (ComplexSlice (pointsList) == true) {
					mouseDown = false;
					pointsList.Clear ();

					if (startSliceIfPossible) {
						pointsList.Add (pos);
						mouseDown = true;
						startedSlice = false;
					}
				}
			}
		}

		if (mouseDown == true && Input.GetMouseButton (0) == false) {
			mouseDown = false;
			startedSlice = false;
			Slicer2D.complexSliceType = complexSliceType;
			ComplexSlice (pointsList);
			pointsList.Clear ();
		}
	}
	
	private void UpdateComplexClick(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			pointsList.Add(pos);
		}

		if (Input.GetMouseButtonDown (1) || pointsList.Count >= pointsLimit) {
			Slicer2D.complexSliceType = complexSliceType;
			ComplexSlice (pointsList);
			pointsList.Clear ();
		}
	}

	private void UpdateLinearCut(Vector2D pos) {
		if (Player.Instance.isDashing) {
			linearPair.A.Set (pos);
		}

		if (Input.GetMouseButton (0)) {
			linearPair.B.Set (pos);
			mouseDown = true;
		}

		if (mouseDown == true && Player.Instance.isDashing == false) {
			mouseDown = false;
			LinearCut linearCutLine = LinearCut.Create(linearPair, cutSize * visualScale);
			Slicer2D.LinearCutSliceAll (linearCutLine, sliceLayer);
		}
	}

	private void UpdateComplexCut(Vector2D pos) {
//		if (Input.GetMouseButtonDown (0)) {
//			pointsList.Clear ();
//			pointsList.Add (pos);
//			mouseDown = true;
//		}

		if (Player.Instance.isDashing) {
			pointsList.Clear ();
			pointsList.Add (pos);
			mouseDown = true;
		}

		if (pointsList.Count < 1) {
			return;
		}
		
		if (Player.Instance.isDashing) {
			Vector2D posMove = pointsList.Last ().Copy();
			int loopCount = 0;
			while ((Vector2D.Distance (posMove, pos) > minVertexDistance * visualScale)) {
				float direction = (float)Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertexDistance * visualScale);

				pointsList.Add (posMove.Copy());

				loopCount ++;
				if (loopCount > 150) {
					break;
				}
			}
		}

//		if (mouseDown == true && Input.GetMouseButton (0) == false) {
//			ComplexCut complexCutLine = ComplexCut.Create(pointsList, cutSize * visualScale);
//			Slicer2D.ComplexCutSliceAll (complexCutLine, sliceLayer);
//
//			pointsList.Clear ();
//			mouseDown = false;
//		}
		
		if (Player.Instance.isDashing) {
			ComplexCut complexCutLine = ComplexCut.Create(pointsList, cutSize * visualScale);
			Slicer2D.ComplexCutSliceAll (complexCutLine, sliceLayer);

			pointsList.Clear ();
			mouseDown = false;
		}
	}

	private void UpdateComplexTracked(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			pointsList.Clear ();
			complexTracker.trackerList.Clear ();
			pointsList.Add (pos);
		}
						
		if (Input.GetMouseButton (0) && pointsList.Count > 0) {
			Vector2D posMove = pointsList.Last ().Copy();

			int loopCount = 0;
			while ((Vector2D.Distance (posMove, pos) > minVertexDistance)) {
				float direction = (float)Vector2D.Atan2 (pos, posMove);
				posMove.Push (direction, minVertexDistance);
				Slicer2D.complexSliceType = complexSliceType;
				pointsList.Add (posMove.Copy());
				complexTracker.Update(posMove.ToVector2(), 0);
			
				loopCount ++;
				if (loopCount > 150) {
					break;
				}
			}

			mouseDown = true;
			
			complexTracker.Update(posMove.ToVector2(), minVertexDistance);

		} else {
			mouseDown = false;
		}
	}

	
	private void UpdateLinearTracked(Vector2D pos) {
		if (Input.GetMouseButtonDown(0)) {
			pointsList.Clear ();
			linearTracker.trackerList.Clear ();
			pointsList.Add (new Vector2D(Player.Instance.transform.position));
		}
						
		if (Input.GetMouseButton(0) && pointsList.Count > 0) {
			Vector2D posMove = pointsList.Last ().Copy();

			mouseDown = true;
			
			linearTracker.Update(new Vector2D(Player.Instance.transform.position).ToVector2(), minVertexDistance);

		} else {
			mouseDown = false;
		}
	}

	private void UpdatePoint(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			PointSlice(pos);
		}
	}

	private void UpdatePolygon(Vector2D pos) {
		mouseDown = true;

		if (Input.GetMouseButtonDown (0)) {
			PolygonSlice (pos);
		}
	}

	private void UpdateExplode(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			ExplodeInPoint(pos);
		}
	}

	private void UpdateExplodeByPoint(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			ExplodeByPoint(pos);
		}
	}

	private void UpdateCreate(Vector2D pos) {
		if (Input.GetMouseButtonDown (0)) {
			pointsList.Clear ();
			pointsList.Add (pos);
		}

		if (createType == CreateType.Slice) {
			if (Input.GetMouseButton (0)) {
				if (pointsList.Count == 0 || (Vector2D.Distance (pos, pointsList.Last ()) > minVertexDistance * visualScale)) {
					pointsList.Add (pos);
				}

				mouseDown = true;
			}

			if (mouseDown == true && Input.GetMouseButton (0) == false) {
				mouseDown = false;
				CreatorSlice (pointsList);
			}
		} else {
			mouseDown = true;
			if (Input.GetMouseButtonDown (0)) {
				PolygonCreator (pos);
			}
		}
	}

	private void LinearSliceJoints(Pair2D slice) {
		foreach(Joint2D joint in Joint2D.GetJointsConnected()) {
			Vector2 localPosA = joint.anchoredJoint2D.connectedAnchor;
			Vector2 worldPosA = joint.anchoredJoint2D.connectedBody.transform.TransformPoint(localPosA);
			Vector2 localPosB = joint.anchoredJoint2D.anchor;
			Vector2 worldPosB = joint.anchoredJoint2D.transform.TransformPoint(localPosB);

			switch (joint.jointType) {
				case Joint2D.Type.HingeJoint2D:
					worldPosA = joint.anchoredJoint2D.connectedBody.transform.position;
					break;
				default:
					break;
			}
			
			Pair2D jointLine = new Pair2D(worldPosA, worldPosB);

			if (Math2D.LineIntersectLine(slice, jointLine)) {
				Destroy(joint.anchoredJoint2D);
			}
		}
	}

	private void ComplexSliceJoints(List<Vector2D> slice) {
		foreach(Joint2D joint in Joint2D.GetJointsConnected()) {
			Vector2 localPosA = joint.anchoredJoint2D.connectedAnchor;
			Vector2 worldPosA = joint.anchoredJoint2D.connectedBody.transform.TransformPoint(localPosA);
			Vector2 localPosB = joint.anchoredJoint2D.anchor;
			Vector2 worldPosB = joint.anchoredJoint2D.transform.TransformPoint(localPosB);

			switch (joint.jointType) {
				case Joint2D.Type.HingeJoint2D:
					worldPosA = joint.anchoredJoint2D.connectedBody.transform.position;
					break;
				default:
					break;
			}

			Pair2D jointLine = new Pair2D(worldPosA, worldPosB);

			foreach(Pair2D pair in Pair2D.GetList(slice, false)) {
				if (Math2D.LineIntersectLine(pair, jointLine)) {
					Destroy(joint.anchoredJoint2D);
				}
			}
		}	
	}

	private void UpdateComplexTrail() {
		if (trailRenderer == null) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("Slicer2D: Trail Renderer is not attached to the controller");
			}
			return;
		}

		int pointsCount = trailRenderer.GetPositions(trailPositions);
		if (pointsCount < 1) {
			return;
		}

		Vector2D pos = new Vector2D(trailPositions[pointsCount - 1]);

		List<Slice2D> results = complexTrail.Update(pos, trailRenderer.time);

		if (addForce) {
			foreach (Slice2D id in results)  {
				AddForce.ComplexTrail(id, addForceAmount);
			}
		}
	}

	private void UpdateLinearTrail() {
		if (trailRenderer == null) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("Slicer2D: Trail Renderer is not attached to the controller");
			}
			return;
		}

		int pointsCount = trailRenderer.GetPositions(trailPositions);
		if (pointsCount < 1) {
			return;
		}

		Vector2D pos = new Vector2D(trailPositions[pointsCount - 1]);

		List<Slice2D> results = linearTrail.Update(pos, trailRenderer.time);
		if (addForce) {
			foreach (Slice2D id in results)  {
				AddForce.LinearTrail(id, addForceAmount);
			}
		}
	}

	private bool LinearSlice(Pair2D slice) {
		if (sliceJoints) {
			LinearSliceJoints(slice);
		}
		
		List<Slice2D> results = Slicer2D.LinearSliceAll (slice, sliceLayer);
		bool result = false;

		foreach (Slice2D id in results)  {
			if (id.gameObjects.Count > 0) {
				result = true;
			}

			if (sliceResultEvent != null) {
				sliceResultEvent(id);
			}
		}

		if (addForce == true) {
			foreach (Slice2D id in results)  {
				AddForce.LinearSlice(id, addForceAmount);
			}
		}

		return(result);
	}

	private bool ComplexSlice(List <Vector2D> slice) {
		if (sliceJoints) {
			ComplexSliceJoints(slice);
		}

		List<Slice2D> results = Slicer2D.ComplexSliceAll (slice, sliceLayer);
		bool result = false;

		foreach (Slice2D id in results) {
			if (id.gameObjects.Count > 0) {
				result = true;
			}

			if (sliceResultEvent != null) {
				sliceResultEvent(id);
			}
		}

		if (addForce == true) {
			foreach (Slice2D id in results)  {
				AddForce.ComplexSlice(id, addForceAmount);
			}
		}
		return(result);
	}

	private void PointSlice(Vector2D pos) {
		float rotation = 0;

		switch (sliceRotation) {	
			case SliceRotation.Random:
				rotation = UnityEngine.Random.Range (0, Mathf.PI * 2);
				break;

			case SliceRotation.Vertical:
				rotation = Mathf.PI / 2f;
				break;

			case SliceRotation.Horizontal:
				rotation = Mathf.PI;
				break;
		}

		List<Slice2D> results = Slicer2D.PointSliceAll (pos, rotation, sliceLayer);
		foreach (Slice2D id in results) {
			if (sliceResultEvent != null) {
				sliceResultEvent(id);
			}
		}
	}
		
	private void PolygonSlice(Vector2D pos) {
		Polygon2D.defaultCircleVerticesCount = edgeCount;

		Slicer2D.PolygonSliceAll(pos, Polygon2D.Create (polygonType, polygonSize * visualScale), polygonDestroy, sliceLayer);
	}

	private void ExplodeByPoint(Vector2D pos) {
		List<Slice2D> results =	Slicer2D.ExplodeByPointAll (pos, sliceLayer);

		foreach (Slice2D id in results) {
			if (sliceResultEvent != null) {
				sliceResultEvent(id);
			}
		}

		if (addForce == true) {
			foreach (Slice2D id in results) {
				AddForce.ExplodeByPoint(id, addForceAmount, pos);
			}
		}
	}

	private void ExplodeInPoint(Vector2D pos) {
		List<Slice2D> results =	Slicer2D.ExplodeInPointAll (pos, sliceLayer);

		foreach (Slice2D id in results) {
			if (sliceResultEvent != null) {
				sliceResultEvent(id);
			}
		}

		if (addForce == true) {
			foreach (Slice2D id in results) {
				AddForce.ExplodeInPoint(id, addForceAmount, pos);
			}
		}
	}

	private void CreatorSlice(List <Vector2D> slice) {
		Polygon2D newPolygon = Slicer2D.API.CreatorSlice (slice);
		if (newPolygon != null) {
			CreatePolygon (newPolygon);
		}
	}

	private void PolygonCreator(Vector2D pos) {
		Polygon2D.defaultCircleVerticesCount = edgeCount;
		Polygon2D newPolygon = Polygon2D.Create (polygonType, polygonSize).ToOffset (pos);
		CreatePolygon (newPolygon);
	}

	private void CreatePolygon(Polygon2D newPolygon) {
		GameObject newGameObject = new GameObject ();
		newGameObject.transform.parent = transform;
		newGameObject.transform.position = new Vector3(0, 0, zPosition + 0.01f);

		newGameObject.AddComponent<Rigidbody2D> ();
		newGameObject.AddComponent<ColliderLineRenderer2D> ().color = Color.black;

		Slicer2D smartSlicer = newGameObject.AddComponent<Slicer2D> ();
		smartSlicer.textureType = Slicer2D.TextureType.Mesh2D;
		smartSlicer.material = material;

		newPolygon.CreatePolygonCollider (newGameObject);
		newPolygon.CreateMesh (newGameObject, new Vector2 (1, 1), Vector2.zero, PolygonTriangulator2D.Triangulation.Advanced);
	}

	private bool InSlicerComponents(Vector2D point) {
		foreach(Slicer2D slicer in Slicer2D.GetList()) {
			Polygon2D poly = slicer.GetPolygon().ToWorldSpace(slicer.transform);
			if (poly.PointInPoly(point)) {
				return(true);
			}
		}
		return(false);
	}
	
	public class AddForce {

		static public void LinearSlice(Slice2D slice, float forceAmount) {
			float sliceRotation = (float)Vector2D.Atan2 (slice.slice[1], slice.slice[0]);
			foreach (GameObject gameObject in slice.gameObjects) {
				Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
				if (rigidBody2D) {
					foreach (Vector2D p in slice.collisions) {
						Vector2 force = new Vector2 (Mathf.Cos (sliceRotation) * forceAmount, Mathf.Sin (sliceRotation) * forceAmount);
						Physics2DHelper.AddForceAtPosition(rigidBody2D, force, p.ToVector2());
					}
				}
			}
		}

		static public void ComplexSlice(Slice2D slice, float forceAmount) {
			foreach (GameObject gameObject in slice.gameObjects) {
				Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
				if (rigidBody2D) {
					List<Pair2D> list = Pair2D.GetList (slice.collisions, false);
					float forceVal = 2.0f / list.Count;
					foreach (Pair2D p in list) {
						float sliceRotation = (float)Vector2D.Atan2 (p.B, p.A);
						Vector2 force = new Vector2 (Mathf.Cos (sliceRotation) * forceAmount, Mathf.Sin (sliceRotation) * forceAmount);
						Physics2DHelper.AddForceAtPosition(rigidBody2D, forceVal * force, (p.A + p.B).ToVector2() / 2f);
					}
				}
			}
		}

		static public void ExplodeByPoint(Slice2D slice, float forceAmount, Vector2D point) {
			foreach (GameObject gameObject in slice.gameObjects) {
				Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
				if (rigidBody2D) {
					float sliceRotation = (float)Vector2D.Atan2 (point, new Vector2D (gameObject.transform.position));
					Rect rect = Polygon2DList.CreateFromGameObject (gameObject)[0].GetBounds ();
					Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * forceAmount, Mathf.Sin (sliceRotation) * forceAmount), rect.center);
				}
			}
		}

		static public void ExplodeInPoint(Slice2D slice, float forceAmount, Vector2D point) {
			foreach (GameObject gameObject in slice.gameObjects) {
				Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
				if (rigidBody2D) {
					float sliceRotation = (float)Vector2D.Atan2 (point, new Vector2D (gameObject.transform.position));
					Rect rect = Polygon2DList.CreateFromGameObject (gameObject)[0].GetBounds ();
					Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * forceAmount, Mathf.Sin (sliceRotation) * forceAmount), rect.center);
				}
			}
		}

		static public void LinearTrail(Slice2D slice, float forceAmount) {
			foreach (GameObject gameObject in slice.gameObjects) {
				Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
				if (rigidBody2D) {
					float sliceRotation = (float)Vector2D.Atan2 (slice.slice[0], slice.slice[1]);
					foreach (Vector2D p in slice.collisions) {
						Vector2 force = new Vector2 (Mathf.Cos (sliceRotation) * forceAmount, Mathf.Sin (sliceRotation) * forceAmount);
						Physics2DHelper.AddForceAtPosition(rigidBody2D, force, p.ToVector2());
					}
				}
			}
		}

		static public void ComplexTrail(Slice2D slice, float forceAmount) {
			foreach (GameObject gameObject in slice.gameObjects) {
				Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
				if (rigidBody2D) {
					List<Pair2D> list = Pair2D.GetList (slice.collisions, false);
					float forceVal = 2.0f / list.Count;
					foreach (Pair2D p in list) {
						float sliceRotation = (float)Vector2D.Atan2 (p.B, p.A);
						Vector2 force = new Vector2 (Mathf.Cos (sliceRotation) * forceAmount, Mathf.Sin (sliceRotation) * forceAmount);
						Physics2DHelper.AddForceAtPosition(rigidBody2D, forceVal * force, (p.A + p.B).ToVector2() / 2f);
					}
				}
			}
		}
	}
}

/*

	private void ExplodeAll() {
		List<Slice2D> results =	Slicer2D.ExplodeAll (sliceLayer);
		if (addForce == true) {
			foreach (Slice2D id in results) {
				foreach (GameObject gameObject in id.gameObjects) {
					Rigidbody2D rigidBody2D = gameObject.GetComponent<Rigidbody2D> ();
					if (rigidBody2D) {
						float sliceRotation = (float)Vector2D.Atan2 (new Vector2D(0, 0), new Vector2D (gameObject.transform.position));
						Rect rect = Polygon2DList.CreateFromGameObject (gameObject)[0].GetBounds ();
						Physics2DHelper.AddForceAtPosition(rigidBody2D, new Vector2 (Mathf.Cos (sliceRotation) * addForceAmount / 10f, Mathf.Sin (sliceRotation) * addForceAmount / 10f), rect.center);
					}
				}
				if (sliceResultEvent != null) {
					sliceResultEvent(id);
				}
			}
		}
	}

 */