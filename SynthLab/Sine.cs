using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthLab
{
    public class Sine
    {
        private float[] sineVal;
        private float twoPi;
        private int length;

        public Sine(int Length)
        {
            twoPi = (float)(2 * Math.PI);
            length = Length;
            sineVal = new float[Length];
            for (int i = 0; i < Length; i++)
            {
                sineVal[i] = (float)Math.Sin(i / (float)Length * 2 * (float)Math.PI);
            }
        }

        public float Sin(float Angle)
        {
            int angle = (int)(length * Angle / twoPi + 0.5f);
            while (angle >= length)
            {
                angle -= length;
            }
            while (angle < 0)
            {
                angle += length;
            }
            return sineVal[angle];
        }
    }
}
