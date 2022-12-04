using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaBelly
{
    [CustomEditor(typeof(CrossPromotionPlayer))]
    public class CrossPromotionPlayerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            RawImage image = serializedObject.FindProperty("m_displayImage").objectReferenceValue as RawImage;
            if (image && !image.canvas)
            {
                EditorGUILayout.HelpBox("The display image \"" + image.name + "\" must be inside a canvas to be visible.", MessageType.Warning);
            }
            base.OnInspectorGUI();
        }
    }
}