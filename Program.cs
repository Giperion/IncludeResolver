using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IncludeCollector
{
    class Program
    {
        static string ClearFromWhitespaces(string line)
        {
            List<char> result = new List<char>();

            foreach (char ch in line)
            {
                if (!Char.IsWhiteSpace(ch) && !(ch == '\t'))
                {
                    result.Add(ch);
                }
            }

            return new string(result.ToArray());
        }

        class Include : IComparable<Include>
        {
            public string Name;
            public int References;

            public int CompareTo(Include other)
            {
                if (other.References == References) return 0;
                if (other.References > References)
                {
                    return 1;
                }

                return -1;
            }
        }

        static void Main(string[] args)
        {
            string TargetDir = args[0];
            string ExcludeDir = null;
            if (args.Length > 1)
            {
                ExcludeDir = args[1];
            }

            IEnumerable<string> files = Directory.EnumerateFiles(TargetDir, "*.h", SearchOption.AllDirectories);
            IEnumerable<string> files2 = Directory.EnumerateFiles(TargetDir, "*.cpp", SearchOption.AllDirectories);
            IEnumerable<string> files3 = Directory.EnumerateFiles(TargetDir, "*.cc", SearchOption.AllDirectories);

            List<string> AllFiles = new List<string>(files);
            AllFiles.AddRange(files2);
            AllFiles.AddRange(files3);

            if (ExcludeDir != null)
            {
                List<string> filesToRemove = new List<string>();
                // Remove excluded
                foreach (string filename in AllFiles)
                {
                    if (filename.StartsWith(ExcludeDir))
                    {
                        filesToRemove.Add(filename);
                    }
                }

                foreach (string excludedFile in filesToRemove)
                {
                    AllFiles.Remove(excludedFile);
                }
            }

            // Check for repeated files
            List<string> UniqueFiles = new List<string>();
            foreach (string filename in AllFiles)
            {
                string clearName = Path.GetFileName(filename);
                int findIndx = UniqueFiles.FindIndex(theName => theName == clearName);
                if (findIndx != -1)
                {
                    Console.WriteLine("Fuck!");
                }
                else
                {
                    UniqueFiles.Add(clearName);
                }
            }


            List<Include> includes = new List<Include>();

            Dictionary<string, List<Include>> file2Include = new Dictionary<string, List<Include>>();

            foreach (string file in AllFiles)
            {
                using (StreamReader reader = File.OpenText(file))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        line = ClearFromWhitespaces(line);

                        if (line.StartsWith("#include"))
                        {
                            // extract include name
                            char bracketStart = line.FirstOrDefault(ch => ch == '<');
                            char quoteStart = line.FirstOrDefault(ch => ch == '\"');

                            string includeName = null;

                            if (bracketStart != default(char))
                            {
                                int StartIndx = line.IndexOf('<');
                                int EndIndx = line.IndexOf('>', StartIndx + 1);

                                includeName = line.Substring(StartIndx + 1, EndIndx - StartIndx - 1);
                            }

                            if (quoteStart != default(char))
                            {
                                int StartIndx = line.IndexOf('\"');
                                int EndIndx = line.IndexOf('\"', StartIndx + 1);

                                includeName = line.Substring(StartIndx + 1, EndIndx - StartIndx - 1);
                            }

                            string[] SubFiles = includeName.Split('/');
                            if (SubFiles.Length > 1)
                            {
                                includeName = SubFiles[SubFiles.Length - 1];
                            }

                            if (includeName != null)
                            {
                                int ExistedInclude = includes.FindIndex(theInclude => theInclude.Name == includeName);
                                Include theInclude2 = null;
                                if (ExistedInclude == -1)
                                {
                                    theInclude2 = new Include();
                                    theInclude2.Name = includeName;
                                    theInclude2.References = 1;
                                    includes.Add(theInclude2);
                                }
                                else
                                {
                                    theInclude2 = includes.ElementAt(ExistedInclude);
                                    theInclude2.References++;
                                }

                                var PairValueShit = file2Include.FirstOrDefault(thePair => thePair.Key == file);
                                
                                if (string.IsNullOrEmpty( PairValueShit.Key))
                                {
                                    List<Include> filesIncludes = new List<Include>();
                                    filesIncludes.Add(theInclude2);
                                    file2Include.Add(file, filesIncludes);
                                }
                                else
                                {
                                    PairValueShit.Value.Add(theInclude2);
                                }
                            }
                        }
                    }
                }
            }

            includes.Sort();

            foreach (Include item in includes)
            {
                Console.WriteLine($"{item.Name} ref: {item.References}");
            }

            Console.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
