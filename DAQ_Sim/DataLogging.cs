#define DebugLogActions

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataLogging
{
    public class DataLog
    {
        // Attributes
        private string dir;
        private string fileName;
        private bool logIsOK;
        private char delim;

        List<string> entries;

        // Properties
        public int NumEntries{ get; private set; }

        public string FilePath { get { return Path.Combine(dir, fileName);  } }

        // Constructor
        public DataLog(char newDelim = ',')
        {
            SetFileName();
            SetPath();

            entries = new List<string>();
            NumEntries = 0;

            try
            {
                StreamWriter fStream;

                fStream = File.CreateText(FilePath);
                fStream.Close();

                delim = newDelim;

                logIsOK = true;
#if DebugLogActions
                Console.WriteLine("Logfile created: " + FilePath);
#endif
            } catch (IOException e)
            {
                logIsOK = false;
            }
        }

        // Methods
        private void SetFileName()
        {
            DateTime timeNow = DateTime.Now;

            fileName = "DataLog_";
            fileName += timeNow.ToString("yyyy-MM-dd_HH-mm-ss");
            fileName += ".csv";
        }

        private void SetPath()
        {
            dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dir = Path.Combine(dir, "LogData");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public void BufferEntry(string newEntry)
        {
            entries.Add(newEntry);
        }

        public bool WriteEntry(bool tStamp=true, bool incrCtr=true)
        {
            bool success = false;

            StringBuilder logLine;

            if (logIsOK)
            {
                
                logLine = new StringBuilder();

                if( tStamp )
                    logLine.Append(DateTime.Now.ToLongTimeString());

                foreach (string s in entries)
                {
                    if(logLine.Length>0)
                        logLine.Append(delim);

                    logLine.Append(s);
                }

                success = WriteToFile(logLine.ToString());

                if( success && incrCtr )
                    NumEntries++;               
            }

            entries.Clear();

            return success;
        }

        private bool WriteToFile(string textToWrite)
        {

            StreamWriter fStream;
            bool success = false;

            try
            {
                fStream = File.AppendText(Path.Combine(dir, fileName));
                fStream.WriteLine(textToWrite);
                fStream.Flush();
                fStream.Close();
                success = true;
#if DebugLogActions
                Console.WriteLine("Log write: " + textToWrite);
#endif
            }
            catch (Exception e)
            {
                success = false;
#if DebugLogActions
                Console.WriteLine(e.Message);
#endif
            }

            return success;
        }
    }
}
