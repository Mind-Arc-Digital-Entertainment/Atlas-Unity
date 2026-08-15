using System.Collections.Generic;

[System.Serializable]
public class AtlasGameObjectInfo
{
    public string Name;

    public List<AtlasComponentInfo> Components = new();
    public List<AtlasGameObjectInfo> Children = new();
}