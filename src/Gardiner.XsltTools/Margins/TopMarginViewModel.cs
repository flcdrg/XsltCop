using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using Microsoft.Language.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Gardiner.XsltTools.Margins
{
    public class TopMarginViewModel: INotifyPropertyChanged
    {
        private readonly IWpfTextView _textView;
        private readonly ITextBuffer _dataBuffer;
        private TemplateModel _selectedValue;
        private bool _dontUpdateCaret;
        private XmlDocumentSyntax _syntax;

        // Only for testing
        public TopMarginViewModel()
        {
        }

        public TopMarginViewModel([NotNull] IWpfTextView textView)
        {
            if (textView == null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            _textView = textView;
            _dataBuffer = textView.TextDataModel.DataBuffer;

            UpdateList();

            _dataBuffer.PostChanged += DataBufferOnPostChanged;
            _textView.Caret.PositionChanged += OnCaretChanged;
        }

        public TemplateModel SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                if (Equals(value, _selectedValue))
                {
                    return;
                }

                Debug.WriteLine($"SelectedValue {value}");
                _selectedValue = value;
                OnPropertyChanged();
                TemplateListSelectionChanged(value);
            }
        }

        private void OnCaretChanged(object sender, CaretPositionChangedEventArgs e)
        {
            // what template are we inside?
            var bufferPosition = e.NewPosition.BufferPosition;
            var position = bufferPosition.Position;

            IXmlElementSyntax elt = GetDescendants(_syntax)
                .OfType<XmlElementSyntax>()
                .FirstOrDefault(n =>
                {
                    var leadingTriviaWidth = n.GetLeadingTriviaWidth();
                    var start = n.Start + leadingTriviaWidth;

                    return n.Name == "template" && start <= position && position <= (n.Start + n.FullWidth);
                });

            TemplateModel item = null;

            // Update selected item in list
            if (elt != null)
            {
                item = CreateTemplateModel(elt.Attributes.ToList(), (XmlElementSyntax) elt);
            }
            _dontUpdateCaret = true;
            SelectedValue = item;
            _dontUpdateCaret = false;
        }

        private void DataBufferOnPostChanged(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("DataBufferOnPostChanged");
            UpdateList();
        }

        [LogExceptions]
        private void UpdateList()
        {
            var snapshot = _dataBuffer.CurrentSnapshot;

            var text = snapshot.GetText();

            _syntax = Parser.ParseText(text);

            var list = GetDescendants(_syntax)
                .OfType<XmlElementSyntax>()
                .Where(n => n.Name == "template")
                .Select(n => CreateTemplateModel(((IXmlElementSyntax)n).Attributes.ToList(), n))
                //.Where(m => m.Name != null && m.Mode != null && m.Match != null)
                .ToList();

            _dontUpdateCaret = true;
            Templates = list;
            _dontUpdateCaret = false;
        }

        public static IEnumerable<SyntaxNode> GetDescendants([NotNull] SyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var result = new List<SyntaxNode>();
            AddDescendants(node, result);
            return result;
        }

        private static void AddDescendants(SyntaxNode node, ICollection<SyntaxNode> resultList)
        {
            resultList.Add(node);

            foreach (var child in node.ChildNodes)
            {
                AddDescendants(child, resultList);
            }
        }

        private static TemplateModel CreateTemplateModel(IList<XmlAttributeSyntax> attributes, SyntaxNode name)
        {
            var item = new TemplateModel()
            {
                Mode = attributes.Where(a => a.Name == "mode").Select(a => a.Value).FirstOrDefault(),
                Name = attributes.Where(a => a.Name == "name").Select(a => a.Value).FirstOrDefault(),
                Match = attributes.Where(a => a.Name == "match").Select(a => a.Value).FirstOrDefault(),
                Start = name.Start + name.GetLeadingTriviaWidth()
            };
            return item;
        }

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<TemplateModel> Templates { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public void TemplateListSelectionChanged(TemplateModel key)
        {
            if (_dontUpdateCaret || key == null)
            {
                return;
            }

            var snapshotPoint = new SnapshotPoint(_dataBuffer.CurrentSnapshot, key.Start);
            _textView.Caret.MoveTo(snapshotPoint);
            _textView.Caret.EnsureVisible();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}