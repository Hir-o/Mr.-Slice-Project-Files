using UnityEngine;

[ExecuteInEditMode]
public class Mesh2D : MonoBehaviour {
	public PolygonTriangulator2D.Triangulation triangulation = PolygonTriangulator2D.Triangulation.Advanced;

	// Optionable material
	public Material material;
	public Vector2 materialScale = new Vector2(1, 1);
	public Vector2 materialOffset = Vector2.zero;

	public string sortingLayerName; 
	public int sortingLayerID;
	public int sortingOrder;

	void Start () {
		if (GetComponents<Mesh2D>().Length > 1) {
			//Debug.LogError("Multiple 'Mesh2D' components cannot be attached to the same game object");
			return;
		}

		// Generate Mesh from collider
		Polygon2D polygon = Polygon2DList.CreateFromGameObject (gameObject)[0];
		if (polygon != null) {
			polygon.CreateMesh(gameObject, materialScale, materialOffset, triangulation);

			// Setting Mesh material
			if (material != null) {
				MeshRenderer meshRenderer = GetComponent<MeshRenderer> ();
				meshRenderer.material = material;
			
				meshRenderer.sortingLayerName = sortingLayerName;
				meshRenderer.sortingLayerID = sortingLayerID;
				meshRenderer.sortingOrder = sortingOrder;
			}
		}
	}
}
