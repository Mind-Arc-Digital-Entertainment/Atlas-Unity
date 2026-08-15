public static class AtlasProjectTools
{
    public static AtlasProjectSearchResult SearchProject(
        string query)
    {
        return AtlasProjectSearcher.Search(query);
    }
}