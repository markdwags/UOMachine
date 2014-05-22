using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Diagnostics;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MemoryStream m_ManifestStream;
        private static List<Files> m_FailList;
        private static Queue<Files> m_FailQueue = new Queue<Files>();
        private static string m_InstallDirectory;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            m_InstallDirectory = GetInstallDirectory();
            m_ManifestStream = new MemoryStream(1024);
            statusLabel.Content = "Waiting...";
            DownloadManifest();
        }

        private string GetInstallDirectory()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (File.Exists(System.IO.Path.Combine(path, "UOMachine.exe")))
            {
                return path;
            }
            else
            {
                System.Windows.MessageBox.Show("Unable to find UOMachine folder, please select the folder containing UOMachine.");
                FolderBrowserDialog fdg = new FolderBrowserDialog();
                fdg.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult d = fdg.ShowDialog();
                if (d == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fdg.SelectedPath))
                {
                    return fdg.SelectedPath;
                }
                return null;
            }
        }

        private void DownloadManifest()
        {
            statusLabel.Content = "Downloading version info...";
            new Download(Resource1.ManifestURL, m_ManifestStream, new Download.FileDownloaded(OnManifestDownloaded));
        }

        public void OnManifestDownloaded(Files files)
        {
            m_ManifestStream.Seek(0, SeekOrigin.Begin);
            string manifest = System.Text.ASCIIEncoding.ASCII.GetString(m_ManifestStream.GetBuffer()).TrimEnd('\0');

            JavaScriptSerializer deser = new JavaScriptSerializer();
            Manifest m = deser.Deserialize<Manifest>(manifest);
            m_FailList = new List<Files>();

            foreach (Files f in m.files)
            {
                try
                {
                    if (!File.Exists(System.IO.Path.Combine(m_InstallDirectory, f.file)))
                    {
                        m_FailQueue.Enqueue(f);
                    }
                    else
                    {
                        if (!CheckFile(System.IO.Path.Combine(m_InstallDirectory, f.file), f.md5sum))
                        {
                            m_FailQueue.Enqueue(f);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                    return;
                }
            }

            if (!m_FailQueue.Any())
            {
                UpdateTextBox("No new updates available.");
                UpdateLabel("Complete.");
                EnableButton(true);
            } else
                ProcessFailQueue();
        }

        public void ProcessFailQueue()
        {
            Process[] runningUOM = Process.GetProcessesByName("UOMachine");
            if (runningUOM.Length > 0)
                System.Windows.MessageBox.Show("An update is available, please close UOMachine to update", "UOMachine running");

            if (m_FailQueue.Any())
            {
                Files f = m_FailQueue.Dequeue();
                string file = System.IO.Path.Combine(m_InstallDirectory, f.file);
                string directory = System.IO.Path.GetDirectoryName(file);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                string url = Resource1.BaseFilesURL + f.file;
                UpdateLabel("Downloading " + f.file);
                new Download(url, file, new Download.FileDownloaded(QueueFileComplete), new Download.ProgressCallback(QueueFileProgress), f);
            }
            else
            {
                UpdateLabel("Complete");
                EnableButton(true);
            }
        }

        public void QueueFileComplete(Files f) 
        {
            if (f.file == "CHANGELOG.txt")
            {
                UpdateTextBox(new StreamReader(System.IO.Path.Combine(m_InstallDirectory, f.file)).ReadToEnd());
            }
            ProcessFailQueue();
        }

        public void UpdateTextBox(string text)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                textBox1.Text = text;
            }));
        }

        public void UpdateLabel(string text)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                statusLabel.Content = text;
            }));
        }

        public void EnableButton(bool enable)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                button1.IsEnabled = enable;
            }));
        }

        public void UpdateProgessBar(int progress)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                progressBar1.Value = progress;
            }));
        }

        public void QueueFileProgress(int progress)
        {
            UpdateProgessBar(progress);
        }

        public static bool CheckFile(string fileName, string md5) 
        {
            MD5 fileMD5 = MD5.Create();
            string res;

            byte[] buffer = File.ReadAllBytes(fileName);
            res = ToHex(fileMD5.ComputeHash(buffer), false);

            if (res == md5)
                return true;
            return false;
        }

        public static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

    }

    public class Manifest
    {
        public string version { get; set; }
        public Files[] files { get; set; }
    }

    public class Files
    {
        public string file;
        public string md5sum;
    }
}
