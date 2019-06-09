using System;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Gardiner.XsltTools.Utils
{
    // Mads
    public static class FileHelpers
    {
        public static void OpenFileInPreviewTab(string file)
        {
            IVsNewDocumentStateContext newDocumentStateContext = null;

            try
            {
                IVsUIShellOpenDocument3 openDoc3 = VSPackage.GetGlobalService<SVsUIShellOpenDocument>() as IVsUIShellOpenDocument3;

                Guid reason = VSConstants.NewDocumentStateReason.Navigation;
                newDocumentStateContext = openDoc3.SetNewDocumentState((uint)__VSNEWDOCUMENTSTATE.NDS_Provisional, ref reason);

                VSPackage.DTE.ItemOperations.OpenFile(file);
            }
            finally
            {
                if (newDocumentStateContext != null)
                {
                    newDocumentStateContext.Restore();
                }
            }
        }
    }
}
