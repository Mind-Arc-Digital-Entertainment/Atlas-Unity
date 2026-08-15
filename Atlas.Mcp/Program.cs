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
    try
    {
      return await httpClient.GetStringAsync(
          "/atlas/scene"
      );
    }
    catch (HttpRequestException)
    {
      return
          "{\"error\":\"Atlas could not connect to Unity. " +
          "Make sure the Atlas Bridge is running.\"}";
    }
  }
}