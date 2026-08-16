using System;
using System.Collections.Generic;

public static class AtlasSceneTools
{
    /// <summary>
    /// Returns basic information about the currently active Unity scene.
    /// </summary>
    public static AtlasSceneSummary GetActiveScene()
    {
        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        return new AtlasSceneSummary
        {
            Name = scene.Name,
            Path = scene.Path
        };
    }

    /// <summary>
    /// Returns the names of every GameObject in the active scene.
    /// </summary>
    public static List<string> ListSceneObjects()
    {
        return GetSceneObjectList().Objects;
    }

    public static AtlasSceneObjectList GetSceneObjectList()
    {
        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        AtlasSceneObjectList result = new();

        foreach (AtlasGameObjectInfo rootObject
                 in scene.RootObjects)
        {
            AddSceneObject(
                rootObject,
                result
            );
        }

        return result;
    }

    /// <summary>
    /// Finds a GameObject in the active scene by name.
    /// </summary>
    public static AtlasGameObjectInfo InspectGameObject(
        string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        foreach (AtlasGameObjectInfo rootObject
                 in scene.RootObjects)
        {
            AtlasGameObjectInfo result =
                FindGameObject(rootObject, objectName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void AddSceneObject(
       AtlasGameObjectInfo gameObject,
       AtlasSceneObjectList result)
    {
        result.Objects.Add(gameObject.Name);

        result.ObjectReferences.Add(
            new AtlasSceneObjectReference
            {
                Name = gameObject.Name,
                GlobalObjectId = gameObject.GlobalObjectId,
                HierarchyPath = gameObject.HierarchyPath,
                ScenePath = gameObject.ScenePath
            }
        );

        foreach (AtlasGameObjectInfo child
                 in gameObject.Children)
        {
            AddSceneObject(
                child,
                result
            );
        }
    }

    private static AtlasGameObjectInfo FindGameObject(
        AtlasGameObjectInfo gameObject,
        string objectName)
    {
        if (string.Equals(
                gameObject.Name,
                objectName,
                StringComparison.OrdinalIgnoreCase))
        {
            return gameObject;
        }

        foreach (AtlasGameObjectInfo child
                 in gameObject.Children)
        {
            AtlasGameObjectInfo result =
                FindGameObject(child, objectName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}