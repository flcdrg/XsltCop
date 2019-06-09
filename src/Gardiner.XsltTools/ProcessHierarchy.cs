using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using Gardiner.XsltTools.ErrorList;

using JetBrains.Annotations;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Gardiner.XsltTools
{
    public class ProcessHierarchy
    {
        public void Process([NotNull] IVsHierarchy hierarchy, string projectName)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

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
                    Process(nestedHierarchy, projectName);
                }
            }
            else // The node is not the root of another hierarchy, it is a regular node
            {
                ShowNodeName(hierarchy, itemId, projectName);

                // Get the first visible child node
                object value;
                result = hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_FirstVisibleChild, out value);

                while (result == VSConstants.S_OK && value != null)
                {
                    if (value is int && (uint)(int)value == VSConstants.VSITEMID_NIL)
                    {
                        // No more nodes
                        break;
                    }
                    var visibleChildNode = GetItemId(value);

                    // Enter in recursion
                    ProcessHierarchyNodeRecursively(hierarchy, visibleChildNode, projectName);

                    // Get the next visible sibling node
                    result = hierarchy.GetProperty(visibleChildNode, (int)__VSHPROPID.VSHPROPID_NextVisibleSibling,
                        out value);
                }
            }
        }

        // from http://dotneteers.net/blogs/divedeeper/archive/2008/10/16/LearnVSXNowPart36.aspx
        private static uint GetItemId(object pvar)

        {
            if (pvar == null)
            {
                return VSConstants.VSITEMID_NIL;
            }

            if (pvar is int)
            {
                return (uint) (int) pvar;
            }

            if (pvar is uint)
            {
                return (uint) pvar;
            }

            if (pvar is short)
            {
                return (uint) (short) pvar;
            }

            if (pvar is ushort)
            {
                return (ushort) pvar;
            }

            if (pvar is long)
            {
                return (uint) (long) pvar;
            }

            return VSConstants.VSITEMID_NIL;
        }

        private static void ShowNodeName(IVsHierarchy hierarchy, uint itemId, string projectName)
        {
            Guid guid;
            var result = hierarchy.GetGuidProperty(itemId, (int)__VSHPROPID.VSHPROPID_TypeGuid, out guid);

            if (result == VSConstants.S_OK && guid == VSConstants.GUID_ItemType_PhysicalFile)
            {
                string canonicalName;
                result = hierarchy.GetCanonicalName(itemId, out canonicalName);

                if (result == VSConstants.S_OK && canonicalName != null &&
                    (canonicalName.EndsWith(".xsl", StringComparison.InvariantCultureIgnoreCase) 
                    || canonicalName.EndsWith(".xslt", StringComparison.InvariantCultureIgnoreCase)))
                {
                    Debug.WriteLine($"\tCanonical name: {canonicalName} {guid}");

                    var checker = new XsltChecker();
                    var checkResult = checker.CheckFile(canonicalName);

                    if (checkResult == null)
                    {
                        return;
                    }

                    checkResult.Project = projectName;

                    ErrorListService.ProcessLintingResults(checkResult);
                }
            }
        }
    }
}