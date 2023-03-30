using System;
using System.Collections.Generic;
using UnityEngine;
using WonnasmithEditor;

namespace Wonnasmith.Spline
{
    [RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class SplineMesh : MonoBehaviour, IRebuildImmediate
    {
        [HelpBox("Spline üzerinde degisiklik yaptiktan sonra yenilemek için kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(RebuildImmediate))]
        public bool buttonRebuildImmediate;

        [SerializeField] private Bernstein _spline1;
        [SerializeField] private Bernstein _spline2;

        public MeshFilter _splineMeshFilter;
        private Mesh _mesh;
        private Vector3[] _vertices;
        private int[] _triangles;
        private const string _meshName = "SplineMesh";

        public void RebuildImmediate()
        {
            MeshGenerator();
        }

        private void MeshGenerator()
        {
            if (_mesh != null)
            {
                _mesh.Clear();
            }

            _splineMeshFilter.mesh = _mesh = new Mesh();
            _mesh.name = _meshName;

            _vertices = new Vector3[_spline1.PosList.Count * 2];
            _triangles = new int[((_spline1.PosList.Count * 2) - 2) * 3];


            for (int si = 0, vi = 0; vi < _vertices.Length; si++, vi += 2)
            {
                _vertices[vi] = _spline1.PosList[si];
                _vertices[vi + 1] = _spline2.PosList[si];
            }

            int x = 0;
            int y = 1;
            int z = 3;

            bool isTurnNumOdd = true;

            for (int i = 0; i < _triangles.Length; i += 3)
            {
                if (isTurnNumOdd)
                {
                    _triangles[i] = x * 2;
                    _triangles[i + 1] = y;
                    _triangles[i + 2] = z;

                    y += 2;
                    z -= 1;
                }
                else
                {
                    _triangles[i] = x * 2;
                    _triangles[i + 1] = y;
                    _triangles[i + 2] = z;

                    z += 3;
                    x += 1;
                }

                isTurnNumOdd = !isTurnNumOdd;
            }

            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;
        }
    }
}