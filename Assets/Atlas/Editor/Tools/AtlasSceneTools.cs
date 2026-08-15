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
        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        List<string> objects = new();

        foreach (AtlasGameObjectInfo rootObject
                 in scene.RootObjects)
        {
            AddObjectNames(rootObject, objects);
        }

        return objects;
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

    private static void AddObjectNames(
        AtlasGameObjectInfo gameObject,
        List<string> objects)
    {
        objects.Add(gameObject.Name);

        foreach (AtlasGameObjectInfo child
                 in gameObject.Children)
        {
            AddObjectNames(child, objects);
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