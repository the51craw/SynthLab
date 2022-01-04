using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public partial class Oscillator
    {
        /// <summary>
        /// Calls the proper wave generation algorithm, depending on oscillator's waveform
        /// to generate one sample of wave data.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns></returns>
        public double MakeWave()
        {
            switch (WaveForm)
            {
                case WAVEFORM.SQUARE:
                    return MakeSquareWave();
                case WAVEFORM.SAW_UP:
                    return MakeSawUpWave();
                case WAVEFORM.SAW_DOWN:
                    return MakeSawDownWave();
                case WAVEFORM.TRIANGLE:
                    return MakeTriangleWave();
                case WAVEFORM.SINE:
                    return MakeSineWave();
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Algorithm for generating a squarewave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public float MakeSquareWave()
        {
            if (Angle > Phase)
            {
                return -1.0f;
            }
            else
            {
                return 1.0f;
            }
        }

        /// <summary>
        /// Algorithm for generating a sawtooth up wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeSawUpWave()
        {
            double temp = Angle + Phase;
            if (temp > Math.PI * 2)
            {
                temp -= Math.PI * 2;
            }

            return (float)((temp / Math.PI) - 1.0);
        }

        /// <summary>
        /// Algorithm for generating a sawtooth down wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeSawDownWave()
        {
            double temp = Angle + Phase;
            if (temp > Math.PI)
            {
                temp -= Math.PI;
            }

            return (float)(1.0 - (temp / Math.PI));
        }

        /// <summary>
        /// Algorithm for generating a triangle wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public double MakeTriangleWave()
        {
            //                              In order to sync to the other waveforms, triangle
            //   /\            /\           needs to start at zero. Drawing up during first 
            //  /  \          /  \          half period, and down during second half will off-
            // /    \        /    \         set 1/2 PI. Instead, we use the schema below:
            //------------------------- ------------------------------------------------------
            //        \    /        \     / 0 - PI/2 -> from zero to Amplitude
            //         \  /          \   /  PI/2 - PI -> from Amplitude to zero
            //          \/            \ /   PI - 3PI/2 -> from zero to -Amplitude
            //                              3PI/2 - 2PI -> from -Amplitude to zero

            double temp = Angle + Phase;
            if (temp > Math.PI * 2)
            {
                temp -= Math.PI * 2;
            }

            if (temp < Math.PI / 2)
            {
                return (float)(temp / Math.PI / 2);
            }
            else if (temp < Math.PI)
            {
                return (float)(1 - (4 * (temp - Math.PI / 2) / Math.PI / 2));
            }
            else if (temp < 3 * Math.PI / 2)
            {
                return (float)(0 - (4 * (temp - Math.PI) / Math.PI / 2));
            }
            else
            {
                return (float)((4 * (temp - 3 * Math.PI / 2) / Math.PI / 2) - 1);
            }
        }

        /// <summary>
        /// Algorithm for generating a sine wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        public float MakeSineWave()
        {
            return (float)Math.Sin(Angle + Phase);
        }

        /// <summary>
        /// Moves the angle in Radians one StepSize forward. Backs up 2 * PI when
        /// Radians exeeds 2 * PI. 
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>true if Radians backed up, else false (used when generating random waveform to detect when to generate a new sample</returns>
        public Boolean AdvanceAngle()
        {
            Angle += StepSize;
            if (Angle < Math.PI * 2)
            {
                return false;
            }
            Angle -= Math.PI * 2;
            return true;
        }
    }
}
