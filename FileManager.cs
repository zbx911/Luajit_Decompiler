using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string bc_dir_name = "compiled_luajit_bytecode";
        private readonly string dis_dir_name = "disassembled_luajit";
        private readonly string dec_dir_name = "decompiled_luajit_source";
        private static string debug_dir_name = "debug";

        public FileManager()
        {
            try
            {
                CreateDirIfNotExists(bc_dir_name);
                CreateDirIfNotExists(dis_dir_name);
                CreateDirIfNotExists(dec_dir_name);
                CreateDirIfNotExists(debug_dir_name);

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

        private void CreateDirIfNotExists(string dirName)
        {
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);
        }

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

        private void WriteAllText(string path, string text)
        {
            if (!File.Exists(path))
                File.Create(path);
            System.IO.File.WriteAllText(path, text);
        }

        /// <summary>
        /// Writes disassembled bytecode output to its proper directory.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="dis">The output from the disassembler.</param>
        public void WriteDisassembledBytecode(string name, string dis)
        {
            WriteAllText(disassembled_dir_path + @"\" + name + ".txt", dis);
        }

        /// <summary>
        /// Writes decompiled source code to its proper directory.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        /// <param name="dec">Decompiled lua source code.</param>
        public void WriteDecompiledCode(string name, string dec)
        {
            WriteAllText(decompiled_dir_path + @"\" + name + ".lua", dec);
        }

        /// <summary>
        /// Writes to the debug file.
        /// </summary>
        /// <param name="info"></param>
        public static void WriteDebug(string info)
        {
            string IOPath = debug_dir_path + @"\" + "info" + ".txt";
            if (!File.Exists(IOPath))
                File.Create(IOPath).Close();
            System.IO.File.AppendAllText(IOPath, info);
        }

        /// <summary>
        /// Clears the debug file.
        /// </summary>
        public static void ClearDebug()
        {
            string IOPath = debug_dir_path + @"\" + "info" + ".txt";
            if(File.Exists(IOPath))
                File.Delete(IOPath);
        }
    }
}
