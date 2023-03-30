using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WonnasmithEditor;

namespace Wonnasmith.Spline
{
    public class Bernstein : MonoBehaviour
    {
        [HelpBox("Yeni bir node olusturmak icin kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(NodeGenerator))]
        public bool buttonNodeGenerator;

        [HelpBox("Splinda olusturulan noktalari silmek için kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(NodeClear))]
        public bool buttonNodeClear;


        public bool isGiszmosShow;

        [SerializeField] private GameObject nodePrefab;
        [SerializeField, Range(0, 1)] private float t;
        public float radius;
        public List<Transform> _nodeList = new List<Transform>();
        public List<Vector3> PosList { get => _posList; }

        public void NodeGenerator()
        {
            GameObject newNode = Instantiate(nodePrefab);

            newNode.name = "p" + _nodeList.Count;
            newNode.SetActive(true);
            newNode.transform.SetParent(transform);
            _nodeList.Add(newNode.transform);
        }

        public void NodeClear()
        {
            if (_nodeList == null) return;
            if (_nodeList.Count == 0) return;

            for (int i = 0; i < _nodeList.Count; i++)
            {
                DestroyImmediate(_nodeList[i]);
            }

            _nodeList.Clear();

            transform.position = Vector3.zero;
        }
public int idx;
        private Vector3 BernsteinPositionCalculator(List<Transform> transformList, float percent)
        {
            Vector3 bernsteinPos = Vector3.zero;
            int n = transformList.Count - 1;

            float oneDiff_t = 1f - percent;

            for (int v = 0; v < transformList.Count; v++)
            {
                bernsteinPos += transformList[v].position * WonnaMathf.WonnaBinom(n, v) * Mathf.Pow(percent, v) * Mathf.Pow(oneDiff_t, (n - v));
                // if (v == idx)
                // {
                //     Vector3 asdf = transformList[v].position * WonnaMathf.WonnaBinom(n, v) * Mathf.Pow(percent, v) * Mathf.Pow(oneDiff_t, (n - v));
                //     Debug.Log("TOTAL::" + asdf, transformList[v].gameObject);
                //     Debug.Log("pos::" + transformList[v].position, transformList[v].gameObject);
                //     Debug.Log("binom::" + WonnaMathf.WonnaBinom(n, v), transformList[v].gameObject);
                //     Debug.Log("pow1::" + Mathf.Pow(percent, v), transformList[v].gameObject);
                //     Debug.Log("pow2::" + Mathf.Pow(oneDiff_t, (n - v)), transformList[v].gameObject);
                // }

            }

            return bernsteinPos;
        }

        private void DrawLineTest()
        {
            if (_nodeList == null) return;

            for (int i = 0; i < _nodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(_nodeList[i].position, _nodeList[i + 1].position);
            }

        }

        public float splinePercentRate;
        public float pointCount;

        List<Vector3> _posList = new List<Vector3>();

        private void OnDrawGizmos()
        {
            // Debug.Log(BernsteinPositionCalculator(_nodeList, t));

            // return;

            DrawLineTest();

            if (!isGiszmosShow) return;
            if (_nodeList == null) return;

            float temp = 0;

            _posList.Clear();

            splinePercentRate = t / pointCount;

            while (temp < t)
            {
                temp = Mathf.Clamp(temp, 0, t);

                _posList.Add(BernsteinPositionCalculator(_nodeList, temp));

                temp += splinePercentRate;

                if (temp + splinePercentRate > t)
                {
                    break;
                }
            }

            _posList.Add(BernsteinPositionCalculator(_nodeList, t));

            foreach (var item in _posList)
            {
                Gizmos.DrawSphere(item, radius);
            }
        }
    }
}
