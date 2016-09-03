using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using NullGuard;

namespace Gardiner.XsltTools
{
    public sealed partial class XsltToolPackage
    {
        public int OnAfterOpenProject([AllowNull] IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject([AllowNull] IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject([AllowNull] IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject([AllowNull] IVsHierarchy pStubHierarchy, [AllowNull] IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject([AllowNull] IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject([AllowNull] IVsHierarchy pRealHierarchy, [AllowNull] IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution([AllowNull] object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution([AllowNull] object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution([AllowNull] object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

    }
}