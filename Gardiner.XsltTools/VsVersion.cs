using System;
using System.Diagnostics;
using System.IO;

namespace Gardiner.XsltTools
{
    /// <remarks>
    /// From http://stackoverflow.com/a/11097293/25702
    /// </remarks>
    public static class VsVersion
    {
        private static readonly object _lock = new object();
        static Version _vsVersion;

        public static Version FullVersion
        {
            get
            {
                lock (_lock)
                {
                    if (_vsVersion == null)
                    {
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                        if (File.Exists(path))
                        {
                            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);

                            string verName = fvi.ProductVersion;

                            for (int i = 0; i < verName.Length; i++)
                            {
                                if (!char.IsDigit(verName, i) && verName[i] != '.')
                                {
                                    verName = verName.Substring(0, i);
                                    break;
                                }
                            }
                            _vsVersion = new Version(verName);
                        }
                        else
                            _vsVersion = new Version(0, 0); // Not running inside Visual Studio!
                    }
                }

                return _vsVersion;
            }
        }
    }
}