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
    public class Pulse
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // Pulse
        /// Turning on, and especially turning off, a wave causes a sudden
        /// jump from/to zero volt output, which makes a disturbin short
        /// noise. The ADSR has been adjusted to ramp up/down at least
        /// a very short time, unless user sets longer times, in order
        /// to remove that short noise. However, when using pulse rather
        /// than ADSR will of course bring the problem back. Therefore
        /// the class Pulse is introduced to work as an ADSR without
        /// D and S, and with fixed A and R to a short time. When pulse
        /// is selected, the Pulse class is selected rather than the
        /// ADSR class to control the output level over time.
        /// 
        /// When using pulse, the key off is still delayed byt the ADSR if release is grater than zero.
        /// This allows for having also R arrecting the filter, which normally was not possible on
        /// vintage synthesizers.
        /// 
        /// Key on -> ADSR and Pulse starts.
        /// Key off -> ADSR release runs.
        /// ADSR release ends -> Pulse release starts.
        /// Pulse release ends -> oscillator is released.
        /// Oscillator follows volume of ADSR or Pulse depending on the flag UseAdsr.
        /// Filter follows volume of ADSR if set to do so.
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public enum PulseStates
        {
            NONE,
            ATTACK,
            SUSTAIN,
            RELEASE,
            RELEASE_END
        }

        //-----------------------------------------------------------------------------------------------------
        // Pulse properties
        //-----------------------------------------------------------------------------------------------------

        public PulseStates PulseState;
        public double PulseAttackTime;   // 0 - 127 = 0 - 3 seconds
        public double PulseReleaseTime;  // 0 - 127 = 0 - 12 seconds
        public double PulseLevel;
        public double PulseTime;
        public Boolean PedalDown;
        [JsonIgnore]
        public MainPage mainPage;
        [JsonIgnore]
        public ADSR adsr;
        private byte key;
        private double pulseTimeStep;

        public Pulse(MainPage MainPage, ADSR adsr)
        {
            Init();
            mainPage = MainPage;
            this.adsr = adsr;
            PulseAttackTime = 1;
            PulseReleaseTime = 1;
            pulseTimeStep = 1;
        }

        //-----------------------------------------------------------------------------------------------------
        // Pulse private
        //-----------------------------------------------------------------------------------------------------
        private Timer pulseTimer;

        ////////////////////////////////////////////////////////////////////////////////////////////////
        // Pulse funtions
        ////////////////////////////////////////////////////////////////////////////////////////////////

        private Boolean Init()
        {
            PulseState = PulseStates.NONE;
            pulseTimer = new Timer();
            pulseTimer.Interval = 1;
            pulseTimer.Elapsed += PulseTimer_Tick;
            pulseTimer.Start();
            return true;
        }

        private void PulseTimer_Tick(object sender, object e)
        {
            double progress;

            pulseTimer.Stop();

            switch (PulseState)
            {
                case PulseStates.ATTACK:
                    if (PulseTime < PulseAttackTime)
                    {
                        // During attack time, increment and update peak value:
                        PulseTime += pulseTimeStep; // 0.167F;
                        PulseLevel = (float)(PulseTime / PulseAttackTime);
                    }
                    else
                    {
                        PulseState = PulseStates.SUSTAIN;
                        PulseLevel = 1f;
                        PulseTime = 0;
                    }
                    break;
                case PulseStates.SUSTAIN:
                    // Do nothing while key is on
                    break;
                case PulseStates.RELEASE:
                    // Off has been received, time restarted, and we are ready
                    // for release fade, if it has release time left to do:
                    if (PulseTime < PulseReleaseTime && PulseLevel > 0)
                    {
                        progress = (PulseTime) / ((float)PulseReleaseTime);
                        PulseTime += pulseTimeStep; // 0.5167F;
                        PulseLevel = 1 - 1 * progress;
                        if (PulseLevel < 0.00001) //(1f / 128f))
                        {
                            PulseState = PulseStates.RELEASE_END;
                            PulseLevel = 0;
                        }
                    }
                    else
                    {
                        // This is to let the oscillator finish at zero before turning off.
                        // Otherwise turining off with some level left would create a clicking sound.
                        PulseState = PulseStates.RELEASE_END;
                        PulseLevel = 0;
                    }
                    break;
                case PulseStates.RELEASE_END:
                    PulseState = PulseStates.NONE;
                    mainPage.ReleaseOscillator(key, adsr.oscillator.MidiChannel);
                    if (mainPage.dispatcher[adsr.oscillator.MidiChannel].NumberOfOscillatorsInUse() == 0)
                    {
                        mainPage.allowGuiUpdates = true;
                    }
                    break;
            }
            pulseTimer.Start();
        }

        public void PulseStart(byte key)
        {
            this.key = key;
            PulseTime = 0;
            PulseState = PulseStates.ATTACK;
            PulseLevel = 0;
        }

        public void PulseRestart()
        {
            PulseTime = 0;
            PulseStart(key);
        }

        public void PulseRelease()
        {
            pulseTimer.Stop();
            if (PulseState == Pulse.PulseStates.NONE)
            {
                PulseStop();
            }
            else
            {
                PulseReleaseTime = (byte)(PulseReleaseTime < 2 ? 2 : PulseReleaseTime);
                PulseState = PulseStates.RELEASE;
            }
            pulseTimer.Start();
        }

        public void PulseStop()
        {
            PulseState = PulseStates.NONE;
            PulseLevel = 0;
            mainPage.dispatcher[adsr.oscillator.MidiChannel].ReleaseOscillator(key);
        }
    }
}
