using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
namespace FileWalker
{
    class Program
    {
        static void Main(string[] args)
        {
        Start:
            List<string> fileNames = new List<string>();
            if (args.Length > 0)
            {
                var fileForDel = File.ReadAllLines(args[0]).ToList();
                Console.WriteLine("Type delete to proceed");
                var resp1 = Console.ReadLine();
                if (resp1 == "delete")
                {
                    fileForDel.ForEach(X => { try { if (File.Exists(X)) File.Delete(X); Console.WriteLine(X); } catch { Console.WriteLine("Error Deleting: " + X); } });
                }
                Console.WriteLine("Removed");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Please Enter File Name:");
            var fileName = Console.ReadLine();
            Console.WriteLine("Please root Path:");
            var rootPath = Console.ReadLine();
            if (rootPath == "*")
                foreach (var drive in Environment.GetLogicalDrives())
                {
                    if (drive != "C:\\")
                        foreach (var file in GetFiles(drive, fileName))
                        {
                            fileNames.Add(file);
                        }
                }
            else
                foreach (var file in GetFiles(rootPath, fileName))
                {
                    fileNames.Add(file);
                }
            var fileNameNew = "files_" + DateTime.Now.ToString("HHmmss") + ".txt";
            File.AppendAllLines(fileNameNew, fileNames.ToArray());

        WithList:
            Console.WriteLine("Files Found: " + fileNames.Count);
            Console.WriteLine("Commands To Execute[]");
            Console.WriteLine("n | no => to try again");
            Console.WriteLine("delete => to delte all files found");
            Console.WriteLine("show => to show all files found");
            Console.WriteLine("file => to show all files found");
            Console.WriteLine("hash => to get hash (MD5 and SHA256) for all files found");
            Console.WriteLine("creationtime now => set creation time of all files found to now");
            Console.WriteLine("creationtime dt:<datetime> => set creation time of all files found to yyyy-mm-dd hh:mm:ss ");
            Console.WriteLine("creationtime modifiedtime => set creation time of all files found to modified time ");
            Console.WriteLine("execute all => obvious aan! ");
            Console.WriteLine("");
            Console.Write(">>>");

            var resp = Console.ReadLine().ToLower();


            if (resp == "execute all")
            {
                fileNames.ForEach(X => { System.Diagnostics.Process.Start(X); });
            }
            if (resp == "n" || resp == "no")
            {
                Console.Clear();
                goto Start;
            }
            if (resp == "delete")
            {
                fileNames.ForEach(X => { try { if (File.Exists(X)) File.Delete(X); Console.WriteLine(X); } catch { Console.WriteLine("Error Deleting: " + X); } });
                Console.WriteLine("Removed");
            }
            else if (resp == "show")
            {
                fileNames.ForEach(X => { Console.WriteLine(X + "\r\n"); });
            }
            else if (resp == "file")
            {
                System.Diagnostics.Process.Start(fileNameNew);
                Console.WriteLine("Any Other Operation");
                goto WithList;
            }
            else if (resp == "hash")
            {

                fileNames.ForEach(X =>
                {

                    FileInfo fi = new FileInfo(X);
                    var path = fi.Directory.FullName;

                    Console.WriteLine(path);
                    Console.WriteLine(GetHashForExe(X, path, true, false));
                    Console.WriteLine(GetHashForExe(X, path, false, true));
                });
            }
            else if (resp == "creationtime now")
            {

                fileNames.ForEach(X =>
                {
                    var a = DateTime.Now;
                    File.SetCreationTime(X, a);
                    File.SetLastWriteTime(X, a);
                    Console.WriteLine(X + "\tCreationTime: " + DateTime.Now);
                });
            }
            else if (resp.StartsWith("creationtime dt:"))
            {
                DateTime dt;
                try
                {
                    dt = Convert.ToDateTime(resp.Replace("creationtime dt:", ""));
                }
                catch { Console.Clear(); Console.WriteLine("Invalid DateTime Format Provided"); goto Start; }
                fileNames.ForEach(X =>
                {
                    File.SetCreationTime(X, dt);
                    File.SetLastWriteTime(X, dt);
                    Console.WriteLine(X + "\tCreationTime: " + DateTime.Now);

                });
            }
            else if (resp == "creationtime modifiedtime")
            {
                fileNames.ForEach(X =>
                {
                    var a = File.GetLastWriteTime(X);
                    File.SetCreationTime(X, a);
                    File.SetLastWriteTime(X, a);
                    Console.WriteLine(X + "\tCreationTime: " + DateTime.Now);
                });
            }
            Console.ReadKey();
            Console.Clear();
            goto Start;
        }

        static string GetHashForExe(string ExePath, string outputPath, bool isSHA, bool isMD5)
        {
            string psScript = "script_" + DateTime.Now.ToString("hh mm ss") + (isSHA == true ? "_SHA256_" : "_MD5_") + ".ps1";
            string SHA256Script = "get-filehash -path \"" + ExePath + "\" | format-list";
            string MD5Script = "certutil -hashfile \"" + ExePath + "\" MD5";

            string output = "";
            if (isSHA == true)
            {
                output = RunScript(SHA256Script);
            }
            else if (isMD5 == true)
            {
                output = RunScript(MD5Script);
            }
            File.AppendAllText(outputPath + "\\Hash.txt", output + "\r\n\r\n");
            return output;

        }

        private static string RunScript(string scriptText)
        {
            // create Powershell runspace

            Runspace runspace = RunspaceFactory.CreateRunspace();

            // open it

            runspace.Open();

            // create a pipeline and feed it the script text

            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(scriptText);

            // add an extra command to transform the script
            // output objects into nicely formatted strings

            // remove this line to get the actual objects
            // that the script returns. For example, the script

            // "Get-Process" returns a collection
            // of System.Diagnostics.Process instances.

            pipeline.Commands.Add("Out-String");

            // execute the script

            Collection<PSObject> results = pipeline.Invoke();

            // close the runspace

            runspace.Close();

            // convert the script result into a single string

            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.AppendLine(obj.ToString());
            }

            return stringBuilder.ToString();
        }

        public static IEnumerable<string> GetFiles(string root, string searchPattern)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var path = pending.Pop();
                string[] next = null;
                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch { }
                if (next != null && next.Length != 0)
                    foreach (var file in next) yield return file;
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next) pending.Push(subdir);
                }
                catch { }
            }
        }
    }
}

