using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D {
	float speed = 0.25f;
	float rotation = 0;

	public VirtualTransform transform = new VirtualTransform();

	private static Material material = null;
	private static Mesh mesh = null;

	public void Draw() {
		Matrix4x4 matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

		Graphics.DrawMesh(GetMesh(), matrix, GetMaterial(), 0);
	}

	public bool Update () {
		speed *= (1f - 2f * Time.deltaTime);

		Vector2 vec = Vector2D.RotToVec(rotation * Mathf.Deg2Rad).ToVector2();
		transform.position += new Vector3(vec.x, vec.y, 0) * speed;

		transform.lossyScale.x *= (1f - 5f * Time.deltaTime);
		transform.lossyScale.y *= (1f - 5f * Time.deltaTime);

		if (transform.lossyScale.y < 0.05f) {
			return(false);
		} else {
			return(true);
		}
	}

	static public Particle2D Create(float rotation, Vector3 position) {
		Particle2D p = new Particle2D();
		p.speed = 0.025f;
		p.rotation = rotation;

		p.transform.lossyScale = new Vector3(Random.Range(5, 15), Random.Range(5, 15), 1);
		p.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
		p.transform.position = position;

		return(p);
	}

	static public Material GetMaterial() {
		if (material == null) {
			material = new Material (Shader.Find ("Mobile/Particles/Additive"));
			material.mainTexture = Resources.Load<Texture>("Sprites/Flare");
		}
		return(material);
	}

	static public Mesh GetMesh() {
		if (mesh == null) {
			List<Mesh2DTriangle> triangles = new List<Mesh2DTriangle>();
			triangles.Add(Max2DMesh.CreateBox(0.25f));
			mesh = Max2DMesh.ExportMesh(triangles); 
		}
		return(mesh);
	}
}
