using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Max2DLegacy {

	static public void DrawLine(Vector2D p0, Vector2D p1, float z = 0f) {
		if (Max2D.setBorder == true) {
			Color tmcColor = Max2D.setColor;
			float tmpWidth = Max2D.lineWidth;
			Max2D.SetColor(Color.black);
			Max2D.lineWidth = tmpWidth * 2f;
			DrawLinef ((float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y, z);
			Max2D.SetColor(tmcColor);
			Max2D.lineWidth = tmpWidth;
			DrawLinef ((float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y, z);
			Max2D.lineWidth = tmpWidth;
		} else {
			DrawLinef((float)p0.x, (float)p0.y, (float)p1.x, (float)p1.y, z);
		}
	}
		
	static public void DrawLinef(float x0, float y0, float x1, float y1, float z = 0f) {
		Max2D.Check ();

		if (Max2D.lineMode == Max2D.LineMode.Smooth)
			DrawSmoothLine (new Pair2D (new Vector2D (x0, y0), new Vector2D (x1, y1)), z);
		else {
			GL.PushMatrix();
			Max2D.defaultMaterial.SetPass(0);
			GL.Begin(GL.LINES);
			GL.Color(Max2D.setColor);

			Max2DMatrixLegacy.DrawLine (x0, y0, x1, y1, z);

			GL.End();
			GL.PopMatrix();
		}
	}
	static public void DrawLineSquare(Vector2D p, float size, float z = 0f) {
		float width = Max2D.lineWidth;
		Max2D.lineWidth = Max2D.lineWidth * 0.5f;
		DrawLineRectf ((float)p.x - size / 2f, (float)p.y - size / 2f, size, size, z);
		Max2D.lineWidth = width;
	}
	static public void DrawLineRectf(float x, float y, float w, float h, float z = 0f) {
		if (Max2D.lineMode == Max2D.LineMode.Smooth) {
			GL.PushMatrix ();
			Max2D.lineMaterial.SetPass(0);
			GL.Begin (GL.QUADS);

			if (Max2D.setBorder == true) {
				Color tmcColor = Max2D.setColor;
				float tmpWidth = Max2D.lineWidth;

				Max2D.SetColor (Color.black);
				Max2D.lineWidth = tmpWidth * 2f;

				Max2DMatrixLegacy.DrawLineImage (new Pair2D (new Vector2D (x, y), new Vector2D (x + w, y)), z);
				Max2DMatrixLegacy.DrawLineImage (new Pair2D (new Vector2D (x, y), new Vector2D (x, y + h)), z);
				Max2DMatrixLegacy.DrawLineImage (new Pair2D (new Vector2D (x + w, y), new Vector2D (x + w, y + h)), z);
				Max2DMatrixLegacy.DrawLineImage (new Pair2D (new Vector2D (x, y + h), new Vector2D (x + w, y + h)), z);

				Max2D.SetColor (tmcColor);
				Max2D.lineWidth = tmpWidth;
			}

			float tmpLine = Max2D.lineWidth;
			Max2D.lineWidth = tmpLine * 1f;

			Max2D.SetColor (Max2D.setColor);

			Max2DMatrixLegacy.DrawLineImage (new Pair2D(new Vector2D(x, y), new Vector2D(x + w, y)), z);
			Max2DMatrixLegacy.DrawLineImage (new Pair2D(new Vector2D(x, y), new Vector2D(x, y + h)), z);
			Max2DMatrixLegacy.DrawLineImage (new Pair2D(new Vector2D(x + w, y), new Vector2D(x + w, y+ h)), z);
			Max2DMatrixLegacy.DrawLineImage (new Pair2D(new Vector2D(x, y + h), new Vector2D(x + w, y+ h)), z);

			GL.End();
			GL.PopMatrix();

			Max2D.lineWidth = tmpLine;

		} else {
			DrawLine (new Vector2D (x, y), new Vector2D (x + w, y), z);
			DrawLine (new Vector2D (x + w, y), new Vector2D (x + w, y + h), z);
			DrawLine (new Vector2D (x + w, y + h),	new Vector2D (x, y + h), z);
			DrawLine (new Vector2D (x, y + h), new Vector2D (x, y), z);
		}
	}
	static public void DrawPolygon(Transform transform, Polygon2D poly, float z = 0f, bool connect = true) {
		Max2D.Check ();

		switch (Max2D.lineMode) {
			case Max2D.LineMode.Smooth:
				GL.PushMatrix ();
				GL.MultMatrix(transform.localToWorldMatrix);

				Max2D.lineMaterial.SetPass(0);
				GL.Begin(GL.QUADS);

				Max2DMatrixLegacy.DrawSliceImage (transform, poly.pointsList, z, connect);

				GL.End();
				GL.PopMatrix();

				break;

			case Max2D.LineMode.Default:
				GL.PushMatrix();
				GL.MultMatrix(transform.localToWorldMatrix);

				Max2D.defaultMaterial.SetPass(0);
				GL.Begin(GL.LINES);
				GL.Color(Max2D.setColor);

				Max2DMatrixLegacy.DrawSlice(poly.pointsList, z, connect);

				GL.End ();
				GL.PopMatrix();
			
				break;
		}

		foreach (Polygon2D p in poly.holesList) {
			DrawPolygon (transform, p, z);
		}
	}
		static public void DrawSlice(List< Vector2D> slice, float z = 0f) {
		foreach (Pair2D p in Pair2D.GetList(slice, false)) {
			DrawLine (p.A, p.B, z);
		}
	}

	static public void DrawPolygonList(List<Polygon2D> polyList, float z = 0f) {
		foreach (Polygon2D p in polyList) {
			DrawPolygon (p, z);
		}
	}
	static public void DrawStrippedLine(List<Vector2D> pointsList, float minVertexDistance, float z = 0f, bool full = false, Vector2D offset = null) {
		if (offset == null) {
			offset = new Vector2D (0, 0);
		}

		Vector2D vA = null, vB = null;

		if (Max2D.setBorder == true) {
			Color tmcColor = Max2D.setColor;
			float tmpWidth = Max2D.lineWidth;

			GL.PushMatrix();
			Max2D.SetColor (Color.black);
			Max2D.lineMaterial.SetPass(0);
			GL.Begin(GL.QUADS);

			Max2D.lineWidth = 2f * tmpWidth;

			foreach (Pair2D id in Pair2D.GetList(pointsList, full)) {
				vA = new Vector2D (id.A + offset);
				vB = new Vector2D (id.B + offset);

				vA.Push (Vector2D.Atan2 (id.A, id.B), -minVertexDistance / 5 * Max2D.setScale);
				vB.Push (Vector2D.Atan2 (id.A, id.B), minVertexDistance / 5 * Max2D.setScale);

				Max2DMatrixLegacy.DrawLineImage (new Pair2D(vA, vB), z);
			}

			GL.End();
			GL.PopMatrix();

			Max2D.SetColor (tmcColor);
			Max2D.lineWidth = tmpWidth;
		}

		GL.PushMatrix();
		Max2D.lineMaterial.SetPass(0);
		GL.Begin(GL.QUADS);

		foreach (Pair2D id in Pair2D.GetList(pointsList, full)) {
			vA = new Vector2D (id.A + offset);
			vB = new Vector2D (id.B + offset);

			vA.Push (Vector2D.Atan2 (id.A, id.B), -minVertexDistance / 4 * Max2D.setScale);
			vB.Push (Vector2D.Atan2 (id.A, id.B), minVertexDistance / 4 * Max2D.setScale);

			Max2DMatrixLegacy.DrawLineImage (new Pair2D(vA, vB), z);
		}

		GL.End();
		GL.PopMatrix();
	}

	static public void DrawSmoothLine(Pair2D pair, float z = 0f) {
		GL.PushMatrix();
		Max2D.lineMaterial.SetPass(0);
		GL.Begin(GL.QUADS);

		Max2DMatrixLegacy.DrawLineImage (pair, z);

		GL.End();
		GL.PopMatrix();
	}

	static public void DrawPolygon(Polygon2D poly, float z = 0f, bool connect = true) {
		Max2D.Check ();

		switch (Max2D.lineMode) {
			case Max2D.LineMode.Smooth:
				GL.PushMatrix ();
				Max2D.lineMaterial.SetPass(0);
				GL.Begin(GL.QUADS);

				Max2DMatrixLegacy.DrawSliceImage (poly.pointsList, z, connect);

				GL.End();
				GL.PopMatrix();

				break;

			case Max2D.LineMode.Default:
				GL.PushMatrix();
				Max2D.defaultMaterial.SetPass(0);
				GL.Begin(GL.LINES);
				GL.Color(Max2D.setColor);

				Max2DMatrixLegacy.DrawSlice(poly.pointsList, z, connect);

				GL.End ();
				GL.PopMatrix();
			
				break;
		}

		foreach (Polygon2D p in poly.holesList) {
			DrawPolygon (p, z);
		}
	}
}
