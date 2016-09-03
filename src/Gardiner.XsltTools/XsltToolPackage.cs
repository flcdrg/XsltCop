using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

using EnvDTE;

using EnvDTE80;

using Gardiner.XsltTools.ErrorList;
using Gardiner.XsltTools.Logging;
using Gardiner.XsltTools.Properties;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using NullGuard;

using Task = System.Threading.Tasks.Task;

namespace Gardiner.XsltTools
{
    /// <summary>
    ///     This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The minimum requirement for a class to be considered a valid package for Visual Studio
    ///         is to implement the IVsPackage interface and register itself with the shell.
    ///         This package uses the helper classes defined inside the Managed Package Framework (MPF)
    ///         to do it: it derives from the Package class that provides the implementation of the
    ///         IVsPackage interface and uses the registration attributes defined in the framework to
    ///         register itself and its components with the shell. These attributes tell the pkgdef creation
    ///         utility what data to put into .pkgdef file.
    ///     </para>
    ///     <para>
    ///         To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...
    ///         &gt; in .vsixmanifest file.
    ///     </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideOptionPage(typeof(Options), "XsltCop", "General", 101, 111, true, new[] { "xslt", "xpath" }, ProvidesLocalizedCategoryName = false)]
    // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.CodeWindow_string)]
    public sealed partial class XsltToolPackage : AsyncPackage, IVsSolutionEvents, IVsUpdateSolutionEvents2
    {
        /// <summary>
        /// XsltToolPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "02f27f4d-ff6c-44a0-ac75-79c681d381bd";

        private uint _pdwCookieSolution;
        private uint _pdwCookieSolutionBm;
        private IVsSolution _spSolution;

        private IVsSolutionBuildManager2 _spSolutionBm;
        private DTE2 _dte;

        public static Options Options { get; private set; }

        public int OnBeforeCloseSolution([AllowNull] object pUnkReserved)
        {
            TableDataSource.Instance.CleanAllErrors();
            return VSConstants.S_OK;
        }


        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction,
            ref int pfCancel)
        {
            try
            {
                object o;
                pHierProj.GetProperty((uint) VSConstants.VSITEMID.Root, (int) __VSHPROPID.VSHPROPID_Name, out o);
                var name = o as string;

                Debug.WriteLine($"UpdateProjectCfg_Begin {name}");

                // get files from project
                var processor = new ProcessHierarchy();
                processor.Process(pHierProj, name);

#if DEBUG
#pragma warning disable S125 // Sections of code should not be "commented out"
                // For testing telemetry
                // throw new InvalidOperationException("Oh dear");
#pragma warning restore S125 // Sections of code should not be "commented out"

#endif
            }
            catch (Exception ex)
            {
                Telemetry.Log(ex);
            }
            return VSConstants.S_OK;
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
            Options = (Options)GetDialogPage(typeof(Options));

            var provider = await HockeyClientTelemetryProvider.Create(Options).ConfigureAwait(true);
            Telemetry.Initialise(provider, _dte);

            Logger.Initialize(this, Vsix.Name);

            _spSolution = (IVsSolution)ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution));
            _spSolution.AdviseSolutionEvents(this, out _pdwCookieSolution);

            // To listen events that fired as a IVsUpdateSolutionEvents2
            _spSolutionBm = (IVsSolutionBuildManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager));
            _spSolutionBm.AdviseUpdateSolutionEvents(this, out _pdwCookieSolutionBm);

            PromptUser();

            await base.InitializeAsync(cancellationToken, progress).ConfigureAwait(true);
        }

        private void PromptUser()
        {
            const string permissionKey = "Gardiner.XsltTool.AskPermissionForFeedback";

#if DEBUG
            // Reset value for testing
            // UserRegistryRoot.SetValue(permissionKey, false);
#endif
            try
            {
                var userHasBeenPrompted = bool.Parse(UserRegistryRoot.GetValue(permissionKey, false).ToString());

                if (userHasBeenPrompted)
                    return;

                var hwnd = new IntPtr(_dte.MainWindow.HWnd);
                var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd)?.RootVisual;

                if (window != null)
                {
                    var answer = MessageBox.Show(window, Resources.PermissionPrompt, string.Format(CultureInfo.CurrentCulture, Resources.PermissionPromptCaption, Vsix.Name), MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (answer == MessageBoxResult.Yes)
                        Options.FeedbackAllowed = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                UserRegistryRoot.SetValue(permissionKey, true);
            }
        }
    }
}