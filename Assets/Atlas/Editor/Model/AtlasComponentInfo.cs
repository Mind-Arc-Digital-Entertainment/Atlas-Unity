using System.Collections.Generic;

[System.Serializable]
public class AtlasComponentInfo
{
    public string TypeName;

    public AtlasScriptInfo Script;

    public List<AtlasPropertyInfo> Properties = new();
}