using System.Collections.Generic;
using HomaGames.HomaBelly;
using UnityEditor;
using UnityEngine;

public class HomaBellyLogWindow : HomaBellyBaseWindow
{
    private Vector2 logTraceScrollPosition;
    private List<KeyValuePair<HomaBellyEditorLog.Level, string>> logTrace = new List<KeyValuePair<HomaBellyEditorLog.Level, string>>();

    protected override void Draw(Rect windowPosition)
    {
        Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(16, 16));
        EditorGUILayout.BeginHorizontal();
#if UNITY_2019_3_OR_NEWER
        EditorGUILayout.Space(15);
#else
        EditorGUILayout.Space();
#endif
        logTraceScrollPosition = EditorGUILayout.BeginScrollView(logTraceScrollPosition,
            false, false, GUI.skin.horizontalScrollbar,
            GUI.skin.verticalScrollbar,HomaGamesStyle.DarkGreyBoxStyle);
        if (Event.current.type == EventType.Layout)
        {
            if(logTrace.Count!=HomaBellyEditorLog.LogTrace.Count)
                logTraceScrollPosition = new Vector2(0,40*HomaBellyEditorLog.LogTrace.Count);
            logTrace.Clear();
            logTrace.AddRange(HomaBellyEditorLog.LogTrace);
        }

        if (logTrace != null)
        {
            foreach (KeyValuePair<HomaBellyEditorLog.Level, string> logEntry in logTrace)
            {
                Texture2D icon = null;
                if (logEntry.Key == HomaBellyEditorLog.Level.WARNING)
                {
                    icon = HomaGamesStyle.WarningIcon;
                }

                if (logEntry.Key == HomaBellyEditorLog.Level.ERROR)
                {
                    icon = HomaGamesStyle.ErrorIcon;
                }
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(icon), GUILayout.Height(20), GUILayout.Width(20));
                GUILayout.TextField($"{logEntry.Value}", HomaGamesStyle.LogLabelStyle);
                GUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
#if UNITY_2019_3_OR_NEWER
        EditorGUILayout.Space(15);
#else
        EditorGUILayout.Space();
#endif
        EditorGUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIContent buttonContent = new GUIContent();
        buttonContent.image = HomaGamesStyle.ExternalLinkIcon;
        buttonContent.text = "Copy to Clipboard";
        if (GUILayout.Button(buttonContent, HomaGamesStyle.ButtonStyleTexts))
        {
            EditorAnalyticsProxy.TrackEditorAnalyticsEvent("copy_logs_to_clipboard");
            EditorGUIUtility.systemCopyBuffer = HomaBellyEditorLog.GetCopiableLogTrace();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUIUtility.SetIconSize(originalIconSize);
    }

    protected override void OnVisibleFocus()
    {
        // NO-OP
    }
}
