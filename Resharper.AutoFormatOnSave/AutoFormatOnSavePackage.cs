using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Process = System.Diagnostics.Process;
using Task = System.Threading.Tasks.Task;
using Timer = System.Timers.Timer;

namespace Resharper.AutoFormatOnSave
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(Guids.PACKAGE_GUID_STRING)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(OptionPage), "ReSharper.AutoFormatOnSave", "Allowed File Extensions", 0, 0 , true)]
    public sealed class AutoFormatOnSavePackage : AsyncPackage
    {
        private const uint TIMER_INTERVAL = 1000;
        
        /// <summary>
        /// The name of the ReSharper silent code cleanup command
        /// </summary>
        private readonly List<string> _resharperSilentCleanupCodeCommandsNames = new List<string>() { "ReSharper_SilentCleanupCode", "ReSharper.ReSharper_SilentCleanupCode" };
        
        /// <summary>
        /// Timer to run background checks, mainly to check whether all saves have completed
        /// </summary>
        private readonly Timer _timer = new Timer(TIMER_INTERVAL);

        /// <summary>
        /// The documents that will be reformatted with a timestamp when they were last reformatted
        /// </summary>
        private readonly Dictionary<Document, DateTime> _documentsToReformat = new Dictionary<Document, DateTime>();

        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoFormatOnSavePackage"/> class.
        /// </summary>
        public AutoFormatOnSavePackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.

            _log = new Log();
            _timer.Elapsed += TimerOnElapsed;
        }

        /// <summary>
        /// The <see cref="DTE"/> global service
        /// </summary>
        private DTE Dte { get; set; } 

        /// <summary>
        /// The ReSharper <see cref="Command"/> to use
        /// </summary>
        private Command Command { get; set; }


        /// <summary>
        /// Is the solution active
        /// </summary>
        private bool IsSolutionActive { get; set; }

        /// <summary>
        /// Is currently reformatting recently saved files
        /// </summary>
        private bool IsReformatting { get; set; }


        /// <summary>
        /// Positive value when the build engine is running
        /// </summary>
        private int BuildingSolution { get; set; }


        /// <summary>
        /// The allowed file extensions for code cleanup.
        /// </summary>
        private List<string> AllowedFileExtensions
        {
            get
            {
                List<string> allowedFileExtensions = new List<string>();
                OptionPage optionPage = (OptionPage) GetDialogPage(typeof(OptionPage));

                if (optionPage.IsCsAllowed)
                    allowedFileExtensions.Add(".cs");

                if (optionPage.IsXamlAllowed)
                    allowedFileExtensions.Add(".xaml");

                if (optionPage.IsVbAllowed)
                    allowedFileExtensions.Add(".vb");

                if (optionPage.IsJsAllowed)
                    allowedFileExtensions.Add(".js");

                if (optionPage.IsTsAllowed)
                    allowedFileExtensions.Add(".ts");

                if (optionPage.IsCssAllowed)
                    allowedFileExtensions.Add(".css");

                if (optionPage.IsHtmlAllowed)
                    allowedFileExtensions.Add(".html");

                if (optionPage.IsXmlAllowed)
                    allowedFileExtensions.Add(".xml");

                return allowedFileExtensions;
            }
        }


        /// <summary>
        /// The time of the last reformat
        /// </summary>
        private DateTime LastReformat { get; set; }


        /// <summary>
        /// Visual Studio <see cref="BuildEvents"/>
        /// </summary>
        private BuildEvents BuildEvents { get; set; }

        /// <summary>
        /// Visual Studio <see cref="DocumentEvents"/>
        /// </summary>
        private DocumentEvents DocumentEvents { get; set; }

        /// <summary>
        /// Visual Studio <see cref="SolutionEvents"/>
        /// </summary>
        private SolutionEvents SolutionEvents { get; set; }


        /// <summary>
        /// Called Periodically
        /// </summary>
        /// <param name="sender">The timer</param>
        /// <param name="args">The <see cref="ElapsedEventArgs"/></param>
        private async void TimerOnElapsed(object sender, ElapsedEventArgs args)
        {
            Log.WriteLine($"{nameof(TimerOnElapsed)}()");

            try
            {
                if (BuildingSolution > 0)
                {
                    _documentsToReformat.Clear();
                    Log.WriteLine("Solution Building.");
                    return;
                }

                if (Dte.Application.Mode == vsIDEMode.vsIDEModeDebug ||
                    IsReformatting ||
                    !IsSolutionActive ||
                    !_documentsToReformat.Any() ||
                    !IsVisualStudioForegroundWindow())
                    return;

                Log.WriteLine("Going on to reformat.");

                //remove all unsaved documents from the dictionary
                foreach (var document in _documentsToReformat.Where(d => !d.Key.Saved).Select(d => d.Key).ToList())
                {
                    _documentsToReformat.Remove(document);
                }

                DateTime now = DateTime.Now;
                if ((now - LastReformat).TotalSeconds < 5) //ignore any documents that have been saved if a reformat has happened within the last 5 seconds
                {
                    _documentsToReformat.Clear();
                    Log.WriteLine($"LastReformat has happened within the last 5 seconds ({LastReformat} sec).");
                    return;
                }

                bool anyDocumentSavedSinceLastCheck = _documentsToReformat.Any(d => (now - d.Value).TotalMilliseconds < _timer.Interval);
                if (!_documentsToReformat.Any() || anyDocumentSavedSinceLastCheck)
                {
                    Log.WriteLine("No documents have been saved since the last check.");
                    return;
                }

                Log.WriteLine($"Call {nameof(ReformatDocuments)}() now.");
                await ReformatDocuments(_documentsToReformat.OrderBy(d => d.Value).Select(d => d.Key).ToList(), true);
            }
            catch (Exception ex)
            {
                //TODO: Handle exception case
                Log.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Reformat a given list of documents, ensuring that the background timer is stopped during the process
        /// </summary>
        /// <param name="documentsToReformat">The <see cref="Document"/>s to reformat</param>
        /// <param name="saveDocumentsAfterwards">True if the changed <see cref="Document"/>s should be saved afterwards</param>
        /// <returns></returns>
        private async Task ReformatDocuments(IReadOnlyCollection<Document> documentsToReformat, bool saveDocumentsAfterwards = false)
        {
            Log.WriteLine($"{nameof(ReformatDocuments)}(Number of documents to reformat: {documentsToReformat.Count})");

            IsReformatting = true;
            _timer.Stop();
            LastReformat = DateTime.Now;

            Window originallyActiveWindow = Dte.ActiveWindow;

            await Task.Run(() => 
            {
                try
                {
                    Document originallyActiveDocument = originallyActiveWindow?.Document;
                    List<Document> activeDocumentCollection = originallyActiveDocument != null
                        ? Enumerable.Repeat(originallyActiveDocument, 1).ToList()
                        : Enumerable.Empty<Document>().ToList();

                    List<Document> recentlySavedDocuments = documentsToReformat
                        .Except(activeDocumentCollection)
                        .Concat(originallyActiveDocument != null && _documentsToReformat.ContainsKey(originallyActiveDocument)
                        ? activeDocumentCollection
                        : Enumerable.Empty<Document>())
                        .ToList();

                    foreach (var document in recentlySavedDocuments)
                    {
                        Log.WriteLine($"Document: {document.Name}.");

                        //activate the document that was just saved to run the ReSharper command in it
                        document.Activate();
                        
                        try
                        {
                            Log.WriteLine("Execute ReSharper command.");
                            Dte.ExecuteCommand(Command.Name);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLine(ex.Message);
                        }
                        finally
                        {
                            _documentsToReformat.Remove(document);
                        }
                    }

                    if (saveDocumentsAfterwards)
                    {
                        Log.WriteLine("Save Documents afterwards.");

                        foreach (var document in recentlySavedDocuments.Where(d => !d.Saved))
                        {
                            document.Save();
                        }
                    }

                    originallyActiveWindow?.Activate(); //Reactivate original window
                }
                finally
                {
                    foreach (var document in documentsToReformat)
                    {
                        _documentsToReformat.Remove(document);
                    }

                    _timer.Start();
                    IsReformatting = false;
                }
            });
        }

        /// <summary>
        /// Called when a build has begun
        /// </summary>
        /// <param name="scope">The build scope</param>
        /// <param name="action">The build action</param>
        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            Log.WriteLine($"{nameof(OnBuildBegin)}(Scope: {scope}, Action: {action})");

            switch (action)
            {
                case vsBuildAction.vsBuildActionBuild:
                case vsBuildAction.vsBuildActionRebuildAll:
                case vsBuildAction.vsBuildActionDeploy:
                {
                    ++BuildingSolution;
                    break;
                }
            }
        }

        /// <summary>
        /// Called when a build is done
        /// </summary>
        /// <param name="scope">The build scope</param>
        /// <param name="action">The build action</param>
        private void OnBuildDone(vsBuildScope scope, vsBuildAction action)
        {
            Log.WriteLine($"{nameof(OnBuildDone)}(Scope: {scope}, Action: {action})");

            switch (action)
            {
                case vsBuildAction.vsBuildActionBuild:
                case vsBuildAction.vsBuildActionRebuildAll:
                case vsBuildAction.vsBuildActionDeploy:
                {
                    --BuildingSolution;
                    break;
                }
            }
        }

        /// <summary>
        /// Called when a document is saved
        /// </summary>
        /// <param name="document">The document that is saved</param>
        private void OnDocumentSaved(Document document)
        {
            Log.WriteLine($"{nameof(OnDocumentSaved)}(Document: {document.Name})");

            if (IsReformatting || BuildingSolution > 0)
                return;

            string extension = Path.GetExtension(document.FullName);
            if (!AllowedFileExtensions.Contains(extension))
                return;

            _documentsToReformat[document] = DateTime.Now;
        }

        /// <summary>
        /// Called when a document is getting closed
        /// </summary>
        /// <param name="document">The document that is getting closed</param>
        private void OnDocumentClosing(Document document)
        {
            Log.WriteLine($"{nameof(OnDocumentClosing)}(Document: {document.Name})");
            
            string extension = Path.GetExtension(document.FullName);
            if (!AllowedFileExtensions.Contains(extension))
                return;

            _documentsToReformat.Remove(document);
        }

        /// <summary>
        /// Called when a solution is opened
        /// </summary>
        private void OnOpenedSolution()
        {
            Log.WriteLine($"{nameof(OnOpenedSolution)}()");
            IsSolutionActive = true;
        }

        /// <summary>
        /// Called before a solution is closing
        /// </summary>
        private void OnBeforeClosingSolution()
        {
            Log.WriteLine($"{nameof(OnBeforeClosingSolution)}()");
            IsSolutionActive = false;
        }

        /// <summary>
        /// Disconnect from Visual Studio events
        /// </summary>
        private void DisconnectFromVsEvents()
        {
            Log.WriteLine($"{nameof(DisconnectFromVsEvents)}()");

            if (DocumentEvents != null)
            {
                DocumentEvents.DocumentSaved -= OnDocumentSaved;
                DocumentEvents.DocumentClosing -= OnDocumentClosing;
                DocumentEvents = null;
            }

            if (BuildEvents != null)
            {
                BuildEvents.OnBuildBegin -= OnBuildBegin;
                BuildEvents.OnBuildDone -= OnBuildDone;
                BuildEvents = null;
            }

            if (SolutionEvents != null)
            {
                SolutionEvents.Opened -= OnOpenedSolution;
                SolutionEvents.BeforeClosing -= OnBeforeClosingSolution;
                SolutionEvents = null;
            }
        }


        private bool IsVisualStudioForegroundWindow()
        {
            Log.WriteLine($"{nameof(IsVisualStudioForegroundWindow)}()");
            NativeWindowMethods.GetWindowThreadProcessId(NativeWindowMethods.GetForegroundWindow(), out uint foregroundProcessId);
            int visualStudioProcessId = Process.GetCurrentProcess().Id;

            Log.WriteLine($"Is VisualStudio foreground window: {visualStudioProcessId == foregroundProcessId}");
            return visualStudioProcessId == foregroundProcessId;
        }

        private async Task InitializeLogging()
        {
            IVsOutputWindow output = (IVsOutputWindow) await GetServiceAsync(typeof(IVsOutputWindow));
            output.CreatePane(Guids.OUTPUT_PANE_GUID, "ReSharper.AutoFormatOnSave", 1, 1);
            output.GetPane(Guids.OUTPUT_PANE_GUID, out IVsOutputWindowPane outputPane);

            _log.InitializeLog(outputPane);
        }

#region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            await base.InitializeAsync(cancellationToken, progress);

            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await InitializeLogging();

            Dte = (DTE) await GetServiceAsync(typeof(DTE));
            IEnumerable<Command> availableCleanupCommands = Dte.Commands.OfType<Command>().Where(c => _resharperSilentCleanupCodeCommandsNames.Contains(c.Name));
            Command = availableCleanupCommands.FirstOrDefault();
            if (Command == null)
            {
                Log.WriteLine("No Command found.");
                return;
            }

            DisconnectFromVsEvents();

            Log.WriteLine("Subscribe to VS events.");
            DocumentEvents = Dte.Events.DocumentEvents;
            DocumentEvents.DocumentSaved += OnDocumentSaved;
            DocumentEvents.DocumentClosing += OnDocumentClosing;

            BuildEvents = Dte.Events.BuildEvents;
            BuildEvents.OnBuildBegin += OnBuildBegin;
            BuildEvents.OnBuildDone += OnBuildDone;

            SolutionEvents = Dte.Events.SolutionEvents;
            SolutionEvents.Opened += OnOpenedSolution;
            SolutionEvents.BeforeClosing += OnBeforeClosingSolution;

            _timer.Start();

            Log.WriteLine("Initialized successful.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisconnectFromVsEvents();

                _log.Dispose();

                _timer.Elapsed -= TimerOnElapsed;
                _timer.Dispose();
            }

            base.Dispose(disposing);
        }

#endregion
    }
}
