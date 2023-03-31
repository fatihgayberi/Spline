using System;
using System.Collections;
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

        public abstract void NodeGenerator();
        public abstract void NodeClear();
        public abstract void OnNodeDeleteButtonClick(NodeController nodeController);


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

        protected void OnNodeDeleteButtonClick(NodeController nodeController, List<NodeController> nodeList)
        {
            if (EditorApplication.isPlaying) return;
            if (nodeController == null) return;
            if (nodeList == null) return;

            if (nodeList.Contains(nodeController))
            {
                nodeList.Remove(nodeController);
                DestroyImmediate(nodeController.gameObject);
            }

            AllNodeRename(nodeList);
        }

        protected void NodeGenerator(Transform parent, List<NodeController> nodeList)
        {
            if (EditorApplication.isPlaying) return;

            if (nodeList == null)
            {
                nodeList = new List<NodeController>();
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
            newNode.transform.SetParent(parent);

            NodeController nodeController = newNode.GetComponent<NodeController>();

            nodeList.Add(nodeController);

            AllNodeRename(nodeList);
        }

        protected void NodeClear(List<NodeController> nodeList, List<Vector3> posList)
        {
            if (EditorApplication.isPlaying) return;
            if (nodeList == null) return;
            if (nodeList.Count == 0) return;

            for (int i = 0; i < nodeList.Count; i++)
            {
                if (nodeList[i] == null) continue;

                if (nodeList[i].gameObject != null)
                {
                    DestroyImmediate(nodeList[i].gameObject);
                }
            }

            nodeList.Clear();

            if (posList != null)
            {
                posList.Clear();
            }

            transform.position = Vector3.zero;
        }

        protected void AllNodeRename(List<NodeController> nodeList)
        {
            if (nodeList == null) return;

            for (int i = 0; i < nodeList.Count; i++)
            {
                nodeList[i].name = _nodeName + i;
            }
        }

        private Vector3 BernsteinPositionCalculator(List<NodeController> transformList, float percent)
        {
            Vector3 bernsteinPos = Vector3.zero;
            int n = transformList.Count - 1;
            
            if (percent <= 0) return transformList[0].transform.position;
            if (percent >= 1) return transformList[n].transform.position;

            for (int v = 0; v < transformList.Count; v++)
            {
                bernsteinPos += transformList[v].transform.position * WonnaMathf.WonnaBernstein(n, v, percent);
            }

            return bernsteinPos;
        }

        protected void DrawLineTest(List<NodeController> nodeList)
        {
            if (nodeList == null) return;

            Gizmos.color = Color.white;

            for (int i = 0; i < nodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(nodeList[i].transform.position, nodeList[i + 1].transform.position);
            }
        }

        protected void DrawPointTest(List<Vector3> posList)
        {
            Gizmos.color = Color.red;

            foreach (var item in posList)
            {
                Gizmos.DrawSphere(item, 0.3f);
            }
        }

        protected void PositionListUpdate(List<NodeController> nodeList, List<Vector3> posList)
        {
            if (nodeList == null) return;
            if (nodeList.Count == 0) return;

            if (nodeList == null)
            {
                nodeList = new List<NodeController>();
            }

            float temp = 0;

            posList.Clear();

            float splinePercentRate = t / pointCount;

            while (temp < t)
            {
                temp = Mathf.Clamp(temp, 0, t);

                posList.Add(BernsteinPositionCalculator(nodeList, temp));

                temp += splinePercentRate;

                if (temp + splinePercentRate > t)
                {
                    break;
                }
            }

            posList.Add(BernsteinPositionCalculator(nodeList, t));
        }
    }
}
