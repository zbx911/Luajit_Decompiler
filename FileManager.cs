using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Luajit_Decompiler
{
    /// <summary>
    /// Purpose of this class is to manage the files associated with disassembling and decompiling all compiled luajit files in a directory.
    /// </summary>
    class FileManager
    {
        public string bytecode_dir_path;
        public string disassembled_dir_path;
        public string decompiled_dir_path;
        private static string debug_dir_path;
        private string bc_dir_name = "compiled_luajit_bytecode";
        private string dis_dir_name = "disassembled_luajit";
        private string dec_dir_name = "decompiled_luajit_source";
        private static string debug_dir_name = "debug";

        public FileManager()
        {
            try
            {
                if (!Directory.Exists(bc_dir_name))
                    Directory.CreateDirectory(bc_dir_name);

                if (!Directory.Exists(dis_dir_name))
                    Directory.CreateDirectory(dis_dir_name);

                if (!Directory.Exists(dec_dir_name))
                    Directory.CreateDirectory(dec_dir_name);

                if (!Directory.Exists(debug_dir_name))
                    Directory.CreateDirectory(debug_dir_name);

                bytecode_dir_path = Path.GetFullPath(bc_dir_name);
                disassembled_dir_path = Path.GetFullPath(dis_dir_name);
                decompiled_dir_path = Path.GetFullPath(dec_dir_name);
                debug_dir_path = Path.GetFullPath(debug_dir_name);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("InputManager Exception!");
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.StackTrace);
                Console.Read();
            }
        }
        #region Input
        /// <summary>
        /// Returns a dictionary containing the name of a file and its associated bytecode inside a byte array.
        /// </summary>
        public Dictionary<string, byte[]> GetAllCompiledLuajitBytes()
        {
            Dictionary<string, byte[]> fileData = new Dictionary<string, byte[]>();
            var fileNames = Directory.GetFiles(bytecode_dir_path).OrderBy(x => x); //order by name.
            foreach(string fn in fileNames)
            {
                string path = Path.Combine(bytecode_dir_path, fn);
                byte[] fileBytes = File.ReadAllBytes(path);
                fileData.Add(Path.GetFileName(fn), fileBytes);
            }
            return fileData;
        }
        #endregion
        #region Output
        /// <summary>
        /// Writes disassembled bytecode output to its proper directory.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="dis">The output from the disassembler.</param>
        public void WriteDisassembledBytecode(string name, string dis)
        {
            string oPath = disassembled_dir_path + @"\" + name + ".txt";
            System.IO.File.WriteAllText(oPath, dis);
        }

        /// <summary>
        /// Writes decompiled source code to its proper directory.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="dec">Decompiled lua source code.</param>
        public void WriteDecompiledCode(string name, string dec)
        {
            string oPath = decompiled_dir_path + @"\" + name + ".lua";
            System.IO.File.WriteAllText(oPath, dec);
        }

        /// <summary>
        /// Writes to the debug file.
        /// </summary>
        /// <param name="info"></param>
        public static void WriteDebug(string info)
        {
            string IOPath = debug_dir_path + @"\" + "info" + ".txt";
            if (!File.Exists(IOPath))
                File.Create(IOPath);
            System.IO.File.AppendAllText(IOPath, "\r\n" + info);
        }

        /// <summary>
        /// Clears the debug file.
        /// </summary>
        public static void ClearDebug()
        {
            string IOPath = debug_dir_path + @"\" + "info" + ".txt";
            if (!File.Exists(IOPath))
                File.Create(IOPath);
            System.IO.File.WriteAllText(IOPath, "");
        }
        #endregion
    }
}
