using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using EnvDTE;

using EnvDTE80;

using Gardiner.XsltTools.ErrorList;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
    public sealed class XsltToolPackage : Package, IVsSolutionEvents, IVsUpdateSolutionEvents2
    {
        /// <summary>
        ///     XsltToolPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "02f27f4d-ff6c-44a0-ac75-79c681d381bd";

        private uint _pdwCookieSolution;
        private uint _pdwCookieSolutionBm;
        private IVsSolution _spSolution;

        private IVsSolutionBuildManager2 _spSolutionBm;
        private DTE2 _dte;

        public static Options Options { get; private set; }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            TableDataSource.Instance.CleanAllErrors();
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
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
                ProcessHierarchy(pHierProj, name);
            }
            catch (Exception ex)
            {
                Telemetry.Log(ex);
            }
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction,
            int fSuccess,
            int fCancel)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Initialization of the package; this method is called right after the package is sited, so this is the place
        ///     where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            _dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
            Options = (Options)GetDialogPage(typeof(Options));

            Telemetry.Initialise(new HockeyClientTelemetryProvider(Options), _dte);
                
            Logger.Initialize(this, Vsix.Name);
            base.Initialize();

            _spSolution = (IVsSolution) ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution));
            _spSolution.AdviseSolutionEvents(this, out _pdwCookieSolution);

            // To listen events that fired as a IVsUpdateSolutionEvents2
            _spSolutionBm = (IVsSolutionBuildManager2) ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager));
            _spSolutionBm.AdviseUpdateSolutionEvents(this, out _pdwCookieSolutionBm);

            PromptUser();
        }

        private void PromptUser()
        {
            const string permissionKey = "Gardiner.XsltTool.AskPermissionForFeedback";

            try
            {
                var userHasBeenPrompted = bool.Parse(UserRegistryRoot.GetValue(permissionKey, false).ToString());

                if (!userHasBeenPrompted)
                {

                    var hwnd = new IntPtr(_dte.MainWindow.HWnd);
                    var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;

                    const string msg = "Provide automatic feedback of crash and de-identified usage data to help improve this extension?\r\r(This is an open source project hosted at https://github.com/flcdrg/XsltCop)";
                    var answer = MessageBox.Show(window, msg, Vsix.Name, MessageBoxButton.YesNo, MessageBoxImage.Question);

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

        private void ProcessHierarchy(IVsHierarchy hierarchy, string projectName)
        {
            // Traverse the nodes of the hierarchy from the root node
            ProcessHierarchyNodeRecursively(hierarchy, VSConstants.VSITEMID_ROOT, projectName);
        }

        private void ProcessHierarchyNodeRecursively(IVsHierarchy hierarchy, uint itemId, string projectName)
        {
            IntPtr nestedHierarchyValue;
            uint nestedItemIdValue;

            // First, guess if the node is actually the root of another hierarchy (a project, for example)
            var nestedHierarchyGuid = typeof(IVsHierarchy).GUID;
            var result = hierarchy.GetNestedHierarchy(itemId, ref nestedHierarchyGuid, out nestedHierarchyValue,
                out nestedItemIdValue);

            if (result == VSConstants.S_OK && nestedHierarchyValue != IntPtr.Zero &&
                nestedItemIdValue == VSConstants.VSITEMID_ROOT)
            {
                // Get the new hierarchy
                var nestedHierarchy = Marshal.GetObjectForIUnknown(nestedHierarchyValue) as IVsHierarchy;
                Marshal.Release(nestedHierarchyValue);

                if (nestedHierarchy != null)
                {
                    ProcessHierarchy(nestedHierarchy, projectName);
                }
            }
            else // The node is not the root of another hierarchy, it is a regular node
            {
                ShowNodeName(hierarchy, itemId, projectName);

                // Get the first visible child node
                object value;
                result = hierarchy.GetProperty(itemId, (int) __VSHPROPID.VSHPROPID_FirstVisibleChild, out value);

                while (result == VSConstants.S_OK && value != null)
                {
                    if (value is int && (uint) (int) value == VSConstants.VSITEMID_NIL)
                    {
                        // No more nodes
                        break;
                    }
                    var visibleChildNode = Convert.ToUInt32(value);

                    // Enter in recursion
                    ProcessHierarchyNodeRecursively(hierarchy, visibleChildNode, projectName);

                    // Get the next visible sibling node
                    result = hierarchy.GetProperty(visibleChildNode, (int) __VSHPROPID.VSHPROPID_NextVisibleSibling,
                        out value);
                }
            }
        }

        private void ShowNodeName(IVsHierarchy hierarchy, uint itemId, string projectName)
        {
            Guid guid;
            var result = hierarchy.GetGuidProperty(itemId, (int) __VSHPROPID.VSHPROPID_TypeGuid, out guid);

            if (result == VSConstants.S_OK && guid == VSConstants.GUID_ItemType_PhysicalFile)
            {
                string canonicalName;
                result = hierarchy.GetCanonicalName(itemId, out canonicalName);

                if (result == VSConstants.S_OK && canonicalName != null &&
                    (canonicalName.EndsWith(".xsl") || canonicalName.EndsWith(".xslt")))
                {
                    Debug.WriteLine($"\tCanonical name: {canonicalName} {guid}");

                    var checker = new XsltChecker();
                    var checkResult = checker.CheckFile(canonicalName);
                    checkResult.Project = projectName;

                    ErrorListService.ProcessLintingResults(checkResult);
                }
            }
        }
    }
}