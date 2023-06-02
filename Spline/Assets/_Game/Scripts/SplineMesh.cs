using System;
using System.Collections.Generic;
using UnityEngine;
using WonnasmithEditor;

namespace Wonnasmith.Spline
{
    [ExecuteInEditMode, RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class SplineMesh : SplineBase
    {
        [HelpBox("* Spline üzerinde degisiklik yaptiktan sonra yeni mesh oluşturmak için kullanilir.\n\n* Mesh tek yüzey oluştugu icin hazirlanan mesh belirlenen point noktalarindan kaynakli olarak ters oluşmuş olabilir.", HelpBoxMessageType.Warning)]
        [Space(20), Button(nameof(MeshRebuild))]
        public bool isMeshRebuild;

        [SerializeField] private float width;

        private MeshFilter _splineMeshFilter;
        private MeshRenderer _splineMeshRenderer;
        private Mesh _mesh;
        private Vector3[] _vertices;
        private Material _defaultSplineMaterial;
        private int[] _triangles;
        private const string _meshName = "SplineMesh";
        protected const string _splineMeshMaterialPath = "SplineMeshMaterial/DefaultSplineMaterial";

        public void MeshRebuild()
        {
            MeshGenerator();
        }

        private void MeshGenerator()
        {
            if (_posList == null) return;

            if (_splineMeshFilter == null)
            {
                _splineMeshFilter = GetComponent<MeshFilter>();
            }

            if (_mesh != null)
            {
                _mesh.Clear();
            }

            MeshMaterialEdit();

            _splineMeshFilter.mesh = _mesh = new Mesh();
            _mesh.name = _meshName;

            _vertices = new Vector3[_posList.Count * 2];
            _triangles = new int[((_posList.Count * 2) - 2) * 3];
            
            Vector3 normal;

            for (int si = 0, vi = 0; vi < _vertices.Length; si++, vi += 2)
            {
                if (si + 1 < _posList.Count)
                {
                    normal = GetPointTangent(_posList[si], _posList[si + 1]);

                }
                else
                {
                    normal = GetPointTangent(_posList[si - 1], _posList[si]);
                }

                _vertices[vi] = _posList[si] - normal * (width / 2);
                _vertices[vi + 1] = _posList[si] + normal * (width / 2);
            }

            int x = 0;
            int y = 1;
            int z = 3;

            bool isTurnNumOdd = true;

            for (int i = _triangles.Length - 1; i >= 0; i -= 3)
            {
                if (isTurnNumOdd)
                {
                    _triangles[i] = x * 2;
                    _triangles[i - 1] = y;
                    _triangles[i - 2] = z;

                    y += 2;
                    z -= 1;
                }
                else
                {
                    _triangles[i] = x * 2;
                    _triangles[i - 1] = y;
                    _triangles[i - 2] = z;

                    z += 3;
                    x += 1;
                }

                isTurnNumOdd = !isTurnNumOdd;
            }

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
        }

        private void MeshMaterialEdit()
        {
            if (_splineMeshRenderer == null)
            {
                _splineMeshRenderer = GetComponent<MeshRenderer>();

                if (_splineMeshRenderer.material == null)
                {
                    _defaultSplineMaterial = Resources.Load(_splineMeshMaterialPath, typeof(Material)) as Material;

                    if (_defaultSplineMaterial != null)
                    {
                        _splineMeshRenderer.material = _defaultSplineMaterial;
                    }
                }
            }
        }
    }
}