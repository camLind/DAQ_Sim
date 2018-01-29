using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataLogging
{
    public class DataLog
    {
        private string filePath;
        private string fileName;
        private bool logIsOK;

        private char delim;

        List<string> entries;

        public DataLog(char newDelim = ',')
        {
            SetFileName();
            SetPath();

            entries = new List<string>();

            try
            {
                StreamWriter fStream;

                fStream = File.CreateText(Path.Combine(filePath, fileName));
                fStream.Close();

                //Write Header

                delim = newDelim;

                logIsOK = true;
            } catch (IOException e)
            {
                logIsOK = false;
            }
        }

        private void SetFileName()
        {
            DateTime timeNow = DateTime.Now;

            fileName = "DataLog_";
            fileName += timeNow.ToString("yyyy-MM-dd_hh-mm-ss");
            fileName += ".csv";
        }

        private void SetPath()
        {
            filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            filePath = Path.Combine(filePath, "LogData");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
        }

        public void BufferEntry(string newEntry)
        {
            entries.Add(newEntry);
        }

        public bool WriteEntry()
        {
            bool success = false;

            if (logIsOK)
            {
                try
                {
                    StreamWriter fStream;

                    fStream = File.AppendText(Path.Combine(filePath, fileName));
                    fStream.Write(DateTime.Now.ToLongTimeString());

                    foreach (string s in entries)
                    {
                        fStream.Write(delim);
                        fStream.Write(s);
                    }

                    fStream.WriteLine();
                    fStream.Flush();
                    fStream.Close();

                    success = true;
                }
                catch (Exception e)
                {
                    success = false;
                    Console.WriteLine(e.Message);
                }
            }

            entries.Clear();

            return success;
        }
    }
}
