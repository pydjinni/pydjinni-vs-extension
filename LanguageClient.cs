using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PyDjinni
{
    [ContentType("pydjinni")]
    [Export(typeof(ILanguageClient))]
    [RunOnContext(RunningContext.RunOnHost)]
    public class LanguageClient : ILanguageClient
    {

        private async void OnServerInstalled(object sender, EventArgs e)
        {
            await StartAsync.InvokeAsync(this, EventArgs.Empty);
        }

        private async void OnSettingsSaved(General general)
        {
            await StopAsync.InvokeAsync(this, EventArgs.Empty);
            await StartAsync.InvokeAsync(this, EventArgs.Empty);
        }


        public string Name => "PyDjinni Language Extension";

        public IEnumerable<string> ConfigurationSections => ["pydjinni"];

        public bool ShowNotificationOnInitializeFailed => true;

        public IEnumerable<string> FilesToWatch => ["pydjinni.yaml", "**/*.pydjinni"];

        public object InitializationOptions => null;

        public event AsyncEventHandler<EventArgs> StartAsync;
        public event AsyncEventHandler<EventArgs> StopAsync;

        private async Task<string> GetSolutionRootAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            EnvDTE.DTE dte = (EnvDTE.DTE)ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE));
            return dte.Solution.FullName;
        }


        public async Task<Connection> ActivateAsync(CancellationToken token)
        {
            await Task.Yield();

            var arguments = new List<string> { "--connection", "STDIO" };
            if (General.Instance.DebugLogsEnabled)
            {
                arguments.AddRange(["--log", "pydjinni-language-server.log"]);
            }

            Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = General.Instance.ServerExecutable,
                    Arguments = $"start {string.Join(" ", arguments)}",
                    WorkingDirectory = await GetSolutionRootAsync(),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            return new Connection(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);

        }

        public async Task OnLoadedAsync()
        {
            General.Saved += OnSettingsSaved;
            LanguageServerManager.Instance.Installed += OnServerInstalled;
            if (LanguageServerManager.Instance.IsInstalled)
            {
                await StartAsync.InvokeAsync(this, EventArgs.Empty);
            }
        }

        public Task OnServerInitializedAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<InitializationFailureContext> OnServerInitializeFailedAsync(ILanguageClientInitializationInfo initializationState)
        {
            return null;
        }
    }
}
