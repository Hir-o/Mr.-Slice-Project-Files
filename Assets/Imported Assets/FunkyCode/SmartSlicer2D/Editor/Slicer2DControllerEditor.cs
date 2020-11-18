using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Slicer2DController))]
public class Slicer2DControllerEditor : Editor{
	static bool visualsFoldout = true;
	static bool foldout = true;

	override public void OnInspectorGUI()
	{
		Slicer2DController script = target as Slicer2DController;
		script.sliceType = (Slicer2DController.SliceType)EditorGUILayout.EnumPopup ("Slicer Type", script.sliceType);
		script.sliceLayer.SetLayerType((Slice2DLayerType)EditorGUILayout.EnumPopup ("Slicer Layer", script.sliceLayer.GetLayerType()));

		EditorGUI.indentLevel = EditorGUI.indentLevel + 2;

		if (script.sliceLayer.GetLayerType() == Slice2DLayerType.Selected) {
			for (int i = 0; i < 8; i++) {
				script.sliceLayer.SetLayer(i, EditorGUILayout.Toggle ("Layer " + (i + 1), script.sliceLayer.GetLayerState(i)));
			}
		}

		EditorGUI.indentLevel = EditorGUI.indentLevel - 2;

		visualsFoldout = EditorGUILayout.Foldout(visualsFoldout, "Visuals" );
		if (visualsFoldout) {
			EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
			script.drawSlicer = EditorGUILayout.Toggle ("Enable Visuals", script.drawSlicer);

			if (script.drawSlicer == true) {
				script.lineType = (Max2DMesh.LineType)EditorGUILayout.EnumPopup ("Line Type", script.lineType);
				script.zPosition = EditorGUILayout.FloatField ("Slicer Z", script.zPosition);
				script.slicerColor = (Color)EditorGUILayout.ColorField ("Slicer Color", script.slicerColor);
				script.visualScale = EditorGUILayout.Slider("Slicer Scale", script.visualScale, 1f, 50f);

				if (script.lineType == Max2DMesh.LineType.Default) {
					script.lineBorder = EditorGUILayout.Toggle ("Border", script.lineBorder);

					if (script.lineBorder == true) { // Disable?
						script.borderScale = EditorGUILayout.Slider("Border Scale", script.borderScale, 1f, 5f);
					}
				}

				script.lineWidth = EditorGUILayout.Slider("Line Width", script.lineWidth, 0.05f, 5f);

				script.lineEndWidth = EditorGUILayout.Slider("Line End Width", script.lineEndWidth, 0.05f, 5f);
				script.lineEndSize = EditorGUILayout.Slider("Line End Size", script.lineEndSize, 0.05f, 5f);
				
				script.minVertexDistance = EditorGUILayout.Slider("Min Vertex Distance", script.minVertexDistance, 0.05f, 5f);
			
				script.vertexSpace = EditorGUILayout.Slider("Vertex Space", script.vertexSpace, 0f, 1f);

				if (script.lineWidth < 0.01f) {
					script.lineWidth = 0.01f;
				}
				
				if (script.lineEndSize < 0.05f) {
					script.lineEndSize = 0.05f;
				}

				if (script.minVertexDistance < 0.05f) {
					script.minVertexDistance = 0.05f;
				}
			}
			
			EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
		}
			
		SliceTypesUpdate (script);
	}

	void SliceTypesUpdate(Slicer2DController script) {
		switch (script.sliceType) {

			case Slicer2DController.SliceType.Linear:
				foldout = EditorGUILayout.Foldout(foldout, "Linear Slicer" );
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.addForce = EditorGUILayout.Toggle ("Add Force", script.addForce);
					if (script.addForce) {
						script.addForceAmount = EditorGUILayout.FloatField ("Force Amount", script.addForceAmount);
					}

					script.startSliceIfPossible = EditorGUILayout.Toggle ("Start Slice If Possible", script.startSliceIfPossible);
					script.endSliceIfPossible = EditorGUILayout.Toggle ("End Slice If Possible", script.endSliceIfPossible);

					script.strippedLinear = EditorGUILayout.Toggle ("Stripped", script.strippedLinear);

					script.sliceJoints = EditorGUILayout.Toggle ("Slice Joints", script.sliceJoints);

					script.displayCollisions = EditorGUILayout.Toggle ("Display Collisions", script.displayCollisions);
					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.LinearCut:
				foldout = EditorGUILayout.Foldout(foldout, "Linear Cut Slicer" );
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.complexSliceType = (Slicer2D.SliceType)EditorGUILayout.EnumPopup ("Slice Mode", script.complexSliceType);
					
					script.cutSize = EditorGUILayout.FloatField ("Linear Cut Size", script.cutSize);
					if (script.cutSize < 0.01f) {
						script.cutSize = 0.01f;
					}

					script.sliceJoints = EditorGUILayout.Toggle ("Slice Joints", script.sliceJoints);

					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

				case Slicer2DController.SliceType.ComplexCut:
				foldout = EditorGUILayout.Foldout(foldout, "Complex Cut Slicer" );
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.complexSliceType = (Slicer2D.SliceType)EditorGUILayout.EnumPopup ("Slice Mode", script.complexSliceType);
					
					script.cutSize = EditorGUILayout.FloatField ("Complex Cut Size", script.cutSize);
					if (script.cutSize < 0.01f) {
						script.cutSize = 0.01f;
					}

					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;


			case Slicer2DController.SliceType.Complex:
				foldout = EditorGUILayout.Foldout (foldout, "Complex Slicer");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
					script.complexSliceType = (Slicer2D.SliceType)EditorGUILayout.EnumPopup ("Slice Mode", script.complexSliceType);
					
					script.addForce = EditorGUILayout.Toggle ("Add Force", script.addForce);
					if (script.addForce) {
						script.addForceAmount = EditorGUILayout.FloatField ("Force Amount", script.addForceAmount);
					}

					script.startSliceIfPossible = EditorGUILayout.Toggle ("Start Slice If Possible", script.startSliceIfPossible);
					script.endSliceIfPossible = EditorGUILayout.Toggle ("End Slice If Possible", script.endSliceIfPossible);
					
					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.ComplexClick:
				foldout = EditorGUILayout.Foldout (foldout, "Complex Click");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.complexSliceType = (Slicer2D.SliceType)EditorGUILayout.EnumPopup ("Slice Mode", script.complexSliceType);

					script.pointsLimit = EditorGUILayout.IntField ("Points Limit", script.pointsLimit);
				
					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.Point:
				foldout = EditorGUILayout.Foldout (foldout, "Point Slicer");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
					script.sliceRotation = (Slicer2DController.SliceRotation)EditorGUILayout.EnumPopup ("Slice Rotation", script.sliceRotation);
					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.Polygon:
				foldout = EditorGUILayout.Foldout(foldout, "Polygon Slicer");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.complexSliceType = (Slicer2D.SliceType)EditorGUILayout.EnumPopup ("Slice Mode", script.complexSliceType);

					script.polygonType = (Polygon2D.PolygonType)EditorGUILayout.EnumPopup ("Type", script.polygonType);
					script.polygonSize = EditorGUILayout.FloatField ("Size", script.polygonSize);
					script.polygonDestroy = EditorGUILayout.Toggle ("Destroy Result", script.polygonDestroy);

					if (script.polygonType == Polygon2D.PolygonType.Circle) {
						script.edgeCount = (int)EditorGUILayout.Slider("Edge Count", script.edgeCount, 3, 100);
					}

					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;
			
			case Slicer2DController.SliceType.ComplexTrail:
			case Slicer2DController.SliceType.LinearTrail:
				foldout = EditorGUILayout.Foldout (foldout, "Trail Slicer");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.trailRenderer = (TrailRenderer)EditorGUILayout.ObjectField ("Trail Renderer", script.trailRenderer, typeof(TrailRenderer), true);

					script.addForce = EditorGUILayout.Toggle ("Add Force", script.addForce);
					if (script.addForce) {
						script.addForceAmount = EditorGUILayout.FloatField ("Force Amount", script.addForceAmount);
					}

					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.ComplexTracked:
				foldout = EditorGUILayout.Foldout (foldout, "Tracked Slicer");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					//???
					script.complexSliceType = (Slicer2D.SliceType)EditorGUILayout.EnumPopup ("Slice Mode", script.complexSliceType);

					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.ExplodeByPoint:
				foldout = EditorGUILayout.Foldout (foldout, "Explode By Point");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.addForce = EditorGUILayout.Toggle ("Add Force", script.addForce);
					if (script.addForce) {
						script.addForceAmount = EditorGUILayout.FloatField ("Force Amount", script.addForceAmount);
					}

					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.Explode:
				foldout = EditorGUILayout.Foldout(foldout, "Explosion");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
					script.addForce = EditorGUILayout.Toggle ("Add Force", script.addForce);
					if (script.addForce)
						script.addForceAmount = EditorGUILayout.FloatField ("Force Amount", script.addForceAmount);
					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;

			case Slicer2DController.SliceType.Create:
				foldout = EditorGUILayout.Foldout (foldout, "Polygon Creator");
				if (foldout) {
					EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

					script.createType = (Slicer2DController.CreateType)EditorGUILayout.EnumPopup ("Creation Type", script.createType);
					if (script.createType == Slicer2DController.CreateType.PolygonType) {
						script.polygonType = (Polygon2D.PolygonType)EditorGUILayout.EnumPopup ("Type", script.polygonType);
						script.polygonSize = EditorGUILayout.FloatField ("Size", script.polygonSize);
						
						if (script.polygonType == Polygon2D.PolygonType.Circle) {
							script.edgeCount = (int)EditorGUILayout.Slider("Edge Count", script.edgeCount, 3, 100);
						}
					}

					script.material = (Material)EditorGUILayout.ObjectField ("Material", script.material, typeof(Material), true);

					EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
				}
				break;
		}
	}
}