using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowScript : MonoBehaviour {
	public Material material;
	private Mesh mesh;

	void Start () {
		Polygon2D polygon = Polygon2DList.CreateFromGameObject(gameObject)[0];
		polygon = polygon.ToOffset(new Vector2D(0.125f, -0.125f));
		mesh = polygon.CreateMesh(Vector2.zero, Vector2.zero);
		Update();
	}

	void Update() {
		Vector3 position = transform.position + new Vector3(0, 0, 1);
		Quaternion rotation = transform.rotation;
		Vector3 scale = transform.lossyScale;
		Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

		Graphics.DrawMesh(mesh, matrix, material, 0);
	}
}
