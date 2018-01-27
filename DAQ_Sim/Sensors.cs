using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace DAQ_Sim
{
    public class Sample : INotifyPropertyChanged
    {
        private int internID;
        private double internVal;
        private string internStr;

        public int id
        {
            get
            {
                return this.internID;
            }
        }
        public double val
        {
            get
            {
                return this.internID;
            }
            set
            {
                if( value != this.internVal )
                {
                    this.internVal = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string valStr
        {
            get
            {
                return this.internStr;
            }
            set
            {
                if (value != this.internStr)
                {
                    this.internStr = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Sample()
        {
            internID = 0;
            internVal = 0;
            internStr = "none";
        }
        public Sample(Sensor s)
        {
            internID = s.GetSensId();
            internVal = s.GetValue(false);
            internStr = s.GetValueString();
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if( PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class DAQSimulator
    {
        private List<AnalogueSensor> analogueSensors;
        private double[] analogueSamples;
        private int numAnalogueDevices;

        public Sample[] lastSample;
        
        // Default constructor for DAQ Simulator
        public DAQSimulator(int numADevs)
        {
            AnalogueSensor tempSensor;
            numAnalogueDevices = numADevs;

            double anSensMin = 0;
            double anSensMax = 10;
            int anSensBits = 10;

            analogueSensors = new List<AnalogueSensor>();
            analogueSamples = new double[numADevs];

            lastSample = new Sample[numADevs];

            for( int i=0; i<numAnalogueDevices; i++ )
            {
                tempSensor = new AnalogueSensor(i, anSensMin, anSensMax, anSensBits);

                analogueSensors.Add(tempSensor);

                lastSample[i] = new Sample(tempSensor);
            }
        }

        // SampleAnalogueSensors: Simulate sampling operation of all sensors
        public void DoSampleAnalogueSensors()
        {
            int i = 0;
            foreach(AnalogueSensor s in analogueSensors)
            {
                analogueSamples[i] = s.GetValue(true);

                lastSample[i].val = s.GetValue(false);
                lastSample[i].valStr = s.GetValueString();

                i++;
            }
        }

        // GetLastSamples: Return the values of the last analogue samples
        public double[] GetLastSamples() { return analogueSamples; }

        public string[][] AnalogueSamplesStrings()
        {
            string[][] retData = new string[numAnalogueDevices][];

            for (int i = 0; i < numAnalogueDevices; i++)
            {
                retData[i] = analogueSensors[i].GetIDValuePair();
            }

            return retData;
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
    public class Sensor
    {
        // TODO move min, max numbits to analogue class
        // sensor class parameters
        private int sId;
        protected double dVal;
        protected Random rSenVal;

        // Default constructor
        // Provide default parameters for initialization
        // ID=0; Range=0-10V; Resolution=8 bits
        //
        public Sensor()
        {
            sId = 0;
            InitializeSensor();
        }

        // Constructor allowing for customized initialization
        // of parameters for the sensor
        public Sensor(int id)
        {
            sId = id;
            InitializeSensor();
        }

        // Methods

        // InitializeSensor()
        // Initialize sensor parameters that are general to
        // the class and called by each constructor
        private void InitializeSensor()
        {
            this.rSenVal = new Random(sId); //TODO: check for range setting of random class
            this.dVal = 0.0F;
        }

        // GetSensId()
        // Return the ID of the sensor
        public int GetSensId() { return sId; }

        // IsOff()
        // Return if the sensor is "OFF". This means that
        // the simulated value is equal to 0V
        public bool IsOff() { return dVal == 0.0F; }

        // GetValue()
        // Return the current value of the sensor
        public virtual double GetValue(bool newSample=false)
        {
            return dVal;
        }

        // GetValueString
        // Return the value of the sensor as a string (default)
        public virtual String GetValueString() { return dVal.ToString(); }

        // Get id string pair
        public string[] GetIDValuePair()
        {
            string[] pair = new string[2];

            pair[0] = sId.ToString();
            pair[1] = this.GetValueString();

            return pair;
        }

        public Sample GetLastSample()
        {
            Sample retVal = new Sample(this);

            return retVal;
        }
    }

    public class AnalogueSensor : Sensor
    {
        private double minVal;
        private double maxVal;
        private int numBits;
        // TODO:MAFilter movAvgFilter;

        // Default constructor
        // Provide default parameters for initialization
        // Range=0-10V; Resolution=8 bits
        public AnalogueSensor() : base()
        {
            minVal = 0;
            maxVal = 10;
            numBits = 8;

            //movAvgFilter = new MAFilter();
        }

        // Constructor allowing for customized initialization
        // of parameters for the sensor
        public AnalogueSensor(int id, double min, double max, int bits) :
            base(id)
        {
            minVal = min;
            maxVal = max;
            numBits = bits;
        }

        // GetValue()
        // Simulate a new sensor value and return this
        public override double GetValue(bool newSample = false)
        {
            if( newSample )
            {
                dVal = rSenVal.NextDouble() * (maxVal - minVal) + minVal;
                // TODO try to use numBits to define resolution (rounding then should allow for definition of boolean logic)
                // TODO: modify resolution (modulo operator?)

                // TODO: filter values
            }

            return dVal;
        }

        // GetValueString()
        // Return the value as a string
        public override String GetValueString()
        {
            return dVal.ToString("F3");     // TODO: what does F3 do?
        }
    }
}
