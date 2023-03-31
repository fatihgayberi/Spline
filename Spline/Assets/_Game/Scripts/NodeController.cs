using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WonnasmithEditor;

namespace Wonnasmith.Spline
{
    [ExecuteInEditMode]
    public class NodeController : MonoBehaviour
    {
        public static event SplineBase.SplineBaseNodeDeleteButtonClick NodeDeleteButtonClick;

        [HelpBox("Spline üzerindeki noktayi silmek için kullanilir", HelpBoxMessageType.Info)]
        [Space(20), Button(nameof(NodeDeleteButton))]
        public bool buttonNodeGenerator;

        public void NodeDeleteButton()
        {
            NodeDeleteButtonClick?.Invoke(this);
        }
    }
}
