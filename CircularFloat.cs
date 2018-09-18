using System;

namespace DRWatchdogV2
{
    /// <summary>
    /// Limited purpose float with limited values that are written over. Useful for finding average.
    /// </summary>
    class CircularFloat
    {
        private int index = 0;
        public bool dataLoaded = false;
        public int length;
        private float[] buffer;

        public CircularFloat(int l)
        {
            length = l;
            buffer = new float[l];
        }
        
        public void Add(float val)
        {
            buffer[index] = val; 
            index++;
            if (index == length) { index = 0; dataLoaded = true; }
        }

        public float ReadAverage()
        {
            if (!dataLoaded) throw new Exception("data in circular float not loaded");

            float av = 0;
            foreach( float f in buffer)
            {
                av += f;
            }
            av = av / length;
            return av;
            
        }

        public float ReadLast()
        {
            if(index==0)
            {
                return buffer[length-1];
            }
            return buffer[index - 1];
        }
    }
}
