using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Shell.TableManager;

namespace Gardiner.XsltTools.ErrorList
{
    internal class SinkManager : IDisposable
    {
        private readonly ITableDataSink _sink;
        private readonly TableDataSource _errorList;
        private readonly List<TableEntriesSnapshot> _snapshots = new List<TableEntriesSnapshot>();

        internal SinkManager(TableDataSource errorList, ITableDataSink sink)
        {
            _sink = sink;
            _errorList = errorList;

            errorList.AddSinkManager(this);
        }

        public void Dispose()
        {
            // Called when the person who subscribed to the data source disposes of the cookie (== this object) they were given.
            _errorList.RemoveSinkManager(this);
        }

        internal void Clear()
        {
            _sink.RemoveAllSnapshots();
        }

        internal void UpdateSink(IEnumerable<TableEntriesSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
            {
                var existing = _snapshots.FirstOrDefault(s => s.Url == snapshot.Url);

                if (existing != null)
                {
                    _snapshots.Remove(existing);
                    _sink.ReplaceSnapshot(existing, snapshot);
                }
                else
                {
                    _sink.AddSnapshot(snapshot);
                }

                _snapshots.Add(snapshot);
            }
        }

        internal void RemoveSnapshots(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                var existing = _snapshots.FirstOrDefault(s => s.Url == url);

                if (existing != null)
                {
                    _snapshots.Remove(existing);
                    _sink.RemoveSnapshot(existing);
                }
            }
        }
    }
}