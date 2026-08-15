using System.Text;
using UnityEditor;
using UnityEngine;

public static class AtlasProjectSearchProbe
{
    [MenuItem("Atlas/Tools/Test Project Search")]
    public static void TestProjectSearch()
    {
        const string query = "moveSpeed";

        AtlasProjectSearchResult result =
            AtlasProjectTools.SearchProject(query);

        StringBuilder report = new();

        report.AppendLine(
            "=== ATLAS PROJECT SEARCH ==="
        );

        report.AppendLine(
            $"Query: {result.Query}"
        );

        report.AppendLine(
            $"Matches: {result.Matches.Count}"
        );

        foreach (AtlasProjectSearchMatch match
                 in result.Matches)
        {
            report.AppendLine();
            report.AppendLine(
                $"{match.Path}:{match.Line}"
            );

            report.AppendLine(
                $"  {match.Text}"
            );
        }

        Debug.Log(report.ToString());
    }
}