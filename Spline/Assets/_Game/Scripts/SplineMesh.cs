using System;
using System.Collections.Generic;
using UnityEditor;
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

        private List<NodeController> _nodeList1;
        private List<NodeController> _nodeList2;

        private List<Vector3> _posList1;
        private List<Vector3> _posList2;

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
            if (_posList1 == null | _posList2 == null) return;

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

            _vertices = new Vector3[_posList1.Count * 2];
            _triangles = new int[((_posList1.Count * 2) - 2) * 3];

            for (int si = 0, vi = 0; vi < _vertices.Length; si++, vi += 2)
            {
                _vertices[vi] = _posList1[si] - transform.position;
                _vertices[vi + 1] = _posList2[si] - transform.position;
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

        public override void NodeGenerator()
        {
            NodeGenerator(transform, _nodeList1);
            NodeGenerator(transform, _nodeList2);
        }

        public override void NodeClear()
        {
            NodeClear(_nodeList1, _posList1);
            NodeClear(_nodeList2, _posList2);
        }

        public override void OnNodeDeleteButtonClick(NodeController nodeController)
        {
            int nodeIndex;

            if (_nodeList1.Contains(nodeController))
            {
                nodeIndex = _nodeList1.IndexOf(nodeController);

                OnNodeDeleteButtonClick(nodeController, _nodeList1);
                OnNodeDeleteButtonClick(_nodeList2[nodeIndex], _nodeList2);
            }
            else if (_nodeList2.Contains(nodeController))
            {
                nodeIndex = _nodeList2.IndexOf(nodeController);

                OnNodeDeleteButtonClick(nodeController, _nodeList2);
                OnNodeDeleteButtonClick(_nodeList1[nodeIndex], _nodeList1);
            }
        }

        public void OnDrawGizmos()
        {
            DrawLineTest(_nodeList1);
            DrawLineTest(_nodeList2);

            PositionListUpdate(_nodeList1, _posList1);
            PositionListUpdate(_nodeList2, _posList2);

            DrawPointTest(_posList1);
            DrawPointTest(_posList2);
        }
    }
}