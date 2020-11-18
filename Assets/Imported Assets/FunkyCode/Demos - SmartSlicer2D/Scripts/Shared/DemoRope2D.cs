using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DemoRope2D : MonoBehaviour {
	public GameObject anchorBody;
	public GameObject connectedBody;

	public Color color;

	private List<HingeJoint2D> nodes = new List<HingeJoint2D> ();

	public Material material;
	public Texture texture;

	public float width = .5f;

	void Start () {
		Vector2D position = new Vector2D(anchorBody.transform.position);

		material = new Material (Shader.Find ("Sprites/Default"));
		texture = Resources.Load ("Sprites/Rope/SmallRope") as Texture;
		material.mainTexture = texture;

		GameObject prev = anchorBody;

		int ropeId = 1;
		GameObject gObject = null;
		float distance = 1.5f;
		while (Vector2D.Distance (position, new Vector2D(connectedBody.transform.position)) > distance) {
			double direction = Vector2D.Atan2 (new Vector2D(connectedBody.transform.position), position);

			gObject = new GameObject();

			HingeJoint2D hingeJoint = gObject.AddComponent<HingeJoint2D>();
			hingeJoint.connectedBody = prev.GetComponent<Rigidbody2D>();

			gObject.transform.parent = transform;
			gObject.transform.position = position.ToVector2();
			gObject.name = "Rope " + ropeId;
			
			//gObject.AddComponent<JointRenderer2D> ().color = color;
			gObject.AddComponent<CircleCollider2D> ().radius = .25f;

			nodes.Add(hingeJoint);

			position.Push (direction, distance);

			prev = gObject;
			ropeId++;
		}

		if (gameObject != null) {
			HingeJoint2D joint = gObject.AddComponent<HingeJoint2D>();
			joint.connectedBody = connectedBody.GetComponent<Rigidbody2D>();
	
			nodes.Add(joint);
		}

	}


	public void OnRenderObject() {
		if (Camera.current != Camera.main) {
			return;
		}

		Max2D.SetLineWidth (0.5f);
		Max2D.SetColor (Color.white);
		Max2D.SetBorder (false);
		Max2D.SetLineMode(Max2D.LineMode.Default);

		GL.PushMatrix ();
		material.SetPass(0);
		GL.Begin(GL.QUADS);

		float lineOffset = -0.001f;
		float z = transform.position.z + lineOffset;

		Pair2D prevPair = null;

		foreach(HingeJoint2D joint in nodes) {
			if (joint == null) {
				continue;
			}
		
			Pair2D pairA = null;
			Pair2D pairB = null;

			if (joint == nodes.Last()) {
				pairA = GetPair(joint.connectedBody.transform.position,joint.transform.position);

				if (prevPair != null) {
					pairB = prevPair;
				} else {
					pairB = GetPair(joint.transform.position, joint.connectedBody.transform.position);
				}

			} else {
				pairA = GetPair(joint.transform.position, joint.connectedBody.transform.position);

				if (prevPair != null) {
					pairB = prevPair;
				} else {
					pairB = GetPair(joint.transform.position , joint.connectedBody.transform.position);
				}
			}


		

			GL.TexCoord2(0, 0);
			GL.Vertex3((float)pairA.A.x, (float)pairA.A.y, z);
			GL.TexCoord2(1, 0);
			GL.Vertex3((float)pairA.B.x, (float)pairA.B.y, z);
			GL.TexCoord2(1, 1);
			GL.Vertex3((float)pairB.A.x, (float)pairB.A.y, z);
			GL.TexCoord2(0,1);
			GL.Vertex3((float)pairB.B.x, (float)pairB.B.y, z);

			prevPair = new Pair2D(pairA.B, pairA.A);
		}

		GL.End ();
		GL.PopMatrix ();
	}

	Pair2D GetPair(Vector2 pA, Vector2 pB) {
		Vector2D posA = new Vector2D(pA);

		double rot = Vector2D.Atan2(pA, pB);

		Vector2D posA0 = posA.Copy();
		Vector2D posA1 = posA.Copy();

		posA0.Push(rot - Mathf.PI / 2, width);
		posA1.Push(rot + Mathf.PI / 2, width);

		return(new Pair2D(posA0, posA1));
	}
}
