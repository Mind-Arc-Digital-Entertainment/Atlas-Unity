using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AtlasSceneProbe
{
    [MenuItem("Atlas/Inspect Active Scene")]
    public static void InspectActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        StringBuilder report = new();

        report.AppendLine("=== ATLAS SCENE REPORT ===");
        report.AppendLine($"Scene: {scene.name}");
        report.AppendLine($"Path: {scene.path}");
        report.AppendLine();

        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            AppendGameObject(report, rootObject.transform, 0);
        }

        Debug.Log(report.ToString());
    }

    private static void AppendGameObject(
        StringBuilder report,
        Transform transform,
        int depth)
    {
        string indent = new(' ', depth * 2);

        report.AppendLine($"{indent}{transform.name}");

        Component[] components = transform.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
            {
                report.AppendLine($"{indent}  - Missing Script");
                continue;
            }

            report.AppendLine(
                $"{indent}  - {component.GetType().Name}"
            );
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            AppendGameObject(
                report,
                transform.GetChild(i),
                depth + 1
            );
        }
    }
}