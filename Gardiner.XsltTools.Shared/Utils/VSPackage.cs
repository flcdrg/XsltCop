using System;

using EnvDTE;

using EnvDTE80;

using Microsoft.VisualStudio.Shell;

namespace Gardiner.XsltTools.Utils
{
    public static class VSPackage
    {
        private static DTE2 _dte;

        internal static DTE2 DTE => _dte ?? (_dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2);

        public static T GetGlobalService<T>(Type type = null) where T : class
        {
            return Package.GetGlobalService(type ?? typeof(T)) as T;
        }
    }
}