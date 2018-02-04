using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DataLogging;
using CamHelperFunctions;

namespace DAQ_Sim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        // Timer objects
        DispatcherTimer timeUpdater;
        ActionWaiter samplingTimer;
        ActionWaiter loggingTimer;

        // DAQ simulator objects
        DAQSimulator daqSim;
        MAFilter[] aiFilters;

        // Datalogging
        DataLog logToFile;

        // Main Window function
        public MainWindow()
        {
            InitializeComponent();

            // Initialize timers
            timeUpdater = new DispatcherTimer();
            timeUpdater.Interval = Config.TimeSecondsKey("sysTimeUpdateInterval", 0.5F);
            timeUpdater.Tick += new EventHandler(TimeUpdater_Elapsed);
            timeUpdater.Start();

            samplingTimer = new ActionWaiter("SampleTimer", Config.TimeSecondsKey("sampleTime", 1.0F));
            samplingTimer.Ready += new EventHandler(SamplingWaiter_Elapsed);

            loggingTimer = new ActionWaiter("LoggingTimer", Config.TimeSecondsKey("loggingTime", 1.0F));
            loggingTimer.Ready += new EventHandler(LoggingTimer_Elapsed);

            // Initialize DAQ simulator and filtering objects
            daqSim = new DAQSimulator();
            aiFilters = new MAFilter[daqSim.AIDevCount];

            for( int i=0; i < aiFilters.Length; i++ )
            {
                string name = "aiFilter_" + i.ToString("G2");
                aiFilters[i] = new MAFilter(name);
            }
            
            dgAnalogueSamples.ItemsSource = daqSim.ai;
            dgDigitalSamples.ItemsSource = daqSim.di;

            // Setup data logging
            LogInitialize();

            // Window default state
            btnSample.IsEnabled = true;
            btnLog.IsEnabled = true;
        }

        //////////////////////////////////////////////////////
        // Timer event handling

        private void TimeUpdater_Elapsed(object sender, EventArgs e)
        {
            tbTimeNow.Text = DateTime.Now.ToString("HH:mm:ss.f");
        }

        private void SamplingWaiter_Elapsed(object sender, EventArgs e)
        {
            EnableControl(btnSample);
        }

        private void LoggingTimer_Elapsed(object sender, EventArgs e)
        {
            EnableControl(btnLog);
        }

        public delegate void DelegateEnableControl(Control ctl);
        private void EnableControl(Control ctl)
        {
            if( Dispatcher.CheckAccess() )
            {
                ctl.IsEnabled = true;
            } else
            {
                Dispatcher.Invoke(new DelegateEnableControl(EnableControl),ctl);
            }           
        }

        //////////////////////////////////////////////////////
        // Simulator value sampling

        private void PerformSample()
        {
            DateTime timeNow = DateTime.Now;

            tbLastSampleTime.Text = timeNow.ToString("HH:mm:ss.f");
            tbNextSampleTime.Text = timeNow.Add(samplingTimer.Interval).ToString("HH:mm:ss.f");

            btnSample.IsEnabled = false;
            daqSim.DoSampleSensors();

            for (int i = 0; i < aiFilters.Length; i++)
                aiFilters[i].AddValue(daqSim.ai[i].SensValue);

            samplingTimer.Go();
        }

        //////////////////////////////////////////////////////
        // Datalog functions
        private void LogInitialize()
        {
            logToFile = new DataLog(Config.Charkey("dataLogDelim", ','));
            tbLogPath.Text = logToFile.FilePath;

            tbLogEntryCount.Text = logToFile.NumEntries.ToString();

            logToFile.BufferEntry("Timestamp");

            foreach (Sensor s in daqSim.ai)
                logToFile.BufferEntry(s.name);

            foreach (Sensor s in daqSim.di)
                logToFile.BufferEntry(s.name);

            logToFile.WriteEntry(tStamp: false, incrCtr: false);
        }

        private void PerformLogWrite()
        {
            DateTime timeNow = DateTime.Now;

            tbLastLogTime.Text = timeNow.ToString("HH:mm:ss.f");
            tbNextLogTime.Text = timeNow.Add(loggingTimer.Interval).ToString("HH:mm:ss.f");

            btnLog.IsEnabled = false;

            for (int i = 0; i < aiFilters.Length; i++)
                logToFile.BufferEntry(aiFilters[i].output.ToString("F3"));

            foreach (Sensor s in daqSim.di)
                logToFile.BufferEntry(s.valStr);

            if( logToFile.WriteEntry() )
            {
                tbLogEntryCount.Text = logToFile.NumEntries.ToString();
                loggingTimer.Go();
            } else
            {
                tbLogEntryCount.Text = logToFile.NumEntries.ToString() + " !--ERR--!";
            }
        }

        //////////////////////////////////////////////////////
        // User-initiated event handling
        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            PerformSample();
        }

        private void btnLog_Click(object sender, RoutedEventArgs e)
        {
            PerformLogWrite();
        }

        private void menuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            string caption = "About DAQ Sim";

            string msgText = "DAQ Simulator Program";
            msgText += "\n\n By: Cameron Lindberg";
            msgText += "\n Version: 1.0";
            msgText += "\n\n";
            msgText += "Developed for completion of 'Assignment #1: C# coding Assignment'";
            msgText += " for the course IIA1314 Object-Oriented Analysis, Design and Programming";
            msgText += " at the University College of Southeast Norway. 2018.";

            MessageBox.Show(msgText, caption, MessageBoxButton.OK);
        }

        private void menuFileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            samplingTimer.Stop();
            loggingTimer.Stop();
        }
    }
}
