using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using Updater;

namespace Updater
{
    public class Download
    {

        public delegate void FileDownloaded(Files f);
        public delegate void ProgressCallback(int progress);

        private string m_Url;
        private Stream m_Stream;
        private Thread m_Thread;
        private FileDownloaded m_CompletionCallback;
        private ProgressCallback m_ProgressCallback;
        private Files m_File;

        public Download(string url, Stream stream, FileDownloaded completionCallback) {
            try
            {
                m_Url = url;
                m_Stream = stream;
                m_CompletionCallback = completionCallback;
                m_Thread = new Thread(new ThreadStart(DownloadFile));
                m_Thread.Start();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Download(string url, string file, FileDownloaded completionCallback, ProgressCallback progressCallback, Files f)
        {
            try
            {
                m_Url = url;
                m_CompletionCallback = completionCallback;
                m_Stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
                m_ProgressCallback = progressCallback;
                m_File = f;
                m_Thread = new Thread(new ThreadStart(DownloadFile));
                m_Thread.Start();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void DownloadFile()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(m_Url);
            HttpWebResponse webResponse;
            Stream webStream;

            webResponse = (HttpWebResponse)webRequest.GetResponse();
            if (webResponse.StatusCode == HttpStatusCode.OK)
            {
                webStream = webResponse.GetResponseStream();
                long bytes = 0;
                long length = webResponse.ContentLength;
                byte[] buffer = new byte[length > 8192 ? 8192 : length];

                while (bytes < webResponse.ContentLength)
                {
                    int num = (int)Math.Min(buffer.Length, length - bytes);
                    int read = webStream.Read(buffer, 0, num);
                    m_Stream.Write(buffer, 0, read);
                    bytes += read;
                    if (m_ProgressCallback != null)
                    {
                        int progress = (int)(100 * bytes / length);
                        m_ProgressCallback(progress);
                    }
                }
                if (m_Stream is FileStream)
                    m_Stream.Close();
                if (m_CompletionCallback != null)
                {
                    m_CompletionCallback(m_File);
                }
            }
            else
            {
                throw new Exception("File could not be downloaded.");
            }
        }
    }
}
