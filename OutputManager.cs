using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luajit_Decompiler
{
    /// <summary>
    /// TODO: Implement the output management.
    /// </summary>
    class OutputManager
    {
        private string path;

        public OutputManager(string path)
        {
            this.path = path;
        }

        public void Write(string text)
        {
            System.IO.File.WriteAllText(path, text);
        }

        public void Write(string[] text)
        {
            System.IO.File.WriteAllLines(path, text);
        }

        public void Write(List<string> text)
        {
            foreach (string t in text)
                Write(t);
        }
    }
}
