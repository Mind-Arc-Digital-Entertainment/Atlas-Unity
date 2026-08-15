using UnityEditor;
using UnityEngine;

public static class AtlasScriptReadProbe
{
    [MenuItem("Atlas/Tools/Test Read Script")]
    public static void TestReadScript()
    {
        const string path =
            "Assets/Atlas/Test/Scripts/TestEnemy.cs";

        AtlasScriptReadResult result =
            AtlasProjectTools.ReadScript(path);

        if (!result.Found)
        {
            Debug.LogWarning(
                $"Atlas could not read script: {path}"
            );

            return;
        }

        Debug.Log(
            "=== ATLAS SCRIPT READ ===\n" +
            $"Path: {result.Path}\n\n" +
            result.Source
        );
    }
}