using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AtlasSceneInspector
{
    public static AtlasSceneInfo InspectActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        AtlasSceneInfo sceneInfo = new()
        {
            Name = scene.name,
            Path = scene.path
        };

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            sceneInfo.RootObjects.Add(
                InspectGameObject(rootObject)
            );
        }

        return sceneInfo;
    }

    private static AtlasGameObjectInfo InspectGameObject(
        GameObject gameObject)
    {
        AtlasGameObjectInfo objectInfo = new()
        {
            Name = gameObject.name
        };

        foreach (Component component
                 in gameObject.GetComponents<Component>())
        {
            objectInfo.Components.Add(
                InspectComponent(component)
            );
        }

        Transform transform = gameObject.transform;

        for (int i = 0; i < transform.childCount; i++)
        {
            objectInfo.Children.Add(
                InspectGameObject(
                    transform.GetChild(i).gameObject
                )
            );
        }

        return objectInfo;
    }

    private static AtlasComponentInfo InspectComponent(
        Component component)
    {
        if (component == null)
        {
            return new AtlasComponentInfo
            {
                TypeName = "Missing Script"
            };
        }

        AtlasComponentInfo componentInfo = new()
        {
            TypeName = component.GetType().Name
        };

        componentInfo.Script = InspectScript(component);

        SerializedObject serializedObject =
            new(component);

        SerializedProperty property =
            serializedObject.GetIterator();

        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.propertyPath == "m_Script")
            {
                continue;
            }

            componentInfo.Properties.Add(
                new AtlasPropertyInfo
                {
                    Name = property.displayName,
                    PropertyPath = property.propertyPath,
                    Type = property.propertyType.ToString(),
                    Value = GetPropertyValue(property)
                }
            );
        }

        return componentInfo;
    }

    private static AtlasScriptInfo InspectScript(
        Component component)
    {
        if (component is not MonoBehaviour monoBehaviour)
        {
            return null;
        }

        MonoScript monoScript =
            MonoScript.FromMonoBehaviour(monoBehaviour);

        if (monoScript == null)
        {
            return null;
        }

        string path =
            AssetDatabase.GetAssetPath(monoScript);

        AtlasScriptInfo scriptInfo = new()
        {
            Path = path,
            IsProjectSource =
                path.StartsWith("Assets/")
        };

        if (!scriptInfo.IsProjectSource)
        {
            return scriptInfo;
        }

        string absolutePath =
            Path.GetFullPath(path);

        if (File.Exists(absolutePath))
        {
            scriptInfo.Source =
                File.ReadAllText(absolutePath);
        }

        return scriptInfo;
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
                return property.stringValue;

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