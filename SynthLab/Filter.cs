using MathNet.Numerics.IntegralTransforms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public class Filter
    {
        public double Q;
        public double FrequencyCenter;
        public double KeyboardFollow;
        public double Gain;
        public double Mix;
        public int FilterFunction = 0;
        public int ModulationWheelTarget = 0;
        //public int Id;
        //public int polyId;

        [JsonIgnore]
        public MainPage mainPage;
        [JsonIgnore]
        public Complex[] fftData;

        [JsonIgnore]
        public Oscillator oscillator;

        //public void Init(int poly, int oscillatorId)
        //{
        //    this.Id = oscillatorId;
        //    this.polyId = poly;
        //}

        public Filter(MainPage mainPage, Oscillator oscillator)
        {
            this.mainPage = mainPage;
            this.oscillator = oscillator;
        }

        public void PostCreationInit()
        {
            fftData = new Complex[mainPage.SampleCount];
            for (int i = 0; i < mainPage.SampleCount; i++)
            {
                fftData[i] = new Complex(0, 0);
            }
        }

        public double[] Apply(double[] waveData, int Key)
        {
            double[] result = new double[waveData.Length];

            if (fftData != null && fftData.Length > 0)
            {
                for (int i = 0; i < waveData.Length; i++)
                {
                    fftData[i] = waveData[i];
                }

                try
                {
                    Fourier.Forward(fftData, FourierOptions.AsymmetricScaling);
                }
                catch (Exception exception)
                {
                    //ContentDialog error = new Error("Unexpected error using FFT: " + exception.Message);
                    //_ = error.ShowAsync();
                }

                // Then filter the data:
                double q;
                double fc = 440;
                double y;

                //switch (Filter.FilterFunction)
                //{
                //    case (int)FilterFunction.FIXED:
                //        //q = Math.Pow((double)Filter.Q * (double)Filter.Q / fftData.Length, 2f) / 100f;
                //        q = Math.Pow((double)Filter.Q * (double)Filter.Q / 440, 2f) / 100f;
                //        fc = key * (Filter.KeyboardFollow / 128) + (Filter.CenterFrequency - 64);
                //        break;
                //    case (int)FilterFunction.ADSR_POSITIVE:
                //        q = Math.Pow((double)Filter.Q * (double)Filter.Q / fftData.Length, 2f) / 100f;
                //        fc = (int)(Adsr.AdsrLevel * Filter.Gain);
                //        break;
                //    case (int)FilterFunction.ADSR_NEGATIVE:
                //        q = Math.Pow((double)Filter.Q * (double)Filter.Q / fftData.Length, 2f) / 100f;
                //        fc = 1 - (int)(Adsr.AdsrLevel) * Filter.Gain;
                //        break;
                //    case (int)FilterFunction.AM_MODULATOR:
                //        if (AM_Modulator != null)
                //        {
                //            q = Math.Pow((double)Filter.Q * (double)Filter.Q / fftData.Length, 2f) / 100f;
                //        }
                //        break;
                //    case (int)FilterFunction.FM_MODULATOR:
                //        if (FM_Modulator != null)
                //        {
                //            q = Math.Pow((double)Filter.Q * (double)Filter.Q / fftData.Length, 2f) / 100f;
                //        }
                //        break;
                //    case (int)FilterFunction.XM_MODULATOR:
                //        if (XM_Modulator != null)
                //        {
                //            q = Math.Pow((double)Filter.Q * (double)Filter.Q / fftData.Length, 2f) / 100f;
                //        }
                //        break;
                //}
                //fc = (uint)((uint)((FrequencyInUse + (WaveShape.CenterFrequency - 64) / 12.7f) * WaveShape.KeyboardFollow / (double)127f) / FrequencyInUse * mainPage.SampleCount);
                //fc = (uint)(mainPage.Patch.Oscillators[0][Id].FrequencyInUse / 440f * mainPage.SampleCount *
                //    ((Knob)mainPage.FilterGUIs[Id].SubControls.ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value);

                // MIDI key = 0 - 127 => fcKey = 8.176 - 12543.850 Hz
                // Fc knob = 0 - 127 => fc = 440 - fcKey
                // = 440 + fcKnob * (fcKey - 440)
                double kf = 440 + KeyboardFollow * (mainPage.NoteFrequency[Key] - 440);
                q = Q / 4.0;
                q = Math.Pow(q * q / fftData.Length, 2f) / 100.0;
                switch (FilterFunction)
                {
                    case 1:
                        fc = kf / (12543.850 - 8.176) + FrequencyCenter / 16.0;
                        break;
                    case 2:
                        fc = kf / (12543.850 - 8.176) + oscillator.Adsr.AdsrLevel * FrequencyCenter / 16.0;
                        break;
                    case 3:
                        fc = kf / (12543.850 - 8.176) + (1 - oscillator.Adsr.AdsrLevel) * FrequencyCenter / 16.0;
                        break;
                    case 4:
                        fc = kf / (12543.850 - 8.176) + oscillator.PitchEnvelope.Value * FrequencyCenter / 16.0;
                        break;
                    case 5:
                        if (oscillator.XM_Modulator != null)
                        {
                            fc = kf / (12543.850 - 8.176) + (1 + oscillator.XM_Modulator.MakeWave()) * FrequencyCenter / 16.0;
                        }
                        else
                        {
                            fc = kf / (12543.850 - 8.176) + FrequencyCenter / 16.0;
                        }
                        break;
                    //case 6:
                    //    if (mainPage.Patch.Oscillators[polyId][Id].XM_Modulator != null)
                    //    {
                    //        q = q * mainPage.Patch.Oscillators[polyId][Id].XM_Modulator.MakeWave() / 128;
                    //    }
                    //    break;
                }
                //if (mainPage.Patch.Oscillators[0][Id].Adsr)
                for (int i = 1; i < fftData.Length; i++)
                {
                    //if (mainPage.Patch.Oscillators[0][Id].AM_Modulator != null && 
                    //    ((Rotator)mainPage.FilterGUIs[Id].SubControls.ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value.FilterFunction == (int)FilterFunction.AM_MODULATOR)
                    //{
                    //    fc = (uint)((1 - mainPage.Patch.Oscillators[0][Id].AM_Modulator.WaveData[i]) * (mainPage.WaveShape[this.Id].Gain / 128f) * (fftData.Length / 4) * ((float)key / 128f));
                    //}
                    //else if (mainPage.Patch.Oscillators[0][Id].FM_Modulator != null && mainPage.WaveShape[this.Id].FilterFunction == (int)FilterFunction.FM_MODULATOR)
                    //{
                    //    fc = (uint)(1 - mainPage.Patch.Oscillators[0][Id].FM_Modulator.WaveData[i]) * mainPage.WaveShape[this.Id].Gain;
                    //}
                    //else if (mainPage.Patch.Oscillators[0][Id].XM_Modulator != null && mainPage.WaveShape[this.Id].FilterFunction == (int)FilterFunction.XM_MODULATOR)
                    //{
                    //    fc = (uint)(1 - mainPage.Patch.Oscillators[0][Id].XM_Modulator.WaveData[i]) * mainPage.WaveShape[this.Id].Gain;
                    //}
                    y = 1 - (q * (i - fc) * (i - fc));
                    y = y < 0 ? 0 : y;
                    //if (i > 0 && i < fftData.Length)
                    {
                        fftData[i] = new Complex(fftData[i].Real * y, fftData[i].Imaginary * y);
                        //fftData[mainPage.SampleCount / 2 - i - 1] = 
                        //    new Complex(fftData[mainPage.SampleCount / 2 - i - 1].Real * y, fftData[mainPage.SampleCount / 2 - i - 1].Imaginary * y);
                    }
                }

                //for (int i = (int)(mainPage.SampleCount / 4); i < mainPage.SampleCount / 2; i++)
                //{
                //    fftData[i] = new Complex(0, 0);
                //}

                // Return to time domain:
                try
                {
                    Fourier.Inverse(fftData, FourierOptions.AsymmetricScaling);
                }
                catch (Exception exception)
                {
                    //ContentDialog error = new Error("Unexpected error using FFT: " + exception.Message);
                    //_ = error.ShowAsync();
                }

                for (int i = 0; i < fftData.Length; i++)
                {
                    result[i] = fftData[i].Real * (1 + Gain / 8.0) + waveData[i] * Mix / 127.0;
                }
            }
            return result;
        }
    }
}
