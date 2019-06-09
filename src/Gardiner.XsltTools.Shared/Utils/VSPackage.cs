using System;

using EnvDTE;

using EnvDTE80;

using Microsoft.VisualStudio.Shell;

namespace Gardiner.XsltTools.Utils
{
    public static class VSPackage
    {
        private static DTE2 _dte;

#pragma warning disable VSTHRD010
        internal static DTE2 DTE => _dte ?? (_dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2);
#pragma warning restore VSTHRD010

        public static T GetGlobalService<T>(Type type = null) where T : class
        {
            return Package.GetGlobalService(type ?? typeof(T)) as T;
        }
    }
}