﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    /////////////////////////////////////////////////////////////////////////////////////////////////
    // GUI updates
    // In order to ensure all GUI updates are been done in correct thread, all updates are done in
    // UpdateTimer_Tick in MainPage. Flags denoting GUI updates needs and functions performing
    // them are declared here, thus in MainPage.
    /////////////////////////////////////////////////////////////////////////////////////////////////

    public sealed partial class MainPage : Page
    {
        public bool ControlsNeedsUpdate = false;
        public bool OscillographNeedsUpdate = false;
        public bool[] AdsrNeedsUpdate;
        public int ModulationWheelNeedsUpdate = -1; // 0 - 255 indicates the update is needed and the setting.

        /// <summary>
        /// Updates the GUI for current oscillator, pitch enveloope, ADSR and display but not the graphs.
        /// Use this to set all pots, rotators etc. when changing layout and when loading a patch.
        /// </summary>
        public void UpdateGui()
        {
            try
            {
                if (initDone && selectedOscillator != null)
                {
                    // Turn off all view leds:
                    for (int i = 0; i < Patch.OscillatorsInLayout; i++)
                    {
                        ((Rotator)OscillatorGUIs[i].SubControls
                            .ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 0;
                        for (int poly = 0; poly < Patch.Polyphony; poly++)
                        {
                            Patch.Oscillators[poly][i].ViewMe = false;
                        }
                    }

                    // Turn on view led for selected oscillator;
                    ((Rotator)OscillatorGUIs[selectedOscillator.Id].SubControls
                        .ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 1;
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        Patch.Oscillators[poly][selectedOscillator.Id].ViewMe = true;
                    }

                    // Set oscillator controls according to settings in oscillator:
                    //UpdateOscillatorGuis();

                    // Set filter controls according to settings in filter:
                    UpdateFilterGuis();

                    // Draw display frequency:
                    ((DigitalDisplay)DisplayGUI.
                    SubControls.ControlsList[(int)DisplayControls.DIGITS]).DisplayValue(
                    selectedOscillator.FrequencyInUse);

                    // ADSR:
                    if (Patch.Layout == Layouts.FOUR_OSCILLATORS)
                    {
                        for (int osc = 0; osc < Patch.OscillatorsInLayout; osc++)
                        {
                            ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.
                                ControlsList[(int)AdsrControls.ADSR_A]).Value = Patch.Oscillators[0][osc].Adsr.AdsrAttackTime;
                            ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.
                                ControlsList[(int)AdsrControls.ADSR_D]).Value = Patch.Oscillators[0][osc].Adsr.AdsrDecayTime;
                            ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.
                                ControlsList[(int)AdsrControls.ADSR_S]).Value = Patch.Oscillators[0][osc].Adsr.AdsrSustainLevel;
                            ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.
                                ControlsList[(int)AdsrControls.ADSR_R]).Value = Patch.Oscillators[0][osc].Adsr.AdsrReleaseTime;// - 1;
                            //((Graph)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.GRAPH]).Draw();
                            DrawAdsr(OscillatorToAdsr(osc), osc);
                        }
                    }

                    // Pitch envelope:
                    if (Patch.Layout == Layouts.FOUR_OSCILLATORS)
                    {
                        for (int osc = 0; osc < Patch.OscillatorsInLayout; osc++)
                        {
                            // Set pots:
                            ((Knob)PitchEnvelopeGUIs[osc].SubControls.
                                ControlsList[(int)PitchEnvelopeControls.DEPTH]).Value =
                                Patch.Oscillators[0][osc].PitchEnvelope.Depth;
                            ((Knob)PitchEnvelopeGUIs[osc].SubControls.
                                ControlsList[(int)PitchEnvelopeControls.SPEED]).Value =
                                (int)(Patch.Oscillators[0][osc].PitchEnvelope.Speed);// * 500);
                        }
                    }
                    else
                    {
                        // Set pots:
                        ((Knob)PitchEnvelopeGUIs[0].SubControls.
                            ControlsList[(int)PitchEnvelopeControls.DEPTH]).Value =
                            selectedOscillator.PitchEnvelope.Depth;
                        ((Knob)PitchEnvelopeGUIs[0].SubControls.
                            ControlsList[(int)PitchEnvelopeControls.SPEED]).Value =
                            (int)(selectedOscillator.PitchEnvelope.Speed) * 500;
                    }

                    // Draw oscilloscope:
                    if (Patch.Oscillators[0][selectedOscillator.Id] != null && Patch.Oscillators[selectedOscillator.PolyId][selectedOscillator.Id].WaveShape != null
                        && Patch.Oscillators[0][selectedOscillator.Id].WaveData != null && Patch.Oscillators[selectedOscillator.PolyId][selectedOscillator.Id].WaveShape != null
                        && Patch.Oscillators[0][selectedOscillator.Id].WaveData.Length > 0 && Patch.Oscillators[selectedOscillator.PolyId][selectedOscillator.Id].WaveShape.WaveData.Length > 0)
                    {
                        Point[] graphData = Patch.Oscillators[0][selectedOscillator.Id].MakeGraphData(66);
                        ((Graph)DisplayGUI.SubControls.ControlsList[(int)DisplayControls.OSCILLOGRAPH]).Draw(graphData);
                    }
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Error("Unexpected error: " + exception.Message);
                _ = error.ShowAsync();
            }
        }

        public void UpdateOscillatorGuis()
        {
            for (int osc = 0; osc < Patch.OscillatorCount; osc++)
            {
                switch (((Rotator)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection)
                {
                    case 0:
                        ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                            (int)Patch.Oscillators[0][osc].AM_Sensitivity / 2;
                        break;
                    case 1:
                        ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                            (int)Patch.Oscillators[0][osc].FM_Sensitivity / 2;
                        break;
                    case 2:
                        ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                            (int)Patch.Oscillators[0][osc].XM_Sensitivity / 2;
                        break;
                }
                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.FREQUENCY]).Value =
                    (int)Patch.Oscillators[0][osc].Frequency * 128 / 1000;
                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.FINE_TUNE]).Value =
                    (int)Patch.Oscillators[0][osc].FineTune * 128 / 1000;
                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.VOLUME]).Value =
                    (int)Patch.Oscillators[0][osc].Volume * 128;
            }
        }

        /// <summary>
        /// Updates one oscillator's filter GUI's with values from
        /// oscillator poly 0 filter (all polys has the same values for the GUI's).
        /// </summary>
        /// <param name="oscillatorId"></param>
        /// <param name="control">Default NONE for all controls</param>
        /// <param name="value">Default 0, used when control is not default</param>
        public void UpdateFilterGuis()
        {
            switch (Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                case Layouts.SIX_OSCILLATORS:
                    // Each oscillator has it's own filter, loop all and update GUI's:
                    for (int osc = 0; osc < Patch.OscillatorsInLayout; osc++)
                    {
                        ((Rotator)FilterGUIs[osc].SubControls.
                            ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection = Patch.Oscillators[0][osc].Filter.FilterFunction;
                        ((Rotator)FilterGUIs[osc].SubControls.
                            ControlsList[(int)FilterControls.MODULATION_WHEEL_TARGET]).Selection = Patch.Oscillators[0][osc].Filter.ModulationWheelTarget;
                        ((Knob)FilterGUIs[osc].SubControls.
                            ControlsList[(int)FilterControls.Q]).Value = (int)Patch.Oscillators[0][osc].Filter.Q;
                        ((Knob)FilterGUIs[osc].SubControls.
                            ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value = (int)Patch.Oscillators[0][osc].Filter.FrequencyCenter;
                        ((Knob)FilterGUIs[osc].SubControls.
                            ControlsList[(int)FilterControls.KEYBOARD_FOLLOW]).Value = (int)Patch.Oscillators[0][osc].Filter.KeyboardFollow;
                        ((Knob)FilterGUIs[osc].SubControls.
                            ControlsList[(int)FilterControls.GAIN]).Value = (int)Patch.Oscillators[0][osc].Filter.Gain;
                        ((Knob)FilterGUIs[osc].SubControls.
                            ControlsList[(int)FilterControls.FILTER_MIX]).Value = (int)Patch.Oscillators[0][osc].Filter.Mix;
                    }
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    // Oscillators are organized in four columns and two rows. The row under contains filter GUI's shared by two 
                    // oscillators each:
                    for (int osc = 0; osc < 4; osc++)
                    {
                        if (!OscillatorGUIs[osc].IsSelected && !OscillatorGUIs[osc + 4].IsSelected)
                        {
                            OscillatorGUIs[osc].IsSelected = true;
                        }
                        if (OscillatorGUIs[osc].IsSelected)
                        {
                            ((Rotator)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection = Patch.Oscillators[0][osc].Filter.FilterFunction;
                            ((Rotator)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.MODULATION_WHEEL_TARGET]).Selection = Patch.Oscillators[0][osc].Filter.ModulationWheelTarget;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.Q]).Value = (int)Patch.Oscillators[0][osc].Filter.Q;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value = (int)Patch.Oscillators[0][osc].Filter.FrequencyCenter;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.KEYBOARD_FOLLOW]).Value = (int)Patch.Oscillators[0][osc].Filter.KeyboardFollow;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.GAIN]).Value = (int)Patch.Oscillators[0][osc].Filter.Gain;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.FILTER_MIX]).Value = (int)Patch.Oscillators[0][osc].Filter.Mix;
                        }
                        else
                        {
                            ((Rotator)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection = Patch.Oscillators[0][osc + 4].Filter.FilterFunction;
                            ((Rotator)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.MODULATION_WHEEL_TARGET]).Selection = Patch.Oscillators[0][osc + 4].Filter.ModulationWheelTarget;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.Q]).Value = (int)Patch.Oscillators[0][osc + 4].Filter.Q;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value = (int)Patch.Oscillators[0][osc + 4].Filter.FrequencyCenter;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.KEYBOARD_FOLLOW]).Value = (int)Patch.Oscillators[0][osc + 4].Filter.KeyboardFollow;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.GAIN]).Value = (int)Patch.Oscillators[0][osc + 4].Filter.Gain;
                            ((Knob)FilterGUIs[osc].SubControls.
                                ControlsList[(int)FilterControls.FILTER_MIX]).Value = (int)Patch.Oscillators[0][osc + 4].Filter.Mix;
                        }
                    }
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    // There is only one filter GUI, set it to reflect the selected oscillator (if any):
                    if (selectedOscillator != null)
                    {
                        ((Rotator)FilterGUIs[0].SubControls.
                            ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection = Patch.Oscillators[0][selectedOscillator.Id].Filter.FilterFunction;
                        ((Rotator)FilterGUIs[0].SubControls.
                            ControlsList[(int)FilterControls.MODULATION_WHEEL_TARGET]).Selection = Patch.Oscillators[0][selectedOscillator.Id].Filter.ModulationWheelTarget;
                        ((Knob)FilterGUIs[0].SubControls.
                            ControlsList[(int)FilterControls.Q]).Value = (int)Patch.Oscillators[0][selectedOscillator.Id].Filter.Q;
                        ((Knob)FilterGUIs[0].SubControls.
                            ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value = (int)Patch.Oscillators[0][selectedOscillator.Id].Filter.FrequencyCenter;
                        ((Knob)FilterGUIs[0].SubControls.
                            ControlsList[(int)FilterControls.KEYBOARD_FOLLOW]).Value = (int)Patch.Oscillators[0][selectedOscillator.Id].Filter.KeyboardFollow;
                        ((Knob)FilterGUIs[0].SubControls.
                            ControlsList[(int)FilterControls.GAIN]).Value = (int)Patch.Oscillators[0][selectedOscillator.Id].Filter.Gain;
                        ((Knob)FilterGUIs[0].SubControls.
                            ControlsList[(int)FilterControls.FILTER_MIX]).Value = (int)Patch.Oscillators[0][selectedOscillator.Id].Filter.Mix;
                    }
                    break;
            }
        }

        public void UpdateWiring()
        {
            // Modulators are not included in json because that would cause circural
            // references. Instead, their Id's are stored in json. Connect the modulators:
            int modulatorId;
            int modulatedId;
            for (int osc = 0; osc < Patch.OscillatorCount; osc++)
            {
                if (Patch.Oscillators[0][osc].AM_ModulatorId > -1)
                {
                    modulatorId = Patch.Oscillators[0][osc].AM_ModulatorId;
                    modulatedId = Patch.Oscillators[0][osc].Id;
                    Patch.Wiring.wires[FindWire(modulatorId, modulatedId, SocketType.AM)].Indicator.IsOn = true;
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        Patch.Oscillators[poly][osc].AM_Modulator = Patch.Oscillators[poly][modulatorId];
                    }
                }

                if (Patch.Oscillators[0][osc].FM_ModulatorId > -1)
                {
                    modulatorId = Patch.Oscillators[0][osc].FM_ModulatorId;
                    modulatedId = Patch.Oscillators[0][osc].Id;
                    Patch.Wiring.wires[FindWire(modulatorId, modulatedId, SocketType.FM)].Indicator.IsOn = true;
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        Patch.Oscillators[poly][osc].FM_Modulator = Patch.Oscillators[poly][modulatorId];
                    }
                }

                if (Patch.Oscillators[0][osc].XM_ModulatorId > -1)
                {
                    modulatorId = Patch.Oscillators[0][osc].XM_ModulatorId;
                    modulatedId = Patch.Oscillators[0][osc].Id;
                    Patch.Wiring.wires[FindWire(modulatorId, modulatedId, SocketType.XM)].Indicator.IsOn = true;
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        Patch.Oscillators[poly][osc].XM_Modulator = Patch.Oscillators[poly][modulatorId];
                    }
                }
            }
        }

        private void DrawAdsr(int adsr, int osc)
        {
            Graph graph = (Graph)AdsrGUIs[adsr].SubControls.ControlsList[(int)AdsrControls.GRAPH];
            Rect adsrBounds = new Rect(new Point(0, 0),
                new Point(imgAdsrGraphBackground.ActualWidth - 4, imgAdsrGraphBackground.ActualHeight - 2));
            graph.Points.Clear();
            graph.AddPoint(new Point(
                adsrBounds.Left,
                adsrBounds.Bottom));
            graph.AddPoint(new Point(
                    adsrBounds.Left + Patch.Oscillators[0][osc].Adsr.AdsrAttackTime / 2,
                    adsrBounds.Top));
            graph.AddPoint(new Point(
                    graph.Points[1].X + Patch.Oscillators[0][osc].Adsr.AdsrDecayTime / 2,
                    adsrBounds.Bottom - (adsrBounds.Bottom - adsrBounds.Top)
                    * Patch.Oscillators[0][osc].Adsr.AdsrSustainLevel / 127));
            graph.AddPoint(new Point(
                    adsrBounds.Right - (Patch.Oscillators[0][osc].Adsr.AdsrReleaseTime - 1) / 2,
                    graph.Points[2].Y));
            graph.AddPoint(new Point(
                    adsrBounds.Right,
                    adsrBounds.Bottom));
            graph.Draw();
        }

        public void FollowCommonAdsrGui(Knob knob)
        {
            for (int gui = 0; gui < AdsrGUIs.Length; gui++)
            {
                switch (knob.Id)
                {
                    case (int)AdsrControls.ADSR_A:
                        ((Knob)AdsrGUIs[gui].SubControls.ControlsList[(int)AdsrControls.ADSR_A]).Value = knob.Value;
                        break;
                    case (int)AdsrControls.ADSR_D:
                        ((Knob)AdsrGUIs[gui].SubControls.ControlsList[(int)AdsrControls.ADSR_D]).Value = knob.Value;
                        break;
                    case (int)AdsrControls.ADSR_S:
                        ((Knob)AdsrGUIs[gui].SubControls.ControlsList[(int)AdsrControls.ADSR_S]).Value = knob.Value;
                        break;
                    case (int)AdsrControls.ADSR_R:
                        ((Knob)AdsrGUIs[gui].SubControls.ControlsList[(int)AdsrControls.ADSR_R]).Value = knob.Value;
                        break;
                }
            }
        }

        public void FollowModulationWheel(int Value)
        {
            // Let GUI follow. 
            if (selectedOscillator != null)
            {
                for (int filter = 0; filter < FilterGUIs.Length; filter++)
                {
                    switch (((Rotator)FilterGUIs[filter].SubControls.ControlsList[(int)FilterControls
                        .MODULATION_WHEEL_TARGET]).Selection - 1)
                    {
                        case (int)FilterControls.Q:
                            ((Knob)FilterGUIs[filter].SubControls.ControlsList[(int)FilterControls.Q]).Value = Value;
                            break;
                        case (int)FilterControls.FREQUENCY_CENTER:
                            ((Knob)FilterGUIs[filter].SubControls.ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value = Value;
                            break;
                        case (int)FilterControls.KEYBOARD_FOLLOW:
                            ((Knob)FilterGUIs[filter].SubControls.ControlsList[(int)FilterControls.KEYBOARD_FOLLOW]).Value = Value;
                            break;
                        case (int)FilterControls.GAIN:
                            ((Knob)FilterGUIs[filter].SubControls.ControlsList[(int)FilterControls.GAIN]).Value = Value;
                            break;
                        case (int)FilterControls.FILTER_MIX:
                            ((Knob)FilterGUIs[filter].SubControls.ControlsList[(int)FilterControls.FILTER_MIX]).Value = Value;
                            break;
                    }
                }

                for (int adsr = 0; adsr < AdsrGUIs.Length; adsr++)
                {
                    switch (((Rotator)AdsrGUIs[adsr].SubControls.ControlsList[(int)AdsrControls
                        .ADSR_MODULATION_WHEEL_USE]).Selection)
                    {
                        case 1:
                            ((Knob)AdsrGUIs[adsr].SubControls.ControlsList[(int)AdsrControls.ADSR_A]).Value = Value;
                            break;
                        case 2:
                            ((Knob)AdsrGUIs[adsr].SubControls.ControlsList[(int)AdsrControls.ADSR_D]).Value = Value;
                            break;
                        case 3:
                            ((Knob)AdsrGUIs[adsr].SubControls.ControlsList[(int)AdsrControls.ADSR_S]).Value = Value;
                            break;
                        case 4:
                            ((Knob)AdsrGUIs[adsr].SubControls.ControlsList[(int)AdsrControls.ADSR_R]).Value = Value;
                            break;
                    }
                    DrawAdsr(adsr, adsr);
                }

                for (int pitchEnvelope = 0; pitchEnvelope < PitchEnvelopeGUIs.Length; pitchEnvelope++)
                {
                    switch (((Rotator)PitchEnvelopeGUIs[pitchEnvelope]
                        .SubControls.ControlsList[(int)PitchEnvelopeControls.PITCH_ENV_MODULATION_WHEEL_USE]).Selection)
                    {
                        case (int)PitchEnvelopeControls.DEPTH:
                            ((Knob)PitchEnvelopeGUIs[pitchEnvelope]
                                .SubControls.ControlsList[(int)PitchEnvelopeControls.DEPTH]).Value = Value;
                            break;
                        case (int)PitchEnvelopeControls.SPEED:
                            ((Knob)PitchEnvelopeGUIs[pitchEnvelope]
                                .SubControls.ControlsList[(int)PitchEnvelopeControls.SPEED]).Value = Value * 500 / 128;
                            break;
                    }
                }

                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                {
                    switch (Patch.Oscillators[0][osc].ModulationWheelTarget)
                    {
                        case 1:
                            if (Patch.Oscillators[0][osc].ModulationKnobTarget == 0)
                            {
                                ((Knob)OscillatorGUIs[osc]
                                    .SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = Value * 256 / 128;
                            }
                            break;
                        case 2:
                            if (Patch.Oscillators[0][osc].ModulationKnobTarget == 1)
                            {
                                ((Knob)OscillatorGUIs[osc]
                                    .SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = Value * 256 / 128;
                            }
                            break;
                        case 3:
                            if (Patch.Oscillators[0][osc].ModulationKnobTarget == 2)
                            {
                                ((Knob)OscillatorGUIs[osc]
                                    .SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = Value * 256 / 128;
                            }
                            break;
                        case 4:
                            ((Knob)OscillatorGUIs[osc]
                                .SubControls.ControlsList[(int)OscillatorControls.FREQUENCY]).Value = Value * 1000 / 128;
                            break;
                        case 5:
                            ((Knob)OscillatorGUIs[osc]
                                .SubControls.ControlsList[(int)OscillatorControls.FINE_TUNE]).Value = Value * 1000 / 128;
                            break;
                        case 6:
                            ((Knob)OscillatorGUIs[osc]
                                .SubControls.ControlsList[(int)OscillatorControls.VOLUME]).Value = Value;
                            break;
                    }
                }
            }

            // Let objects follow.
            if (selectedOscillator != null)
            {
                for (int filter = 0; filter < Patch.Oscillators[0].Count; filter++)
                {
                    switch (Patch.Oscillators[0][filter].Filter.ModulationWheelTarget - 1)
                    {
                        case (int)FilterControls.Q:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][filter].Filter.Q = (byte)Value;
                                Patch.Oscillators[poly][filter].WaveShape.ApplyFilter(Patch.Oscillators[poly][filter].Key);
                            }
                            break;
                        case (int)FilterControls.FREQUENCY_CENTER:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][filter].Filter.FrequencyCenter = (byte)Value;
                                Patch.Oscillators[poly][filter].WaveShape.ApplyFilter(Patch.Oscillators[poly][filter].Key);
                            }
                            break;
                        case (int)FilterControls.KEYBOARD_FOLLOW:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][filter].Filter.KeyboardFollow = (byte)Value;
                                Patch.Oscillators[poly][filter].WaveShape.ApplyFilter(Patch.Oscillators[poly][filter].Key);
                            }
                            break;
                        case (int)FilterControls.GAIN:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][filter].Filter.Gain = (byte)Value;
                                Patch.Oscillators[poly][filter].WaveShape.ApplyFilter(Patch.Oscillators[poly][filter].Key);
                            }
                            break;
                        case (int)FilterControls.FILTER_MIX:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][filter].Filter.Mix = (byte)Value;
                                Patch.Oscillators[poly][filter].WaveShape.ApplyFilter(Patch.Oscillators[poly][filter].Key);
                            }
                            break;
                    }
                }

                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                {
                    switch (Patch.Oscillators[0][osc].Adsr.AdsrModulationWheelTarget)
                    {
                        case 1:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][osc].Adsr.AdsrAttackTime = (byte)Value;
                            }
                            break;
                        case 2:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][osc].Adsr.AdsrDecayTime = (byte)Value;
                            }
                            break;
                        case 3:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][osc].Adsr.AdsrSustainLevel = (byte)Value;
                            }
                            break;
                        case 4:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][osc].Adsr.AdsrReleaseTime = (byte)(Value);
                            }
                            break;
                    }
                }

                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                {
                    switch (Patch.Oscillators[0][osc].PitchEnvelope.PitchEnvModulationWheelTarget)
                    {
                        case (int)PitchEnvelopeControls.DEPTH:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][osc].PitchEnvelope.Depth = (byte)Value;
                            }
                            break;
                        case (int)PitchEnvelopeControls.SPEED:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][osc].PitchEnvelope.Speed = (byte)Value;
                            }
                            break;
                    }
                }

                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                {
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        switch (Patch.Oscillators[poly][osc].ModulationWheelTarget)
                        {
                            case 1:
                                Patch.Oscillators[poly][osc].AM_Sensitivity = Value * 2;
                                break;
                            case 2:
                                Patch.Oscillators[poly][osc].FM_Sensitivity = Value * 2;
                                break;
                            case 3:
                                Patch.Oscillators[poly][osc].XM_Sensitivity = Value * 2;
                                break;
                            case 4:
                                Patch.Oscillators[poly][osc].Frequency = Value;
                                break;
                            case 5:
                                Patch.Oscillators[poly][osc].FineTune = Value;
                                break;
                            case 6:
                                Patch.Oscillators[poly][osc].Volume = (byte)Value;
                                break;
                        }
                    }
                }
            }
            //allowGuiUpdates = true;
        }
    }
}
