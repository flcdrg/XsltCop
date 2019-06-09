using System;
using System.IO;
using System.Runtime.InteropServices;

using EnvDTE;

namespace Gardiner.XsltTools.Utils
{
    // mads
    internal static class ProjectHelpers
    {
        ///<summary>Gets the base directory of a specific Project, or of the active project if no parameter is passed.</summary>
        public static string GetRootFolder(Project project = null)
        {
            try
            {
                project = project ?? GetActiveProject();

                if (project == null || project.Collection == null)
                {
                    var doc = VSPackage.DTE.ActiveDocument;
                    if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                    {
                        return GetProjectFolder(doc.FullName);
                    }

                    return string.Empty;
                }
                if (string.IsNullOrEmpty(project.FullName))
                {
                    return null;
                }

                string fullPath;
                try
                {
                    fullPath = project.Properties.Item("FullPath").Value as string;
                }
                catch (ArgumentException)
                {
                    try
                    {
                        // MFC projects don't have FullPath, and there seems to be no way to query existence
                        fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                    }
                    catch (ArgumentException)
                    {
                        // Installer projects have a ProjectPath.
                        fullPath = project.Properties.Item("ProjectPath").Value as string;
                    }
                }

                if (String.IsNullOrEmpty(fullPath))
                {
                    return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : "";
                }

                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }

                if (File.Exists(fullPath))
                {
                    return Path.GetDirectoryName(fullPath);
                }

                return "";
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //Logger.Log(ex);
                return string.Empty;
            }
        }

        ///<summary>Gets the currently active project (as reported by the Solution Explorer), if any.</summary>
        public static Project GetActiveProject()
        {
            try
            {
                Array activeSolutionProjects = VSPackage.DTE.ActiveSolutionProjects as Array;

                if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                {
                    return activeSolutionProjects.GetValue(0) as Project;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //Logger.Log("Error getting the active project" + ex);
            }

            return null;
        }


        ///<summary>Gets the directory containing the project for the specified file.</summary>
        private static string GetProjectFolder(ProjectItem item)
        {
            if (item == null || item.ContainingProject == null || item.ContainingProject.Collection == null || string.IsNullOrEmpty(item.ContainingProject.FullName)) // Solution items
            {
                return null;
            }

            return GetRootFolder(item.ContainingProject);
        }

        ///<summary>Gets the directory containing the project for the specified file.</summary>
        public static string GetProjectFolder(string fileNameOrFolder)
        {
            if (string.IsNullOrEmpty(fileNameOrFolder))
            {
                return GetRootFolder();
            }

            ProjectItem item = GetProjectItem(fileNameOrFolder);
            string projectFolder = null;

            if (item != null)
            {
                projectFolder = GetProjectFolder(item);
            }

            return projectFolder;
        }

        internal static ProjectItem GetProjectItem(string fileName)
        {
            try
            {
                return VSPackage.DTE.Solution.FindProjectItem(fileName);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //Logger.Log(exception.Message);

                return null;
            }
        }

        public static ProjectItem AddFileToProject(string parentFileName, string fileName)
        {
            if (Path.GetFullPath(parentFileName) == Path.GetFullPath(fileName) || !File.Exists(fileName))
            {
                return null;
            }

            fileName = Path.GetFullPath(fileName);  // WAP projects don't like paths with forward slashes

            var item = GetProjectItem(parentFileName);

            if (item == null || item.ContainingProject == null || string.IsNullOrEmpty(item.ContainingProject.FullName))
            {
                return null;
            }

            var dependentItem = GetProjectItem(fileName);

            if (dependentItem != null && item.ContainingProject.GetType().Name == "OAProject" && item.ProjectItems != null)
            {
                // WinJS
                ProjectItem addedItem = null;

                try
                {
                    addedItem = dependentItem.ProjectItems.AddFromFile(fileName);

                    // create nesting
                    if (Path.GetDirectoryName(parentFileName) == Path.GetDirectoryName(fileName))
                    {
                        addedItem.Properties.Item("DependentUpon").Value = Path.GetFileName(parentFileName);
                    }
                }
                catch (COMException) { }
#pragma warning disable CA1031 // Do not catch general exception types
                catch { return dependentItem; }
#pragma warning restore CA1031 // Do not catch general exception types

                return addedItem;
            }

            if (dependentItem != null) // File already exists in the project
            {
                return null;
            }
            else if (item.ContainingProject.GetType().Name != "OAProject" && item.ProjectItems != null && Path.GetDirectoryName(parentFileName) == Path.GetDirectoryName(fileName))
            {   // WAP
                try
                {
                    return item.ProjectItems.AddFromFile(fileName);
                }
                catch (COMException) { }
            }
            else if (Path.GetFullPath(fileName).StartsWith(GetRootFolder(item.ContainingProject), StringComparison.OrdinalIgnoreCase))
            {   // Website
                try
                {
                    return item.ContainingProject.ProjectItems.AddFromFile(fileName);
                }
                catch (COMException) { }
            }

            return null;
        }
    }
}
