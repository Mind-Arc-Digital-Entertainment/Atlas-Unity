using System;
using System.IO;
using UnityEditor;

public static class AtlasProjectSearcher
{
    public static AtlasProjectSearchResult Search(
        string query)
    {
        AtlasProjectSearchResult result = new()
        {
            Query = query
        };

        if (string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        string[] scriptGuids =
            AssetDatabase.FindAssets("t:MonoScript");

        foreach (string guid in scriptGuids)
        {
            string assetPath =
                AssetDatabase.GUIDToAssetPath(guid);

            /*
             * For Atlas project intelligence we only
             * want project-owned scripts right now.
             *
             * Package source can be added later as a
             * separate, intentional capability.
             */
            if (!assetPath.StartsWith(
                    "Assets/",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!assetPath.EndsWith(
                    ".cs",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fullPath =
                Path.GetFullPath(assetPath);

            if (!File.Exists(fullPath))
            {
                continue;
            }

            string[] lines;

            try
            {
                lines = File.ReadAllLines(fullPath);
            }
            catch
            {
                continue;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].IndexOf(
                        query,
                        StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                result.Matches.Add(
                    new AtlasProjectSearchMatch
                    {
                        Path = assetPath,
                        Line = i + 1,
                        Text = lines[i].Trim()
                    }
                );
            }
        }

        return result;
    }
}