using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CableRenderer : MonoBehaviour
{
    Mesh m_Mesh;

    Vector3[] m_Vertices;
    int[] m_Triangles;


    [SerializeField]
    float m_Thiccness = .2f;

    [SerializeField]
    int m_Sides = 5;

    [SerializeField]
    Transform[] Path;


    public void Start()
    {
        Init();
    }
    public void SetPath(Transform[] path)
    {
        Path = path;
    }

    public void Init()
    {
        m_Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_Mesh;

        CreateShape();
    }

    // Update is called once per frame
    void Update()
    {   
        for (int i = 0; i < Path.Length; i++)
		{
            int B1 = i * m_Sides;
            for (int s = 0; s < m_Sides; s++)
			{
                float p = (float)s / (float)m_Sides;

                Vector3 relative = Path[i].position + (Path[i].transform.up * Mathf.Sin(p * Mathf.PI * 2) + (Path[i].transform.right * Mathf.Cos(p * Mathf.PI * 2))) * m_Thiccness;

                m_Vertices[B1 + s] = (relative - transform.position);


            }
        }

        UpdateMesh();
    }

    void UpdateMesh()
	{
        m_Mesh.Clear();
        m_Mesh.vertices = m_Vertices;
        m_Mesh.triangles = m_Triangles;
    }
    void CreateShape()
	{



        m_Vertices = new Vector3[Path.Length * m_Sides];

        
        List<int> tri = new List<int>();
        for (int i = 0; i < Path.Length - 1; i++)
		{
            int B1 = i * m_Sides;
            int B2 = (i + 1) * m_Sides;
            for (int s = 0; s < m_Sides; s++)
			{
                int[] indexs = new int[] { B1 + s, B2 + s, B1 + (s == m_Sides - 1 ? 0 : s + 1), B2 + (s == m_Sides - 1 ? 0 : s + 1) };
                tri.Add(indexs[0]);
                tri.Add(indexs[1]);
                tri.Add(indexs[2]);
                tri.Add(indexs[1]);
                tri.Add(indexs[3]);
                tri.Add(indexs[2]);

            }
		}
        m_Triangles = tri.ToArray();



	}

	private void OnDrawGizmos()
	{
       if (m_Vertices != null)
       {
           //foreach (Vector3 v in m_Vertices)
           //    Gizmos.DrawSphere(transform.worldToLocalMatrix * v, .1f);
       
            for (int i = 0; i < m_Triangles.Length - 1;i++)
			{
               Gizmos.color = Color.green;
               Gizmos.DrawLine(m_Vertices[m_Triangles[i]] + transform.position, m_Vertices[m_Triangles[i + 1]] + transform.position);
			}
       }
	}
}
