using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace bot_luis_qna.Log
{
    public class LogHelper
    {
        private string logFile;
        private StreamWriter writer;
        private FileStream fileStream = null;

        public LogHelper(string fileName)
        {
            logFile = fileName;
            CreateDirectory(logFile);
        }
        //Usage
        //Log log = new Log(AppDomain.CurrentDomain.BaseDirectory + @"/log/Log.txt");
        //log.log(basePath);
        public void log(string info)
        {
            try
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(logFile);
                if (!fileInfo.Exists)
                {
                    fileStream = fileInfo.Create();
                    writer = new StreamWriter(fileStream, Encoding.UTF8);
                }
                else
                {
                    fileStream = fileInfo.Open(FileMode.Append, FileAccess.Write);
                    writer = new StreamWriter(fileStream, Encoding.UTF8);
                }
                writer.WriteLine(DateTime.Now + ": " + info);
                //writer.WriteLine("----------------------------------");
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
        }

        public void CreateDirectory(string infoPath)
        {
            DirectoryInfo directoryInfo = Directory.GetParent(infoPath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }
    }
}