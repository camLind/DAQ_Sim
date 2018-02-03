#define displayWindow

using System.Linq;

namespace DAQ_Sim
{
    //////////////////////////////////////////////////////////////////////////
    // MAFilter Class
    // By: Cameron Lindberg
    // Version: 1.1
    // Last Update: 2018-02-03
    //
    // Version 1.1
    // - Add filter name for easier identification
    // - Add debug to console for functionality verification
    //
    // Version 1.0
    // - Write up basic functionality
    //
    // Implements a sliding window filter
    class MAFilter
    {
        private string name;
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

        // Overloaded constructor
        // Allowing the window length to be set
        public MAFilter(string name, int windowSize)
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

        // Test methods

        // Output window contents to the system console
        // This allows the internal functionality of the
        // filter to be verified
        // Output is a tab delimited line containing
        // - filter name
        // - contents
        // - filtered value
        private void WriteFilterToConsole()
        {
            System.Console.Write(name);
            for (int i = 0; i < length; i++)
                System.Console.Write("\t" + valueWindow[i].ToString());
            System.Console.WriteLine("\t" + output.ToString());
        }
    }
}
