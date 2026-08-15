using System;
using System.Reflection;
using UnityEditor;

public static class AtlasConsoleInspector
{
    public static AtlasConsoleInfo InspectConsole()
    {
        AtlasConsoleInfo result = new();

        Assembly editorAssembly =
            Assembly.GetAssembly(typeof(SceneView));

        Type logEntriesType =
            editorAssembly.GetType("UnityEditor.LogEntries");

        Type logEntryType =
            editorAssembly.GetType("UnityEditor.LogEntry");

        if (logEntriesType == null ||
            logEntryType == null)
        {
            throw new InvalidOperationException(
                "Atlas could not access Unity Console internals."
            );
        }

        MethodInfo getCountMethod =
            logEntriesType.GetMethod(
                "GetCount",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        MethodInfo getEntryMethod =
            logEntriesType.GetMethod(
                "GetEntryInternal",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (getCountMethod == null ||
            getEntryMethod == null)
        {
            throw new InvalidOperationException(
                "Atlas could not locate Unity Console methods."
            );
        }

        FieldInfo messageField =
            logEntryType.GetField(
                "message",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        FieldInfo modeField =
            logEntryType.GetField(
                "mode",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        FieldInfo fileField =
            logEntryType.GetField(
                "file",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        FieldInfo lineField =
            logEntryType.GetField(
                "line",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        int count =
            (int)getCountMethod.Invoke(null, null);

        for (int i = 0; i < count; i++)
        {
            object logEntry =
                Activator.CreateInstance(logEntryType);

            object[] parameters =
            {
                i,
                logEntry
            };

            bool success =
                (bool)getEntryMethod.Invoke(
                    null,
                    parameters
                );

            if (!success)
            {
                continue;
            }

            // GetEntryInternal may update the object
            // passed through the parameter array.
            logEntry = parameters[1];

            string message =
                messageField?.GetValue(logEntry)
                    as string;

            int mode =
                modeField != null
                    ? (int)modeField.GetValue(logEntry)
                    : 0;

            string file =
                fileField?.GetValue(logEntry)
                    as string;

            int line =
                lineField != null
                    ? (int)lineField.GetValue(logEntry)
                    : 0;

            result.Entries.Add(
                new AtlasConsoleEntry
                {
                    Type = GetLogType(mode),
                    Message = message,
                    File = file,
                    Line = line
                }
            );
        }

        return result;
    }

    private static string GetLogType(int mode)
    {
        /*
         * Unity's Console mode is an internal bitmask.
         * For this first probe we're deliberately
         * keeping classification conservative.
         */

        const int ErrorMask =
            (1 << 0) |
            (1 << 1) |
            (1 << 4) |
            (1 << 6) |
            (1 << 8);

        const int WarningMask =
            (1 << 7);

        if ((mode & ErrorMask) != 0)
        {
            return "Error";
        }

        if ((mode & WarningMask) != 0)
        {
            return "Warning";
        }

        return "Log";
    }
}