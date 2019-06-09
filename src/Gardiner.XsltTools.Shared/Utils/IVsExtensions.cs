using System;

using JetBrains.Annotations;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Web.Editor.EditorHelpers;

namespace Gardiner.XsltTools.Utils
{
    public static class IVsExtensions
    {
        public static string GetFileName([NotNull] this ITextBuffer buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            // TextBufferExtensions.GetFileName uses ITextDocument; I don't know if
            // it's possible for a buffer (eg, from a native editor) to not have it
            var firstTry = TextBufferExtensions.GetFileName(buffer);
            if (firstTry != null)
            {
                return firstTry;
            }

            IVsTextBuffer bufferAdapter;

            if (!buffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out bufferAdapter))
            {
                return null;
            }

            var persistFileFormat = bufferAdapter as IPersistFileFormat;
            string ppzsFilename = null;
            int returnCode = -1;

            if (persistFileFormat != null)
            {
                try
                {
                    uint pnFormatIndex;
                    returnCode = persistFileFormat.GetCurFile(out ppzsFilename, out pnFormatIndex);
                }
                catch (NotImplementedException)
                {
                    return null;
                }
            }

            if (returnCode != VSConstants.S_OK)
            {
                return null;
            }

            return ppzsFilename;
        }
    }
}