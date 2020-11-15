using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;

using EnvDTE;

using Gardiner.XsltTools.Classification;
using Gardiner.XsltTools.Commands;
using Gardiner.XsltTools.ErrorList;
using Gardiner.XsltTools.Logging;

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;


namespace Gardiner.XsltTools
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("XSLT")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Order(After = Priority.High)]
    internal class CreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider = null;

        [Import]
        public IClassificationTypeRegistryService Registry { get; set; }


        //private XmlStore _store;
        private IVsTextLines _buffer;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            try
            {
                IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

                textView.Properties.GetOrCreateSingletonProperty(
                    () => new ImportHrefGoToDefinition(textViewAdapter, textView));

                ITextDocument document;

                textViewAdapter.GetBuffer(out _buffer);

                if (TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out document))
                {
                    XsltClassifier classifier;
                    textView.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(XsltClassifier), out classifier);

                    if (classifier != null)
                    {
                        var snapshot = textView.TextBuffer.CurrentSnapshot;
                        classifier.OnClassificationChanged(new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
                    }

                    //TableDataSource.Instance.CleanAllErrors();

                    var fileName = Path.GetFileName(document.FilePath);
                    Debug.WriteLine(fileName);

                    var checker = new XsltChecker();
                    var result = checker.CheckFile(fileName);

                    if (result == null)
                    {
                        return;
                    }

                    var dte = (DTE) ServiceProvider.GetService(typeof(DTE));

                    var projectItem = dte.Solution.FindProjectItem(fileName);

                    result.Project = projectItem.ContainingProject.Name;

                    //XmlEditorService es = (XmlEditorService) ServiceProvider.GetService(typeof(XmlEditorService));
                    //_store = es.CreateXmlStore();
                    ////_store.UndoManager = _undoManager;

                    //_model = _store.OpenXmlModel(new Uri(document.FilePath));

                    //var doc = GetParseTree();
                    //XmlSerializer serializer = new XmlSerializer(typeof(VSTemplate));
                    ErrorListService.ProcessLintingResults(result);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Debug.WriteLine(ex);
                // Telemetry.Log(ex);
            }
        }
    }
}