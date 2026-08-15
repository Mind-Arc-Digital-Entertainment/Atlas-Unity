using System.Text;
using UnityEditor;
using UnityEngine;

public static class AtlasToolProbe
{
    [MenuItem("Atlas/Tools/Get Active Scene")]
    public static void TestGetActiveScene()
    {
        AtlasSceneSummary scene =
            AtlasSceneTools.GetActiveScene();

        Debug.Log(
            $"Atlas Tool: GetActiveScene\n" +
            $"Name: {scene.Name}\n" +
            $"Path: {scene.Path}"
        );
    }

    [MenuItem("Atlas/Tools/List Scene Objects")]
    public static void TestListSceneObjects()
    {
        var objects =
            AtlasSceneTools.ListSceneObjects();

        StringBuilder report = new();

        report.AppendLine(
            "Atlas Tool: ListSceneObjects"
        );

        foreach (string objectName in objects)
        {
            report.AppendLine(
                $"- {objectName}"
            );
        }

        Debug.Log(report.ToString());
    }

    [MenuItem("Atlas/Tools/Inspect Enemy")]
    public static void TestInspectEnemy()
    {
        AtlasGameObjectInfo enemy =
            AtlasSceneTools.InspectGameObject("Enemy");

        if (enemy == null)
        {
            Debug.LogWarning(
                "Atlas could not find Enemy."
            );

            return;
        }

        StringBuilder report = new();

        report.AppendLine(
            $"Atlas Tool: InspectGameObject"
        );

        report.AppendLine(
            $"Object: {enemy.Name}"
        );

        foreach (AtlasComponentInfo component
                 in enemy.Components)
        {
            report.AppendLine(
                $"Component: {component.TypeName}"
            );

            if (component.Script != null)
            {
                report.AppendLine(
                    $"  Script: {component.Script.Path}"
                );
            }

            foreach (AtlasPropertyInfo property
                     in component.Properties)
            {
                report.AppendLine(
                    $"  {property.Name}: {property.Value}"
                );
            }
        }

        Debug.Log(report.ToString());
    }
}