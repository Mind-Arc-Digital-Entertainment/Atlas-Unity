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

            AppendScriptPath(
                report,
                component,
                depth + 2
            );

            AppendSerializedProperties(
                report,
                component,
                depth + 2
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

    private static void AppendSerializedProperties(
        StringBuilder report,
        Component component,
        int depth)
    {
        SerializedObject serializedObject = new(component);
        SerializedProperty property =
            serializedObject.GetIterator();

        string indent = new(' ', depth * 2);

        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Unity exposes the script reference itself as m_Script.
            // It isn't useful project state for our report.
            if (property.propertyPath == "m_Script")
            {
                continue;
            }

            string value = GetPropertyValue(property);

            report.AppendLine(
                $"{indent}{property.displayName}: {value}"
            );
        }
    }

    private static void AppendScriptPath(
    StringBuilder report,
    Component component,
    int depth)
    {
        if (component is not MonoBehaviour monoBehaviour)
        {
            return;
        }

        MonoScript monoScript =
            MonoScript.FromMonoBehaviour(monoBehaviour);

        if (monoScript == null)
        {
            return;
        }

        string path = AssetDatabase.GetAssetPath(monoScript);
        string indent = new(' ', depth * 2);

        report.AppendLine($"{indent}Script: {path}");
    }

    private static string GetPropertyValue(
        SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                return property.intValue.ToString();

            case SerializedPropertyType.Boolean:
                return property.boolValue.ToString();

            case SerializedPropertyType.Float:
                return property.floatValue.ToString("0.###");

            case SerializedPropertyType.String:
                return $"\"{property.stringValue}\"";

            case SerializedPropertyType.Enum:
                return property.enumDisplayNames[
                    property.enumValueIndex
                ];

            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue != null
                    ? property.objectReferenceValue.name
                    : "NULL";

            case SerializedPropertyType.Vector2:
                return property.vector2Value.ToString();

            case SerializedPropertyType.Vector3:
                return property.vector3Value.ToString();

            case SerializedPropertyType.Color:
                return property.colorValue.ToString();

            default:
                return $"<{property.propertyType}>";
        }
    }
}