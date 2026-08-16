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
    /// Preserves the legacy list contract.
    /// </summary>
    public static List<string> ListSceneObjects()
    {
        return GetSceneObjectList().Objects;
    }

    /// <summary>
    /// Returns scene object names together with identity references.
    /// </summary>
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
    /// Legacy convenience method.
    ///
    /// Returns the object only when the name resolves uniquely.
    /// An ambiguous name now returns null rather than selecting
    /// an arbitrary first match.
    /// </summary>
    public static AtlasGameObjectInfo InspectGameObject(
        string objectName)
    {
        AtlasObjectResponse response =
            InspectGameObjectByName(objectName);

        return response.Found
            ? response.Object
            : null;
    }

    /// <summary>
    /// Inspects a GameObject by case-insensitive convenience name.
    ///
    /// Multiple matching objects are reported as ambiguous.
    /// </summary>
    public static AtlasObjectResponse InspectGameObjectByName(
        string objectName)
    {
        List<AtlasGameObjectInfo> matches = new();

        if (string.IsNullOrWhiteSpace(objectName))
        {
            return CreateLookupResponse(
                "name",
                matches
            );
        }

        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        foreach (AtlasGameObjectInfo rootObject
                 in scene.RootObjects)
        {
            CollectByName(
                rootObject,
                objectName,
                matches
            );
        }

        return CreateLookupResponse(
            "name",
            matches
        );
    }

    /// <summary>
    /// Inspects a GameObject by exact GlobalObjectId.
    /// </summary>
    public static AtlasObjectResponse
        InspectGameObjectByGlobalObjectId(
            string globalObjectId)
    {
        List<AtlasGameObjectInfo> matches = new();

        if (string.IsNullOrWhiteSpace(globalObjectId))
        {
            return CreateLookupResponse(
                "globalObjectId",
                matches
            );
        }

        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        foreach (AtlasGameObjectInfo rootObject
                 in scene.RootObjects)
        {
            CollectByGlobalObjectId(
                rootObject,
                globalObjectId,
                matches
            );
        }

        return CreateLookupResponse(
            "globalObjectId",
            matches
        );
    }

    /// <summary>
    /// Inspects a GameObject by exact scene path and hierarchy path.
    /// </summary>
    public static AtlasObjectResponse InspectGameObjectByPath(
        string scenePath,
        string hierarchyPath)
    {
        List<AtlasGameObjectInfo> matches = new();

        if (string.IsNullOrWhiteSpace(scenePath) ||
            string.IsNullOrWhiteSpace(hierarchyPath))
        {
            return CreateLookupResponse(
                "path",
                matches
            );
        }

        AtlasSceneInfo scene =
            AtlasSceneInspector.InspectActiveScene();

        foreach (AtlasGameObjectInfo rootObject
                 in scene.RootObjects)
        {
            CollectByPath(
                rootObject,
                scenePath,
                hierarchyPath,
                matches
            );
        }

        return CreateLookupResponse(
            "path",
            matches
        );
    }

    private static void AddSceneObject(
        AtlasGameObjectInfo gameObject,
        AtlasSceneObjectList result)
    {
        result.Objects.Add(gameObject.Name);

        result.ObjectReferences.Add(
            CreateObjectReference(gameObject)
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

    private static void CollectByName(
        AtlasGameObjectInfo gameObject,
        string objectName,
        List<AtlasGameObjectInfo> matches)
    {
        if (string.Equals(
                gameObject.Name,
                objectName,
                StringComparison.OrdinalIgnoreCase))
        {
            matches.Add(gameObject);
        }

        foreach (AtlasGameObjectInfo child
                 in gameObject.Children)
        {
            CollectByName(
                child,
                objectName,
                matches
            );
        }
    }

    private static void CollectByGlobalObjectId(
        AtlasGameObjectInfo gameObject,
        string globalObjectId,
        List<AtlasGameObjectInfo> matches)
    {
        if (string.Equals(
                gameObject.GlobalObjectId,
                globalObjectId,
                StringComparison.Ordinal))
        {
            matches.Add(gameObject);
        }

        foreach (AtlasGameObjectInfo child
                 in gameObject.Children)
        {
            CollectByGlobalObjectId(
                child,
                globalObjectId,
                matches
            );
        }
    }

    private static void CollectByPath(
        AtlasGameObjectInfo gameObject,
        string scenePath,
        string hierarchyPath,
        List<AtlasGameObjectInfo> matches)
    {
        bool sceneMatches =
            string.Equals(
                gameObject.ScenePath,
                scenePath,
                StringComparison.Ordinal
            );

        bool hierarchyMatches =
            string.Equals(
                gameObject.HierarchyPath,
                hierarchyPath,
                StringComparison.Ordinal
            );

        if (sceneMatches &&
            hierarchyMatches)
        {
            matches.Add(gameObject);
        }

        foreach (AtlasGameObjectInfo child
                 in gameObject.Children)
        {
            CollectByPath(
                child,
                scenePath,
                hierarchyPath,
                matches
            );
        }
    }

    private static AtlasObjectResponse CreateLookupResponse(
        string lookupKind,
        List<AtlasGameObjectInfo> matches)
    {
        AtlasObjectResponse response = new()
        {
            Found = matches.Count == 1,
            Ambiguous = matches.Count > 1,
            MatchCount = matches.Count,
            LookupKind = lookupKind
        };

        if (matches.Count == 1)
        {
            response.Object = matches[0];
            return response;
        }

        if (matches.Count > 1)
        {
            foreach (AtlasGameObjectInfo match
                     in matches)
            {
                response.Matches.Add(
                    CreateObjectReference(match)
                );
            }
        }

        return response;
    }

    private static AtlasSceneObjectReference
        CreateObjectReference(
            AtlasGameObjectInfo gameObject)
    {
        return new AtlasSceneObjectReference
        {
            Name = gameObject.Name,
            GlobalObjectId =
                gameObject.GlobalObjectId,
            HierarchyPath =
                gameObject.HierarchyPath,
            ScenePath =
                gameObject.ScenePath
        };
    }
}