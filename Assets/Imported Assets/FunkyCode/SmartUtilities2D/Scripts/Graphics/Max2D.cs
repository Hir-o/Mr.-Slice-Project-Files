using System.Collections.Generic;
using UnityEngine;

public class Max2D {
	public enum LineMode {Smooth, Glow, Default};
	
	public static Material lineMaterial;
	public static Material lineNewMaterial;	
	public static Material lineLegacyMaterial; // Should Be Only In Legacy Class?

	public static Material defaultMaterial; 

	public static float lineWidth = 0.2f;
	public static Max2D.LineMode lineMode = Max2D.LineMode.Smooth;
	public static bool setBorder = false;
	public static Color setColor = Color.white;

	public static float setScale = 1f;

	static public void SetScale(float scale) {
		setScale = scale;
	}

	static public void SetBorder(bool border) {
		setBorder = border;
	}

	static public void SetLineMode(LineMode mode) {
		lineMode = mode;
	}

	public static void SetLineWidth (float size) {
		lineWidth = Mathf.Max(.01f, size / 5f);
	}

	public static void Check() {
		if (lineLegacyMaterial == null || lineNewMaterial == null) {
			lineNewMaterial = new Material (Shader.Find ("Legacy Shaders/Transparent/VertexLit"));
			lineNewMaterial.mainTexture = Resources.Load ("Textures/LineTexture16") as Texture;

			lineLegacyMaterial = new Material (Shader.Find ("Legacy Shaders/Transparent/VertexLit"));
			lineLegacyMaterial.mainTexture = Resources.Load ("Textures/LegacyLineTexture") as Texture;

			lineNewMaterial.SetInt("_ZWrite", 1);
			lineNewMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			lineNewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineNewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

			lineMaterial = lineNewMaterial;
		}
		if (defaultMaterial == null) {
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			defaultMaterial = new Material(shader);
			defaultMaterial.hideFlags = HideFlags.HideAndDontSave;
			defaultMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			defaultMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			defaultMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			defaultMaterial.SetInt("_ZWrite", 0);
		}
	}

	static public void SetColor(Color color) {
		Check ();
		lineMaterial.SetColor ("_Emission", color);
		setColor = color;
	}

	static public void DrawPolygonMesh(Polygon2D polygon) {
		Mesh mesh = polygon.CreateMesh(Vector2.zero, Vector2.zero);

		Max2D.DrawMesh(Max2D.defaultMaterial, mesh, new Vector2D(0, 0), 0);
	}

	static public void DrawMesh(Material material, Mesh mesh, Vector2D offset, float z = 0f) {
		if (mesh == null) {
			return;
		}
		
		GL.PushMatrix ();
		material.SetPass(0);
		GL.Begin(GL.TRIANGLES);

		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < mesh.triangles.GetLength (0); i++ ) {
			list.Add (mesh.vertices [mesh.triangles [i]]);
			if (list.Count > 2) {
				Max2DMatrix.DrawTriangle (list [0].x, list [0].y, list [1].x, list [1].y, list [2].x, list [2].y, offset, z);
				list.Clear ();
			}
		}

		GL.End ();
		GL.PopMatrix ();
	}

	static public void DrawMesh(Material material, Mesh mesh, Transform transform, Vector2D offset, float z = 0f) {
		if (mesh == null) {
			return;
		}
		
		GL.PushMatrix ();
		material.SetPass(0);
		GL.Begin(GL.TRIANGLES);

		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < mesh.triangles.GetLength (0); i++ ) {
			list.Add (transform.TransformPoint(mesh.vertices [mesh.triangles [i]]));
			if (list.Count > 2) {
				Max2DMatrix.DrawTriangle (list [0].x, list [0].y, list [1].x, list [1].y, list [2].x, list [2].y, offset, z);
				list.Clear ();
			}
		}

		GL.End ();
		GL.PopMatrix ();
	}

	static public void DrawSquare(Vector2D p, float size, float z = 0f) {
		Vector2D p0 = new Vector2D (p.x - size, p.y - size);
		Vector2D p1 = new Vector2D (p.x + size, p.y - size);
		Vector2D p2 = new Vector2D (p.x + size, p.y + size);
		Vector2D p3 = new Vector2D (p.x - size, p.y + size);

		DrawTriangle (p0, p1, p2, new Vector2D(0, 0), z);
		DrawTriangle (p2, p3, p0, new Vector2D(0, 0), z);
	}

	static public void DrawImage(Material material, Vector2 pos, Vector2 size, float rot, float z = 0f) {
        GL.PushMatrix ();
        material.SetPass (0); 
		
		rot = rot * Mathf.Deg2Rad + Mathf.PI;

		float rectAngle = Mathf.Atan2(size.y, size.x);
		float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

		Vector2 v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
		Vector2 v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
		Vector2 v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
		Vector2 v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
		
        GL.Begin (GL.QUADS);
        GL.TexCoord2 (0, 0);
        GL.Vertex3 (v1.x, v1.y, z);
        GL.TexCoord2 (0, 1);
        GL.Vertex3 (v2.x, v2.y, z);
        GL.TexCoord2 (1, 1);
        GL.Vertex3 (v3.x, v3.y, z);
        GL.TexCoord2 (1, 0);
        GL.Vertex3 (v4.x, v4.y, z);
        GL.End ();

        GL.PopMatrix ();
    }

	static public void DrawImage(Transform transform, Material material, Vector2D pos, Vector2D size, float z = 0f) {
		GL.PushMatrix ();
		GL.MultMatrix(transform.localToWorldMatrix);

		material.SetPass(0);
		GL.Begin (GL.QUADS);

		GL.TexCoord2 (0, 0);
		GL.Vertex3 ((float)pos.x - (float)size.x, (float)pos.y - (float)size.y, z);
		GL.TexCoord2 (0, 1);
		GL.Vertex3 ((float)pos.x - (float)size.x, (float)pos.y + (float)size.y, z);
		GL.TexCoord2 (1, 1);
		GL.Vertex3 ((float)pos.x + (float)size.x, (float)pos.y + (float)size.y, z);
		GL.TexCoord2 (1, 0);
		GL.Vertex3 ((float)pos.x + (float)size.x, (float)pos.y - (float)size.y, z);

		GL.End ();
		GL.PopMatrix ();
	}

	static public void DrawImage(Material material, Vector2D pos, Vector2D size, float z = 0f) {
		GL.PushMatrix ();
		material.SetPass(0);
		GL.Begin (GL.QUADS);

		GL.TexCoord2 (0, 0);
		GL.Vertex3 ((float)pos.x - (float)size.x, (float)pos.y - (float)size.y, z);
		GL.TexCoord2 (0, 1);
		GL.Vertex3 ((float)pos.x - (float)size.x, (float)pos.y + (float)size.y, z);
		GL.TexCoord2 (1, 1);
		GL.Vertex3 ((float)pos.x + (float)size.x, (float)pos.y + (float)size.y, z);
		GL.TexCoord2 (1, 0);
		GL.Vertex3 ((float)pos.x + (float)size.x, (float)pos.y - (float)size.y, z);

		GL.End ();
		GL.PopMatrix ();
	}
		
	static public void DrawTriangle(Vector2D p0, Vector2D p1, Vector2D p2, Vector2D offset, float z = 0f) {
		DrawTrianglef ((float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y, (float)p2.x, (float)p2.y, offset, z);
	}

	static public void DrawTrianglef(float x0, float y0, float x1, float y1, float x2, float y2, Vector2D offset, float z = 0f) {
		GL.PushMatrix();
		defaultMaterial.SetPass(0);
		GL.Begin(GL.TRIANGLES);
		GL.Color(setColor);

		Max2DMatrix.DrawTriangle(x0, y0, x1, y1, x2, y2, offset, z);

		GL.End();
		GL.PopMatrix();
	}











	// Destruction 2D
	static public void DrawStrippedLine(List<Vector2D> pointsList, float minVertsDistance, float z = 0f, bool full = false, Vector2D offset = null)
	{
		if (offset == null)
			offset = new Vector2D (0, 0);

		Vector2D vA = null, vB = null;

		if (setBorder == true) {
			Color tmcColor = setColor;
			float tmpWidth = lineWidth;

			GL.PushMatrix();
			SetColor (Color.black);
			lineMaterial.SetPass(0);
			GL.Begin(GL.QUADS);

			lineWidth = 2f * tmpWidth;

			foreach (Pair2D id in Pair2D.GetList(pointsList, full)) {
				vA = new Vector2D (id.A + offset);
				vB = new Vector2D (id.B + offset);

				vA.Push (Vector2D.Atan2 (id.A, id.B), -minVertsDistance / 5 * setScale);
				vB.Push (Vector2D.Atan2 (id.A, id.B), minVertsDistance / 5 * setScale);

				Max2DMatrix.DrawLineImage (new Pair2D(vA, vB), z);
			}

			GL.End();
			GL.PopMatrix();

			SetColor (tmcColor);
			lineWidth = tmpWidth;
		}

		GL.PushMatrix();
		lineMaterial.SetPass(0);
		GL.Begin(GL.QUADS);

		foreach (Pair2D id in Pair2D.GetList(pointsList, full)) {
			vA = new Vector2D (id.A + offset);
			vB = new Vector2D (id.B + offset);

			vA.Push (Vector2D.Atan2 (id.A, id.B), -minVertsDistance / 4 * setScale);
			vB.Push (Vector2D.Atan2 (id.A, id.B), minVertsDistance / 4 * setScale);

			Max2DMatrix.DrawLineImage (new Pair2D(vA, vB), z);
		}

		GL.End();
		GL.PopMatrix();
	}

	static public void DrawImage(Transform transform, Material material, Vector2 pos, Vector2 size, float rot, float z = 0f, Vector2D scale = null)
    {
		if (scale == null) {
			scale = new Vector2D(1, 1);
		}

        GL.PushMatrix ();
		GL.MultMatrix(transform.localToWorldMatrix);
        material.SetPass (0); 
		
		rot = rot * Mathf.Deg2Rad + Mathf.PI;

		float rectAngle = Mathf.Atan2(size.y, size.x);
		float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);
		float distX = dist * (float)scale.x;
		float distY = dist * (float)scale.y;

		pos.x *= (float)scale.x;
		pos.y *= (float)scale.y;

		Vector2 v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * distX, pos.y + Mathf.Sin(rectAngle + rot) * distY);
		Vector2 v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * distX, pos.y + Mathf.Sin(-rectAngle + rot) * distY);
		Vector2 v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * distX, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * distY);
		Vector2 v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * distX, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * distY);
		
        GL.Begin (GL.QUADS);
        GL.TexCoord2 (0, 0);
        GL.Vertex3 (v1.x, v1.y, z);
        GL.TexCoord2 (0, 1);
        GL.Vertex3 (v2.x, v2.y, z);
        GL.TexCoord2 (1, 1);
        GL.Vertex3 (v3.x, v3.y, z);
        GL.TexCoord2 (1, 0);
        GL.Vertex3 (v4.x, v4.y, z);
        GL.End ();

        GL.PopMatrix ();
    }

	static public void DrawMesh(Material material, Mesh mesh, Transform transform, Vector2D offset, float z = 0f, Vector2D scale = null)
	{
		if (mesh == null)
			return;

		if (scale == null) {
			scale = new Vector2D(1, 1);
		}
		
		GL.PushMatrix ();
		material.SetPass (0); 
		GL.Begin(GL.TRIANGLES);

		for (int i = 0; i < mesh.triangles.GetLength (0); i = i + 3 ) {
			Vector2 vA = mesh.vertices [mesh.triangles [i]];
			Vector2 vB = mesh.vertices [mesh.triangles [i + 1]];
			Vector2 vC = mesh.vertices [mesh.triangles [i + 2]];

			vA.x *= (float)scale.x;
			vA.y *= (float)scale.y;
			vB.x *= (float)scale.x;
			vB.y *= (float)scale.y;
			vC.x *= (float)scale.x;
			vC.y *= (float)scale.y;

			Vector2 pA = transform.TransformPoint(vA);
			Vector2 pB = transform.TransformPoint(vB);
			Vector2 pC = transform.TransformPoint(vC);
			
			Max2DMatrix.DrawTriangle (pA, pB, pC, offset, z);
		}

		GL.End ();
		GL.PopMatrix ();
	}






	// Lighting 2D
	static public void Vertex3(Vector2D p, float z) {
        GL.Vertex3((float)p.x, (float)p.y, z);
	}
	
    static public void iDrawImage2(Material material, Vector2 pos, Vector2 size, float rot, float z = 0f)
    {
        GL.PushMatrix ();
        material.SetPass (0); 
        
        rot = rot * Mathf.Deg2Rad + Mathf.PI;

        float rectAngle = Mathf.Atan2(size.y, size.x);
        float dist = Mathf.Sqrt(size.x * size.x + size.y * size.y);

        Vector2 v1 = new Vector2(pos.x + Mathf.Cos(rectAngle + rot) * dist, pos.y + Mathf.Sin(rectAngle + rot) * dist);
        Vector2 v2 = new Vector2(pos.x + Mathf.Cos(-rectAngle + rot) * dist, pos.y + Mathf.Sin(-rectAngle + rot) * dist);
        Vector2 v3 = new Vector2(pos.x + Mathf.Cos(rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(rectAngle + Mathf.PI + rot) * dist);
        Vector2 v4 = new Vector2(pos.x + Mathf.Cos(-rectAngle + Mathf.PI + rot) * dist, pos.y + Mathf.Sin(-rectAngle + Mathf.PI + rot) * dist);
        
        GL.Begin (GL.QUADS);
        GL.TexCoord2 (0, 0);
        GL.Vertex3 (v1.x, v1.y, z);
        GL.TexCoord2 (0, 1);
        GL.Vertex3 (v2.x, v2.y, z);
        GL.TexCoord2 (1, 1);
        GL.Vertex3 (v3.x, v3.y, z);
        GL.TexCoord2 (1, 0);
        GL.Vertex3 (v4.x, v4.y, z);
        GL.End ();

        GL.PopMatrix ();
    }

	
    static public void iDrawMesh(Mesh mesh, Transform transform, Vector2D offset, float z)
    {
        if (mesh == null)
            return;
    
        GL.PushMatrix();
        defaultMaterial.SetPass(0);
        GL.Begin(GL.TRIANGLES);
        GL.Color(setColor);

        List<Vector2> list = new List<Vector2>();
        for (int i = 0; i <  mesh.triangles.GetLength (0); i++ ) {
            list.Add (transform.TransformPoint(mesh.vertices [mesh.triangles [i]]));
            if (list.Count > 2) {
                Max2DMatrix.DrawTriangle(list [0].x, list [0].y, list [1].x, list [1].y, list [2].x, list [2].y, offset, z);

                list.Clear ();
            }
        }

        GL.End();
        GL.PopMatrix();    
    }

	static public void iDrawImage(Material material, Vector2D pos, Vector2D size, float z = 0f) {
        GL.PushMatrix ();
        material.SetPass (0); 

        GL.Begin (GL.QUADS);
        GL.TexCoord2 (0, 0);
        GL.Vertex3 ((float)(pos.x - size.x),(float)( pos.y - size.y), z);
        GL.TexCoord2 (0, 1);
        GL.Vertex3 ((float)(pos.x - size.x),(float)( pos.y + size.y), z);
        GL.TexCoord2 (1, 1);
        GL.Vertex3 ((float)(pos.x + size.x), (float)(pos.y + size.y), z);
        GL.TexCoord2 (1, 0);
        GL.Vertex3 ((float)(pos.x + size.x), (float)(pos.y - size.y), z);
        GL.End ();

        GL.PopMatrix ();
    }
}