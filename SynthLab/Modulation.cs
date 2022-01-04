using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    /// <summary>
    /// Modulation, part of Oscillator
    /// </summary>
    
    public partial class Oscillator
    {

        /// <summary>
        /// Calls proper modulation algoritms for any modulator that is attached to the current oscillator.
        /// </summary>
        /// <param name="oscillator"></param>
        public void Modulate()
        {
            if (FM_Modulator != null)
            {
                FM_Modulate(this);
            }

            if (AM_Modulator != null)
            {
                CurrentSample *= AM_Modulate(this);
            }

            //if (XM_Modulator != null)
            //{
            //    //Phase = XM_Modulate(this);
            //    if (ModulationType == ModulationType.PM)
            //   {
            //        Phase = XM_Sensitivity / 138 * XM_Modulator.Angle;
            //    }
            //    else if (ModulationType == ModulationType.DX)
            //    {
            //        ModulateDxWave(this);
            //    }
            //}

            if (XM_Modulator != null && ModulationType == ModulationType.PM)
            {
                //Phase = XM_Modulate(this);
                Phase = XM_Sensitivity / 138 * XM_Modulator.Angle;
            }
        }

        /// <summary>
        /// Amplitude modulates current oscillator or a modulator.
        /// Also checks if this modulator has any modulators attached and, if
        /// so, calls them recursively.
        /// </summary>
        /// <param name="oscillator" the current oscillator or a modulator that was attached to a modulator></param>
        /// <returns></returns>
        private double AM_Modulate(Oscillator oscillator)
        {
            double amplitude = 1;

            try
            {
                if (oscillator.AM_Modulator != null)
                {
                    //if (oscillator.AM_Modulator.AM_Modulator != null)
                    //{
                    //    amplitude *= 1 + oscillator.AM_Modulator.AM_Modulate(oscillator.AM_Modulator) * oscillator.AM_Modulator.Get_AM_Sensitivity() / 64;
                    //}

                    if (oscillator.AM_Modulator.FM_Modulator != null)
                    {
                        amplitude *= 1 + oscillator.AM_Modulator.FM_Modulate(oscillator.AM_Modulator) * oscillator.AM_Modulator.Get_FM_Sensitivity() / 128;
                    }

                    if (oscillator.AM_Modulator.AM_Modulator != null)
                    {
                        amplitude *= 1 + oscillator.AM_Modulator.AM_Modulate(oscillator.AM_Modulator) * oscillator.AM_Modulator.Get_AM_Sensitivity() / 128;
                    }

                    //if (oscillator.AM_Modulator.XM_Modulator != null)
                    //{
                    //    //amplitude *= 1 + oscillator.AM_Modulator.XM_Modulate(oscillator.AM_Modulator) * oscillator.AM_Modulator.Get_XM_Sensitivity() / 128;
                    //    oscillator.AM_Modulator.Phase = oscillator.AM_Modulator.XM_Modulator.Angle - Math.PI;
                    //}

                    if (oscillator.AM_Modulator.Keyboard)
                    {
                        amplitude *= 1 + oscillator.AM_Modulator.mainPage.Patch.Oscillators[PolyId][Id].WaveShape.WaveData[((int)(oscillator.AM_Modulator.Angle * (float)mainPage.SampleCount / Math.PI / 2)) % mainPage.SampleCount]
                            * oscillator.Get_AM_Sensitivity() / 20;
                    }
                    else
                    {
                        if (oscillator.AM_Modulator.WaveForm == WAVEFORM.RANDOM)
                        {
                            amplitude *= oscillator.AM_Modulator.RandomValue * oscillator.Get_AM_Sensitivity() / 2000;
                        }
                        else if (oscillator.AM_Modulator.WaveForm == WAVEFORM.SQUARE && oscillator.AM_Modulator.XM_Modulator != null)
                        {
                            oscillator.AM_Modulator.Phase = Math.PI + (oscillator.AM_Modulator.XM_Sensitivity / 256.0) * (oscillator.AM_Modulator.XM_Modulator.MakeWave()) * Math.PI;
                            amplitude *= 1 + oscillator.AM_Modulator.MakeSquareWave();
                        }
                        else
                        {
                            amplitude *= 1 + oscillator.AM_Modulator.mainPage.Patch.Oscillators[oscillator.AM_Modulator.PolyId][oscillator.AM_Modulator.Id]
                                .WaveShape.WaveData[((int)(oscillator.AM_Modulator.Angle * (float)mainPage.SampleCount / Math.PI / 2)) % mainPage.SampleCount]
                                * oscillator.Get_AM_Sensitivity() / 200;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Error("Unexpected error using FFT: " + exception.Message);
                _ = error.ShowAsync();
            }
            return amplitude;
        }

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
        private double FM_Modulate(Oscillator oscillator)
        {
            double fm = 1;

            if (oscillator.FM_Modulator != null)
            {
                if (oscillator.FM_Modulator.FM_Modulator != null)
                {
                    fm *= 1 + oscillator.FM_Modulator.FM_Modulate(oscillator.FM_Modulator) * oscillator.FM_Modulator.Get_FM_Sensitivity() / 128;
                }

                if (oscillator.FM_Modulator.AM_Modulator != null)
                {
                    fm *= 1 + oscillator.FM_Modulator.AM_Modulate(oscillator.FM_Modulator) * oscillator.FM_Modulator.Get_AM_Sensitivity() / 128;
                }

                if (oscillator.FM_Modulator.XM_Modulator != null)
                {
                    fm *= 1 + oscillator.FM_Modulator.XM_Modulate(oscillator.FM_Modulator) * oscillator.FM_Modulator.Get_XM_Sensitivity() / 128;
                }

                // Using LFO or none sine waveforms we can not use the genuine algorithm for FM synthesis,
                // but a change in step size makes a fair approximation:
                double freqOsc = oscillator.FrequencyInUse;
                if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(oscillator.Id)].SubControls
                    .ControlsList[(int)PitchEnvelopeControls.MOD_PITCH]).Selection == 1)
                {
                    freqOsc = (fm + oscillator.PitchEnvelope.Value) * oscillator.FrequencyInUse;
                }
                fm = freqOsc * Math.PI * 2 / mainPage.SampleRate;
                if (oscillator.FM_Modulator.Keyboard)
                {
                    fm += oscillator.FM_Modulator.mainPage.Patch.Oscillators[PolyId][Id].WaveShape
                        .WaveData[((int)(oscillator.FM_Modulator.Angle * mainPage.SampleCount / Math.PI * 2)) % mainPage.SampleCount]
                        * oscillator.Get_FM_Sensitivity() / 200;
                }
                else
                {
                    if (oscillator.FM_Modulator.WaveForm == WAVEFORM.RANDOM)
                    {
                        fm += oscillator.FM_Modulator.RandomValue * oscillator.Get_FM_Sensitivity() / 2000;
                    }
                    else
                    {
                        fm += oscillator.FM_Modulator.mainPage.Patch.Oscillators[oscillator.FM_Modulator.PolyId][oscillator.FM_Modulator.Id]
                            .WaveShape.WaveData[((int)(oscillator.FM_Modulator.Angle * mainPage.SampleCount / Math.PI / 2))
                            % mainPage.SampleCount] * oscillator.Get_FM_Sensitivity() / 2000;
                    }
                }
                double stepMin = 10 * Math.PI * 2 / mainPage.SampleRate;
                double stepMax = 10000 * Math.PI * 2 / mainPage.SampleRate;
                fm = fm < stepMin ? stepMin : fm;
                fm = fm > stepMax ? stepMax : fm;
                oscillator.StepSize = fm;
            }
            return fm;
        }

        /// <summary>
        /// Phase modulates a wave. If the current oscillator is a SQUARE waveform, the time ratio between
        /// high and low is altered while otherwise the angle of the waveform is shifted.
        /// </summary>
        /// <param name="oscillator"></param>
        private double XM_Modulate(Oscillator oscillator)
        {
            double amplitude = 1;

            if (oscillator.XM_Modulator != null)
            {
                if (oscillator.XM_Modulator.AM_Modulator != null)
                {
                    amplitude *= 1 + oscillator.XM_Modulator.AM_Modulate(oscillator.XM_Modulator) * oscillator.XM_Modulator.Get_AM_Sensitivity() / 128;
                }

                //if (oscillator.XM_Modulator.FM_Modulator != null)
                //{
                //    amplitude *= 1 + oscillator.XM_Modulator.FM_Modulate(oscillator.XM_Modulator) * oscillator.XM_Modulator.Get_FM_Sensitivity() / 128;
                //}

                if (oscillator.XM_Modulator.XM_Modulator != null)
                {
                    amplitude *= 1 + oscillator.XM_Modulator.XM_Modulate(oscillator.XM_Modulator) * oscillator.XM_Modulator.Get_XM_Sensitivity() / 128;
                }

                //if (oscillator.XM_Modulator.WaveForm == WAVEFORM.SQUARE && oscillator.XM_Modulator.XM_Modulator != null)
                //{
                //    oscillator.XM_Modulator.Phase = Math.PI + (oscillator.AM_Modulator.XM_Sensitivity / 256.0) * (oscillator.AM_Modulator.XM_Modulator.MakeWave()) * Math.PI;
                //    amplitude *= 1 + oscillator.AM_Modulator.MakeSquareWave();
                //}
                
				if (ModulationType == 0)
                {
                    oscillator.Phase += XM_Modulator.Angle - Math.PI;
                }
            }
            oscillator.Phase = oscillator.XM_Sensitivity / 138 * oscillator.XM_Modulator.Angle;
            return amplitude;
        }

        /// <summary>
        /// DX modulation synthesis is done when generating a wave thus is not
        /// handled by the usual modulation scheme. If any modulation except DX
        /// modulaiton is to be applied to any of the oscillators involved in
        /// the DX modulation, it is done here.
        /// </summary>
        /// <param name="oscillator"></param>
        private void ModulateDxWave(Oscillator oscillator)
        {
            if (oscillator.AM_Modulator != null)
            {
                oscillator.CurrentSample *= (1 + oscillator.AM_Modulator.mainPage.Patch.Oscillators[PolyId][Id].WaveShape
                    .WaveData[((int)(oscillator.AM_Modulator.Angle * mainPage.SampleCount / Math.PI * 2
                     * oscillator.Get_AM_Sensitivity() / 255)) % mainPage.SampleCount]) / 2;
            }
            else if (oscillator.FM_Modulator != null &&
                !(oscillator.FM_Modulator.WaveForm == WAVEFORM.SINE && oscillator.FM_Modulator.Keyboard))
            {
                // FM socket is already in use for the DX style modulation, so do nothing here.
                // However, the PM socket can be used instead, see below.
            }
            else if (oscillator.XM_Modulator != null)
            {
                oscillator.FrequencyInUse = ModulateDxWaveFromPM(oscillator);
                AdvanceAngle(oscillator.XM_Modulator);

                if (oscillator.FM_Modulator.XM_Modulator != null)
                {
                    oscillator.FM_Modulator.FrequencyInUse = ModulateDxWaveFromPM(oscillator.FM_Modulator);
                    AdvanceAngle(oscillator.FM_Modulator.XM_Modulator);

                    if (oscillator.FM_Modulator.FM_Modulator.XM_Modulator != null)
                    {
                        oscillator.FM_Modulator.FM_Modulator.FrequencyInUse = ModulateDxWaveFromPM(oscillator.FM_Modulator.FM_Modulator);
                        AdvanceAngle(oscillator.FM_Modulator.FM_Modulator.XM_Modulator);
                    }
                }
            }

            if (oscillator.FM_Modulator.FM_Modulator != null
                && oscillator.FM_Modulator.FM_Modulator.WaveForm == WAVEFORM.SINE
                && oscillator.FM_Modulator.FM_Modulator.Keyboard)
            {
                // This oscillator is DX modulated too, recursive call:
                ModulateDxWave(oscillator.FM_Modulator);
            }

            //oscillator.CurrentSample *= (1 + oscillator.XM_Modulator.mainPage.Patch.Oscillators[PolyId][Id].WaveShape
            //    .WaveData[((int)(oscillator.XM_Modulator.Angle * mainPage.SampleCount / Math.PI * 2
            //     * oscillator.Get_XM_Sensitivity() / 255)) % mainPage.SampleCount]) / 2;
        }

        /// <summary>
        /// PM modulation is for square wave only, and this is a DX sinus wave.
        /// However, FM modulation from other oscillators can be allowed in parallel
        /// with DX modulation. Since the FM socket is already in use, we should allow
        /// the PM socket to be used for non-DX modulation of DX-generated waveforms.
        /// </summary>
        /// <param name="oscillator"></param>
        /// <returns></returns>
        private double ModulateDxWaveFromPM(Oscillator oscillator)
        {
            double freqOsc = oscillator.finetunedFrequency;
            if (((Rotator)mainPage.PitchEnvelopeGUIs[oscillator.Id].SubControls
                .ControlsList[(int)PitchEnvelopeControls.MOD_PITCH]).Selection == 1)
            {
                freqOsc = (1 + oscillator.PitchEnvelope.Value) * freqOsc;
            }
            double stepSize = (float)freqOsc * Math.PI * 2 / mainPage.SampleRate;
            if (oscillator.XM_Modulator.Keyboard)
            {
                stepSize += oscillator.XM_Modulator.mainPage.Patch.Oscillators[PolyId][Id].WaveShape
                    .WaveData[((int)(oscillator.XM_Modulator.Angle * mainPage.SampleCount / Math.PI * 2)) % mainPage.SampleCount]
                    * (float)oscillator.Get_XM_Sensitivity() / 200;
            }
            else
            {
                stepSize += oscillator.XM_Modulator.mainPage.Patch.Oscillators[PolyId][Id].WaveShape
                    .WaveData[((int)(oscillator.XM_Modulator.Angle * mainPage.SampleCount / Math.PI * 2)) % mainPage.SampleCount]
                    * (float)oscillator.Get_XM_Sensitivity() / 2000f;
            }
            double stepMin = 10 * Math.PI * 2 / mainPage.SampleRate;
            double stepMax = 10000 * Math.PI * 2 / mainPage.SampleRate;
            stepSize = stepSize < stepMin ? stepMin : stepSize;
            stepSize = stepSize > stepMax ? stepMax : stepSize;
            return stepSize / Math.PI * 2 * mainPage.SampleRate;
        }

        private float Get_AM_Sensitivity()
        {
            float sensitivity = AM_Sensitivity;
            if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(Id)]
                .SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_AM]).Selection == 1)
            {
                sensitivity *= PitchEnvelope.Value;
            }
            return sensitivity;
        }

        private float Get_FM_Sensitivity()
        {
            float sensitivity = FM_Sensitivity;
            if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(Id)]
                .SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_FM]).Selection == 1)
            {
                sensitivity *= PitchEnvelope.Value;
            }
            return sensitivity;
        }

        private float Get_XM_Sensitivity()
        {
            float sensitivity = XM_Sensitivity;
            if (((Rotator)mainPage.PitchEnvelopeGUIs[mainPage.OscillatorToPitchEnvelope(Id)]
                .SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_XM]).Selection == 1)
            {
                sensitivity *= PitchEnvelope.Value;
            }
            return sensitivity;
        }
    }
}
