using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WonnasmithEditor;

namespace Wonnasmith.Spline
{
    [ExecuteInEditMode]
    public abstract class SplineBase : MonoBehaviour
    {
        public delegate void SplineBaseNodeDeleteButtonClick(NodeController nodeController);

        [HelpBox("Yeni bir node olusturmak icin kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(NodeGenerator))]
        public bool buttonNodeGenerator;

        [HelpBox("Splinda olusturulan noktalari silmek için kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(NodeClear))]
        public bool buttonNodeClear;

        [Space(20), SerializeField][Min(1)] private int pointCount;
        [SerializeField, Range(0, 1)] private float t;

        [SerializeField] private float tangentGizmoLength;

        private GameObject _nodePrefab;
        private const string _nodeName = "NODE_";
        private const string _nodePrefabPath = "NodePrefab/NODE";

        public List<NodeController> _nodeList = new List<NodeController>();
        public List<Vector3> _posList = new List<Vector3>();

        private void OnEnable()
        {
            if (EditorApplication.isPlaying) return;

            NodeController.NodeDeleteButtonClick += OnNodeDeleteButtonClick;
        }
        private void OnDisable()
        {
            if (EditorApplication.isPlaying) return;

            NodeController.NodeDeleteButtonClick -= OnNodeDeleteButtonClick;
        }

        protected void OnNodeDeleteButtonClick(NodeController nodeController)
        {
            if (EditorApplication.isPlaying) return;
            if (nodeController == null) return;
            if (_nodeList == null) return;

            if (_nodeList.Contains(nodeController))
            {
                _nodeList.Remove(nodeController);
                DestroyImmediate(nodeController.gameObject);
            }

            AllNodeRename();
        }

        public void NodeGenerator()
        {
            if (EditorApplication.isPlaying) return;

            if (_nodeList == null)
            {
                _nodeList = new List<NodeController>();
            }

            if (_nodePrefab == null)
            {
                _nodePrefab = Resources.Load(_nodePrefabPath, typeof(GameObject)) as GameObject;

                if (_nodePrefab == null) return;
            }

            GameObject newNode = Instantiate(_nodePrefab);

            if (newNode == null) return;

            newNode.transform.position = transform.position;
            newNode.name = _nodeName;
            newNode.SetActive(true);
            newNode.transform.SetParent(transform);

            NodeController nodeController = newNode.GetComponent<NodeController>();

            _nodeList.Add(nodeController);

            AllNodeRename();
        }

        protected void NodeClear()
        {
            if (EditorApplication.isPlaying) return;
            if (_nodeList == null) return;
            if (_nodeList.Count == 0) return;

            for (int i = 0; i < _nodeList.Count; i++)
            {
                if (_nodeList[i] == null) continue;

                if (_nodeList[i].gameObject != null)
                {
                    DestroyImmediate(_nodeList[i].gameObject);
                }
            }

            _nodeList.Clear();

            if (_posList != null)
            {
                _posList.Clear();
            }

            transform.position = Vector3.zero;
        }

        protected void AllNodeRename()
        {
            if (_nodeList == null) return;

            for (int i = 0; i < _nodeList.Count; i++)
            {
                _nodeList[i].name = _nodeName + i;
            }
        }

        public Vector3 BernsteinPositionCalculator(float percent)
        {
            Vector3 bernsteinPos = Vector3.zero;
            int n = _nodeList.Count - 1;

            if (percent <= 0) return _nodeList[0].transform.position;
            if (percent >= 1) return _nodeList[n].transform.position;

            for (int v = 0; v < _nodeList.Count; v++)
            {
                bernsteinPos += _nodeList[v].transform.position * WonnaMathf.WonnaBernstein(n, v, percent);
            }

            return bernsteinPos;
        }

        private void PositionListUpdate()
        {
            if (_nodeList == null) return;
            if (_nodeList.Count == 0) return;

            if (_nodeList == null)
            {
                _nodeList = new List<NodeController>();
            }

            float temp = 0;

            _posList.Clear();

            float splinePercentRate = t / (pointCount - 1);

            while (temp < t)
            {
                temp = Mathf.Clamp(temp, 0, t);

                _posList.Add(BernsteinPositionCalculator(temp));

                temp += splinePercentRate;

                if (temp + splinePercentRate > t)
                {
                    break;
                }
            }

            _posList.Add(BernsteinPositionCalculator(t));
        }

        protected Vector3 GetPointTangent(Vector3 p1, Vector3 p2)
        {
            Vector3 n = p2 - p1;

            n.Normalize();

            Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

            return rotation * n;
        }

        private void DrawLineTest()
        {
            if (_nodeList == null) return;

            Gizmos.color = Color.white;

            for (int i = 0; i < _nodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(_nodeList[i].transform.position, _nodeList[i + 1].transform.position);
            }
        }

        private void DrawPointTest()
        {
            Gizmos.color = Color.red;

            foreach (var item in _posList)
            {
                Gizmos.DrawSphere(item, 0.3f);
            }
        }

        private void DrawPointTangent()
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.blue;

            for (int i = 0; i < _posList.Count - 1; i++)
            {
                Vector3 n = _posList[i + 1] - _posList[i];

                n.Normalize();

                Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

                Vector3 rotatedVector = rotation * n;

                Gizmos.DrawLine(_posList[i] - rotatedVector * tangentGizmoLength / 2, _posList[i] + rotatedVector * tangentGizmoLength / 2);
            }

            Gizmos.color = prevColor;
        }

        private void DrawNodePoint()
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = Color.yellow;

            foreach (var item in _nodeList)
            {
                Gizmos.DrawSphere(item.transform.position, 0.5f);
            }

            Gizmos.color = prevColor;
        }


        private void OnDrawGizmos()
        {
            DrawNodePoint();

            DrawLineTest();

            PositionListUpdate();

            DrawPointTest();

            DrawPointTangent();
        }
    }
}
