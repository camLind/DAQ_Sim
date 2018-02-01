# define ConsoleOutput

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;
using System.Threading;

namespace DAQ_Sim
{
    // Class ActionWaiter
    // Used as an interval timer to raise an event after a preset period
    // of time has elapsed. The timer is restarted using the Go() command
    public class ActionWaiter
    {
        private int waitTime;
        private Thread myThread;
        private AutoResetEvent waitToStart;
        private bool runMe;

        public event EventHandler Ready;

        // Constructor
        public ActionWaiter(TimeSpan newWaitTime)
        {
            runMe = true;

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
                while (runMe)
                {
#if ConsoleOutput
                    Console.WriteLine("Timer started: " + DateTime.Now.ToLongTimeString());
#endif
                    // wait for elapsed time
                    Thread.Sleep(waitTime);
#if ConsoleOutput
                    Console.WriteLine("Timer ticked: " + DateTime.Now.ToLongTimeString());
#endif
                    OnReadySet(new EventArgs());

                    waitToStart.WaitOne();
                }
            } catch (ThreadInterruptedException e)
            {
                Console.WriteLine("Sample timer interrupted.");
            }
        }

        // Initiate the next interval countdown
        public void Go()
        {
            waitToStart.Set();
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
    }

    // Class DAQSimulator
    public class DAQSimulator
    {
        public List<AnalogueSensor> analogueSensors { get; private set; }
        public List<DigitalSensor> digitalSensors { get; private set; }

        private int numAnalogueDevices;
        private int numDigitalDevices;
        
        // Default constructor for DAQ Simulator
        public DAQSimulator(int numADevs, int numDDevs)
        {
            numAnalogueDevices = numADevs;

            double anSensMin = -1;
            double anSensMax = 1;
            int anSensBits = 10;

            analogueSensors = new List<AnalogueSensor>();
            
            for( int i=0; i<numAnalogueDevices; i++ )
                analogueSensors.Add(new AnalogueSensor(i, anSensMin, anSensMax, anSensBits));

            numDigitalDevices = numDDevs;
            digitalSensors = new List<DigitalSensor>();
            for ( int i=0; i<numDigitalDevices; i++)
                digitalSensors.Add(new DigitalSensor(i + 20));
        }

        // SampleSensors: Simulate sampling operation of all sensors
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
    // Version: 2.0
    // Last Update: 2018-02-25
    //
    // Version 2.0:
    // - Implement bit resolution and integer data storage
    //   instead of using floating point numbers
    // - It is intended that this will work with booleans as well (bitness = 0)
    //
    // Version 1.1:
    // - Provide basic framework for functionality of sensors to be simulated
    //
    // Initially based on the example given in the assignment description
    // provided by Nils-Olav: C# coding Assignment Version 1.1 (B): Jan 8, 2018
    //
    public class Sensor : INotifyPropertyChanged
    {
        // sensor class parameters
        private int sId;
        private Random rSenVal;

        protected int intVal;
        protected int numBits;
        protected int maxIntVal;

        public event PropertyChangedEventHandler PropertyChanged;

        // Properties (read-only)
        // Return id of the sensor
        public int id
        {
            get
            {
                return this.sId;
            }
        }

        // Sensor value
        // Provides value read
        // Setting the internal value via this property initiates
        // the event that the sensor has changed.
        public int value
        {
            get
            {
                return intVal;
            }
            protected set
            {
                if (value != intVal)
                {
                    intVal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int SensValue
        {
            get
            {
                return intVal;
            }
        }

        // Return string representation of the sensor value
        public string valStr
        {
            get
            {
                return GetValueString();
            }
        }

        // Constructor
        // Default id of 0
        public Sensor()
        {
            CreateSensor(0,8);
        }
        public Sensor(int id, int bitness)
        {
            CreateSensor(id, bitness);
        }

        // Methods

        private void CreateSensor(int id, int bitness)
        {
            sId = id;
            rSenVal = new Random(sId); //TODO: check for range setting of random class

            if (bitness > 32)
                numBits = 32;
            else
                numBits = bitness;

            // Math.Pow() function only handles doubles
            // Iterate to achieve powers of 2
            maxIntVal = 1;
            for( int n = 1; n <= numBits; n++)
                maxIntVal = maxIntVal*2;

            intVal = 0;
        }

        // DoSampling()
        // Return the current value of the sensor
        // Header identified. Implementation to be performed in child classes
        public void DoSampling() {
            value = rSenVal.Next(maxIntVal);    // sets random values from 0 to (maxIntVal-1)
        }

        // GetValueString
        // Return the value of the sensor as a string (default)
        protected virtual String GetValueString() { return SensValue.ToString(); }

        // Raise property changed event. Used to provide notification of new value acquisition
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

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

        /*
        // DoSampling()
        // Simulate a new sensor value and return this
        public override void DoSampling()
        {
            double newVal = rSenVal.NextDouble() * (maxVal - minVal) + minVal;
            // TODO try to use numBits to define resolution (rounding then should allow for definition of boolean logic)
            // TODO: modify resolution (modulo operator?)

            this.value = newVal;
        }
        */

        // SensValue property
        // Using the built in min and max properties
        // return the scaled floating point representation
        // of the integer value in the parent sonsor class
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
        protected override String GetValueString()
        {
            return SensValue.ToString("F3");     // TODO: what does F3 do?
        }
    }

    public class DigitalSensor : Sensor
    {
        private const string tString = "ON";
        private const string fString = "OFF";

        public DigitalSensor(int id=0) : base(id,1)
        {
            value = 0;
        }

        /*
        // DoSampling()
        // Simulate a new sensor value and return this
        public override void DoSampling()
        {
            double newVal = rSenVal.NextDouble() < 0.5F ? 0.0F:1.0F;
            this.value = newVal;
        }
        */

        // SensValue property
        public new bool SensValue
        {
            get
            {
                return intVal != 0;
            }
        }


        // GetValueString()
        // Return the value as a string
        protected override String GetValueString()
        {
            return SensValue ? tString:fString;
        }
    }
}
