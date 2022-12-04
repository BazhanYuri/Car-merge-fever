using System.Collections.Generic;
using System.Linq.Expressions;
using HomaGames.HomaBelly;
using UnityEditor;
using UnityEngine;

public class HomaBellyPackagesWindow : HomaBellyBaseWindow
{
    private Vector2 installedPackagesScrollPosition;

    protected override void Draw(Rect windowPosition)
    {
        // Gather packages to be installed
        PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();
        List<PackageComponent> components = new List<PackageComponent>();
        if (pluginManifest != null && pluginManifest.Packages != null)
        {
            if (pluginManifest.Packages.CorePackages != null)
            {
                components.AddRange(pluginManifest.Packages.CorePackages);
            }

            if (pluginManifest.Packages.MediationLayers != null)
            {
                components.AddRange(pluginManifest.Packages.MediationLayers);
            }

            if (pluginManifest.Packages.AttributionPlatforms != null)
            {
                components.AddRange(pluginManifest.Packages.AttributionPlatforms);
            }

            if (pluginManifest.Packages.AdNetworks != null)
            {
                components.AddRange(pluginManifest.Packages.AdNetworks);
            }

            if (pluginManifest.Packages.AnalyticsSystems != null)
            {
                components.AddRange(pluginManifest.Packages.AnalyticsSystems);
            }

            if (pluginManifest.Packages.Others != null)
            {
                components.AddRange(pluginManifest.Packages.Others);
            }
        }

        // Start drawing
        Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(16, 16));

        GUILayout.Space(EditorGUIUtility.singleLineHeight);

        var scrollViewHeight = windowPosition.height - 280;
        
        if (components != null && components.Count > 0 && scrollViewHeight>20)
        {
            foreach (PackageComponent component in components)
            {
                PackageComponent packageComponent = pluginManifest.Packages.GetPackageComponent(component.Id, component.Version);
                if (packageComponent != null)
                {
                    Texture2D icon = HomaGamesStyle.WhiteCircleIcon;
                    string installationFailureTooltip = "";
                    string troubleshootingLink = "";
                    if (!homaBellyWindowController.IsInstalling())
                    {
                        if (PackageCommon.IsPackageInstalled(packageComponent))
                        {
                            icon = HomaGamesStyle.FoundIcon;
                        }
                        else
                        {
                            icon = HomaGamesStyle.NotFoundIcon;

                            if (packageComponent.Data != null)
                            {
                                packageComponent.Data.TryGetValue("s_install_failure_tooltip", out installationFailureTooltip);
                                packageComponent.Data.TryGetValue("s_troubleshooting_link", out troubleshootingLink);    
                            }
                        }
                    }

                    GUILayout.BeginHorizontal(new GUIContent("", installationFailureTooltip), new GUIStyle()
                    {
                        padding = new RectOffset((int)windowPosition.width / 2 - 100, 0, 0, 0)
                    });
                    GUILayout.Label(new GUIContent(icon), GUILayout.Height(25), GUILayout.Width(20));
                    GUIStyle secondaryLeftAlignet = new GUIStyle(HomaGamesStyle.SecondaryLabelStyle);
                    secondaryLeftAlignet.alignment = TextAnchor.MiddleLeft;
                    GUILayout.Label($"{packageComponent.GetName()}", secondaryLeftAlignet, GUILayout.Height(25), GUILayout.Width(200));
                    if (!string.IsNullOrEmpty(troubleshootingLink))
                    {
                        if (GUILayout.Button(new GUIContent("Troubleshoot", HomaGamesStyle.ExternalLinkIcon, ""), HomaGamesStyle.ButtonOnlyTextHyperlinkStyle, GUILayout.Height(20)))
                        {
                            Application.OpenURL(troubleshootingLink);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(HomaGamesStyle.NotFoundIcon), GUILayout.Height(20), GUILayout.Width(20));
                    GUILayout.Label($"Could not find {component.GetName()}");
                    GUILayout.EndHorizontal();
                }
            }
        }
        
        EditorGUIUtility.SetIconSize(originalIconSize);
    }

    protected override void OnVisibleFocus()
    {
        // NO-OP
    }
}
