using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CylinderPlatformMaker : MonoBehaviour
{
    public float r; // radius
    public int numberOfEdge; // edge count hexagon, pentagon...

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] trinagles;
    private float triangleCenterAngle;
    public GameObject x;
    private List<GameObject> xList = new List<GameObject>();
    public bool reset;

    private void Awake()
    {
        triangleCenterAngle = 360 / numberOfEdge;
        GenerateCircle(); //StartCoroutine(GenerateCircle());

    }

    private void Start()
    {

    }

    private void Update()
    {
        //if (reset)
        //{
        //    reset = false;
        //    triangleCenterAngle = 360 / numberOfEdge;
        //    GenerateCircle();
        //}
    }

    private void GenerateCircle()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Circle";

        vertices = new Vector3[numberOfEdge + 1];
        trinagles = new int[numberOfEdge + 1];

        int verticesLength = vertices.Length;
        int trianglesLength = trinagles.Length;
        float sinAngle, cosAngle;

        vertices[0] = Vector3.zero;

        for (int i = 1; i < verticesLength; i++)
        {
            float circleSliceAngle = (i - 1) * triangleCenterAngle * Mathf.PI / 180;
            float posX, posZ;

            sinAngle = EpsilonEditor(Mathf.Sin(circleSliceAngle));
            cosAngle = EpsilonEditor(Mathf.Cos(circleSliceAngle));
            posX = r * sinAngle;
            posZ = r * cosAngle;
            Debug.Log("Sin: " + Mathf.Sin(circleSliceAngle) + " Cos: " + Mathf.Cos(circleSliceAngle));

            //yield return new WaitForSeconds(0f);

            vertices[i] = new Vector3(posX, 0, posZ);
            //Debug.Log("posX:" + posX + " posZ:" + posZ + " Angle:" + (circleSliceAngle));
        }

        mesh.vertices = vertices;
        trinagles[0] = 0;
        trinagles[1] = 1;
        trinagles[2] = 2;
        trinagles[3] = 0;
        trinagles[4] = 2;
        trinagles[5] = 3;

        for (int ti = 0, vi = 1; vi < numberOfEdge; ti += 3, vi++)
        {
            Debug.Log("::::" + ti);

            if (ti < trianglesLength - 1)
                trinagles[ti] = 0;

            if (ti + 1 < trianglesLength - 1)
                trinagles[ti + 1] = vi;


            if (ti + 2 < trianglesLength - 1)
                trinagles[ti + 2] = vi + 1;
        }


        trinagles[trianglesLength - 3] = 0;
        trinagles[trianglesLength - 2] = verticesLength;
        trinagles[trianglesLength - 1] = 1;

        mesh.triangles = trinagles;

        // StartCoroutine(SphereDrawTest());
    }

    private IEnumerator SphereDrawTest()
    {
        int verticesLength = vertices.Length;

        for (int i = 0; i < verticesLength; i++)
        {
            yield return new WaitForSeconds(0.05f);
            GameObject newX = Instantiate(x, vertices[i], Quaternion.identity);
            xList.Add(newX);
            newX.name = (i + 1) + "";
        }

        for (int i = 0; i < verticesLength; i++)
        {
            Debug.Log(Vector3.Distance(xList[0].transform.position, xList[i].transform.position));
        }

    }


    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        Gizmos.color = Color.black;

        for (int i = 0; i < trinagles.Length; ++i)
        {
            Debug.Log(i);
            // Gizmos.DrawSphere(trinagles[i], 0.1f);
        }
        // for (int i = 0; i < vertices.Length; ++i)
        // {
        //     Debug.Log(i);
        //     Gizmos.DrawSphere(vertices[i], 0.1f);
        // }
    }

    private float EpsilonEditor(float number)
    {
        if (Mathf.Approximately(0, number)) return 0;

        else return number;
    }
}