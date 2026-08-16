using System.Collections.Generic;

[System.Serializable]
public class AtlasGameObjectInfo
{
    public string Name;

    public string GlobalObjectId;
    public string HierarchyPath;
    public string ScenePath;

    public List<AtlasComponentInfo> Components = new();
    public List<AtlasGameObjectInfo> Children = new();
}