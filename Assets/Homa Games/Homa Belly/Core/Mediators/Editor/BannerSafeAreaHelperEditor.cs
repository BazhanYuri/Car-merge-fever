using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    [CustomEditor(typeof(BannerSafeAreaHelper))]
    [CanEditMultipleObjects]
    public class BannerSafeAreaHelperEditor : Editor
    {
        private SerializedProperty defaultBannerPositionProperty;
        private SerializedProperty EnableExtraPlacementIdProperty;
        private SerializedProperty ExtraPlacementsProperty;

        private BannerSafeAreaHelper TypedTarget => (BannerSafeAreaHelper)target;
        
        void OnEnable()
        {
            defaultBannerPositionProperty = serializedObject.FindProperty(nameof(BannerSafeAreaHelper.DefaultBannerPosition));
            EnableExtraPlacementIdProperty = serializedObject.FindProperty(nameof(BannerSafeAreaHelper.EnableExtraPlacementId));
            ExtraPlacementsProperty = serializedObject.FindProperty(nameof(BannerSafeAreaHelper.ExtraPlacements));
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool hasASafeAreaChild = false;
            foreach (Transform child in TypedTarget.transform)
            {
                if (child.gameObject.name == "SafeArea")
                {
                    hasASafeAreaChild = true;
                    break;
                }
            }
            
            if (! hasASafeAreaChild)
            {
                EditorGUILayout.HelpBox("This component will resize its child named \"SafeArea\" to fit " +
                                        "in Unity's safe area, and outside of banners. It needs a child named SafeArea " +
                                        "to work.", MessageType.Error);

                if (GUILayout.Button("Move all children into a SafeArea object"))
                {
                    var safeArea = new GameObject("SafeArea");
                    safeArea.layer = TypedTarget.gameObject.layer;
                    var safeAreaTransform = safeArea.AddComponent<RectTransform>();

                    safeAreaTransform.parent = TypedTarget.transform;
                    
                    safeAreaTransform.anchorMin = Vector2.zero;
                    safeAreaTransform.anchorMax = Vector2.one;
                    
                    safeAreaTransform.anchoredPosition = Vector2.zero;
                    safeAreaTransform.sizeDelta = Vector2.zero;
                    
                    safeAreaTransform.localScale = Vector3.one;
                    safeAreaTransform.localRotation = Quaternion.identity;
                    
                    foreach (Transform child in TypedTarget.transform)
                    {
                        child.parent = safeAreaTransform;
                    }
                }
                
                EditorGUILayout.Space(20);
            }
            
            
            EditorGUILayout.PropertyField(defaultBannerPositionProperty);
            EditorGUILayout.PropertyField(EnableExtraPlacementIdProperty);

            if (EnableExtraPlacementIdProperty.boolValue)
            {
                EditorGUILayout.PropertyField(ExtraPlacementsProperty);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
