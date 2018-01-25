using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ_Sim
{
    class DAQSimulator
    {
        private List<AnalogueSensor> analogueSensors;
        private double[] analogueSamples;
        private int numAnalogueDevices;
        
        public DAQSimulator(int numADevs)
        {
            AnalogueSensor tempSensor;
            numAnalogueDevices = numADevs;

            double anSensMin = 0;
            double anSensMax = 10;
            int anSensBits = 10;

            analogueSamples = new double[numADevs];

            for( int i=0; i<numAnalogueDevices; i++ )
            {
                tempSensor = new AnalogueSensor(i, anSensMin, anSensMax, anSensBits);
                analogueSensors.Add(tempSensor);
            }
        }

        public double[] SampleAnalogueSensors()
        {
            int i = 0;
            foreach(AnalogueSensor s in analogueSensors)
            {
                analogueSamples[i] = s.GetValue();
                i++;
            }

            return analogueSamples;
        }

        public double[] GetLast
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
    class Sensor
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

        // Initialize sensor parameters that are general to
        // the class and called by each constructor
        private void InitializeSensor()
        {
            this.rSenVal = new Random(sId); //TODO: check for range setting of random class
            this.dVal = 0.0F;
        }

        // Return the ID of the sensor
        public int GetSensId() { return sId; }

        // Return if the sensor is "OFF". This means that
        // the simulated value is equal to 0V
        public bool IsOff() { return dVal == 0.0F; }

        // Return the current value of the sensor
        public virtual double GetValue()
        {
            return dVal;
        }

        public virtual String GetValueString() { return dVal.ToString(); }
    }

    class AnalogueSensor : Sensor
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

        public AnalogueSensor(int id, double min, double max, int bits) :
            base(id)
        {
            minVal = min;
            maxVal = max;
            numBits = bits;
        }

        public override double GetValue()
        {
            dVal = rSenVal.NextDouble() * (maxVal - minVal) + minVal;
            // TODO try to use numBits to define resolution (rounding then should allow for definition of boolean logic)
            // TODO: modify resolution (modulo operator?)

            // TODO: filter values

            return dVal;
        }

        public override String GetValueString()
        {
            return dVal.ToString("F3");     // TODO: what does F3 do?
        }
    }
}
