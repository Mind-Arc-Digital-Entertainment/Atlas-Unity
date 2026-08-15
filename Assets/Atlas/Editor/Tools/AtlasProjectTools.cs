using System;
using System.IO;

public static class AtlasProjectTools
{
    public static AtlasProjectSearchResult SearchProject(
        string query)
    {
        return AtlasProjectSearcher.Search(query);
    }

    public static AtlasScriptReadResult ReadScript(
        string assetPath)
    {
        AtlasScriptReadResult result = new()
        {
            Found = false,
            Path = assetPath
        };

        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return result;
        }

        /*
         * Atlas only reads project-owned scripts through this
         * tool. Package source can become a separate capability
         * later if we decide we need it.
         */
        if (!assetPath.StartsWith(
                "Assets/",
                StringComparison.OrdinalIgnoreCase))
        {
            return result;
        }

        if (!assetPath.EndsWith(
                ".cs",
                StringComparison.OrdinalIgnoreCase))
        {
            return result;
        }

        string fullPath =
            Path.GetFullPath(assetPath);

        if (!File.Exists(fullPath))
        {
            return result;
        }

        result.Source =
            File.ReadAllText(fullPath);

        result.Found = true;

        return result;
    }
}