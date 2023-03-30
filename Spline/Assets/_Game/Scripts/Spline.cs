using System;
using System.Collections.Generic;
using UnityEngine;
using WonnasmithEditor;

namespace Wonnasmith.Spline
{
    public class Spline : MonoBehaviour, IRebuildImmediate
    {
        [HelpBox("Spline üzerinde degisiklik yaptiktan sonra yenilemek için kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(RebuildImmediate))]
        public bool buttonRebuildImmediate;

        [HelpBox("Yeni bir node olusturmak icin kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(NodeGenerator))]
        public bool buttonNodeGenerator;

        [HelpBox("Splinda olusturulan noktalari silmek için kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(NodeClear))]
        public bool buttonNodeClear;

        [SerializeField] private GameObject nodePrefab;

        [SerializeField, Range(0, 1)] private float splinePercent;
        [SerializeField, Range(0.01f, 1)] private float splinePercentRate;

        private List<Vector3> _posList = new List<Vector3>();
        public List<Vector3> PosList { get => _posList; }

        private List<GameObject> _nodeList = new List<GameObject>();



        private void PositinInitialize()
        {
            if (_posList == null) _posList = new List<Vector3>();

            _posList.Clear();

            float temp = 0;

            while (temp < splinePercent)
            {
                temp = Mathf.Clamp(temp, 0, splinePercent);

                _posList.Add(Calculate_P(temp));

                temp += splinePercentRate;
            }
        }

        public void RebuildImmediate()
        {
            PositinInitialize();
        }

        public void NodeGenerator()
        {
            GameObject newNode = Instantiate(nodePrefab);

            newNode.name = "p" + _nodeList.Count;
            newNode.SetActive(true);
            newNode.transform.SetParent(transform);
            _nodeList.Add(newNode);
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

        private Vector3 Calculate_P(float percent)
        {
            if (_nodeList == null) return Vector3.zero;
            if (_nodeList.Count == 0) return Vector3.zero;
            if (_nodeList.Count == 1) return _nodeList[0].transform.position;

            List<Vector3> posList = new List<Vector3>();
            List<Vector3> tempPosList = new List<Vector3>();

            for (int i = 0; i < _nodeList.Count - 1; i++)
            {
                posList.Add(WonnaMathf.WonnaLerp(_nodeList[i].transform.position, _nodeList[i + 1].transform.position, percent));
            }

            if (posList == null) return Vector3.zero;

            while (posList.Count > 1)
            {
                tempPosList.Clear();

                for (int i = 0; i < posList.Count - 1; i++)
                {
                    tempPosList.Add(WonnaMathf.WonnaLerp(posList[i], posList[i + 1], percent));
                }

                posList.Clear();

                foreach (Vector3 pos in tempPosList)
                {
                    posList.Add(pos);
                }
            }

            return posList[0];
        }

        public float radius;


        private void OnDrawGizmos()
        {
            if (_posList == null) return;

            foreach (var item in _posList)
            {
                Gizmos.DrawSphere(item, radius);
            }
        }
    }
}