using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using EnvDTE;

using Gardiner.XsltTools.Classification;
using Gardiner.XsltTools.Commands;
using Gardiner.XsltTools.ErrorList;
using Gardiner.XsltTools.Logging;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.XmlEditor;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;


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
        private XmlModel _model = null;
        private LanguageService _xmlLanguageService;
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
                        return;

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
            catch (Exception ex)
            {
                Telemetry.Log(ex);
            }
        }

        /// <summary>
        /// Get an up to date XML parse tree from the XML Editor.
        /// </summary>
        XDocument GetParseTree()
        {
            LanguageService langsvc = GetXmlLanguageService();

            // don't crash if the language service is not available
            if (langsvc != null)
            {
                Source src = langsvc.GetSource(_buffer);

                // We need to access this method to get the most up to date parse tree.
                // public virtual XmlDocument GetParseTree(Source source, IVsTextView view, int line, int col, ParseReason reason) {
                MethodInfo mi = langsvc.GetType().GetMethod("GetParseTree");
                int line = 0, col = 0;
                mi.Invoke(langsvc, new object[] { src, null, line, col, ParseReason.Check });
            }

            // Now the XmlDocument should be up to date also.
            return _model.Document;
        }
        /// <summary>
        /// Get the XML Editor language service
        /// </summary>
        /// <returns></returns>
        LanguageService GetXmlLanguageService()
        {
            if (_xmlLanguageService == null)
            {
                var vssp = (IOleServiceProvider) ServiceProvider.GetService(typeof(IOleServiceProvider));
                var xmlEditorGuid = new Guid("f6819a78-a205-47b5-be1c-675b3c7f0b8e");
                var iunknown = new Guid("00000000-0000-0000-C000-000000000046");
                IntPtr ptr;
                if (ErrorHandler.Succeeded(vssp.QueryService(ref xmlEditorGuid, ref iunknown, out ptr)))
                {
                    try
                    {
                        _xmlLanguageService = Marshal.GetObjectForIUnknown(ptr) as LanguageService;
                    }
                    finally
                    {
                        Marshal.Release(ptr);
                    }
                }
            }
            return _xmlLanguageService;
        }
    }
}