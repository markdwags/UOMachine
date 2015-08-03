using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace VersionInfo
{
    public class Filter
    {
        private static List<string> m_FilterList;

        public static void Init()
        {
            m_FilterList = new List<string>();

            string line = "";

            StreamReader file = new StreamReader(@"C:\Users\Johnny Mantas\Documents\gh-pages\VersionInfo\.filter");

            while ((line = file.ReadLine()) != null)
            {
                if (line.Length > 0 && !line.StartsWith("#"))
                {
                    m_FilterList.Add(line.TrimEnd('\n'));
                }
            }
        }

        public static bool Matches(string file)
        {
            foreach (string filter in m_FilterList)
            {
                Regex r = new Regex(filter, RegexOptions.IgnoreCase);

                Match m = r.Match(file);

                if (m.Success)
                    return true;
            }
            return false;
        }
    }
}
