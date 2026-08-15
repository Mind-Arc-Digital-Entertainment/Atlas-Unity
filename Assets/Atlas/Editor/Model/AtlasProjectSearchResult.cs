using System.Collections.Generic;

[System.Serializable]
public class AtlasProjectSearchResult
{
    public string Query;

    public List<AtlasProjectSearchMatch> Matches = new();
}