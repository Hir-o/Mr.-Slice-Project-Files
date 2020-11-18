using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Slicer2D))]
public class Slicer2DEditor : Editor {
	static bool foldout = true;

	override public void OnInspectorGUI() {
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();

		Slicer2D script = target as Slicer2D;

		script.textureType = (Slicer2D.TextureType)EditorGUILayout.EnumPopup ("Texture Type", script.textureType);
		script.colliderType = (Slicer2D.ColliderType)EditorGUILayout.EnumPopup ("Collider Type", script.colliderType);
		script.triangulation = (PolygonTriangulator2D.Triangulation)EditorGUILayout.EnumPopup ("Triangulation", script.triangulation);
		script.centerOfMass = (Slicer2D.CenterOfMass)EditorGUILayout.EnumPopup ("Center of Mass", script.centerOfMass);
		script.slicingLayer = (Slicing2DLayer)EditorGUILayout.EnumPopup ("Slicing Layer", script.slicingLayer);
		script.supportJoints = EditorGUILayout.Toggle("Support Joints", script.supportJoints);
		script.slicingLimit = EditorGUILayout.Toggle("Slicing Limit", script.slicingLimit);

		if (script.slicingLimit) {
			script.maxSlices = EditorGUILayout.IntSlider("Max Slices", script.maxSlices, 1, 10);
		}

		script.recalculateMass = EditorGUILayout.Toggle("Recalculate Mass", script.recalculateMass);

		script.anchors = EditorGUILayout.Toggle("Anchors", script.anchors);

		if (script.anchors) {
			SerializedProperty anchorList = serializedObject.FindProperty ("anchorsList");

			EditorGUILayout.PropertyField (anchorList, true);
		}

		if (EditorGUI.EndChangeCheck()) {
			serializedObject.ApplyModifiedProperties();
		}

		if (script.textureType == Slicer2D.TextureType.Mesh2D) {
			foldout = EditorGUILayout.Foldout(foldout, "Mesh2D" );
			if (foldout) {
				EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
				script.material = (Material)EditorGUILayout.ObjectField("Material", script.material, typeof(Material), true);
				script.materialScale = EditorGUILayout.Vector2Field("Material Scale", script.materialScale);
				script.materialOffset = EditorGUILayout.Vector2Field("Material Offset", script.materialOffset);
				EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
			}
		}

		if (script.textureType == Slicer2D.TextureType.Mesh3D) {
			foldout = EditorGUILayout.Foldout(foldout, "Mesh3D" );
			if (foldout) {
				script.material = (Material)EditorGUILayout.ObjectField("Material", script.material, typeof(Material), true);
			}
		}
	}
}