using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Gardiner.XsltTools.ErrorList
{
    internal class TableDataSource : ITableDataSource
    {
        private static TableDataSource _instance;

        private static readonly Dictionary<string, TableEntriesSnapshot> _snapshots =
            new Dictionary<string, TableEntriesSnapshot>();

        private readonly List<SinkManager> _managers = new List<SinkManager>();

        private TableDataSource()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var compositionService = (IComponentModel) ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel));
            if (compositionService == null)
            {
                throw new ArgumentNullException(nameof(compositionService));
            }

            compositionService.DefaultCompositionService.SatisfyImportsOnce(this);

            var manager = TableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            manager.AddSource(this, StandardTableColumnDefinitions.DetailsExpander,
                StandardTableColumnDefinitions.ErrorSeverity, StandardTableColumnDefinitions.ErrorCode,
                StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.BuildTool,
                StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.ErrorCategory,
                StandardTableColumnDefinitions.Text, StandardTableColumnDefinitions.DocumentName,
                StandardTableColumnDefinitions.Line, StandardTableColumnDefinitions.Column);
        }

        [Import]
        private ITableManagerProvider TableManagerProvider { get; set; }

        public static TableDataSource Instance
        {
            get { return _instance ?? (_instance = new TableDataSource()); }
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        public bool HasErrors
#pragma warning restore CA1822
        {
            get { return _snapshots.Any(); }
        }

        public void AddSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Add(manager);
            }
        }

        public void RemoveSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Remove(manager);
            }
        }

        public void UpdateAllSinks()
        {
            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.UpdateSink(_snapshots.Values);
                }
            }
        }

        public void AddErrors(AccessibilityResult result)
        {
            if (result == null || !result.Violations.Any())
            {
                return;
            }

            result.Violations = result.Violations.Where(v => !_snapshots.Any(s => s.Value.Errors.Contains(v)));

            var snapshot = new TableEntriesSnapshot(result);
            _snapshots[result.Url] = snapshot;

            UpdateAllSinks();
        }

        public void CleanErrors(params string[] urls)
        {
            foreach (var url in urls)
            {
                if (_snapshots.ContainsKey(url))
                {
                    _snapshots[url].Dispose();
                    _snapshots.Remove(url);
                }
            }

            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.RemoveSnapshots(urls);
                }
            }

            UpdateAllSinks();
        }

        public void CleanAllErrors()
        {
            foreach (var url in _snapshots.Keys)
            {
                var snapshot = _snapshots[url];
                snapshot?.Dispose();
            }

            _snapshots.Clear();

            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.Clear();
                }
            }

            UpdateAllSinks();
        }

        #region ITableDataSource members

        public string SourceTypeIdentifier
        {
            get { return StandardTableDataSources.ErrorTableDataSource; }
        }

        public string Identifier
        {
            get { return "8339E980-B78E-48EF-A9A5-C8BA7947A37B"; }
        }

        public string DisplayName
        {
            get { return Vsix.Name; }
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            return new SinkManager(this, sink);
        }

        #endregion
    }
}