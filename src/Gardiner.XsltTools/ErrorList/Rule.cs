using System.Collections.Generic;
using System.IO;

using Microsoft.VisualStudio.Shell.Interop;

namespace Gardiner.XsltTools.ErrorList
{
    internal class Rule
    {
        private string _fileName;

        private int _position = -1;
        public string Description { get; set; }
        public string Help { get; set; }
        public string HelpUrl { get; set; }
        public string Html { get; set; }
        public string Id { get; set; }
        public string Impact { get; set; }
        public List<string> Tags { get; private set; }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                SetLineAndColumn();
            }
        }

        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                SetLineAndColumn();
            }
        }

        //[JsonIgnore]
        public int Line { get; set; }
        //[JsonIgnore]
        public int Column { get; set; }

        public __VSERRORCATEGORY GetSeverity()
        {
            switch (Impact)
            {
                case "critical":
                case "serious":
                    return __VSERRORCATEGORY.EC_ERROR;

                case "moderate":
                    return __VSERRORCATEGORY.EC_WARNING;
            }

            return __VSERRORCATEGORY.EC_MESSAGE;
        }

        private void SetLineAndColumn()
        {
            if (Line != 0 || Column != 0 || Position == -1 || string.IsNullOrEmpty(FileName))
                return;

            var lineCount = 0;
            var columnCount = 0;
            var bufferPos = 0;
            var hasBackslashN = false;

            using (var reader = new StreamReader(FileName))
            {
                var buffer = new char[Position];
                reader.ReadBlock(buffer, 0, Position);

                while (bufferPos < Position)
                {
                    if (buffer[bufferPos] == '\r')
                    {
                        lineCount++;
                        columnCount = 0;
                    }
                    else if (buffer[bufferPos] == '\n')
                    {
                        hasBackslashN = true;
                    }

                    columnCount++;
                    bufferPos++;
                }
            }

            Line = lineCount;
            Column = columnCount - (hasBackslashN ? 1 : 0);
        }

        public override bool Equals(object obj)
        {
            var cast = obj as Rule;

            if (cast == null)
                return false;

            var thisHash = GetHashCode();
            var objHash = cast.GetHashCode();

            return thisHash.Equals(objHash);
        }

        public override int GetHashCode()
        {
            return $"{Id} {FileName} {Position}".GetHashCode();
        }
    }
}