using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Keyboard.cs calculates this once during MainPage Init().
        /// It is used in formula for calculating frequency from 
        /// key number based on key 69 beeing 440 Hz.
        /// NoteFrequency = 440 * Math.Pow(FrequencyFactor, key - 69)
        /// </summary>
        public Double FrequencyFactor;

        public void KeyOn(byte key, int channel, byte velocity)
         {
            Boolean oscillatorWithAcceptedChannelFound = false;
            bool isDone = false;

            if (initDone && AnyOscillatorHasOutput())
            {
                if (key != 0xff)
                {
                    allowGuiUpdates = false;
                    if (dispatcher[channel].KeyIsPlaying(key))
                    {
                        int poly = dispatcher[channel].RePress(key);
                        //try
                        //{
                        //    dispatcher[channel].IsPressed[poly] = true;
                        //}
                        //catch (Exception exception)
                        //{
                        //    ContentDialog error = new Error("Unexpected error: " + exception.Message);
                        //    _ = error.ShowAsync();
                        //}
                        for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                        {
                            try
                            {
                                Patch.Oscillators[poly][osc].Adsr.AdsrRestart();
                            }
                            catch (Exception exception)
                            {
                                ContentDialog error = new Error("Unexpected error: " + exception.Message);
                                _ = error.ShowAsync();
                            }
                            StartPitchEnvelope(poly, osc);
                        }
                    }
                    else
                    {
                        int poly = dispatcher[channel].TryAssignPoly(key);
                        if (poly > -1)
                        {
                            for (int osc = 0; osc < Patch.OscillatorCount && !isDone; osc++)
                            {
                                if (Patch.Oscillators[poly][osc].IsOutput() &&
                                    Patch.Oscillators[poly][osc].MidiChannel == channel)
                                {
                                    oscillatorWithAcceptedChannelFound = true;
                                    Patch.Oscillators[poly][osc].PolyId = poly;
                                    Patch.Oscillators[poly][osc].InitOscillator(key, velocity);
                                    //Patch.Oscillators[poly][osc].previousAdsrPulseLevel = 0;
                                    Patch.Oscillators[poly][osc].Adsr.AdsrLevel = Patch.Oscillators[poly][osc].Adsr.AdsrAttackTime > 0 ? 0 : 1;
                                    FrameServer.PolyServers[poly].Key = key;
                                    FrameServer.PolyServers[poly].FrequencyInUse = Patch.Oscillators[poly][osc].FrequencyInUse;
                                    StartPitchEnvelope(poly, osc);
                                    FrameServer.PolyServers[poly].IsOn = true;
                                    //Patch.Oscillators[poly][osc].Adsr.Pulse.PulseStart(key);
                                    Patch.Oscillators[poly][osc].Adsr.AdsrStart(key);
                                    isDone = true;
                                }
                            }
                            if (!oscillatorWithAcceptedChannelFound)
                            {
                                //dispatcher[channel].ReleasePoly(poly);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("No free poly!");
                        }
                    }
                }
            }
        }

        private void StartPitchEnvelope(int poly, int osc)
        {
            try
            {
                Patch.Oscillators[poly][osc].PitchEnvelope.Start();
            }
            catch (Exception exception)
            {
                ContentDialog error = new Error("Unexpected error using FFT: " + exception.Message);
                _ = error.ShowAsync();
            }
            Patch.Oscillators[poly][osc].PitchEnvelope.CalculateValue();
            if (Patch.Oscillators[poly][osc].AM_Modulator != null)
            {
                Patch.Oscillators[poly][osc].AM_Modulator.PitchEnvelope.Start();
            }
            if (Patch.Oscillators[poly][osc].FM_Modulator != null)
            {
                Patch.Oscillators[poly][osc].FM_Modulator.PitchEnvelope.Start();
            }
            if (Patch.Oscillators[poly][osc].XM_Modulator != null)
            {
                Patch.Oscillators[poly][osc].XM_Modulator.PitchEnvelope.Start();
            }
        }

        public void KeyOff(byte key, int channel)
        {
            if (initDone)
            {
                if (key != 0xff && dispatcher[channel].KeyIsPlaying(key))
                {
                    int poly = dispatcher[channel].TryGetPolyFromKey(key);
                    if (poly > -1)
                    {
                        for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                        {
                            if (Patch.Oscillators[poly][osc].IsOutput() && Patch.Oscillators[poly][osc].MidiChannel == channel
                                && Patch.Oscillators[poly][osc].Key == key)
                            {
                                if (!dispatcher[channel].PedalHold)
                                {
                                    if (Patch.Oscillators[poly][osc].Adsr.Pulse)
                                    {
                                        Patch.Oscillators[poly][osc].Adsr.AdsrReleaseTime = 4;
                                    }
                                    Patch.Oscillators[poly][osc].Adsr.AdsrRelease();
                                }
                                Patch.Oscillators[poly][osc].PitchEnvelope.Stop();
                            }
                        }
                    }
                }
                //if (initDone && dispatcher.NumberOfOscillatorsInUse() == 0 && osc < Patch.OscillatorCount)
                //{
                //    switch (Patch.Layout)
                //    {
                //        case Layouts.FOUR_OSCILLATORS:
                //        case Layouts.SIX_OSCILLATORS:
                //        case Layouts.EIGHT_OSCILLATORS:
                //            if (((Rotator)((CompoundControl)FilterGUIs[osc]).SubControls.ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection > 0)
                //            {
                //                UpdateFilterGui(osc);
                //            }
                //            break;
                //        case Layouts.TWELVE_OSCILLATORS:
                //            if (((Rotator)FilterGUIs[0].SubControls.ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection > 0)
                //            {
                //                UpdateFilterGui(0);
                //            }
                //            break;
                //    }
                //}
            }
        }

        public void PedalDown()
        {
            for (int ch = 0; ch < 16; ch++)
            {
                dispatcher[ch].PedalHold = true;
            }
            for (int osc = 0; osc < Patch.OscillatorCount; osc++)
            {
                for (int poly = 0; poly < Patch.Polyphony; poly++)
                {
                    Patch.Oscillators[poly][osc].Adsr.pedal.Selection = 1;
                }
            }
        }

        public void PedalUp()
        {
            for (int ch = 0; ch < 16; ch++)
            {
                dispatcher[ch].PedalHold = false;
            }
            for (int osc = 0; osc < Patch.OscillatorCount; osc++)
            {
                for (int poly = 0; poly < Patch.Polyphony; poly++)
                {
                    Patch.Oscillators[poly][osc].Adsr.pedal.Selection = 0;
                }
            }
        }

        public void ReleaseOscillator(byte key, int channel)
        {
            if (initDone)
            {
                if (key != 0xff)
                {
                    int poly = dispatcher[channel].TryGetPolyFromKey(key);
                    if (poly > -1)
                    {
                        for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                        {
                            if (dispatcher[channel].KeyIsPlaying(key))
                            {
                                FrameServer.PolyServers[poly].IsOn = false;
                            }
                        }
                        dispatcher[channel].ReleaseOscillator(key);
                    }
                }
            }
        }

        /// <summary>
        /// An oscillator has output if its volume is not zero.
        /// </summary>
        /// <returns></returns>
        private Boolean AnyOscillatorHasOutput()
        {
            for (int i = 0; i < Patch.OscillatorCount; i++)
            {
                if (Patch.Oscillators[0][i].Volume > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void Keyboard_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            int velocity = (int)(127 * (pp.Position.Y / ((Key)((Image)sender).Tag).Image.ActualHeight / 0.8));
            velocity = velocity > 127 ? 127 : velocity;
            String keyName = ((Key)((Image)sender).Tag).KeyName;
            KeyOn((byte)((Key)((Image)sender).Tag).KeyNumber, selectedOscillator == null ? 0 : selectedOscillator.MidiChannel, (byte)velocity);
        }

        private void Keyboard_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            int velocity = (int)(127 * (pp.Position.Y / ((Key)((Image)sender).Tag).Image.ActualHeight / 0.8));
            velocity = velocity > 127 ? 127 : velocity;
            String keyName = ((Key)((Image)sender).Tag).KeyName;
            byte keyNumber = (byte)((Key)((Image)sender).Tag).KeyNumber;
            KeyOff((byte)((Key)((Image)sender).Tag).KeyNumber, selectedOscillator == null ? 0 : selectedOscillator.MidiChannel);
        }

        private void Keyboard_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            ((Key)((Image)sender).Tag).Velocity = (int)(pp.Position.Y / ((Key)((Image)sender).Tag).Image.ActualHeight);
            int velocity = (int)(127 * (pp.Position.Y / ((Key)((Image)sender).Tag).Image.ActualHeight / 0.8));
            velocity = velocity > 127 ? 127 : velocity;
        }

        public double[] NoteFrequency;

        public void InitFrequencies()
        {
            FrequencyFactor = Math.Pow(2.0, 1.0 / 12.0);
            NoteFrequency = new double[128];
            for (int i = 0; i < 128; i++)
            {
                NoteFrequency[i] = 440.0 * Math.Pow(FrequencyFactor, (double)i - 69.0);
            }
            //NoteFrequency[000] = 8.176f;
            //NoteFrequency[001] = 8.662f;
            //NoteFrequency[002] = 9.177f;
            //NoteFrequency[003] = 9.723f;
            //NoteFrequency[004] = 10.301f;
            //NoteFrequency[005] = 10.913f;
            //NoteFrequency[006] = 11.562f;
            //NoteFrequency[007] = 12.250f;
            //NoteFrequency[008] = 12.978f;
            //NoteFrequency[009] = 13.750f;
            //NoteFrequency[010] = 14.568f;
            //NoteFrequency[011] = 15.434f;
            //NoteFrequency[012] = 16.352f;
            //NoteFrequency[013] = 17.324f;
            //NoteFrequency[014] = 18.354f;
            //NoteFrequency[015] = 19.445f;
            //NoteFrequency[016] = 20.602f;
            //NoteFrequency[017] = 21.827f;
            //NoteFrequency[018] = 23.125f;
            //NoteFrequency[019] = 24.500f;
            //NoteFrequency[020] = 25.957f;
            //NoteFrequency[021] = 27.5F;
            //NoteFrequency[022] = 29.13524F;
            //NoteFrequency[023] = 30.86771F;
            //NoteFrequency[024] = 32.7032F;
            //NoteFrequency[025] = 34.64783F;
            //NoteFrequency[026] = 36.7081F;
            //NoteFrequency[027] = 38.89087F;
            //NoteFrequency[028] = 41.20344F;
            //NoteFrequency[029] = 43.65353F;
            //NoteFrequency[030] = 46.2493F;
            //NoteFrequency[031] = 48.99943F;
            //NoteFrequency[032] = 51.91309F;
            //NoteFrequency[033] = 55F;
            //NoteFrequency[034] = 58.27047F;
            //NoteFrequency[035] = 61.73541F;
            //NoteFrequency[036] = 65.40639F;
            //NoteFrequency[037] = 69.29566F;
            //NoteFrequency[038] = 73.41619F;
            //NoteFrequency[039] = 77.78175F;
            //NoteFrequency[040] = 82.40689F;
            //NoteFrequency[041] = 87.30706F;
            //NoteFrequency[042] = 92.49861F;
            //NoteFrequency[043] = 97.99886F;
            //NoteFrequency[044] = 103.8262F;
            //NoteFrequency[045] = 110F;
            //NoteFrequency[046] = 116.5409F;
            //NoteFrequency[047] = 123.4708F;
            //NoteFrequency[048] = 130.8128F;
            //NoteFrequency[049] = 138.5913F;
            //NoteFrequency[050] = 146.8324F;
            //NoteFrequency[051] = 155.5635F;
            //NoteFrequency[052] = 164.8138F;
            //NoteFrequency[053] = 174.6141F;
            //NoteFrequency[054] = 184.9972F;
            //NoteFrequency[055] = 195.9977F;
            //NoteFrequency[056] = 207.6523F;
            //NoteFrequency[057] = 220F;
            //NoteFrequency[058] = 233.0819F;
            //NoteFrequency[059] = 246.9417F;
            //NoteFrequency[060] = 261.6480F;
            //NoteFrequency[061] = 277.1826F;
            //NoteFrequency[062] = 293.6648F;
            //NoteFrequency[063] = 311.127F;
            //NoteFrequency[064] = 329.6276F;
            //NoteFrequency[065] = 349.2282F;
            //NoteFrequency[066] = 369.9944F;
            //NoteFrequency[067] = 391.9954F;
            //NoteFrequency[068] = 415.3047F;
            //NoteFrequency[069] = 440F;
            //NoteFrequency[070] = 466.1638F;
            //NoteFrequency[071] = 493.8833F;
            //NoteFrequency[072] = 523.2511F;
            //NoteFrequency[073] = 554.3653F;
            //NoteFrequency[074] = 587.3295F;
            //NoteFrequency[075] = 622.254F;
            //NoteFrequency[076] = 659.2551F;
            //NoteFrequency[077] = 698.4565F;
            //NoteFrequency[078] = 739.9888F;
            //NoteFrequency[079] = 783.9909F;
            //NoteFrequency[080] = 830.6094F;
            //NoteFrequency[081] = 880F;
            //NoteFrequency[082] = 932.3275F;
            //NoteFrequency[083] = 987.7666F;
            //NoteFrequency[084] = 1046.502F;
            //NoteFrequency[085] = 1108.731F;
            //NoteFrequency[086] = 1174.659F;
            //NoteFrequency[087] = 1244.508F;
            //NoteFrequency[088] = 1318.51F;
            //NoteFrequency[089] = 1396.913F;
            //NoteFrequency[090] = 1479.978F;
            //NoteFrequency[091] = 1567.982F;
            //NoteFrequency[092] = 1661.219F;
            //NoteFrequency[093] = 1760F;
            //NoteFrequency[094] = 1864.655F;
            //NoteFrequency[095] = 1975.533F;
            //NoteFrequency[096] = 2093.005F;
            //NoteFrequency[097] = 2217.461F;
            //NoteFrequency[098] = 2349.318F;
            //NoteFrequency[099] = 2489.016F;
            //NoteFrequency[100] = 2637.02F;
            //NoteFrequency[101] = 2793.826F;
            //NoteFrequency[102] = 2959.955F;
            //NoteFrequency[103] = 3135.963F;
            //NoteFrequency[104] = 3322.438F;
            //NoteFrequency[105] = 3520F;
            //NoteFrequency[106] = 3729.31F;
            //NoteFrequency[107] = 3951.066F;
            //NoteFrequency[108] = 4186.009F;
            //NoteFrequency[109] = 4434.922f;
            //NoteFrequency[110] = 4698.636f;
            //NoteFrequency[111] = 4978.032f;
            //NoteFrequency[112] = 5274.041f;
            //NoteFrequency[113] = 5587.652f;
            //NoteFrequency[114] = 5919.911f;
            //NoteFrequency[115] = 6271.927f;
            //NoteFrequency[116] = 6644.875f;
            //NoteFrequency[117] = 7040.000f;
            //NoteFrequency[118] = 7458.620f;
            //NoteFrequency[119] = 7902.133f;
            //NoteFrequency[120] = 8372.018f;
            //NoteFrequency[121] = 8869.844f;
            //NoteFrequency[122] = 9397.273f;
            //NoteFrequency[123] = 9956.063f;
            //NoteFrequency[124] = 10548.080f;
            //NoteFrequency[125] = 11175.300f;
            //NoteFrequency[126] = 11839.820f;
            //NoteFrequency[127] = 12543.850f;
        }
    }
}
