using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using Wonnasmith.Spline;

namespace WonnasmithEditor
{
    [Overlay(typeof(SceneView), "Spline Gizmo Ayarlari", true)]
    public class SplineGizmoOverlay : Overlay
    {
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop = 4;
            root.style.paddingBottom = 4;
            root.style.minWidth = 180;

            var gizmoLengthField = new FloatField("Gizmo Length");
            gizmoLengthField.SetValueWithoutNotify(SplineGizmoSettings.GizmoLength);
            gizmoLengthField.RegisterValueChangedCallback(evt =>
            {
                SplineGizmoSettings.GizmoLength = evt.newValue;
                SceneView.RepaintAll();
            });
            root.Add(gizmoLengthField);

            var arrowHeadSizeField = new FloatField("Arrow Head Size");
            arrowHeadSizeField.SetValueWithoutNotify(SplineGizmoSettings.ArrowHeadSize);
            arrowHeadSizeField.RegisterValueChangedCallback(evt =>
            {
                SplineGizmoSettings.ArrowHeadSize = evt.newValue;
                SceneView.RepaintAll();
            });
            root.Add(arrowHeadSizeField);

            var nodePointRadiusField = new FloatField("Node Point Radius");
            nodePointRadiusField.SetValueWithoutNotify(SplineGizmoSettings.NodePointRadius);
            nodePointRadiusField.RegisterValueChangedCallback(evt =>
            {
                SplineGizmoSettings.NodePointRadius = evt.newValue;
                SceneView.RepaintAll();
            });
            root.Add(nodePointRadiusField);

            var samplePointRadiusField = new FloatField("Sample Point Radius");
            samplePointRadiusField.SetValueWithoutNotify(SplineGizmoSettings.SamplePointRadius);
            samplePointRadiusField.RegisterValueChangedCallback(evt =>
            {
                SplineGizmoSettings.SamplePointRadius = evt.newValue;
                SceneView.RepaintAll();
            });
            root.Add(samplePointRadiusField);

            var showBinormalToggle = new Toggle("Show Binormal");
            showBinormalToggle.SetValueWithoutNotify(SplineGizmoSettings.ShowBinormal);
            showBinormalToggle.RegisterValueChangedCallback(evt =>
            {
                SplineGizmoSettings.ShowBinormal = evt.newValue;
                SceneView.RepaintAll();
            });
            root.Add(showBinormalToggle);

            var showNormalToggle = new Toggle("Show Normal");
            showNormalToggle.SetValueWithoutNotify(SplineGizmoSettings.ShowNormal);
            showNormalToggle.RegisterValueChangedCallback(evt =>
            {
                SplineGizmoSettings.ShowNormal = evt.newValue;
                SceneView.RepaintAll();
            });
            root.Add(showNormalToggle);

            return root;
        }
    }
}
