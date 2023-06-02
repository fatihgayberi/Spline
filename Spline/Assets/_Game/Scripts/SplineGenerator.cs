using UnityEngine;

public class SplineGenerator : MonoBehaviour
{
    public int resolution = 10; // Spline çözünürlüğü (nokta sayısı)
    public float length = 5f; // Spline uzunluğu
    public Transform[] controlPoints; // Kontrol noktaları (spline'ın şeklini belirler)

    private Mesh mesh;
    private Vector3[] vertices;

    void Start()
    {
        GenerateSplineMesh();
    }

    void GenerateSplineMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Nokta sayısına bağlı olarak, spline'da oluşturulacak olan vertex dizisini oluşturuyoruz.
        vertices = new Vector3[resolution];

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1); // [0, 1] aralığında t değeri hesaplanır

            // Bernstein polinomları kullanılarak spline noktasının pozisyonu hesaplanır
            Vector3 splinePoint = Vector3.zero;
            for (int j = 0; j < controlPoints.Length; j++)
            {
                splinePoint += controlPoints[j].position * Bernstein(j, controlPoints.Length - 1, t);
            }

            vertices[i] = splinePoint;
        }

        mesh.vertices = vertices;

        // Mesh üzerindeki üçgenlerin belirlenmesi
        int[] triangles = new int[(resolution - 1) * 6];
        int triangleIndex = 0;
        for (int i = 0; i < resolution - 1; i++)
        {
            if (i + 1 >= resolution) continue; // İndeks kontrolü

            triangles[triangleIndex++] = i;
            triangles[triangleIndex++] = i + 1;
            triangles[triangleIndex++] = i + resolution;

            triangles[triangleIndex++] = i + resolution;
            triangles[triangleIndex++] = i + 1;
            triangles[triangleIndex++] = i + resolution + 1;
        }

        mesh.triangles = triangles;
    }

    // Bernstein polinomu hesaplama fonksiyonu
    float Bernstein(int i, int n, float t)
    {
        float coefficient = Factorial(n) / (Factorial(i) * Factorial(n - i));
        return coefficient * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
    }

    // Faktöriyel hesaplama fonksiyonu
    int Factorial(int number)
    {
        if (number == 0)
            return 1;

        int result = 1;
        for (int i = 1; i <= number; i++)
        {
            result *= i;
        }

        return result;
    }
}
