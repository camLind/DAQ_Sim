# define ConsoleOutput      // Flag for enabling debug output to the system console

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace DAQ_Sim
{
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
#if ConsoleOutput   // For debugging - notify when the timer started
                    Console.WriteLine(name + ": Timer started: " + DateTime.Now.ToLongTimeString());
#endif
                    running = true;
                    Thread.Sleep(waitTime);     // wait for elapsed time
                    running = false;

#if ConsoleOutput   // For debugging - notify when the timer stopped
                    Console.WriteLine(name + ": Timer ticked: " + DateTime.Now.ToLongTimeString());
#endif
                    OnReadySet(new EventArgs());    // fire timer ticked event

                    waitToStart.WaitOne();          // wait for restart signal
                }
            } catch (ThreadInterruptedException e)
            {
                Console.WriteLine(name + ": Sample timer interrupted.");
            }
        }

        // Initiate the next interval countdown
        // Return true if the timer could be started
        public bool Go()
        {
            bool success = false;

            if ( runMe && !running)
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
            if( myThread.ThreadState == ThreadState.WaitSleepJoin)
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

    //////////////////////////////////////////////////////////////////////////
    // DAQ Simulator
    // By: Cameron Lindberg
    // Version: 1.1
    // Last Update: 2018-02-02
    //
    // Version 1.1
    // - Tidy up and added more descriptive comments
    //
    // Version 1.0
    // - Achieve functionality / sensor implementation
    //
    // Enable simulation of multiple sensor instances
    // this provides simulation of a DAQ module with different functionality
    public class DAQSimulator
    {
        // Collection of analogue sensor devices
        // Publicly visible but cannot be changed outside the class
        public List<AnalogueSensor> analogueSensors { get; private set; }
        // Collection of digital sensor devices
        // Publicly visible but cannot be changed outside the class
        public List<DigitalSensor> digitalSensors { get; private set; }

        private int numAnalogueDevices;     // total number of analogue sensors
        private int numDigitalDevices;      // total number of digital sensors

        // configuration for the analogue sensors
        private const int anSensIDStart = 0;
        private const double anSensMin = -1;
        private const double anSensMax = 1; 
        private const int anSensBits = 10;

        // configuration for the digital sensors
        private const int diSensIDStart = 20;

        // Constructor for DAQ Simulator
        public DAQSimulator(int numADevs, int numDDevs)
        {
            // Initialize the analogue sensors
            numAnalogueDevices = numADevs;
            analogueSensors = new List<AnalogueSensor>();          
            for( int i=0; i<numAnalogueDevices; i++ )
                analogueSensors.Add(
                    new AnalogueSensor(anSensIDStart + i, anSensMin, anSensMax, anSensBits));

            // Initialise the digital sensors
            numDigitalDevices = numDDevs;
            digitalSensors = new List<DigitalSensor>();
            for ( int i=0; i<numDigitalDevices; i++)
                digitalSensors.Add(
                    new DigitalSensor(diSensIDStart + i));
        }

        // DoSampleSensors: Simulate sampling operation of all sensors
        public void DoSampleSensors()
        {
            foreach(AnalogueSensor sensor in analogueSensors)
                sensor.DoSampling();

            foreach(DigitalSensor sensor in digitalSensors)
                sensor.DoSampling();
        }

    }

    //////////////////////////////////////////////////////////////////////////
    // Sensor Class
    // By: Cameron Lindberg
    // Version: 2.1
    // Last Update: 2018-02-02
    //
    // Version 2.1
    // - Tidy-up
    // - Change sId to the property id
    //
    // Version 2.0:
    // - Implement bit resolution and integer data storage
    //   instead of using floating point numbers
    // - This works with booleans as well (bitness = 1)
    //
    // Version 1.1:
    // - Provide basic framework for functionality of sensors to be simulated
    // - Based on the example given in the assignment description
    //   provided by Nils-Olav: C# coding Assignment Version 1.1 (B): Jan 8, 2018
    //
    // Base clase for simulation of sensors
    // Provides random value generation for sensor value
    // Fires an event when the value has been updated
    //
    public class Sensor : INotifyPropertyChanged
    {
        // sensor class parameters
        private Random rSenVal;

        protected int intVal;
        protected int numBits;
        protected int maxIntVal;

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties (read-only)
        // Return id of the sensor
        public int id { get; private set; }

        // Sensor value
        // Provides value in the raw integer form
        // Setting the internal value via this property initiates
        // the event that the sensor has changed.
        public int value
        {
            get { return intVal; }
            protected set
            {
                if (value != intVal)
                {
                    intVal = value; // assign the value
                    NotifyPropertyChanged();    // notify event changed
                }
            }
        }

        // SensValue provides the option to return the value
        // of the sensor in a local data type
        // Returning of the value as different types is achieved
        // using method hiding. This is done with the new keyword
        // in the property definition in the sub-class
        public int SensValue {
            get { return intVal; }
        }

        // Property to acces the string representation of the sensor value
        public string valStr {
            get { return GetValueString(); }
        }

        // Constructor
        // Define the id and bit resolution
        public Sensor(int newId, int bitness)
        {
            id = newId;
            rSenVal = new Random(id); //TODO: check for range setting of random class

            if (bitness > 32)
                numBits = 32;
            else
                numBits = bitness;

            // Math.Pow() function only handles doubles
            // Iterate to achieve powers of 2
            maxIntVal = 1;
            for (int n = 1; n <= numBits; n++)
                maxIntVal = maxIntVal * 2;

            intVal = 0;
        }

        // Methods
        
        // DoSampling()
        // Update the current value of the sensor
        // Since this is a simulator, use random value
        public void DoSampling() {
            value = rSenVal.Next(maxIntVal);    // sets random values from 0 to (maxIntVal-1)
        }

        // GetValueString
        // Return the value of the sensor as a string (default)
        // This method can be overriden by child classes
        protected virtual string GetValueString() {
            return SensValue.ToString();
        }

        // Raise property changed event. Used to provide notification of new value acquisition
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // AnalogueSensor Class
    // By: Cameron Lindberg
    // Version: 2.1
    // Last Update: 2018-02-02
    //
    // Version 2.1
    // - Tidy-up
    //
    // Version 2.0:
    // - Update in accordance with v2.0 of SensorClass
    // - This works with booleans as well (bitness = 1)
    //
    // Version 1.0:
    // - Provide basic functionality of analogue sensors to be simulated
    //
    // Provides simulation of analogue sensor
    // Min & Max values are assigned with the constructor
    // The simulated value is then within these bounds
    public class AnalogueSensor : Sensor
    {
        private double minVal;
        private double maxVal;

        // Constructor allowing for customized initialization
        // of parameters for the sensor
        // Default parameters for initialization
        // Range=0-10V; Resolution=8 bits
        public AnalogueSensor(int id=0, double min=0, double max=10, int bits=8) :
            base(id, bits)
        {
            minVal = min;
            maxVal = max;
        }

        // SensValue property
        // Using the built in min and max properties
        // return the scaled floating point representation
        // of the integer value in the parent sonsor class
        // Hides the SensValue property of the base clase
        public new double SensValue
        {
            get
            {
                double tempVal;

                // ratio of integer value along the possible range
                // note that the minimum value is 0
                tempVal = (double)intVal / (maxIntVal - 1);
                // apply the ratio to the range of the output
                tempVal = tempVal * (maxVal - minVal);
                // add the output minimum offset
                tempVal += minVal;

                return tempVal;
            }
        }

        // GetValueString()
        // Return the value as a string
        protected override string GetValueString()
        {
            return SensValue.ToString("F6");
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // DigitalSensor Class
    // By: Cameron Lindberg
    // Version: 2.1
    // Last Update: 2018-02-02
    //
    // Version 2.1
    // - Tidy-up
    //
    // Version 2.0:
    // - Update in accordance with v2.0 of SensorClass
    //
    // Version 1.0:
    // - Provide basic functionality of digital sensors to be simulated
    //
    // Provides simulation of digital sensor
    public class DigitalSensor : Sensor
    {
        private const string tString = "ON";
        private const string fString = "OFF";

        public DigitalSensor(int id=0) : base(id,1)
        {
            // nothing to do here
        }

        // SensValue property
        // Return the boolean representation
        // of the integer value in the parent sonsor class
        // Hides the SensValue property of the base clase
        public new bool SensValue
        {
            get { return intVal != 0; }
        }

        // GetValueString()
        // Return the value as a string
        protected override string GetValueString()
        {
            return SensValue ? tString:fString;
        }
    }
}
