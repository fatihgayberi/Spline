using System;
using UnityEngine;
using UnityEditor;

namespace WonnasmithEditor
{
    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return base.GetHeight();

            try
            {
                return Mathf.Max(40f, EditorStyles.helpBox.CalcHeight(new GUIContent(helpBoxAttribute.text), EditorGUIUtility.currentViewWidth) + 4);
            }
            catch (ArgumentException)
            {
                // UI Toolkit tabanli Inspector, GetHeight()'i bir OnGUI cagrisi disinda
                // (property binding sirasinda) da tetikleyebiliyor; bu durumda hicbir
                // GUI fonksiyonu cagrilamiyor, bu yuzden metin uzunluguna gore tahmini bir yukseklik donuyoruz.
                const float charsPerLine = 60f;
                const float lineHeight = 14f;
                int lines = Mathf.Max(1, Mathf.CeilToInt(helpBoxAttribute.text.Length / charsPerLine));
                return Mathf.Max(40f, lines * lineHeight + 16f);
            }
        }

        public override void OnGUI(Rect position)
        {
            var helpBoxAttribute = attribute as HelpBoxAttribute;
            if (helpBoxAttribute == null) return;
            EditorGUI.HelpBox(position, helpBoxAttribute.text, GetMessageType(helpBoxAttribute.messageType));
        }

        private MessageType GetMessageType(HelpBoxMessageType helpBoxMessageType)
        {
            switch (helpBoxMessageType)
            {
                default:
                case HelpBoxMessageType.None: return MessageType.None;
                case HelpBoxMessageType.Info: return MessageType.Info;
                case HelpBoxMessageType.Warning: return MessageType.Warning;
                case HelpBoxMessageType.Error: return MessageType.Error;
            }
        }
    }
}