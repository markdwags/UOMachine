using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace VersionInfo
{
    public class FileEntry
    {
        private string m_Filename;
        private string m_MD5Sum;

        public string file
        {
            get { return m_Filename; }
        }

        public string md5sum
        {
            get { return m_MD5Sum; }
        }

        public FileEntry(string file)
        {
            MD5 fileMD5 = MD5.Create();
            string res;

            byte[] buffer = File.ReadAllBytes(Path.Combine(Program.FilesPath, file));
            res = ToHex(fileMD5.ComputeHash(buffer), false);

            m_Filename = file;
            m_MD5Sum = res;
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", m_Filename, m_MD5Sum);
        }

        public static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
    public class Json
    {
        public string version;
        public List<FileEntry> files;

        public Json()
        {
            files = new List<FileEntry>();
        }

        public void Add(FileEntry file) 
        {
           files.Add(file);
        }
    }
    class Program
    {
        public static string FilesPath = @"C:\Users\John\Documents\GitHub\gh-pages\files\";
        public static Json json;

        static void Main(string[] args)
        {
            json = new Json();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.IO.Path.Combine(FilesPath, "UOMachine.exe"));

            json.version = fvi.FileVersion;

            try
            {
                foreach (string d in Directory.GetFiles(FilesPath))
                {
                    Console.WriteLine();
                    json.Add(new FileEntry(GetRelativePath(d, FilesPath)));
                }

                foreach (string d in Directory.GetDirectories(FilesPath))
                {
                    AddDir(d, FilesPath, json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            JavaScriptSerializer ser = new JavaScriptSerializer();
            File.WriteAllText(Path.Combine(FilesPath + @"\..\", "version.json"), ser.Serialize(json));
        }

        static void AddDir(string dir, string relative, Json json)
        {
            foreach (string d in Directory.GetDirectories(dir))
            {
                AddDir(d, relative, json);
            }

            foreach (string d in Directory.GetFiles(dir))
            {
                json.Add(new FileEntry(GetRelativePath(d, FilesPath)));
            }
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        //class JSON_PrettyPrinter
        //{
        //    public static string Process(string inputText)
        //    {
        //        bool escaped = false;
        //        bool inquotes = false;
        //        int column = 0;
        //        int indentation = 0;
        //        Stack<int> indentations = new Stack<int>();
        //        int TABBING = 8;
        //        StringBuilder sb = new StringBuilder();
        //        foreach (char x in inputText)
        //        {
        //            sb.Append(x);
        //            column++;
        //            if (escaped)
        //            {
        //                escaped = false;
        //            }
        //            else
        //            {
        //                if (x == '\\')
        //                {
        //                    escaped = true;
        //                }
        //                else if (x == '\"')
        //                {
        //                    inquotes = !inquotes;
        //                }
        //                else if (!inquotes)
        //                {
        //                    if (x == ',')
        //                    {
        //                        // if we see a comma, go to next line, and indent to the same depth
        //                        sb.Append("\r\n");
        //                        column = 0;
        //                        for (int i = 0; i < indentation; i++)
        //                        {
        //                            sb.Append(" ");
        //                            column++;
        //                        }
        //                    }
        //                    else if (x == '[' || x == '{')
        //                    {
        //                        // if we open a bracket or brace, indent further (push on stack)
        //                        indentations.Push(indentation);
        //                        indentation = column;
        //                    }
        //                    else if (x == ']' || x == '}')
        //                    {
        //                        // if we close a bracket or brace, undo one level of indent (pop)
        //                        indentation = indentations.Pop();
        //                    }
        //                    else if (x == ':')
        //                    {
        //                        // if we see a colon, add spaces until we get to the next
        //                        // tab stop, but without using tab characters!
        //                        while ((column % TABBING) != 0)
        //                        {
        //                            sb.Append(' ');
        //                            column++;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        return sb.ToString();
        //    }

        //}
    }
}
