using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using ExtensionMethods;

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
            res = fileMD5.ToHex(buffer, false);

            m_Filename = file;
            m_MD5Sum = res;
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", m_Filename, m_MD5Sum);
        }
    }

    public class VersionInfo
    {
        public string version;
        public string updated;
        public List<FileEntry> files;

        public VersionInfo()
        {
            files = new List<FileEntry>();
            updated = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }

        public void Add(FileEntry file) 
        {
           files.Add(file);
        }
    }

    class Program
    {
        public static string FilesPath = @"C:\Users\John\Documents\GitHub\gh-pages\files\";
        public static VersionInfo versionInfo;

        static void Main(string[] args)
        {
            versionInfo = new VersionInfo();

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(System.IO.Path.Combine(FilesPath, "UOMachine.exe"));

            versionInfo.version = fvi.FileVersion;

            try
            {
                foreach (string d in Directory.GetFiles(FilesPath))
                {
                    Console.WriteLine();
                    versionInfo.Add(new FileEntry(GetRelativePath(d, FilesPath)));
                }

                foreach (string d in Directory.GetDirectories(FilesPath))
                {
                    AddDir(d, FilesPath, versionInfo);
                }

                JavaScriptSerializer ser = new JavaScriptSerializer();
                File.WriteAllText(Path.Combine(FilesPath + @"\..\", "version.json"), ser.Serialize(versionInfo));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void AddDir(string dir, string relative, VersionInfo versionInfo)
        {
            foreach (string d in Directory.GetDirectories(dir))
            {
                AddDir(d, relative, versionInfo);
            }

            foreach (string d in Directory.GetFiles(dir))
            {
                versionInfo.Add(new FileEntry(GetRelativePath(d, FilesPath)));
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
    }
}

namespace ExtensionMethods
{
    public static class MD5Extension
    {
        public static string ToHex(this MD5 md5, byte[] buffer, bool upperCase)
        {
            byte[] bytes = md5.ComputeHash(buffer);
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }
}