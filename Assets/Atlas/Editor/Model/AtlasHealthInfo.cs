using System.Collections.Generic;

[System.Serializable]
public class AtlasHealthInfo
{
    public string ProtocolVersion;
    public string UnityVersion;

    public string ProjectName;
    public string ProjectPath;

    public bool BridgeRunning;
    public bool IsCompiling;

    public List<string> Capabilities = new();
}