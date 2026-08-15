using System.Collections.Generic;

[System.Serializable]
public class AtlasSceneInfo
{
    public string Name;
    public string Path;

    public List<AtlasGameObjectInfo> RootObjects = new();
}