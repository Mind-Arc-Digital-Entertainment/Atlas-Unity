using System.Text;
using UnityEditor;
using UnityEngine;

public static class AtlasSceneProbe
{
    [MenuItem("Atlas/Inspect Active Scene")]
    public static void InspectActiveScene()
    {
        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        StringBuilder report = new();

        report.AppendLine("=== ATLAS SCENE REPORT ===");
        report.AppendLine($"Scene: {scene.Name}");
        report.AppendLine($"Path: {scene.Path}");
        report.AppendLine();

        foreach (AtlasGameObjectInfo rootObject
                 in scene.RootObjects)
        {
            AppendGameObject(
                report,
                rootObject,
                0
            );
        }

        Debug.Log(report.ToString());
    }

    private static void AppendGameObject(
        StringBuilder report,
        AtlasGameObjectInfo gameObject,
        int depth)
    {
        string indent =
            new(' ', depth * 2);

        report.AppendLine(
            $"{indent}{gameObject.Name}"
        );

        foreach (AtlasComponentInfo component
                 in gameObject.Components)
        {
            report.AppendLine(
                $"{indent}  - {component.TypeName}"
            );

            if (component.Script != null)
            {
                report.AppendLine(
                    $"{indent}    Script: {component.Script.Path}"
                );

                if (component.Script.IsProjectSource &&
                    !string.IsNullOrWhiteSpace(
                        component.Script.Source))
                {
                    report.AppendLine(
                        $"{indent}    Source:"
                    );

                    report.AppendLine(
                        $"{indent}    ---"
                    );

                    foreach (string line in
                             component.Script.Source.Split('\n'))
                    {
                        report.AppendLine(
                            $"{indent}    {line.TrimEnd('\r')}"
                        );
                    }

                    report.AppendLine(
                        $"{indent}    ---"
                    );
                }
                else if (!component.Script.IsProjectSource)
                {
                    report.AppendLine(
                        $"{indent}    Source: <Package Source Skipped>"
                    );
                }
            }

            foreach (AtlasPropertyInfo property
                     in component.Properties)
            {
                report.AppendLine(
                    $"{indent}    {property.Name}: {property.Value}"
                );
            }
        }

        foreach (AtlasGameObjectInfo child
                 in gameObject.Children)
        {
            AppendGameObject(
                report,
                child,
                depth + 1
            );
        }
    }
}