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
      "Inspects a GameObject in the active Unity scene by name and " +
      "returns its components, serialized properties, and script metadata."
  )]
  public static async Task<string> InspectGameObject(
      AtlasUnityClient unity,
      [Description("The exact or case-insensitive name of the GameObject to inspect.")]
        string objectName)
  {
    return await unity.InspectGameObjectAsync(objectName);
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
      string objectName)
  {
    string encodedName =
        Uri.EscapeDataString(objectName);

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