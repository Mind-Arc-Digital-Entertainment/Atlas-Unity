using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class AtlasHealthTools
{
    public const string ProtocolVersion = "1.1";

    public static AtlasHealthInfo GetHealth()
    {
        string projectPath =
            Directory.GetParent(Application.dataPath)?.FullName
            ?? string.Empty;

        return new AtlasHealthInfo
        {
            ProtocolVersion = ProtocolVersion,
            UnityVersion = Application.unityVersion,

            ProjectName = Application.productName,
            ProjectPath = projectPath,

            BridgeRunning = AtlasLocalBridge.IsRunning,
            IsCompiling = EditorApplication.isCompiling,

            Capabilities = new List<string>
            {
                "get_health",
                "get_active_scene",
                "list_scene_objects",
                "inspect_game_object",
                "search_project",
                "read_script",
                "get_console_logs"
            }
        };
    }
}