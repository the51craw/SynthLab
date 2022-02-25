//using AudioEffectComponent;
using MathNet.Numerics.IntegralTransforms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using System.ServiceModel;

namespace SynthLab
{
    public enum ModulationType
    {
        PM,
        FM,
        DX,
        NORMAL
    }

    public sealed partial class MainPage : Page
    {
        public OscillatorGUI[] oscillatorGUI;
    }

    /// <summary>
    /// The frame server is the one recieving requests from the AudioGraph system.
    /// The reason for this frame server is that the AudioGraph system calls oscillators
    /// that are supposed to produce output sound at different times. Since Addition
    /// synthesis is supposed to create a mix of sound from multiple oscillators, those
    /// oscillators need to run in sync.
    /// The frame server keeps a list of oscillators that are actually supposed
    /// to create output sound. It is the only object registered as an input node to be
    /// called from the AudioGraph system.
    /// When a request comes in it generates the sound by running the logic of the 
    /// oscillators that are set to create the sound, and multiplies all samples
    /// togehter to create the final sound, which is delivered to the AudioGraph system.
    /// It also has the reverb effects.
    /// Note that playing multiple keys constitutes playing different frequencies,
    /// and the buffer size is different fore each frequency. Ergo, we need one frame
    /// server for each played key. Since the polyphony is Patch.Polyphony, we need Patch.Polyphony frame servers.
    /// </summary>

    public class OscillatorGUI
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Control references:
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region controlReferences

        public Knob knobModulation;
        public Knob knobFrequency;
        public Knob knobFinetune;
        public Knob knobVolume;
        public AreaButton btnView;
        public Rotator SelectorWave;
        public Rotator SelectorKeyboard;
        public Rotator SelectorModulation;
        //public Rotator SelectorAdsrPulse;
        public Rotator SelectorView;
        public Indicator View;
        public Indicator Sounding;

        //public Echo Echo;
        //public Reverb Reverb;
        #endregion controlReferences
    }

    public partial class Oscillator
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Properties
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region properties

        /// <summary>
        /// One-dimensional oscillator id 0 - 11. Does not account for polyphony,
        /// which is a third dimension after row and column.
        /// </summary>
        public int  Id;

        /// <summary>
        /// The oscillator has a waveshape object that is used to pre-create
        /// the shape of a waveform thet the oscillator uses to crete a wave
        /// form of expected frequency. This saves some time since the wave
        /// shape does not always need to be re-created.
        /// </summary>
        public WaveShape WaveShape;

        /// <summary>
        /// The oscillator has its own filter, but it is sometimes used from
        /// the WaveShape object.
        /// </summary>
        public Filter Filter;

        /// <summary>
        /// The oscillator has its own ADSR and Pulse envelope generators.
        /// </summary>
        public ADSR Adsr;

        /// <summary>
        /// The oscillator has its own pitch envelope.
        /// </summary>
        public PitchEnvelope PitchEnvelope;

        /// <summary>
        /// Polyphonic Id, the first index of the oscillator.
        /// </summary>
        public int PolyId;

        /// <summary>
        /// Oscillators normally react on all MIDI channels, but can also be set
        /// to a specific channel. All = -1, channel = 0 - 15;
        /// </summary>
        public int MidiChannel;

        /// <summary>
        /// Oscillators normally does not have velocity sensitivity, but that can be activated.
        /// </summary>
        public Boolean VelocitySensitive;

        /// <summary>
        /// If VelocitySensitive is true, velocity affects the output volume of an oscillator
        /// if it is an oscillator that creates output sound, i.e. not a modulator.
        /// </summary>
        public byte Velocity;

        /// <summary>
        /// Frequency and FineTune uses SetFrequency() to combine into FrequenceInUse,
        /// the base frequency used either as a fixed LFO frequency or to be translated
        /// to keyboard frequency. Also sets StepSize.
        /// </summary>
        public double Frequency { get { return frequency; } set { frequency = value; SetFrequency(); } }
        public double FrequencyLfo { get { return frequencyLfo; } set { frequencyLfo = value; SetFrequency(); } }

        /// <summary>
        /// Frequency and FineTune uses SetFrequency() to combine into FrequenceInUse,
        /// the base frequency used either as a fixed LFO frequency or to be translated
        /// to keyboard frequency. Also sets StepSize.
        /// </summary>
        public double FineTune { get { return finetune; } set { finetune = value; SetFrequency(); } }
        public double FineTuneLfo { get { return finetuneLfo; } set { finetuneLfo = value; SetFrequency(); } }

        /// <summary>
        /// The combination of Frequency and FineTune frequencies to use as
        /// keyboard frequency after translation to KeyboardAdjustedFrequency.
        /// </summary>
        public double FinetunedFrequency { get { return finetunedFrequency; } set { finetunedFrequency = value; } }

        /// <summary>
        /// The combination of Frequency and FineTune frequencies to use as a fixed (or FM modulated) LFO frequency.
        /// </summary>
        public double LfoFrequency { get { return lfoFrequency; } set { lfoFrequency = value; SetFrequency(); } }

        /// <summary>
        /// The actual frequency used when a keyboard key is pressed.
        /// </summary>
        public double KeyboardAdjustedFrequency { get { return keyboardAdjustedFrequency; } set { keyboardAdjustedFrequency = value; } }

        /// <summary>
        /// The actual frequency in use, LFO or base frequency for translation when a keyboard key is pressed.
        /// </summary>
        public double FrequencyInUse;

        /// <summary>
        /// The keyboard key pressed. Calls SetKeyboardAdjustedFrequency(key) to translate
        /// FrequencyInUse into KeyboardAdjustedFrequency when a key is pressed.
        /// </summary>
        public byte Key { get { return key; } set { key = value; SetKeyboardAdjustedFrequency(key); } }

        /// <summary>
        /// The output volume of the oscillator. When set to a value greater than zero, the oscillator
        /// output is sent to the output mixer to directly participate in sound generating rather than
        /// to only modulate another oscillator (which it can do at the same time).
        /// </summary>
        public byte Volume;

        /// <summary>
        /// Returns true if volume is > 0 and MIDI channel is correct or -1, which means that oscillator listens to all channels.
        /// </summary>
        /// <returns></returns>
        public Boolean IsOutput()
        {
            return Volume > 0;
        }

        /// <summary>
        /// The waveform generated by the oscillator: SQARE, SAW_UP, SAW_DOWN, TRIANGLE, SINE, RANDOM or NOISE.
        /// </summary>
        public WAVEFORM WaveForm;

        /// <summary>
        /// Keyboard/LFO flag. Denotes whether the oscillator is used as an LFO 
        /// or a tone generator that generates frequencies depending on played key.
        /// True => using Keyboard, false => LFO.
        /// </summary>
        public Boolean Keyboard;

        /// <summary>
        /// UseAdsr: true => use ADSR, false => use pulse
        /// </summary>
        public Boolean UseAdsr;

        /// <summary>
        /// Modulation sensitivity is how much a modulation source modulates an oscillator.
        /// </summary>
        public float AM_Sensitivity;
        /// <summary>
        /// Modulation sensitivity is how much a modulation source modulates an oscillator.
        /// </summary>
        public float FM_Sensitivity;
        /// <summary>
        /// Modulation sensitivity is how much a modulation source modulates an oscillator.
        /// </summary>
        public float XM_Sensitivity;

        /// <summary>
        /// A modulator is an oscillator used to modulate another oscillator
        /// </summary>
        [JsonIgnore]
        public Oscillator AM_Modulator;
        public int AM_ModulatorId = -1;

        /// <summary>
        /// A modulator is an oscillator used to modulate another oscillator
        /// </summary>
        [JsonIgnore]
        public Oscillator FM_Modulator;
        public int FM_ModulatorId = -1;

        /// <summary>
        /// A modulator is an oscillator used to modulate another oscillator
        /// </summary>
        [JsonIgnore]
        public Oscillator XM_Modulator;
        public int XM_ModulatorId = -1;

        /// <summary>
        /// Phase is used to alter high/low time ratio for square waves
        /// and as a phase shift for other wave forms except random and noise.
        /// </summary>

        [JsonIgnore]
        public double Phase;

        /// <summary>
        /// The cyclic position, angle, within a waveform. Varies from zero to 2 PI.
        /// Used to calculate sample values for a waveform.
        /// </summary>

        [JsonIgnore]
        public double Angle;
        //public double WaveShapeAngle;

        /// <summary>
        /// StepSize is used when generating a waveform. Each sample is depending on
        /// how large step within a cycle a momentary sample value in order to create
        /// a certain frequency. Used in GenerateAudioData and MakeGraphData to generate
        /// the waveform. Also altered when FM modulating with non-sine waveforms.
        /// </summary>

        [JsonIgnore]
        public double StepSize;

        /// <summary>
        /// Wave data for one frame to deliver.
        /// </summary>

        [JsonIgnore]
        public double[] WaveData;

        public int ModulationKnobTarget;
        public int ModulationWheelTarget;
        public bool ViewMe;

        #endregion properties

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Property actions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region propertyActions

        /// <summary>
        /// Combines frequency and finetune into finetunedFrequency and lfoFrequency.
        /// Also sets FrequencyInUse to lfoFrequency or finetunedFrequency depending on the Keyboard switch setting.
        /// Also sets StepSize accordingly.
        /// </summary>
        public void SetFrequency()
        {
            // Keyboard frequency range: 10 - 10.000 Hz. Fixed frequency range: 0.1 - 100 Hz.
            // Knobs ranges from 0 to 1000, but for finetunedFrequency we want it to look like
            // coarse ranges from 0 to 10000 in steps of 100 and fine from 0 - 100 in steps of 0.01
            // and for lfoFrequency like coarse ranges from 0 - 100 in steps of 1 and fine in steps of 0.01.
            if (Keyboard)
            {
                finetunedFrequency = frequency + finetune / 100.0f;
                finetunedFrequency = finetunedFrequency > 10000 ? 10000 : finetunedFrequency;
                FrequencyInUse = finetunedFrequency;
            }
            else
            {
                lfoFrequency = frequencyLfo + finetuneLfo / 1000;
                lfoFrequency = lfoFrequency > 100 ? 100 : lfoFrequency;
                lfoFrequency = lfoFrequency < 0.01f ? 0.01f : lfoFrequency;
                FrequencyInUse = lfoFrequency;
            }
            if (mainPage != null && mainPage.FrameServer != null)
            {
                StepSize = FrequencyInUse * Math.PI * 2 / (float)mainPage.SampleRate;
            }
        }

        /// <summary>
        /// Translates from key number to frequency, or selects lfo frequency, depending 
        /// on Keyboard flag, and stores the result in FrequencyInUse, but only if key is not 0x80.
        /// </summary>
        /// <param name="key"></param>
        public void SetKeyboardAdjustedFrequency(byte key)
        {
            if (mainPage != null && key != 0xff)
            {
                keyboardAdjustedFrequency = finetunedFrequency * mainPage.NoteFrequency[key] / 440f;
                if (Keyboard)
                {
                    FrequencyInUse = keyboardAdjustedFrequency;
                }
                else
                {
                    FrequencyInUse = lfoFrequency;
                }
            }
        }
        #endregion construction

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Locals
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region locals

        //public uint SampleCount;
        private Random random = new Random();

        [JsonIgnore]
        public double CurrentSample;

        [JsonIgnore]
        public MainPage mainPage;

        private double frequency = 440;
        private double finetune = 0;
        private double frequencyLfo = 5;
        private double finetuneLfo = 0;
        private ulong time = 0;
        private byte key;
        private double keyboardAdjustedFrequency;
        private double finetunedFrequency;
        private double lfoFrequency;
        public Rotator selectorWave;
        public Rotator selectorKeyboard;
        public Rotator selectorModulation;
        public Rotator selectorAdsrPulse;
        private double am_PitchEnvlelopeSensitivity;
        private double fm_PitchEnvlelopeSensitivity;
        private double xm_PitchEnvlelopeSensitivity;

        [JsonIgnore]
        public ModulationType ModulationType;
        private bool justPassedTwoPi;
        public Complex[] fftData;
        private Boolean advance;
        private double RandomValue;
        private double lastEnd = 0;

        double OriginalModulatorsPhase;
        double OriginalModulatorsAngle;
        double OriginalModulatorsStepSize;

        //public double previousAdsrPulseLevel;

        /// <summary>
        /// Holds the sound generating. Use for short time only in order for oscillator to wait
        /// with generating data because something that could interfere is just going on.
        /// Hold for very short time, max a few milliseconds.
        /// </summary>
        #endregion locals

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region construction

        [JsonConstructor]
        public Oscillator()
        {
        }

        public Oscillator(MainPage mainPage)
        {
            this.mainPage = mainPage;
            WaveData = new double[mainPage.SampleCount];
        }

        public void Init(MainPage mainPage)
        {
            PitchEnvelope = new PitchEnvelope(mainPage, this);
            WaveData = new double[mainPage.SampleCount];
            Adsr = new ADSR(mainPage);
            Adsr.Init(this);
            Keyboard = true;
            frequency = 440;
            lfoFrequency = 5;
            FineTune = 0.0f;
            MidiChannel = 0;
            VelocitySensitive = false;
            Velocity = 0x40;
            Phase = Math.PI;
            AM_Sensitivity = 0;
            FM_Sensitivity = 0;
            XM_Sensitivity = 128;
            am_PitchEnvlelopeSensitivity = 0;
            fm_PitchEnvlelopeSensitivity = 0;
            xm_PitchEnvlelopeSensitivity = 0;
            UseAdsr = true;
            ModulationType = 0;
            //previousAdsrPulseLevel = 0;
            Filter = new Filter(mainPage, this);
            WaveShape = new WaveShape();
            WaveShape.Init();
            WaveShape.PostCreationInit(mainPage, this);
        }

        #endregion construction

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Re-initializations on KeyOn()
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region reInitializations
        /// <summary>
        /// Re-initiates the oscillator at key-on and calls InitModulators
        /// that in turn recursively initiates all connected modulators.
        /// </summary>
        /// <param name="key"></param>
        public void InitOscillator(byte key, byte velocity)
        {
            Key = key;
            Velocity = (byte)(VelocitySensitive ? velocity : 0x40);
            CurrentSample = 1;
            time = 0;
            InitModulators(this, key);
            CheckModulationType();
            WaveShape.MakeWave();
            justPassedTwoPi = false;
            if (WaveForm == WAVEFORM.SAW_DOWN || WaveForm == WAVEFORM.SAW_UP)
            {
                Angle = Math.PI;
            }
            else
            {
                Angle = 0;
            }
            //WaveShapeAngle = 0;
            RandomValue = (float)(random.Next(1000) - 500) / 500f;
            if (Filter.FilterFunction > 0)
            {
                WaveShape.ApplyFilter(key);
            }
        }

        public void CreateWaveData(uint requestedNumberOfSamples)
        {
            WaveData = new double[requestedNumberOfSamples];
        }

        /// <summary>
        /// Initializes all oscillators that are sourcing this one, using recursion.
        /// Frequency is set either to the source's Lfo frequency or to the same frequency 
        /// this oscillator has, depending on the source's keyboard/lfo switch.
        /// Angle, currentSample and Amplitude are set to 0, 0 and 1 respectively.
        /// Phase is calculated from the source's modulation sensitivity.
        /// </summary>
        /// <param name="oscillator" the oscillator whose sources are to be updated></param>
        public void InitModulators(Oscillator oscillator, byte key)
        {
            if (oscillator.AM_Modulator != null)
            {
                oscillator.AM_Modulator.Key = key;
                oscillator.AM_Modulator.SetKeyboardAdjustedFrequency(key);
                oscillator.AM_Modulator.SetStepSize();
                oscillator.AM_Modulator.CurrentSample = 1;
                oscillator.AM_Modulator.Angle = 0;
                //oscillator.AM_Modulator.WaveShapeAngle = 0;
                oscillator.AM_Modulator.SetPhase();
                oscillator.AM_Modulator.RandomValue = (random.Next(1000) - 500) / 500;
                InitModulators(oscillator.AM_Modulator, key);
            }
            if (oscillator.FM_Modulator != null)
            {
                oscillator.FM_Modulator.Key = key;
                oscillator.FM_Modulator.SetKeyboardAdjustedFrequency(key);
                oscillator.FM_Modulator.SetStepSize();
                oscillator.FM_Modulator.CurrentSample = 0;
                oscillator.FM_Modulator.Angle = 0;
                //oscillator.FM_Modulator.WaveShapeAngle = 0;
                oscillator.FM_Modulator.SetPhase();
                oscillator.FM_Modulator.RandomValue = (random.Next(1000) - 500) / 500;
                InitModulators(oscillator.FM_Modulator, key);
            }
            if (oscillator.XM_Modulator != null)
            {
                oscillator.XM_Modulator.Key = key;
                oscillator.XM_Modulator.SetFrequency();
                oscillator.XM_Modulator.SetKeyboardAdjustedFrequency(key);
                oscillator.XM_Modulator.SetStepSize();
                oscillator.XM_Modulator.CurrentSample = 0;
                oscillator.XM_Modulator.Angle = 0;
                //oscillator.XM_Modulator.WaveShapeAngle = 0;
                oscillator.XM_Modulator.SetPhase();
                oscillator.XM_Modulator.RandomValue = (random.Next(1000) - 500) / 500;
                InitModulators(oscillator.XM_Modulator, key);
            }
        }

        private void CheckModulationType()
        {
            if (Keyboard && WaveForm == WAVEFORM.SINE
                && XM_Modulator != null
                && XM_Modulator.Keyboard && XM_Modulator.WaveForm == WAVEFORM.SINE)
            {
                ModulationType = ModulationType.DX;
            }
            else if (Keyboard && WaveForm == WAVEFORM.SQUARE
                && XM_Modulator != null)
            {
                ModulationType = ModulationType.PM;
            }
            else
            {
                ModulationType = ModulationType.NORMAL;
            }
        }

        public void SetStepSize()
        {
            StepSize = (float)FrequencyInUse * Math.PI * 2 / mainPage.SampleRate;
        }

        public void SetPhase()
        {
            Phase = Math.PI * (Get_XM_Sensitivity() / 128.0f);
        }
        #endregion reInitializations

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Audio generating functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region audioGenerating

        /// <summary>
        /// Generates one chunk of audio data.
        /// Also modulates data.
        /// </summary>
        /// <param name="requestedNumberOfSamples"></param>
        /// <returns></returns>
        public void GenerateAudioData(uint requestedNumberOfSamples, float frequencyShift = 0, Boolean alsoMakeModulationData = false)
        {
            //WaveData = new double[requestedNumberOfSamples];
            double freq;
            double pitchEnvelopeValue;

            // Adjust frequency if Pitch envelope is active:
            if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(Id)]
                .SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_PITCH]).Selection == 1)
            {
                pitchEnvelopeValue = PitchEnvelope.Value;
            }
            else
            {
                pitchEnvelopeValue = 0;
            }

            freq = (1 + pitchEnvelopeValue) * FrequencyInUse;
            StepSize = freq * Math.PI * 2 / mainPage.SampleRate;

            if (AM_Modulator != null)
            {
                freq = (1 + AM_Modulator.PitchEnvelope.Value) * AM_Modulator.FrequencyInUse;
                AM_Modulator.StepSize = freq * Math.PI * 2 / mainPage.SampleRate;

                if (AM_Modulator.WaveForm == WAVEFORM.RANDOM)
                {
                    if (AM_Modulator.WaveData == null)
                    {
                        AM_Modulator.WaveData = new double[requestedNumberOfSamples];
                    }
                }
            }

            if (FM_Modulator != null)
            {
                freq = (1 + FM_Modulator.PitchEnvelope.Value) * FM_Modulator.FrequencyInUse;
                FM_Modulator.StepSize = freq * Math.PI * 2 / mainPage.SampleRate;

                if (FM_Modulator.WaveForm == WAVEFORM.RANDOM)
                {
                    if (FM_Modulator.WaveData == null)
                    {
                        FM_Modulator.WaveData = new double[requestedNumberOfSamples];
                    }
                }
            }

            if (XM_Modulator != null)
            {
                freq = (1 + XM_Modulator.PitchEnvelope.Value) * XM_Modulator.FrequencyInUse;
                XM_Modulator.StepSize = freq * Math.PI * 2 / mainPage.SampleRate;
                XM_Modulator.WaveData = new double[mainPage.SampleCount];

                if (XM_Modulator.WaveForm == WAVEFORM.RANDOM)
                {
                    if (XM_Modulator.WaveData == null)
                    {
                        XM_Modulator.WaveData = new double[requestedNumberOfSamples];
                    }
                }

                // Phase of the WaveShape must be re-created and modulated:
                WaveShape.Phase = Math.PI + XM_Sensitivity / 138.0 * XM_Modulator.MakeWave();
                WaveShape.MakeWave();
                mainPage.allowGuiUpdates = true;
            }

            // Re-filter the waveshape (if in use, is filtered and is controlled by Pitch envelope or ADSR):
            if (WaveForm < WAVEFORM.RANDOM && ModulationType != ModulationType.DX && Filter.FilterFunction > 1)
            {
                WaveShape.ApplyFilter(key);
            }

            // Wait if WaveShape is generating data:
            while (WaveShape.Buzy);

            // Used only for Noise waves to have the same mean in all frames, i.e. zero:
            double mean = 0;

            for (int i = 0; i < mainPage.SampleCount; i++)
            {
                MarkModulators(this);
                if (WaveForm == WAVEFORM.NOISE)
                {
                    // Noise is not to be translated to a certain frequency!
                    CurrentSample = 0.001 * (random.Next(1000) - 500);
					mean += CurrentSample;
                }
                else if (WaveForm == WAVEFORM.RANDOM)
                {
                    CurrentSample = RandomValue;
                }
                else if (ModulationType == ModulationType.DX)
                {
                    try
                    {
                        // Yamaha DX style FM synthesis;
                        CurrentSample = MakeDxWave(this);
                    }
                    catch (Exception exception)
                    {
                        ContentDialog error = new Error(exception.Message);
                        _ = error.ShowAsync();
                    }
                }
                else
                {
                    try
                    {
                        // Use WaveShape.WaveData to create a waveform of current frequency:
                        try
                        {
                            CurrentSample = WaveShape.WaveData[((int)(Angle * mainPage.SampleCount / Math.PI / 2)) % mainPage.SampleCount];
                            if (WaveForm == WAVEFORM.SINE)
                            {
                                CurrentSample *= mainPage.EarCompensation.KeyToGain(key);
                            }
                        }
                        catch (Exception exception)
                        {
                            ContentDialog error = new Error("Unexpected error using pre-made wave shape: " + exception.Message);
                            _ = error.ShowAsync();
                        }
                    }
                    catch (Exception exception)
                    {
                        ContentDialog error = new Error(exception.Message);
                        _ = error.ShowAsync();
                    }
                }
                CurrentSample *= Velocity / 127f;
                if (Adsr.Pulse)
                {
                    CurrentSample *= Adsr.AdsrPulseLevel;
                }
                else
                {
                    CurrentSample *= Adsr.AdsrLevel;
                }
                //if (UseAdsr)
                //{
                //    CurrentSample *= previousAdsrPulseLevel + (Adsr.AdsrLevel - previousAdsrPulseLevel) * (double)i / (double)mainPage.SampleCount;
                //}
                //else
                //{
                //    CurrentSample *= previousAdsrPulseLevel + (Adsr.Pulse.PulseLevel - previousAdsrPulseLevel) * (double)i / (double)mainPage.SampleCount;
                //}
                Modulate();
                WaveData[i] = CurrentSample / 10;
                WaveData[i] *= Volume / 128f;
                WaveData[i] = WaveData[i] > 0.1 ? 0.1 : WaveData[i];
                WaveData[i] = WaveData[i] < -0.1 ? -0.1 : WaveData[i];
                AdvanceAngle(this);
                AdvanceModulatorsAngles(this);
            }

            //previousAdsrPulseLevel = UseAdsr ? Adsr.AdsrLevel : Adsr.Pulse.PulseLevel;
			//previousAdsrPulseLevel = Adsr.AdsrLevel;

            if (WaveForm == WAVEFORM.NOISE && (mainPage.Patch.Layout < MainPage.Layouts.TWELVE_OSCILLATORS &&
                ((Rotator)((CompoundControl)mainPage.FilterGUIs[Id]).SubControls.ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection > 0
                || ((Rotator)((CompoundControl)mainPage.FilterGUIs[0]).SubControls.ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection > 0))
            {
                if (mainPage.usingGraphicsCard)
                {
                    //mainPage.Cuda_Filter(WaveData, 0, 0);
                    //ApplyFilter();
                }
                else
                {
                    //mean /= mainPage.SampleCount;
                    //for (int i = 0; i < mainPage.SampleCount; i++)
                    //{
                    //    WaveData[i] -= mean;
                    //}
                    WaveData = Filter.Apply(WaveData, key);

                    // Each frame's mean value differs after filtering, even it out:
                    //mean = 0;
                    //for (int i = 0; i < mainPage.SampleCount; i++)
                    //{
                    //    mean += WaveData[i];
                    //}
                    //mean /= mainPage.SampleCount;
                    //for (int i = 0; i < mainPage.SampleCount; i++)
                    //{
                    //    WaveData[i] -= mean;
                    //}

                    // WaveData ends don't match after filtering since filter does not account for past nor future frames.
                    // Even out the beginning and ramp down adjustmenst to zero in the procedure not to bias off uncontrolled:
                    double diff = WaveData[0] - lastEnd;
                    double stepDown = diff / mainPage.SampleCount;
                    for (int i = 0; i < mainPage.SampleCount; i++)
                    {
                        WaveData[i] -= diff;
                        diff -= stepDown;
                    }
                    lastEnd = WaveData[WaveData.Length - 1];
                }
            }
        }

        #endregion audioGenerating

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // Basic wave generating functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        #region basicWaveGenerating

        /// <summary>
        /// The basic sine FM equation:
        /// y(t) = Amplitude * sin(2π * fc * t + I * sin(2π * fm * t)),
        /// where the parameters are defined as follows:
        /// fc = carrier frequency(Hz)
        /// fm = modulation frequency(Hz)
        /// I = modulation index
        /// 2π * fc * t is pre-calculated into the Angle in calculations below.
        /// I is the modulation sensitivity knob value (must be divided down).
        /// The sin(2π* fm * t) part is recursively calling the function again
        /// for any DX FM modulator until no more DX FM modulatiors are found.
        /// </summary>
        private double MakeDxWave(Oscillator oscillator)
        {
            if (oscillator.XM_Modulator != null && oscillator.XM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.XM_Modulator.Keyboard)
            {
                return Math.Sin(oscillator.Angle
                    + oscillator.XM_Sensitivity / 64f * MakeDxWave(oscillator.XM_Modulator));
            }
            else
            {
                return Math.Sin(oscillator.Angle);
            }

        }

        /// <summary>
        /// Moves the angle in Radians one StepSize forward. Backs up 2 * PI when
        /// Radians exeeds 2 * PI. 
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns>true if Radians backed up, else false (used when generating random waveform to detect when to generate a new sample</returns>
        public void AdvanceAngle(Oscillator oscillator)
        {
            //oscillator.WaveShapeAngle += oscillator.StepSize;
            //while (oscillator.WaveShapeAngle > mainPage.SampleRate / 100)
            //{
            //    oscillator.WaveShapeAngle -= mainPage.SampleRate / 100;
            //}
            oscillator.Angle += oscillator.StepSize;
            while (oscillator.Angle > Math.PI * 2)
            {
                oscillator.Angle -= Math.PI * 2;
                oscillator.RandomValue = (random.Next(1000) - 500) / 500.0;
            }
        }

        public void AdvanceModulatorsAngles(Oscillator oscillator)
        {
            if (oscillator.AM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.AM_Modulator);
                if (oscillator.AM_Modulator.advance)
                {
                    AdvanceAngle(oscillator.AM_Modulator);
                    oscillator.AM_Modulator.advance = false;
                }
            }
            if (oscillator.FM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.FM_Modulator);
                if (oscillator.FM_Modulator.advance)
                {
                    oscillator.AdvanceAngle(oscillator.FM_Modulator);
                    oscillator.FM_Modulator.advance = false;
                }
            }
            if (oscillator.XM_Modulator != null)
            {
                AdvanceModulatorsAngles(oscillator.XM_Modulator);
                if (oscillator.XM_Modulator.advance)
                {
                    oscillator.AdvanceAngle(oscillator.XM_Modulator);
                    oscillator.XM_Modulator.advance = false;
                }
            }
        }

        private void MarkModulators(Oscillator oscillator)
        {
            if (oscillator.AM_Modulator != null)
            {
                MarkModulators(oscillator.AM_Modulator);
                oscillator.AM_Modulator.advance = true;
            }
            if (oscillator.FM_Modulator != null)
            {
                MarkModulators(oscillator.FM_Modulator);
                oscillator.FM_Modulator.advance = true;
            }
            if (oscillator.XM_Modulator != null)
            {
                MarkModulators(oscillator.XM_Modulator);
                oscillator.XM_Modulator.advance = true;
            }
        }
        #endregion basicWaveGenerating
    }
}
