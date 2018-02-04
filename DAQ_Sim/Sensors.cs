using System;
using System.Collections.Generic;
using System.ComponentModel;
using CamHelperFunctions;

namespace DAQ_Sim
{
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
        public List<AnalogueSensor> ai { get; private set; }
        // Collection of digital sensor devices
        // Publicly visible but cannot be changed outside the class
        public List<DigitalSensor> di { get; private set; }

        private int numAnalogueDevices;     // total number of analogue sensors
        private int numDigitalDevices;      // total number of digital sensors

        // configuration for the analogue sensors
        private const int anSensIDStart = 0;
        private double aiSensMin = -1;
        private double aiSensMax = 1; 
        private int aiSensBits = 10;

        // configuration for the digital sensors
        private const int diSensIDStart = 20;

        public int AIDevCount { get { return numAnalogueDevices; } }
        public int DIDevCount { get { return numDigitalDevices; } }

        // Default constructor for DAQ simulator
        // If available, use app.config settings for initialization
        public DAQSimulator()
        {
            numAnalogueDevices = Config.IntKey("numAIDevs", 1);
            numDigitalDevices = Config.IntKey("numDIDevs", 1);

            aiSensMin = Config.DblKey("minAIVolt", 0.0F);
            aiSensMax = Config.DblKey("maxAIVolt", 10.0F);
            aiSensBits = Config.IntKey("numBitsAI", 8);

            CreateSensors();
        }

        // Constructor for DAQ Simulator
        public DAQSimulator(int numADevs, int numDDevs)
        {
            numAnalogueDevices = numADevs;
            numDigitalDevices = numDDevs;

            CreateSensors();
        }

        // Initialize the sensors for simulation
        private void CreateSensors()
        {
            // Initialize the analogue sensors
            ai = new List<AnalogueSensor>();
            for (int i = 0; i < numAnalogueDevices; i++)
                ai.Add(
                    new AnalogueSensor(anSensIDStart + i, aiSensMin, aiSensMax, aiSensBits));

            // Initialise the digital sensors
            di = new List<DigitalSensor>();
            for (int i = 0; i < numDigitalDevices; i++)
                di.Add(
                    new DigitalSensor(diSensIDStart + i));
        }

        // DoSampleSensors: Simulate sampling operation of all sensors
        public void DoSampleSensors()
        {
            foreach(AnalogueSensor sensor in ai)
                sensor.DoSampling();

            foreach(DigitalSensor sensor in di)
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

        //protected string name;
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
        public int rawValue
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

        public string name
        {
            get;
            protected set; 
        }

        // Default Constructor
        public Sensor()
        {
            id = -1;
            numBits = 1;
            name = "sensor";
            InitializeSensor();
        }

        // Constructor
        // Define the id and bit resolution
        public Sensor(int newId, int bitness, string namePrefix="_")
        {
            id = newId;
            numBits = bitness;
            name = namePrefix + id.ToString("G2");
            InitializeSensor();
        }

        // Methods
        
        // General initialization
        // Allows initialization from overloaded constructors
        // without duplicating code
        public void InitializeSensor()
        {
            rSenVal = new Random(id); //TODO: check for range setting of random class

            if (numBits > 32)
                numBits = 32;

            // Math.Pow() function only handles doubles
            // Iterate to achieve powers of 2
            maxIntVal = 1;  // Start at 2^0
            for (int n = 1; n <= numBits; n++)
                maxIntVal = maxIntVal * 2;

            intVal = 0;
        }

        // DoSampling()
        // Update the current value of the sensor
        // Since this is a simulator, use random value
        public void DoSampling() {
            rawValue = rSenVal.Next(maxIntVal);    // sets random values from 0 to (maxIntVal-1)
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
            base(id, bits, "ai_")
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

        public DigitalSensor(int id=0) : base(id,1,"di_")
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
