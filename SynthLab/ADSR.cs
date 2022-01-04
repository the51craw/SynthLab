using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Timers;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace SynthLab
{
    public class ADSR
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // ADSR
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public enum AdsrStates
        {
            NONE,
            ATTACK,
            DECAY,
            SUSTAIN,
            RELEASE,
            RELEASE_END
        }

        //-----------------------------------------------------------------------------------------------------
        // ADSR properties
        //-----------------------------------------------------------------------------------------------------

        //public Pulse Pulse;
        public AdsrStates AdsrState;
        public byte AdsrAttackTime;   // 0 - 127 = 0 - 3 seconds
        public byte AdsrDecayTime;    // 0 - 127 = 0 - 12 seconds
        public byte AdsrSustainLevel { get { return (byte)(sustainLevel * 128); } set { sustainLevel = (float)value / (float)128; } } // between 0  and 1
        public byte AdsrReleaseTime;  // 0 - 127 = 0 - 12 seconds
        public float AdsrLevel; // 0 - 1 Compensated for ears being non-linear in audio level experience.
        public float AdsrPulseLevel; // 0 - 1 Compensated for ears being non-linear in audio level experience.

        /// <summary>
        /// Pulse = false: normal ADSR operation. Pulse = true: volume is not affected by pulse.
        /// </summary>
        public Boolean Pulse;
        public float AdsrTime;
        public Boolean PedalDown;
        public Knob knobA;
        public Knob knobD;
        public Knob knobS;
        public Knob knobR;
        public Rotator pedal;
        public AreaButton output;
        public int AdsrModulationWheelTarget;
        [JsonIgnore]
        public MainPage mainPage;
        [JsonIgnore]
        public Oscillator oscillator;
        public Rect AdsrBounds;
        private byte key;
        private float adsrTimeStep;
        private int channel;

        [JsonConstructor]
        public ADSR(MainPage MainPage)
        {
            this.mainPage = MainPage;
            //Pulse = new Pulse(mainPage, this);
            AdsrAttackTime = 0;
            AdsrDecayTime = 0;
            AdsrSustainLevel = 127;
            AdsrReleaseTime = 0;
            adsrTimeStep = 0.2F;
            AdsrState = AdsrStates.NONE;
            adsrTimer = new Timer();
            adsrTimer.Interval = 1;
            adsrTimer.Elapsed += AdsrTimer_Tick;
            adsrTimer.Start();
        }

        public void Init(Oscillator oscillator)
        {
            this.oscillator = oscillator;
            channel = oscillator.MidiChannel;
        }

        //-----------------------------------------------------------------------------------------------------
        // ADSR private
        //-----------------------------------------------------------------------------------------------------
        private float sustainLevel;
        private Timer adsrTimer;
        float adsrPeakLevel = 1f;    // If key is down after full attack, this value is 1, 
                                     // but if the key is released during attack or decay, 
                                     // it is lower. This is the level to start release from.

        ////////////////////////////////////////////////////////////////////////////////////////////////
        // ADSR funtions
        ////////////////////////////////////////////////////////////////////////////////////////////////

        private void AdsrTimer_Tick(object sender, object e)
        {
            float progress;

            adsrTimer.Stop();

            switch (AdsrState)
            {
                case AdsrStates.ATTACK:
                    if (AdsrTime < AdsrAttackTime)
                    {
                        // During attack time, increment and update peak value:
                        AdsrTime += adsrTimeStep; // 0.167F;
                        AdsrLevel = (float)(AdsrTime / AdsrAttackTime);
                    }
                    else
                    {
                        // Attack time is over:
                        if (AdsrDecayTime > 0)
                        {
                            // Decay time > 0 => go to decay:
                            AdsrState = AdsrStates.DECAY;
                        }
                        else
                        {
                            AdsrState = AdsrStates.SUSTAIN;
                        }
                        // Decay starts over with time = 0:
                        AdsrTime = 0;
                    }
                    // Keep track of level in case key is released during a slope (attack or decay):
                    adsrPeakLevel = AdsrLevel;
                    adsrTimer.Start();
                    break;
                case AdsrStates.DECAY:
                    if (AdsrTime < AdsrDecayTime && AdsrLevel > 0)
                    {
                        // Decay in progress:
                        progress = (AdsrTime) / ((float)AdsrDecayTime);
                        AdsrTime += adsrTimeStep;
                        AdsrLevel = adsrPeakLevel - (adsrPeakLevel - sustainLevel) * progress;
                        if (AdsrLevel < (1f / 128f))
                        {
                            AdsrLevel = 0;
                        }
                    }
                    else
                    {
                        // Decay time ended:
                        // Jump down to sustain level without delay:
                        AdsrLevel = AdsrSustainLevel / 128f;
                        AdsrState = AdsrStates.SUSTAIN;
                        adsrPeakLevel = AdsrLevel;
                    }
                    adsrTimer.Start();
                    break;
                case AdsrStates.SUSTAIN:
                    // Keep track of level in case key was released during a slope (attack or decay):
                    //adsrPeakLevel = AdsrLevel;
                    // Jump down to sustain level without delay:
                    AdsrLevel = AdsrSustainLevel / 128f;
                    AdsrTime = 0;
                    adsrTimer.Start();
                    break;
                case AdsrStates.RELEASE:
                    // Off has been received, time restarted, and we are ready
                    // for release fade, if it has release time left to do:
                    if (AdsrTime < AdsrReleaseTime && AdsrLevel > 0)
                    {
                        progress = (AdsrTime) / ((float)AdsrReleaseTime);
                        AdsrTime += adsrTimeStep; // 0.5167F;
                        AdsrLevel = adsrPeakLevel - adsrPeakLevel * progress;
                        AdsrPulseLevel = 1 - progress;
                        if (AdsrLevel < 0.00001) //(1f / 128f))
                        {
                            AdsrState = AdsrStates.RELEASE_END;
                            AdsrLevel = 0;
                        }
                    }
                    else
                    {
                        // This is to let the oscillator finish at zero before turning off.
                        // Otherwise turining off with some level left would create a clicking sound.
                        AdsrState = AdsrStates.RELEASE_END;
                        AdsrLevel = 0;
                        AdsrPulseLevel = 0;
                    }
                    adsrTimer.Start();
                    break;
                case AdsrStates.RELEASE_END:
                    AdsrState = AdsrStates.NONE;
                    //Pulse.PulseState = Pulse.PulseStates.RELEASE;
                    mainPage.ReleaseOscillator(key, channel);
                    //if (mainPage.dispatcher.NumberOfOscillatorsInUse() == 0)
                    //{
                    //    mainPage.allowGuiUpdates = true;
                    //}
                    break;
            }
        }

        public void AdsrStart(byte key)
        {
            this.key = key;
            AdsrTime = 0;
            AdsrPulseLevel = 1;

            if (AdsrAttackTime > 0)
            {
                AdsrState = AdsrStates.ATTACK;
                AdsrLevel = 0;
            }
            else
            {
                adsrPeakLevel = 1;
                if (AdsrDecayTime > 0)
                {
                    AdsrState = AdsrStates.DECAY;
                }
                else
                {
                    AdsrLevel = AdsrSustainLevel / 127f;
                    AdsrState = AdsrStates.SUSTAIN;
                }
            }
            adsrTimer.Start();
        }

        public void AdsrRestart()
        {
            AdsrTime = 0;
            AdsrStart(key);
        }

        public void AdsrRelease()
        {
            adsrTimer.Stop();
            if (AdsrState == ADSR.AdsrStates.NONE)
            {
                AdsrStop();
            }
            else
            {
                //if (Pulse)
                //{
                //    adsrPeakLevel = 1;
                //}
                //else
                {
                    adsrPeakLevel = AdsrLevel;
                }
                AdsrReleaseTime = (byte)(AdsrReleaseTime < 0.01 ? 0.01 : AdsrReleaseTime);
                AdsrState = AdsrStates.RELEASE;
            }
            adsrTimer.Start();
        }

        public void AdsrStop()
        {
            adsrTimer.Stop();
            AdsrState = AdsrStates.RELEASE_END;
            AdsrLevel = 0;
            //Pulse.PulseRelease();
            adsrTimer.Start();
        }
    }
}
