using System.Collections.Generic;
using UnityEngine;

namespace Wonnasmith.Spline
{
    [ExecuteInEditMode]
    public class Spline : SplineBase
    {
        [SerializeField, HideInInspector] private List<NodeController> _nodeList = new List<NodeController>();
        [SerializeField, HideInInspector] private List<Vector3> _posList;

        public override void NodeGenerator()
        {
            NodeGenerator(transform, _nodeList);
        }

        public override void NodeClear()
        {
            NodeClear(_nodeList, _posList);
        }

        public override void OnNodeDeleteButtonClick(NodeController nodeController)
        {
            OnNodeDeleteButtonClick(nodeController, _nodeList);
        }

        public void OnDrawGizmos()
        {
            DrawLineTest(_nodeList);

            PositionListUpdate(_nodeList, _posList);

            DrawPointTest(_posList);
        }
    }
}
