using System.Text;
using UnityEditor;
using UnityEngine;

public static class AtlasConsoleProbe
{
    [MenuItem("Atlas/Tools/Get Console Logs")]
    public static void GetConsoleLogs()
    {
        AtlasConsoleInfo console =
            AtlasConsoleTools.GetConsoleLogs();

        StringBuilder report = new();

        report.AppendLine(
            "=== ATLAS CONSOLE REPORT ==="
        );

        foreach (AtlasConsoleEntry entry
                 in console.Entries)
        {
            report.AppendLine(
                $"[{entry.Type}] {entry.Message}"
            );

            if (!string.IsNullOrWhiteSpace(entry.File))
            {
                report.AppendLine(
                    $"  {entry.File}:{entry.Line}"
                );
            }
        }

        Debug.Log(report.ToString());
    }

    [MenuItem("Atlas/Test/Generate Test Error")]
    public static void GenerateTestError()
    {
        Debug.LogError(
            "ATLAS_TEST_ERROR"
        );
    }
}