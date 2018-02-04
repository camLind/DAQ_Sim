# define DebugActionWaiter      // Enable tmer state changes to console
# define DebugAppConfig         // Enable app.config updates to console

using System;
using System.Configuration;
using System.Threading;

namespace CamHelperFunctions
{
    //////////////////////////////////////////////////////////////////////////
    // Config
    // By: Cameron Lindberg
    // Version: 1.0
    // Last Update: 2018-02-04
    //
    // Version 1.0
    // - achieve functionality
    //
    // Static class used to simplify extraction of config file keys
    // The methods provide conversion to the expected data types
    // 
    // With purely static declarations, no constructor or internal
    // variables are stored. Not sure if this is "good practice" but
    // provides a suitable interface to the app.config file.
    static class Config
    {
        public static int IntKey(string key, int defaultVal)
        {
            int retVal;

            try
            {
                retVal = int.Parse(ConfigurationManager.AppSettings.Get(key));
#if DebugAppConfig
                Console.WriteLine("App config key: " + key + " = " + retVal.ToString());
#endif
            }
            catch (Exception e)
            {
#if DebugAppConfig
                Console.WriteLine(e.ToString());
#endif
                retVal = defaultVal;
            }

            return retVal;
        }

        public static double DblKey(string key, double defaultVal)
        {
            double retVal;

            try
            {
                retVal = double.Parse(ConfigurationManager.AppSettings.Get(key));
#if DebugAppConfig
                Console.WriteLine("App config key: " + key + " = " + retVal.ToString());
#endif
            }
            catch (Exception e)
            {
#if DebugAppConfig   // For debugging
                Console.WriteLine(e.ToString());
#endif
                retVal = defaultVal;
            }

            return retVal;
        }

        public static char Charkey(string key, char defaultVal)
        {
            char retVal;

            try
            {
                retVal = char.Parse(ConfigurationManager.AppSettings.Get(key));
#if DebugAppConfig
                Console.WriteLine("App config key: " + key + " = " + retVal.ToString());
#endif
            }
            catch (Exception e)
            {
#if DebugAppConfig   // For debugging
                Console.WriteLine(e.ToString());
#endif
                retVal = defaultVal;
            }

            return retVal;
        }

        public static TimeSpan TimeSecondsKey(string key, double defaultVal)
        {
            double tempDbl;
            TimeSpan retVal;

            // Read the key from the config file
            try
            {
                tempDbl = Config.DblKey(key, defaultVal);
                retVal = TimeSpan.FromSeconds(tempDbl);
#if DebugAppConfig
                Console.WriteLine("App config key: " + key + " = " + retVal.ToString());
#endif
            }
            catch (Exception e)
            {
#if DebugAppConfig   // For debugging
                Console.WriteLine(e.ToString());
#endif
                tempDbl = defaultVal;
            }

            // Convert the value from the config file/default param to a timespan
            try
            {
                retVal = TimeSpan.FromSeconds(tempDbl);
#if DebugAppConfig
                Console.WriteLine("App config key: " + key + " = " + retVal.ToString());
#endif
            }
            catch (Exception e)
            {
#if DebugAppConfig   // For debugging
                Console.WriteLine(e.ToString());
#endif
                retVal = new TimeSpan(0, 0, 1);
            }

            return retVal;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // ActionWaiter
    // By: Cameron Lindberg
    // Version: 1.1
    // Last Update: 2018-02-02
    //
    // Version 1.1
    // - added check that timer was active before restart
    // - add thread name
    // - additional code description and tidy-up
    //
    // Version 1.0
    // - achieve functionality
    //
    // Used as an interval timer to raise an event after a preset period
    // of time has elapsed. The timer is restarted using the Go() command
    //
    // The timer is executed in a separate thread and the delay is based
    // on using the sleep() command.
    public class ActionWaiter
    {
        private string name;    // internal identification
        private bool runMe;     // flag to allow timer thread to be closed

        private int waitTime;   // time delay for the timer
        private bool running;   // flag recording if the timer is currently running
        private Thread myThread;    // Thread object for the timer to run
        private AutoResetEvent waitToStart; // Synchronisation mechanism to enable restarting of timer
        private EventArgs e;

        public event EventHandler Ready;    // Event fired when timer has elapsed

        // Constructor
        // timerName: name for the timer identification
        // newWaitTime: time delay for the event to trigger after being started
        public ActionWaiter(string timerName, TimeSpan newWaitTime)
        {
            runMe = true;
            name = timerName;
            running = false;

            waitToStart = new AutoResetEvent(false);
            waitTime = (int)newWaitTime.TotalMilliseconds;
            myThread = new Thread(new ThreadStart(this.executeMe));
            myThread.Start();
        }

        // Timer implementation
        // This is done in a separate thread and uses the Sleep function.
        private void executeMe()
        {
            try
            {
                while (runMe)   // Continue in the thread while runMe is true
                {
                    waitToStart.WaitOne();          // wait for start signal

#if DebugActionWaiter   // For debugging - notify when the timer started
                    Console.WriteLine(name + ": Started: " + DateTime.Now.ToLongTimeString());
#endif
                    running = true;
                    Thread.Sleep(waitTime);     // wait for elapsed time
                    running = false;

#if DebugActionWaiter   // For debugging - notify when the timer stopped
                    Console.WriteLine(name + ": Ticked: " + DateTime.Now.ToLongTimeString());
#endif
                    OnReadySet(new EventArgs());    // fire timer ticked event
                }
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(name + ": Sample timer interrupted.");
            }
        }

        // Initiate the next interval countdown
        // Return true if the timer could be started
        public bool Go()
        {
            bool success = false;

            if (runMe && !running)
            {
                waitToStart.Set();
                success = true;
            }

            return success;
        }

        // Stop the timer, closing the associated thread
        public void Stop()
        {
            runMe = false;
            waitToStart.Set();
            if (myThread.ThreadState == ThreadState.WaitSleepJoin)
                myThread.Interrupt();
        }

        // Raise flag ready event.
        private void OnReadySet(EventArgs e)
        {
            if (Ready != null)
            {
                Ready(this, e);
            }
        }
    }   // END ActionWaiter class
}
