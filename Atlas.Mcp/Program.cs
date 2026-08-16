using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);

/*
 * MCP owns stdout when using stdio transport.
 * Logs must therefore go to stderr so they cannot
 * corrupt MCP protocol messages.
 */
builder.Logging.AddConsole(options =>
{
  options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddHttpClient<AtlasUnityClient>(client =>
    {
      client.BaseAddress =
          new Uri("http://127.0.0.1:52741");
    });

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();


[McpServerToolType]
public static class AtlasTools
{
  [McpServerTool]
  [Description(
      "Returns the name and asset path of the active scene " +
      "in the connected Unity Editor."
  )]
  public static async Task<string> GetActiveScene(
      AtlasUnityClient unity)
  {
    return await unity.GetActiveSceneAsync();
  }

  [McpServerTool]
  [Description(
      "Returns the names of all GameObjects in the active Unity scene."
  )]
  public static async Task<string> ListSceneObjects(
      AtlasUnityClient unity)
  {
    return await unity.ListSceneObjectsAsync();
  }

  [McpServerTool]
  [Description(
      "Inspects exactly one GameObject in the active Unity scene. " +
      "Use one selector form only: objectName, globalObjectId, " +
      "or scenePath together with hierarchyPath. " +
      "Name lookup is case-insensitive and may report ambiguity."
  )]
  public static async Task<string> InspectGameObject(
      AtlasUnityClient unity,
      [Description(
        "Convenience GameObject name lookup. Case-insensitive and may be ambiguous."
    )]
    string? objectName = null,
      [Description(
        "Exact Unity GlobalObjectId for stable scene-object lookup."
    )]
    string? globalObjectId = null,
      [Description(
        "Unity scene asset path. Must be supplied together with hierarchyPath."
    )]
    string? scenePath = null,
      [Description(
        "Canonical Atlas hierarchy path. Must be supplied together with scenePath."
    )]
    string? hierarchyPath = null)
  {
    return await unity.InspectGameObjectAsync(
        objectName,
        globalObjectId,
        scenePath,
        hierarchyPath
    );
  }

  [McpServerTool]
  [Description(
    "Returns the current entries from the connected Unity Editor Console, " +
    "including log type, message, source file, and line number."
)]
  public static async Task<string> GetConsoleLogs(
    AtlasUnityClient unity)
  {
    return await unity.GetConsoleLogsAsync();
  }

  [McpServerTool]
  [Description(
    "Searches project-owned C# scripts in the connected Unity project " +
    "for a case-insensitive text query and returns matching files, lines, " +
    "and source text."
)]
  public static async Task<string> SearchProject(
    AtlasUnityClient unity,
    [Description("The text to search for across project C# scripts.")]
    string query)
  {
    return await unity.SearchProjectAsync(query);
  }

  [McpServerTool]
  [Description(
    "Reads the full source of a project-owned C# script " +
    "from the connected Unity project."
)]
  public static async Task<string> ReadScript(
    AtlasUnityClient unity,
    [Description(
        "Unity asset path of the C# script, for example " +
        "Assets/Scripts/PlayerMovement.cs."
    )]
    string path)
  {
    return await unity.ReadScriptAsync(path);
  }

  [McpServerTool]
  [Description(
    "Returns Atlas bridge health and capability information from the connected Unity project."
)]
  public static async Task<string> GetHealth(
    AtlasUnityClient unity)
  {
    return await unity.GetHealthAsync();
  }
}


public sealed class AtlasUnityClient
{
  private readonly HttpClient httpClient;

  public AtlasUnityClient(HttpClient httpClient)
  {
    this.httpClient = httpClient;
  }

  public async Task<string> GetActiveSceneAsync()
  {
    return await SendRequestAsync(
        "/atlas/scene"
    );
  }

  public async Task<string> ListSceneObjectsAsync()
  {
    return await SendRequestAsync(
        "/atlas/scene/objects"
    );
  }

  public async Task<string> InspectGameObjectAsync(
      string? objectName = null,
      string? globalObjectId = null,
      string? scenePath = null,
      string? hierarchyPath = null)
  {
    bool hasName =
        !string.IsNullOrWhiteSpace(objectName);

    bool hasId =
        !string.IsNullOrWhiteSpace(globalObjectId);

    bool hasScene =
        !string.IsNullOrWhiteSpace(scenePath);

    bool hasPath =
        !string.IsNullOrWhiteSpace(hierarchyPath);

    if (hasScene != hasPath)
    {
      return
          "{\"error\":\"Scene and hierarchyPath must be provided together\"}";
    }

    int selectorCount =
        (hasName ? 1 : 0) +
        (hasId ? 1 : 0) +
        (hasScene && hasPath ? 1 : 0);

    if (selectorCount != 1)
    {
      return
          "{\"error\":\"Exactly one object selector is required: " +
          "objectName, globalObjectId, or scenePath and hierarchyPath\"}";
    }

    if (hasId)
    {
      string encodedId =
          Uri.EscapeDataString(globalObjectId!);

      return await SendRequestAsync(
          $"/atlas/object?id={encodedId}"
      );
    }

    if (hasScene)
    {
      string encodedScene =
          Uri.EscapeDataString(scenePath!);

      string encodedPath =
          Uri.EscapeDataString(hierarchyPath!);

      return await SendRequestAsync(
          $"/atlas/object?scene={encodedScene}&path={encodedPath}"
      );
    }

    string encodedName =
        Uri.EscapeDataString(objectName!);

    return await SendRequestAsync(
        $"/atlas/object?name={encodedName}"
    );
  }

  private async Task<string> SendRequestAsync(
      string path)
  {
    try
    {
      return await httpClient.GetStringAsync(
          path
      );
    }
    catch (HttpRequestException)
    {
      return
          "{\"error\":\"Atlas could not connect to Unity. " +
          "Make sure the Atlas Bridge is running.\"}";
    }
  }

  public async Task<string> GetConsoleLogsAsync()
  {
    return await SendRequestAsync(
        "/atlas/console"
    );
  }

  public async Task<string> SearchProjectAsync(
    string query)
  {
    string encodedQuery =
        Uri.EscapeDataString(query);

    return await SendRequestAsync(
        $"/atlas/project/search?query={encodedQuery}"
    );
  }

  public async Task<string> ReadScriptAsync(
    string path)
  {
    string encodedPath =
        Uri.EscapeDataString(path);

    return await SendRequestAsync(
        $"/atlas/project/script?path={encodedPath}"
    );
  }

  public async Task<string> GetHealthAsync()
  {
    return await SendRequestAsync(
        "/atlas/health"
    );
  }
}