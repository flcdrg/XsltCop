using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

using EnvDTE;

using EnvDTE80;

using Gardiner.XsltTools.ErrorList;
using Gardiner.XsltTools.Logging;

using JetBrains.Annotations;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace Gardiner.XsltTools
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
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideOptionPage(typeof(Options), "XsltCop", "General", 101, 111, true, new[] { "xslt", "xpath" }, ProvidesLocalizedCategoryName = false)]
    [Guid(XsltToolsPackage.PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.CodeWindow_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed partial class XsltToolsPackage : AsyncPackage, IVsSolutionEvents, IVsUpdateSolutionEvents2
    {
         /// <summary>
        /// Gardiner.XsltToolsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "02f27f4d-ff6c-44a0-ac75-79c681d381bd";

        private uint _pdwCookieSolution;
        private uint _pdwCookieSolutionBm;
        private IVsSolution _spSolution;

        private IVsSolutionBuildManager2 _spSolutionBm;
        private DTE2 _dte;

        public static Options Options { get; private set; }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            TableDataSource.Instance.CleanAllErrors();
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Begin([NotNull] IVsHierarchy pHierProj, [NotNull] IVsCfg pCfgProj, [NotNull] IVsCfg pCfgSln, uint dwAction,
            ref int pfCancel)
        {
            if (pHierProj == null)
            {
                throw new ArgumentNullException(nameof(pHierProj));
            }

            if (pCfgProj == null)
            {
                throw new ArgumentNullException(nameof(pCfgProj));
            }

            if (pCfgSln == null)
            {
                throw new ArgumentNullException(nameof(pCfgSln));
            }

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
                // For testing telemetry
                //throw new InvalidOperationException("Oh dear");

#endif
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Debug.WriteLine(ex);
            }
            return VSConstants.S_OK;
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
        // Do any initialization that requires the UI thread after switching to the UI thread.
            _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
            Options = (Options)GetDialogPage(typeof(Options));

            Logger.Initialize(this, Vsix.Name);

            _spSolution = (IVsSolution)ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution));
            _spSolution.AdviseSolutionEvents(this, out _pdwCookieSolution);

            // To listen events that fired as a IVsUpdateSolutionEvents2
            _spSolutionBm = (IVsSolutionBuildManager2)ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager));
            _spSolutionBm.AdviseUpdateSolutionEvents(this, out _pdwCookieSolutionBm);
        await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
    }

    #endregion
}
}
