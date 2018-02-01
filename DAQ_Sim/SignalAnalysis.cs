using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ_Sim
{
    class MAFilter
    {
        private double[] valueWindow;
        private int activeIndex;
        private int length;

        // Public property to retrieve the filtered value
        // Hence this property is set as read-only
        public double output
        {
            get;
            private set;
        }

        // Default constructor
        // Default length = 10;
        public MAFilter()
        {
            length = 10;
            Initialize();
        }

        // Overloaded constructor
        // Allowing the window length to be set
        public MAFilter(int windowSize)
        {
            windowSize = windowSize <= 0 ? 10 : windowSize;
            length = windowSize;
            Initialize();
        }

        // Method: Initialize
        // setup array for data storage
        // initialize indexes
        private void Initialize()
        {
            activeIndex = -1;

            // Allocate memory
            valueWindow = new double[length];

            for (int i = 0; i < length; i++)
                valueWindow[i] = 0.0F;

            output = 0.0F;
        }

        // Method: AddValue
        // Add a value to the sliding window filter
        public void AddValue(double newValue)
        {
            if( activeIndex < 0 )
            {
                // Initialize the array with values
                // from the first call
                for (int i = 0; i < length; i++)
                    valueWindow[i] = newValue;

                // get ready for the next call
                activeIndex = 1;
            } else
            {
                // update the oldest value with the new value
                valueWindow[activeIndex] = newValue;

                IncrementActiveIndex();
            }

            output = valueWindow.Average();
        }

        // Method: IncrementActiveIndex
        // Increment the value of activeIndex so that the next array
        // element will be referenced. If going past the length of the
        // array, return to 0
        // Implemented in a separate function to aid readability of calling code.
        private void IncrementActiveIndex()
        {
            // Using the modulo operator to implement a revolving array index
            // Once activeIndex goes beyond the range of the array
            // the variable is reset to 0
            activeIndex = (activeIndex + 1) % length;
        }
    }
}
