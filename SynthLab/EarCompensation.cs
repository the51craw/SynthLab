using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthLab
{
    /// <summary>
    /// Creates values to compensates for the unlinear ear response
    /// which is most notabliced with soft waves like sine or heavy
    /// filtered wave forms.
    /// Uses lines to approximate the ear response.
    /// Gain = k * frequency + base
    /// </summary>

    public class EarCompensation
    {
        MainPage mainPage;
        public EarCompensation(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }

        public double KeyToGain(int Key)
        {
            return FrequencyToGain(mainPage.NoteFrequency[Key]);
        }

        public double FrequencyToGain(double Frequency)
        {
            double gain = 1.0;

            if (Frequency < 20)
            {
                // We can not hear better, so keep the 100.
            }
            else if (Frequency < 200)
            {
                // F = 20 => gain = 1
                // F = 200 => gain = 1 / 128 
                // 180 => gain = 1 / 128
                gain -= (Frequency - 20) / 180 / 128;
            }
            else if (Frequency < 2000)
            {
                // F = 200 => gain = 100 / 128
                // F = 2000 => gain = 100 / 100 
                // 180 => gain = 100 / 128
                gain -= (Frequency - 20) / 1800;
            }
            //else if (Frequency < 4000)
            //{
            //    gain += 3.0 * (4000 - Frequency);
            //}
            //else if (Frequency < 8000)
            //{
            //    gain -= 3.0 * (8000 - Frequency);
            //}
            //else
            //{
            //    gain += 2.0 * (1000 - Frequency);
            //}
            else gain = 1;

            return gain * 3;
        }
    }
}
