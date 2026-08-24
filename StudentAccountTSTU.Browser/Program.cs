using System.Runtime.Versioning;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Browser;

using StudentAccountTSTU;

internal sealed partial class Program {
    private static async Task Main(string[] args) {
        await System.Runtime.InteropServices.JavaScript.JSHost.ImportAsync("app_storage.js", "/app_storage.js");

        await BuildAvaloniaApp()
            .WithInterFont()
#if DEBUG
            .WithDeveloperTools()
#endif
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}