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

            _posList.Clear();

            if (pointCount <= 1)
            {
                _posList.Add(BernsteinPositionCalculator(t));
                return;
            }

            float temp = 0;

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

        protected Vector3 GetTangent(Vector3 p1, Vector3 p2)
        {
            Vector3 tangent = p2 - p1;

            tangent.Normalize();

            return tangent;
        }

        protected Vector3 GetBinormal(Vector3 p1, Vector3 p2)
        {
            Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

            return rotation * GetTangent(p1, p2);
        }

        protected Vector3 GetNormal(Vector3 p1, Vector3 p2)
        {
            return Vector3.Cross(GetTangent(p1, p2), GetBinormal(p1, p2)).normalized;
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

        private readonly List<(Vector3 position, Action draw)> _gizmoDrawQueue = new List<(Vector3, Action)>();

        private void QueueGizmo(Vector3 position, Action draw)
        {
            _gizmoDrawQueue.Add((position, draw));
        }

        private void FlushGizmoQueue()
        {
            if (_gizmoDrawQueue.Count == 0) return;

            Camera cam = Camera.current;

            if (cam != null)
            {
                Vector3 camPos = cam.transform.position;
                _gizmoDrawQueue.Sort((a, b) =>
                    (b.position - camPos).sqrMagnitude.CompareTo((a.position - camPos).sqrMagnitude));
            }

            Color prevColor = Gizmos.color;
            Matrix4x4 prevMatrix = Gizmos.matrix;

            foreach (var (_, draw) in _gizmoDrawQueue)
            {
                draw();
            }

            Gizmos.color = prevColor;
            Gizmos.matrix = prevMatrix;

            _gizmoDrawQueue.Clear();
        }

        private void DrawPointTest()
        {
            foreach (var item in _posList)
            {
                Vector3 pos = item;

                QueueGizmo(pos, () =>
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(pos, SplineGizmoSettings.SamplePointRadius);
                });
            }
        }

        private static Mesh _arrowHeadMesh;

        private static Mesh GetArrowHeadMesh()
        {
            if (_arrowHeadMesh != null) return _arrowHeadMesh;

            const int segments = 10;
            const float baseRadius = 0.5f;

            Vector3[] vertices = new Vector3[segments + 2];
            vertices[0] = Vector3.zero;
            vertices[segments + 1] = new Vector3(0f, 0f, -1f);

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * baseRadius, Mathf.Sin(angle) * baseRadius, -1f);
            }

            int[] triangles = new int[segments * 3 * 4];
            int t = 0;

            for (int i = 0; i < segments; i++)
            {
                int current = i + 1;
                int next = (i + 1) % segments + 1;

                triangles[t++] = 0;
                triangles[t++] = current;
                triangles[t++] = next;

                triangles[t++] = 0;
                triangles[t++] = next;
                triangles[t++] = current;

                triangles[t++] = segments + 1;
                triangles[t++] = current;
                triangles[t++] = next;

                triangles[t++] = segments + 1;
                triangles[t++] = next;
                triangles[t++] = current;
            }

            _arrowHeadMesh = new Mesh
            {
                name = "SplineArrowHeadMesh",
                vertices = vertices,
                triangles = triangles
            };
            _arrowHeadMesh.RecalculateNormals();
            _arrowHeadMesh.RecalculateBounds();

            return _arrowHeadMesh;
        }

        private void DrawArrow(Vector3 origin, Vector3 direction, Color color, float length)
        {
            Vector3 tip = origin + direction * length;
            Vector3 mid = (origin + tip) / 2f;

            QueueGizmo(mid, () =>
            {
                Gizmos.color = color;
                Gizmos.DrawLine(origin, tip);

                Gizmos.matrix = Matrix4x4.TRS(tip, Quaternion.LookRotation(direction), Vector3.one * SplineGizmoSettings.ArrowHeadSize);
                Gizmos.DrawMesh(GetArrowHeadMesh());
                Gizmos.matrix = Matrix4x4.identity;
            });
        }

        private void DrawBinormal()
        {
            if (!SplineGizmoSettings.ShowBinormal) return;
            if (_posList.Count < 2) return;

            for (int i = 0; i < _posList.Count; i++)
            {
                Vector3 binormal = (i + 1 < _posList.Count)
                    ? GetBinormal(_posList[i], _posList[i + 1])
                    : GetBinormal(_posList[i - 1], _posList[i]);

                DrawArrow(_posList[i], binormal, Color.blue, SplineGizmoSettings.GizmoLength / 2);
                DrawArrow(_posList[i], -binormal, Color.blue, SplineGizmoSettings.GizmoLength / 2);
            }
        }

        private void DrawNormal()
        {
            if (!SplineGizmoSettings.ShowNormal) return;
            if (_posList.Count < 2) return;

            for (int i = 0; i < _posList.Count; i++)
            {
                Vector3 normal = (i + 1 < _posList.Count)
                    ? GetNormal(_posList[i], _posList[i + 1])
                    : GetNormal(_posList[i - 1], _posList[i]);

                DrawArrow(_posList[i], normal, Color.green, SplineGizmoSettings.GizmoLength);
            }
        }

        private void DrawNodePoint()
        {
            foreach (var item in _nodeList)
            {
                Vector3 pos = item.transform.position;

                QueueGizmo(pos, () =>
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(pos, SplineGizmoSettings.NodePointRadius);
                });
            }
        }

        private void OnDrawGizmos()
        {
            DrawLineTest();

            PositionListUpdate();

            DrawNodePoint();

            DrawPointTest();

            DrawBinormal();

            DrawNormal();

            FlushGizmoQueue();
        }
    }
}
