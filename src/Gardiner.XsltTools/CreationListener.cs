using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;

using EnvDTE;

using Gardiner.XsltTools.ErrorList;

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Gardiner.XsltTools
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("xml")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class CreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            try
            {
                var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
                ITextDocument document;

                if (TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out document))
                {
                    var fileName = Path.GetFileName(document.FilePath);
                    Debug.WriteLine(fileName);

                    //TableDataSource.Instance.CleanAllErrors();

                    var checker = new XsltChecker();
                    var result = checker.CheckFile(document.FilePath);

                    var dte = (DTE)ServiceProvider.GetService(typeof(DTE));

                    var projectItem = dte.Solution.FindProjectItem(fileName);

                    result.Project = projectItem.ContainingProject.Name;

                    ErrorListService.ProcessLintingResults(result);
                }
            }
            catch (Exception ex)
            {
                Telemetry.Log(ex);
            }
        }
    }
}