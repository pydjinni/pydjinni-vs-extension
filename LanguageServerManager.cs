using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using static PyDjinni.OptionsProvider;

namespace PyDjinni
{
    class LanguageServerManager
    {
        private readonly DTE2 _dte;
        private EnvDTE.DocumentEvents _events;
        private readonly Package _package;
        private readonly IVsTaskStatusCenterService _tsc;
        private readonly List<Document> _documents;

        private const string SETTINGS_ACTION = "Settings";
        private const string INSTALL_ACTION = "Install PyDjinni";
        private const string INSTALLING_MESSAGE = "Installing PyDjinni...";
        private const string INSTALL_SUCCESS_MESSAGE = "PyDjinni successfully installed!";

        public bool IsInstalled { get; private set; }
        public event EventHandler Installed;

        public LanguageServerManager(DTE2 dte, Package package, IVsTaskStatusCenterService tsc)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _dte = dte;
            _events = dte.Events.DocumentEvents;
            _events.DocumentOpened += DocumentOpened;
            _events.DocumentClosing += DocumentClosing;
            _package = package;
            _tsc = tsc;
            _documents = [];
            General.Saved += OnSettingsSaved;
            IsInstalled = CheckIsInstalled();

        }

        private void DocumentClosing(Document document)
        {
            _documents.Remove(document);
        }

        private void OnSettingsSaved(General general)
        {
            IsInstalled = CheckIsInstalled();
            _documents.ForEach((document) => ShowInstallMessageIfRequiredAsync(document));
        }

        public static LanguageServerManager Instance { get; private set; }

        public static void Initialize(DTE2 dte, Package package, IVsTaskStatusCenterService tsc)
        {
            Instance = new LanguageServerManager(dte, package, tsc);
        }

        private void DocumentOpened(Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var type = Path.GetExtension(document.FullName);
            if (type == ".pydjinni")
            {
                _documents.Add(document);
                ShowInstallMessageIfRequiredAsync(document);
            }

        }

        private void InfoBar_ActionItemClicked(object sender, InfoBarActionItemEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (e.ActionItem.Text == SETTINGS_ACTION)
            {
                _package.ShowOptionPage(typeof(GeneralOptions));
                e.InfoBarUIElement.Close();
            }
            else if (e.ActionItem.Text == INSTALL_ACTION)
            {
                _tsc.PreRegister(new TaskHandlerOptions()
                {
                    Title = INSTALLING_MESSAGE,
                    TaskSuccessMessage = INSTALL_SUCCESS_MESSAGE,
                    ActionsAfterCompletion = CompletionActions.None
                }, new TaskProgressData()
                {
                    CanBeCanceled = true
                }).RegisterTask(InstallPyDjinniTaskAsync(e.InfoBarUIElement));
            }
        }

        private async Task InstallPyDjinniTaskAsync(IVsInfoBarUIElement infoBar)
        {
            System.Diagnostics.Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "pip",
                    Arguments = "install pydjinni",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            await process.WaitForExitAsync();
            IsInstalled = CheckIsInstalled();
            if (IsInstalled)
            {
                Installed(this, EventArgs.Empty);
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            infoBar.Close();
        }

        private bool CheckIsInstalled()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            System.Diagnostics.Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = General.Instance.ServerExecutable,
                    Arguments = "--version",
                    WorkingDirectory = _dte.Solution.FullName,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            try
            {
                process.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task ShowInstallMessageIfRequiredAsync(Document document)
        {
            if (!IsInstalled)
            {
                var model = new InfoBarModel(
                    new[] {
                        new InfoBarTextSpan("PyDjinni Language Server cannot be found."),
                        new InfoBarTextSpan("   "),
                        new InfoBarHyperlink(INSTALL_ACTION),
                        new InfoBarTextSpan("   "),
                        new InfoBarHyperlink(SETTINGS_ACTION)
                    },
                    KnownMonikers.StatusWarning,
                    true);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                InfoBar infoBar = await VS.InfoBar.CreateAsync(document.FullName, model);
                infoBar.ActionItemClicked += InfoBar_ActionItemClicked;
                await infoBar.TryShowInfoBarUIAsync();
            }
        }
    }
}
