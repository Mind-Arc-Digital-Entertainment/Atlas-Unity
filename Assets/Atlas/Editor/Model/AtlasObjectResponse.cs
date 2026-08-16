using System.Collections.Generic;

[System.Serializable]
public class AtlasObjectResponse
{
    public bool Found;
    public bool Ambiguous;
    public int MatchCount;
    public string LookupKind;

    public List<AtlasSceneObjectReference>
        Matches = new();

    public AtlasGameObjectInfo Object;
}