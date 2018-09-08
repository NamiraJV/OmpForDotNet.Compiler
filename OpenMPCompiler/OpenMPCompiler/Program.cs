using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenMPCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = args[1].Substring(1);
            StreamReader reader = new StreamReader(fileName);
            string str = reader.ReadToEnd();
            reader.Close();
            List<string> result = new List<string>();
            string solutionPath = string.Empty;
            try
            {
                solutionPath = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
                string solutionFile = System.IO.Directory.GetFiles(solutionPath, "*.sln")[0];
                CodeProcessor codeProcessor = new CodeProcessor();
                result = codeProcessor.ProcessSolution(Directory.GetCurrentDirectory().ToString(), solutionFile).Result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            str = ReplaceFileNamesFromCommand(str, result);

            StreamWriter writer = new StreamWriter(fileName);
            writer.AutoFlush = true;
            writer.WriteLine(str);
            
            writer.Close();

            // TODO: figure out how to configure this path to compiler
            var p = Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Roslyn\csc.exe",
                string.Join(" ", args));
            p.WaitForExit();


            var filesToDelete = Directory.GetFiles(solutionPath, "*.cs", SearchOption.AllDirectories).Where(f => f.Contains("_tmp_generated_doc"));
            foreach(var file in filesToDelete)
            {
                File.Delete(file);
            }
            
        }

        private static string ReplaceFileNamesFromCommand(string command, List<string> namesToReplace)
        {
            string[] args = command.Split(' ');
            int startIndex = 0;
            int endIndex = 0;
            for (int i = 0, n = args.Length; i < n; i++)
            {
                if(args[i] == "/utf8output")
                {
                    startIndex = i + 1;
                }

                if(args[i].StartsWith("\""))
                {
                    endIndex = i;
                }
            }

            for(int i = startIndex; i < endIndex; i++)
            {
                if (namesToReplace.Contains(args[i]))
                {
                    args[i] = args[i].Substring(0, args[i].Length - 3) + "_tmp_generated_doc.cs";

                }
            }

            return string.Join(" ", args);
        }
    }
}
