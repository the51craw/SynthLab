using MathNet.Numerics.IntegralTransforms;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Foundation;

namespace SynthLab
{
    /// <summary>
    /// All oscillators on poly = 0 has an instance of WaveShape containing a class Filter
    /// to hold filter settings for the oscillator on all poly.
    /// It is the oscillator that implements the filter functionality,
    /// which is why no such functionality is implemented here.
    /// </summary>
    public enum FilterFunction
    {
        OFF,
        FIXED,
        ADSR_POSITIVE,
        ADSR_NEGATIVE,
        AM_MODULATOR,
        FM_MODULATOR,
        XM_MODULATOR
    }

    public class WaveShape
    {
        #region attributes

        //public Filter Filter;

        /// <summary>
        /// Enabled allows for disabling the filter without changing any
        /// settings, especially not setting FilterFunction to NONE.
        /// </summary>
        public Boolean Enabled;

        /// <summary>
        /// Filters internal float[SampleRate / 100] holding filtered data. This data is to be
        /// read using a value for Angle between 0 and 4 PI. It delivers wave data
        /// with a frequency that defined the step length. 
        /// </summary>

        [JsonIgnore]
        public double[] WaveData;
        [JsonIgnore]
        public double[] OriginalWaveData;
        [JsonIgnore]
        public double Angle;
        [JsonIgnore]
        public bool Buzy;

        #endregion attributes

        #region locals

        //private int filterFunction;
        //private byte qSetting;
        //private byte centerFrequency;
        //private byte keyboardFollow = 63;
        //private byte depth;
        //private byte compensation;
        //private int modulationSelector;
        [JsonIgnore]
        private Random random = new Random();
        private double timec = 0;
        private double timem1 = 0;
        private double timem2 = 0;
        private double timem3 = 0;
        //[JsonIgnore]
        //public Complex[] fftData;
        double q;
        double y;
        public WAVEFORM Waveform;
        public double Phase;
        private double angle;
        [JsonIgnore]
        public MainPage mainPage;
        [JsonIgnore]
        public Oscillator oscillator;
        //private int id;
        double stepSize;
        //private int polyId;

        #endregion locals

        #region contstruction

        public void Init()
        {
            Waveform = WAVEFORM.SQUARE;
            Phase = Math.PI;
            Angle = 0;
            Buzy = false;
        }

        public void PostCreationInit(MainPage mainPage, Oscillator oscillator)
        {
            this.oscillator = oscillator;
            this.mainPage = mainPage;
            WaveData = new double[mainPage.SampleCount];
            OriginalWaveData = new double[mainPage.SampleCount];
        }

        #endregion construction

        #region wavecreation

        /// <summary>
        /// Makes a standard waveform.
        /// </summary>
        /// <param name="waveForm"></param>
        public void MakeWave()
        {
            if (WaveData != null && WaveData.Length > 0)
            {
                stepSize = Math.PI * 2 / mainPage.SampleCount;
                angle = 0;
                Buzy = true;

                for (int i = 0; i < mainPage.SampleCount; i++)
                {
                    WaveData[i] = OriginalWaveData[i] = MakeWaveSample(Waveform);
                    AdvanceAngles();
                }

                Buzy = false;
            }
        }

        #endregion wavecreation

        #region filtering

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Filtering
        ///////////////////////////////////////////////////////////////////////////////////////////////////////

        public void ApplyFilter(int key)
        {
            for (int i = 0; i < mainPage.SampleCount; i++)
            {
                WaveData[i] = OriginalWaveData[i];
            }
            WaveData = oscillator.Filter.Apply(WaveData, key);
            mainPage.allowGuiUpdates = true;
        }

        #endregion filtering

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Modulating
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region modulating

        /// <summary>
        /// Frequency modulation is mainly intended for FM synthesis like the Yamaha DX
        /// synthesizer series creates overtones. That syntesis uses only sine waves and
        /// both waves are following the keyboard. However, since we have LFO variant and
        /// five more waveforms there are a few variants supplied for making other effects
        /// possible.
        /// </summary>
        /// <param name="oscillator" the current oscillator or a modulator that was attached to a modulator></param>
        /// <returns>CurrentSample, modified or not modified by AM, FM, and/or PM</returns>
        /// <affects>StepSize may be modified by FM when non-sine waveform is used as FM modulator</affects>
        private double DX_Modulate(Oscillator oscillator)
        {
            double sample = 0;

            if (oscillator.Keyboard && oscillator.WaveForm == WAVEFORM.SINE
                && oscillator.FM_Modulator != null
                && oscillator.FM_Modulator.Keyboard && oscillator.FM_Modulator.WaveForm == WAVEFORM.SINE
                && oscillator.FM_Modulator.FM_Modulator != null
                && oscillator.FM_Modulator.FM_Modulator.Keyboard && oscillator.FM_Modulator.FM_Modulator.WaveForm == WAVEFORM.SINE
                && oscillator.FM_Modulator.FM_Modulator.FM_Modulator != null
                && oscillator.FM_Modulator.FM_Modulator.FM_Modulator.Keyboard && oscillator.FM_Modulator.FM_Modulator.FM_Modulator.WaveForm == WAVEFORM.SINE)
            {
                // The basic FM equation:
                // y(t) = Amplitude * sin(2π * fc * t + I1 * sin(2π * fm1 * t + I2 * sin(2π * fm2 * t + I3 * sin(2π * fm2 * t)))),
                // where the parameters are defined as follows:
                // fc = carrier frequency(Hz)
                // fm = modulation frequency(Hz)
                // I = modulation index

                sample += (float)Math.Sin(Math.PI * 2 * (1 + oscillator.PitchEnvelope.Value)
                        * oscillator.FrequencyInUse * (float)(timec / (float)oscillator.mainPage.SampleRate)
                    + oscillator.FM_Sensitivity / 63f * Math.Sin(Math.PI * 2 * (1 + oscillator.FM_Modulator.PitchEnvelope.Value)
                        * oscillator.FM_Modulator.FrequencyInUse * (float)(timec / (float)oscillator.mainPage.SampleRate)
                    + oscillator.FM_Modulator.FM_Sensitivity / 63f * Math.Sin(Math.PI * 2 * (1 + oscillator.FM_Modulator.FM_Modulator.PitchEnvelope.Value)
                        * oscillator.FM_Modulator.FM_Modulator.FrequencyInUse * (float)(timec / (float)oscillator.mainPage.SampleRate)
                    + oscillator.FM_Modulator.FM_Modulator.FM_Sensitivity / 63f * Math.Sin(Math.PI * 2 * (1 + oscillator.FM_Modulator.FM_Modulator.FM_Modulator.PitchEnvelope.Value)
                        * oscillator.FM_Modulator.FM_Modulator.FM_Modulator.FrequencyInUse * (float)(timec / (float)oscillator.mainPage.SampleRate)))));
                sample = sample * 2 - 1;
            }
            else if (oscillator.Keyboard && oscillator.WaveForm == WAVEFORM.SINE
                && oscillator.FM_Modulator != null
                && oscillator.FM_Modulator.Keyboard && oscillator.FM_Modulator.WaveForm == WAVEFORM.SINE
                && oscillator.FM_Modulator.FM_Modulator != null
                && oscillator.FM_Modulator.FM_Modulator.Keyboard && oscillator.FM_Modulator.FM_Modulator.WaveForm == WAVEFORM.SINE)
            {
                // The basic FM equation:
                // y(t) = Amplitude * sin(2π * fc * t + I1 * sin(2π * fm1 * t + I2 * sin(2π * fm2 * t))),
                // where the parameters are defined as follows:
                // fc = carrier frequency(Hz)
                // fm = modulation frequency(Hz)
                // I = modulation index

                sample += (float)Math.Sin(Math.PI * 2 * (1 + oscillator.PitchEnvelope.Value)
                        * oscillator.FrequencyInUse * (float)(timec / (float)oscillator.mainPage.SampleRate)
                    + oscillator.FM_Sensitivity / 63f * Math.Sin(Math.PI * 2 * (1 + oscillator.FM_Modulator.PitchEnvelope.Value)
                        * oscillator.FM_Modulator.FrequencyInUse * (float)(timec / (float)oscillator.mainPage.SampleRate)
                    + oscillator.FM_Modulator.FM_Sensitivity / 63f * Math.Sin(Math.PI * 2 * (1 + oscillator.FM_Modulator.FM_Modulator.PitchEnvelope.Value)
                        * oscillator.FM_Modulator.FM_Modulator.FrequencyInUse * (float)(timec / (float)oscillator.mainPage.SampleRate))));
                sample = sample * 2 - 1;
            }
            else if (oscillator.Keyboard && oscillator.WaveForm == WAVEFORM.SINE
                && oscillator.FM_Modulator != null
                && oscillator.FM_Modulator.Keyboard && oscillator.FM_Modulator.WaveForm == WAVEFORM.SINE)
            {
                // The basic FM equation:
                // y(t) = Amplitude * sin(2π * fc * t + I * sin(2π * fm * t)),
                // where the parameters are defined as follows:
                // fc = carrier frequency(Hz)
                // fm = modulation frequency(Hz)
                // I = modulation index

                double anglec = Math.PI * 2 * (1 + oscillator.PitchEnvelope.Value) / oscillator.mainPage.SampleCount * timec;
                double anglem1 = Math.PI * 2 * (1 + oscillator.PitchEnvelope.Value) / oscillator.mainPage.SampleCount * timem1;
                double I1 = oscillator.FM_Sensitivity / 63f;
                sample = (float)Math.Sin(anglec + I1 * Math.Sin(anglem1));
            }

            timec += 440 / oscillator.Frequency;
            timem1 += 440 / oscillator.FM_Modulator.Frequency;
            return sample;
        }

        #endregion modulating

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Basic wave generating functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region basicWaveGenerating

        private void AdvanceAngles()
        {
            angle += stepSize;

            if (angle > Math.PI * 2)
            {
                angle -= Math.PI * 2;
            }
        }

        /// <summary>
        /// Calls the proper wave generation algorithm, depending on oscillator's waveform
        /// to generate one sample of wave data.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns></returns>
        public double MakeWaveSample(WAVEFORM WaveForm)
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
                case WAVEFORM.RANDOM:
                    return MakeRandomWave();
                case WAVEFORM.NOISE:
                    return MakeNoiseWave();
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Algorithm for generating a squarewave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private float MakeSquareWave()
        {
            if (angle > Phase)
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
        private double MakeSawUpWave()
        {
            double value = angle / Math.PI - 1.0f;
            return value;
        }

        /// <summary>
        /// Algorithm for generating a sawtooth down wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeSawDownWave()
        {
            double value = 1.0f - angle / Math.PI;
            return value;
        }

        /// <summary>
        /// Algorithm for generating a triangle wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeTriangleWave()
        {
            //                              In order to sync to the other waveforms, triangle
            //   /\            /\           needs to start at zero. Drawing up during first 
            //  /  \          /  \          half period, and down during second half will off-
            // /    \        /    \         set 1/2 PI. Instead, we use the schema below:
            //------------------------- ------------------------------------------------------
            //        \    /        \     / 0 - PI/2 -> from 0 to 1
            //         \  /          \   /  PI/2 - PI -> from 1 to 0
            //          \/            \ /   PI - 3PI/2 -> from 0 to -1
            //                              3PI/2 - 2PI -> from -1 to 0

            double sample = 0;

            if (angle < Math.PI / 2)
            {
                sample = 2 * angle / Math.PI;
            }
            else if (angle < Math.PI)
            {
                sample = 1 - 2 * (angle - Math.PI / 2) / Math.PI;
            }
            else if (angle < 3 * Math.PI / 2)
            {
                sample = 0f - 2 * (angle - Math.PI) / Math.PI;
            }
            else
            {
                sample = 2 * (angle - 3 * Math.PI / 2) / Math.PI - 1;
            }

            return sample;
        }

        /// <summary>
        /// Algorithm for generating a sine wave sample depending on the angle in Radians.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private double MakeSineWave()
        {
            double sample = 0;
            if (oscillator.ModulationType == ModulationType.NORMAL)
            {
                sample = Math.Sin(angle);
            }
            if (oscillator.ModulationType == ModulationType.PM)
            {
                if (oscillator.WaveForm == WAVEFORM.SQUARE && oscillator.XM_Modulator != null)
                {
                    oscillator.Phase = Math.PI * (1 - oscillator.XM_Sensitivity / 256 * (1 + (float)oscillator.XM_Modulator.WaveData
                        [(int)(oscillator.XM_Modulator.Angle * oscillator.mainPage.SampleCount / Math.PI * 2) % oscillator.mainPage.SampleCount]
                        ) / 2);
                    sample = MakeWaveSample(0);
                }
                else
                {
                    sample = WaveData[(int)(angle * oscillator.mainPage.SampleCount / Math.PI * 2) % oscillator.mainPage.SampleCount];
                }
                sample = 0;
            }
            return sample ;
        }

        /// <summary>
        /// Algorithm for generating a random wave sample generated once per cycle.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        float randomValue = 0.0f;
        private float MakeRandomWave()
        {
            if (oscillator.Keyboard)
            {
                if (AdvanceAngle())
                {
                    randomValue = (random.Next(1000) - 500.0f) / 500f;
                }
            }
            return randomValue;
        }

        /// <summary>
        /// Algorithm for generating a random wave sample for each step.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>One sample of wave data</returns>
        private float MakeNoiseWave()
        {
            return 0.002f * (random.Next(1000) - 500);
        }

        /// <summary>
        /// Moves the angle in Radians one StepSize forward. Backs up 2 * PI when
        /// Radians exeeds 2 * PI. 
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>true if Radians backed up, else false (used when generating random waveform to detect when to generate a new sample</returns>
        private Boolean AdvanceAngle()
        {
            angle += Math.PI * 2 / (float)oscillator.mainPage.SampleCount;
            if (angle < Math.PI * 2)
            {
                return false;
            }
            angle -= Math.PI * 2;
            return true;
        }

        #endregion basicWaveGenerating
    }

    public partial class Oscillator
    {

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Oscillograph methods
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region oscillograph

        /// <summary>
        /// Uses the wave-generating functions to produce two cycles of wave data
        /// as an array of 200 integers to use by the oscilloscope as a 200 pixel wide graph.
        /// </summary>
        /// <param name="oscillator" the selected oscillator for which to show waveform></param>
        /// <returns></returns>
        public Point[] MakeGraphData(double height)
        {
            // Must hold oscillator while borrowing it for generating oscillograph data
            //while (mainPage.hold)
            //{
            //    Task.Delay(1);
            //}
            double OriginalPhase = Phase;
            double OriginalAngle = Angle;
            double OriginalStepSize = StepSize;
            BackupOriginalModulators(this);
            ResetModulators(this, FrequencyInUse);

            double[] localBuffer = new double[200];
            StepSize = Math.PI / 50;
            Angle = 0;
            Phase = 0;
            CheckModulationType();
            int graphRandomValue = random.Next(20) - 10;

            for (int i = 0; i < 200; i++)
            {
                MarkModulators(this);
                if (ModulationType == ModulationType.DX)
                {
                    localBuffer[i] = MakeDxWave(this);
                    Modulate();
                    AdvanceAngle(this);
                    AdvanceModulatorsAngles(this);
                }
                else if (WaveForm == WAVEFORM.SINE)
                {
                    localBuffer[i] = MakeWave();
                    Modulate();
                    AdvanceAngle(this);
                    AdvanceModulatorsAngles(this);
                }
                else if (WaveForm == WAVEFORM.RANDOM)
                {
                    if (i % 40 == 0)
                    {
                        graphRandomValue = random.Next(20) - 10;
                    }
                    localBuffer[i] = graphRandomValue;
                }
                else
                {
                    localBuffer[i] = mainPage.Patch.Oscillators[PolyId][Id].WaveShape.WaveData[(i * 4) % mainPage.SampleCount];
                }
                //Modulate();
                //AdvanceAngle(this);
                //AdvanceModulatorsAngles(this);
            }

            Phase = OriginalPhase;
            StepSize = OriginalStepSize;
            Angle = OriginalAngle;
            return NormalizeAmplitude(localBuffer, height);
        }

        /// <summary>
        /// Used by MakeGraphData to translate the float values generated 
        /// by the wave-generating functions into integers with an amplitude of 80 pixels.
        /// </summary>
        /// <param name="inBuffer"></param>
        /// <returns></returns>
        public Point[] NormalizeAmplitude(double[] inBuffer, double height)
        {
            double factor = 0;
            Point[] outBuffer = new Point[inBuffer.Length];
            double max = 0;
            double min = 0;
            double offset = 0;

            // Measure largest amplitude:
            for (int i = 0; i < inBuffer.Length; i++)
            {
                if (inBuffer[i] > 0 && max < inBuffer[i])
                {
                    max = inBuffer[i];
                }
                else if (inBuffer[i] < 0 && min > inBuffer[i])
                {
                    min = inBuffer[i];
                }
            }
            factor = max - min;
            offset = (max + min) / 2;

            // Convert to an amplitude of 80 peak-to-peak and invert signal:
            for (int i = 0; i < inBuffer.Length; i++)
            {
                if (factor > 0)
                {
                    // Graph hight = 92 px. Center is 66 so span is from 20 - 112
                    outBuffer[i] = new Point(i + 9, 1.5 * (44 + (offset - inBuffer[i]) * height / factor));
                }
                else
                {
                    outBuffer[i] = new Point(i + 9, 66);
                }
            }

            RestoreOriginalModulators(this);
            return outBuffer;
        }

        private void BackupOriginalModulators(Oscillator oscillator)
        {
            if (oscillator.XM_Modulator != null)
            {
                BackupOriginalModulators(oscillator.XM_Modulator);
                OriginalModulatorsPhase = oscillator.XM_Modulator.Phase;
                OriginalModulatorsAngle = oscillator.XM_Modulator.Angle;
                OriginalModulatorsStepSize = oscillator.XM_Modulator.StepSize;
            }
        }

        private void RestoreOriginalModulators(Oscillator oscillator)
        {
            if (oscillator.XM_Modulator != null)
            {
                RestoreOriginalModulators(oscillator.XM_Modulator);
                oscillator.XM_Modulator.Phase = OriginalModulatorsPhase;
                oscillator.XM_Modulator.Angle = OriginalModulatorsAngle;
                oscillator.XM_Modulator.StepSize = OriginalModulatorsStepSize;
            }
        }

        private void ResetModulators(Oscillator oscillator, double frequency)
        {
            if (oscillator.XM_Modulator != null)
            {
                ResetModulators(oscillator.XM_Modulator, frequency);
                oscillator.XM_Modulator.Phase = 0;
                oscillator.XM_Modulator.Angle = 0;
                oscillator.XM_Modulator.StepSize = oscillator.XM_Modulator.FrequencyInUse / frequency * Math.PI / 50;
            }
        }

        #endregion oscillograph
    }
}
