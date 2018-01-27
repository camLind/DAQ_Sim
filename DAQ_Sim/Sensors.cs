using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace DAQ_Sim
{
    public class DAQSimulator
    {
        public List<AnalogueSensor> analogueSensors { get; private set; }
        private int numAnalogueDevices;

        //public Sample[] lastSample;
        
        // Default constructor for DAQ Simulator
        public DAQSimulator(int numADevs)
        {
            AnalogueSensor tempSensor;
            numAnalogueDevices = numADevs;

            double anSensMin = 0;
            double anSensMax = 10;
            int anSensBits = 10;

            analogueSensors = new List<AnalogueSensor>();
            
            for( int i=0; i<numAnalogueDevices; i++ )
            {
                tempSensor = new AnalogueSensor(i, anSensMin, anSensMax, anSensBits);
                analogueSensors.Add(tempSensor);
            }
        }

        // SampleAnalogueSensors: Simulate sampling operation of all sensors
        public void DoSampleAnalogueSensors()
        {
            foreach(AnalogueSensor sensor in analogueSensors)
            {
                sensor.DoSampling();
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // Sensor Class
    // By: Cameron Lindberg
    // Version: 1.1
    // Last Update: 2018-02-25
    //
    // Provide framework for functionality of sensors to be simulated
    //
    // Initially based on the example given in the assignment description
    // provided by Nils-Olav: C# coding Assignment Version 1.1 (B): Jan 8, 2018
    //
    public class Sensor : INotifyPropertyChanged
    {
        // TODO move min, max numbits to analogue class
        // sensor class parameters
        private int sId;
        private double dVal;
        protected Random rSenVal;

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
        public double value
        {
            get
            {
                return this.dVal;
            }
            protected set
            {
                if (value != this.dVal)
                {
                    this.dVal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        // Return string representation of the sensor value
        public string valStr
        {
            get
            {
                return this.GetValueString();
            }
        }

        // Constructor
        // Default id of 0
        public Sensor()
        {
            CreateSensor(0);
        }
        public Sensor(int id)
        {
            CreateSensor(id);
        }

        // Methods

        private void CreateSensor(int id)
        {
            sId = id;
            this.rSenVal = new Random(sId); //TODO: check for range setting of random class
            this.dVal = 0.0F;
        }

        // DoSampling()
        // Return the current value of the sensor
        // Header identified. Implementation to be performed in child classes
        public virtual void DoSampling() { }

        // GetValueString
        // Return the value of the sensor as a string (default)
        protected virtual String GetValueString() { return dVal.ToString(); }

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
        private int numBits;
        // TODO:MAFilter movAvgFilter;

        // Constructor allowing for customized initialization
        // of parameters for the sensor
        // Default parameters for initialization
        // Range=0-10V; Resolution=8 bits
        public AnalogueSensor(int id=0, double min=0, double max=10, int bits=8) :
            base(id)
        {
            minVal = min;
            maxVal = max;
            numBits = bits;

            //movAvgFilter = new MAFilter();
        }

        // GetValue()
        // Simulate a new sensor value and return this
        public override void DoSampling()
        {
            double newVal = rSenVal.NextDouble() * (maxVal - minVal) + minVal;
            // TODO try to use numBits to define resolution (rounding then should allow for definition of boolean logic)
            // TODO: modify resolution (modulo operator?)

            // TODO: filter values

            this.value = newVal;
        }

        // GetValueString()
        // Return the value as a string
        protected override String GetValueString()
        {
            return this.value.ToString("F3");     // TODO: what does F3 do?
        }
    }
}
