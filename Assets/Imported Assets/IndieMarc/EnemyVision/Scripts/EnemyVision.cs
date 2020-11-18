using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndieMarc.StealthLOS
{
    public class EnemyVision : MonoBehaviour
    {
        [Header("Linked Enemy")]
        public EnemyLOS target;

        [Header("Vision")]
        public float vision_angle = 30f;
        public float vision_range = 3f;
        public LayerMask obstacle_mask = ~(0);

        [Header("Optimization")]
        public int precision = 60;
        public float refresh_rate = 0f;

        private MeshFilter mesh;
        private float timer = 0f;

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>();
        }

        private void Start()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            vertices.Add(new Vector3(0f, 0f, 0f));
            normals.Add(Vector3.up);
            uv.Add(Vector2.zero);
            int minmax = Mathf.RoundToInt(vision_angle / 2f);

            int tri_index = 0;
            float step_jump = Mathf.Clamp(vision_angle / precision, 0.01f, minmax);
            for (float i = -minmax; i <= minmax; i += step_jump)
            {
                float angle = (float)(i + 90f) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle) * vision_range, 0f, Mathf.Sin(angle) * vision_range);
                vertices.Add(dir);
                normals.Add(Vector2.up);
                uv.Add(Vector2.zero);

                if (tri_index > 0)
                {
                    triangles.Add(0);
                    triangles.Add(tri_index + 1);
                    triangles.Add(tri_index);
                }
                tri_index++;
            }

            mesh.mesh.vertices = vertices.ToArray();
            mesh.mesh.triangles = triangles.ToArray();
            mesh.mesh.normals = normals.ToArray();
            mesh.mesh.uv = uv.ToArray();
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = target.eye.transform.position;
            transform.rotation = target.transform.rotation;

            if (timer > refresh_rate)
            {
                timer = 0f;

                List<Vector3> vertices = new List<Vector3>();
                vertices.Add(new Vector3(0f, 0f, 0f));

                int minmax = Mathf.RoundToInt(vision_angle / 2f);
                float step_jump = Mathf.Clamp(vision_angle / precision, 0.01f, minmax);
                for (float i = -minmax; i <= minmax; i += step_jump)
                {
                    float angle = (float)(i + 90f) * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(Mathf.Cos(angle) * vision_range, 0f, Mathf.Sin(angle) * vision_range);

                    RaycastHit hit;
                    Vector3 pos_world = transform.TransformPoint(Vector3.zero);
                    Vector3 dir_world = transform.TransformDirection(dir.normalized);
                    bool ishit = Physics.Raycast(new Ray(pos_world, dir_world), out hit, vision_range, obstacle_mask.value);
                    if (ishit)
                        dir = dir.normalized * hit.distance;
                    Debug.DrawRay(pos_world, dir_world * (ishit ? hit.distance : vision_range));

                    vertices.Add(dir);
                }

                mesh.mesh.vertices = vertices.ToArray();
            }
        }
    }
}
