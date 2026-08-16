using System.Collections.Generic;

[System.Serializable]
public class AtlasSceneObjectList
{
    public List<string> Objects = new();

    public List<AtlasSceneObjectReference>
        ObjectReferences = new();
}