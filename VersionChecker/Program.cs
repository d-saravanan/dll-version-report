using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace VersionChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path for the development folder. Example: " + @"G:\Projects\InEight\SourceCode\InterStellar\Development\");

            string path = Console.ReadLine();

            Console.WriteLine("Got the following path: " + path);
            Console.WriteLine("Starting to process the solution");

            string[] projectFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);

            SortedDictionary<string, List<string>> projectRefs = new SortedDictionary<string, List<string>>();

            foreach (var projectFile in projectFiles)
            {
                XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
                XDocument projDefinition = XDocument.Load(projectFile);

                var fileName = projectFile.Split('\\').Last();

                var references = projDefinition
                    .Element(msbuild + "Project")
                    .Elements(msbuild + "ItemGroup")
                    .Elements(msbuild + "Reference")
                    .Select(refElem => refElem.FirstAttribute.Value);

                foreach (var reference in references)
                {
                    var includeDetails = reference.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    if (includeDetails.Count() < 2) continue;

                    string key = string.Join(",", includeDetails.Take(2));

                    if (!projectRefs.ContainsKey(key)) projectRefs.Add(key, new List<string>());

                    if (!projectRefs[key].Contains(fileName)) projectRefs[key].Add(fileName);
                }
            }

            StringBuilder outputStream = new StringBuilder();

            foreach (var projRef in projectRefs)
            {
                //Console.WriteLine(projRef.Key);
                outputStream.AppendLine(projRef.Key);
                foreach (var proj in projRef.Value)
                {
                    outputStream.AppendLine(proj.Replace(".csproj", string.Empty));
                    //Console.WriteLine(proj);
                }
            }

            File.WriteAllText(Path.Combine(path, "dlls-and-versions.csv"), outputStream.ToString());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Done Processing all Projects");

            Console.ReadKey();
        }
    }
}
