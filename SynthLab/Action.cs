using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static SynthLab.Oscillator;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Shapes;
using System.Diagnostics;
using System.Numerics;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        #region properties
        /// <summary>
        /// This is the oscillator automatically selected when mouse pointer
        /// is moving over a control in the GUI for the oscillator.
        /// It is valid when using a sub control in this control.
        /// It is set to Oscillators[0][0] at startup.
        /// </summary>
        public Oscillator currentOscillator;

        /// <summary>
        /// This is the latest selected oscillator. It was selected by clicking
        /// the View button of the interface or by changing a setting in a compound control that belongs to the oscillator.
        /// It is set to Oscillators[0][0] at startup and when changing layout.
        /// This is used for GUI's when they are shared to more than one oscillator.
        /// Layout with four oscillators has only the display shared to the four oscillators.
        /// Layout with twelve oscillators has all GUI's shared with all oscillators, except of course the oscillators themselves.
        /// </summary>
        public Oscillator selectedOscillator;

        /// <summary>
        /// This is the pitch envelope automatically selected when mouse pointer
        /// is moving over a control in the GUI for the pitch envelope.
        /// It is valid when using a sub control in this control.
        /// </summary>
        public CompoundControl currentPitchEnvelope;

        /// <summary>
        /// This is the ADSR automatically selected when mouse pointer
        /// is moving over a control in the GUI for the ADSR.
        /// It is valid when using a sub control in this control.
        /// </summary>
        public CompoundControl currentAdsr;

        /// <summary>
        /// This is the display automatically selected when mouse pointer
        /// is moving over a control in the GUI for the display.
        /// It is valid when using a sub control in this control.
        /// </summary>
        public CompoundControl currentDisplay;

        public int AdsrModWheel = 0;
        public int FilterModWheel = 0;
        int osc = 0;
        int srcOsc = -1;

        #endregion properties
        #region main

        public async void Action(Object obj, Object value = null)
        {
            if (obj != null)
            {
                if (value != null)
                {
                    if (obj.GetType() == typeof(Knob))
                    {
                        await KnobAction((Knob)obj, value);
                    }
                    else if (obj.GetType() == typeof(VerticalSlider))
                    {
                        await SliderAction((VerticalSlider)obj, value);
                    }
                    else if (obj.GetType() == typeof(Rotator))
                    {
                        RotatorAction((Rotator)obj, value);
                    }
                    else if (obj.GetType() == typeof(AreaButton))
                    {
                        AreaButtonAction((AreaButton)obj, value);
                    }
                    else if (obj.GetType() == typeof(Graph))
                    {
                        GraphAction((Graph)obj, value);
                    }
                }
                else
                {
                    if (obj.GetType() == typeof(Knob))
                    {
                        await KnobAction((Knob)obj, ((Knob)obj).Value);
                    }
                    else if (obj.GetType() == typeof(VerticalSlider))
                    {
                        await SliderAction ((VerticalSlider)obj, ((VerticalSlider)obj).Value);
                    }
                    else if (obj.GetType() == typeof(Rotator))
                    {
                        RotatorAction((Rotator)obj, ((Rotator)obj).Selection);
                    }
                    else if (obj.GetType() == typeof(AreaButton))
                    {
                        AreaButtonAction((AreaButton)obj, null);
                    }
                }
                //for (int osc = 0; osc < 12; osc++)
                //{
                //    for (int poly = 0; poly < 6; poly++)
                //    {
                //        Oscillators[poly][osc].WaveShape.SetNeedsToBeCreated();
                //    }
                //}
            }
            allowGuiUpdates = true;
        }

        #endregion main
        #region knobs

        public async Task KnobAction(Knob knob, Object obj)
        {
            switch (selectedCompoundType)
            {
                case _compoundType.OSCILLATOR:
                    osc = oscillatorUnderMouse;
                    switch (knob.Id)
                    {
                        case (int)OscillatorControls.MODULATION:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillator oscillator = Oscillators[poly][osc];

                                switch (((Rotator)(OscillatorGUIs[osc].
                                    SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET])).Selection)
                                {
                                    case 0:
                                        oscillator.AM_Sensitivity = (float)knob.Value / 256.0;
                                        break;
                                    case 1:
                                    case 2:
                                        oscillator.FM_Sensitivity = (float)knob.Value / 256.0;
                                        break;
                                    case 3:
                                        oscillator.XM_Sensitivity = (float)knob.Value;
                                        if (oscillator.WaveForm == WAVEFORM.SQUARE)
                                        {
                                            // XM_Sensitivity for a squarewave should not be allowed to be zero in order to 
                                            // match the ration between the high and low to be the same as when PM_Sensitivity
                                            // is 127 (but, of course, inverted):
                                            if (oscillator.XM_Sensitivity == 0)
                                            {
                                                oscillator.XM_Sensitivity = 1;
                                            }
                                            Oscillators[poly][oscillatorUnderMouse].Phase = Math.PI * (1 + (knob.Value - 128) / 138f);
                                        }
                                        break;
                                }
                            }
                            break;
                        case (int)OscillatorControls.FREQUENCY:
                            selectedOscillator = Oscillators[0][osc];
                            for (int poly = 0; poly < 6; poly++)
                            {
                                //if (currentOscillator.Keyboard)
                                //{
                                //    Oscillators[poly][currentOscillator.Id].Frequency = knob.Value;
                                //}
                                //else
                                //{
                                //    Oscillators[poly][currentOscillator.Id].FrequencyLfo = knob.Value;
                                //}

                                //if (Oscillators[poly][currentOscillator.Id].Key != 0xff)
                                //{
                                //    Oscillators[poly][currentOscillator.Id].
                                //        SetKeyboardAdjustedFrequency(Oscillators[poly][currentOscillator.Id].Key);
                                //}

                                Oscillators[poly][currentOscillator.Id].Frequency = knob.Value * 10;
                                Oscillators[poly][currentOscillator.Id].FrequencyLfo = (float)Math.Truncate((float)knob.Value / 10);
                                Oscillators[poly][currentOscillator.Id].SetStepSize();
                            }
                            if (DisplayOnOff.Selection == 0)
                            {
                                // Display is turned off, but we need to show frequency when tuning oscillators:
                                ((DigitalDisplay)DisplayGUI.
                                SubControls.ControlsList[(int)DisplayControls.DIGITS]).DisplayValue(
                                selectedOscillator.FrequencyInUse);
                            }
                            break;
                        case (int)OscillatorControls.FINE_TUNE:
                            selectedOscillator = Oscillators[0][osc];
                            for (int poly = 0; poly < 6; poly++)
                            {
                                //if (currentOscillator.Keyboard)
                                //{
                                //    Oscillators[poly][currentOscillator.Id].FineTune = (float)knob.Value;
                                //}
                                //else
                                //{
                                //    Oscillators[poly][currentOscillator.Id].FineTuneLfo = (float)knob.Value;
                                //}

                                //if (Oscillators[poly][currentOscillator.Id].Key != 0xff)
                                //{
                                //    Oscillators[poly][currentOscillator.Id].
                                //        SetKeyboardAdjustedFrequency(Oscillators[poly][currentOscillator.Id].Key);
                                //}

                                Oscillators[poly][currentOscillator.Id].FineTune = (float)knob.Value;
                                Oscillators[poly][currentOscillator.Id].FineTuneLfo = (float)knob.Value;
                                Oscillators[poly][currentOscillator.Id].SetStepSize();
                            }
                            if (DisplayOnOff.Selection == 0)
                            {
                                // Display is turned off, but we need to show frequency when tuning oscillators:
                                ((DigitalDisplay)DisplayGUI.
                                SubControls.ControlsList[(int)DisplayControls.DIGITS]).DisplayValue(
                                selectedOscillator.FrequencyInUse);
                            }
                            break;
                        case (int)OscillatorControls.VOLUME:
                            selectedOscillator = Oscillators[0][osc];
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Volume = (byte)knob.Value;
                            }
                            //if (Oscillators[0][osc].Volume == 0)
                            //{
                            //    ((Indicator)OscillatorGUIs[osc].SubControls.
                            //        ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = false;
                            //    ((Indicator)OscillatorGUIs[osc].SubControls.
                            //        ControlsList[(int)OscillatorControls.LEDSOUNDING_ADVANCED]).IsOn = false;
                            //}
                            break;
                    }
                    try
                    {
                        //Oscillators[0][osc].WaveShape.SetCanBeUsed(Oscillators[0][osc]);
                        //if (Oscillators[0][osc].WaveShape.CanBeUsed)
                        //{
                        //    ((Indicator)OscillatorGUIs[osc].SubControls.
                        //        ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = true;
                        //    ((Indicator)OscillatorGUIs[osc].SubControls.
                        //        ControlsList[(int)OscillatorControls.LEDSOUNDING_ADVANCED]).IsOn = false;
                        //}
                        //else
                        //{
                        //    ((Indicator)OscillatorGUIs[osc].SubControls.
                        //        ControlsList[(int)OscillatorControls.LEDSOUNDING_ADVANCED]).IsOn = true;
                        //    ((Indicator)OscillatorGUIs[osc].SubControls.
                        //        ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = false;
                        //}
                    }
                    catch (Exception exception)
                    {
                        ContentDialog error = new Message(exception.Message);
                        _ = error.ShowAsync();
                    }
                    break;
                case _compoundType.FILTER:
                    osc = FilterToOscillator(filterUnderMouse);
                    selectedOscillator = Oscillators[0][osc];
                    ((Knob)((CompoundControl)FilterGUIs[filterUnderMouse]).
                    SubControls.ControlsList[knob.Id]).Value = (int)obj;
                    // Don't change parameters in the middle of generating wave shape:
                    while (CurrentActivity == CURRENTACTIVITY.GENERATING_WAVE_SHAPE)
                    {
                        await Task.Delay(1);
                    }
                    CurrentActivity = CURRENTACTIVITY.CHANGING_PARAMETERS;
                    for (int poly = 0; poly < 6; poly++)
                    {
                        switch (knob.Id)
                        {
                            case (int)FilterControls.Q:
                                Oscillators[poly][osc].Filter.Q = (byte)(int)obj;
                                break;
                            case (int)FilterControls.FREQUENCY_CENTER:
                                Oscillators[poly][osc].Filter.FrequencyCenter = (byte)(int)obj;
                                break;
                            case (int)FilterControls.KEYBOARD_FOLLOW:
                                Oscillators[poly][osc].Filter.KeyboardFollow = (byte)(int)obj;
                                break;
                            case (int)FilterControls.GAIN:
                                Oscillators[poly][osc].Filter.Gain = (byte)(int)obj;
                                break;
                            case (int)FilterControls.FILTER_MIX:
                                Oscillators[poly][osc].Filter.Mix = (byte)(int)obj;
                                break;
                        }
                        if (Oscillators[poly][osc].Filter.FilterFunction > 0)
                        {
                            //Oscillators[poly][osc].WaveShape.SetWaveShapeUsage(FindOscillatorByModulator(Oscillators[poly][osc]));
                            Oscillators[poly][osc].WaveShape.SetWaveShapeUsage(Oscillators[poly][osc]);
                        }
                    }
                    CurrentActivity = CURRENTACTIVITY.NONE;
                    break;
                case _compoundType.PITCH_ENVELOPE:
                    osc = PitchEnvelopeToOscillator(pitchEnvelopeUnderMouse);
                    selectedOscillator = Oscillators[0][osc];
                    switch (knob.Id)
                    {
                        case (int)PitchEnvelopeControls.DEPTH:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc]
                                    .PitchEnvelope.Depth = (int)obj;
                            }
                            break;
                        case (int)PitchEnvelopeControls.SPEED:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc]
                                    .PitchEnvelope.Speed = (int)obj;
                            }
                            break;
                    }
                    break;
                case _compoundType.ADSR:
                    osc = AdsrToOscillator(adsrUnderMouse);
                    selectedOscillator = Oscillators[0][osc];
                    switch (knob.Id)
                    {
                        case ((int)AdsrControls.ADSR_A):
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrAttackTime = (byte)(int)obj;
                            }
                            break;
                        case (int)AdsrControls.ADSR_D:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrDecayTime = (byte)(int)obj;
                            }
                            break;
                        case (int)AdsrControls.ADSR_S:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrSustainLevel = (byte)(int)obj;
                            }
                            break;
                        case (int)AdsrControls.ADSR_R:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrReleaseTime = (byte)(int)obj;
                            }
                            break;
                    }
                    //if (AdsrTypeSelection == 1)
                    //{
                    //    FollowCommonAdsrGui(knob);
                    //}
                    break;
            }
            CheckLoad(Oscillators[0][osc]);
            allowGuiUpdates = true;
        }

        #endregion knobs
        #region sliders

        public async Task SliderAction(VerticalSlider ctrl, Object obj)
        {
            // Don't change parameters in the middle of generating wave shape:
            while (CurrentActivity == CURRENTACTIVITY.GENERATING_WAVE_SHAPE)
            {
                await Task.Delay(1);
            }
            if ((OtherControls)ctrl.Id == OtherControls.PITCH_BENDER_WHEEL)
            {
                pitchBenderReleased = false;
                for (int ch = 0; ch < 16; ch++)
                {
                    if (ctrl.Value > 8192)
                    {
                        PitchBend[ch] = (1 + (ctrl.Value - 8192f) / 8191f);
                    }
                    else if (ctrl.Value < 8192)
                    {
                        PitchBend[ch] = 0.5 + (ctrl.Value / 16383f);
                    }
                    else
                    {
                        PitchBend[ch] = 1f;
                    }
                }
            }
            else if ((OtherControls)ctrl.Id == OtherControls.MODULATION_WHEEL)
            {
                CurrentActivity = CURRENTACTIVITY.CHANGING_PARAMETERS;
                FollowModulationWheel((int)obj);
                CurrentActivity = CURRENTACTIVITY.NONE;
            }
            else if ((ControlPanelControls)ctrl.Id == ControlPanelControls.REVERB_VALUE)
            {
                if (ctrl.Value == 0)
                {
                    FrameServer.TurnOffReverb();
                    Reverb.Selection = 0;
                }
                else
                {
                    Reverb.Selection = 1;
                    FrameServer.TurnOnReverb();
                    FrameServer.reverbEffectDefinition.DecayTime = (double)ctrl.Value / 2.0;
                }
                //FrameServer.reverbEffectDefinition.RoomSize = ctrl.Value;
                //FrameServer.reverbEffectDefinition.ReflectionsDelay = (uint)ctrl.Value;
            }
        }

        #endregion sliders
        #region rotators

        public async void RotatorAction(Rotator ctrl, Object obj)
        {
            int oscId = 0;
            int connectorId = -1;
            int wireId = -1;
            switch (selectedCompoundType)
            {
                case _compoundType.OSCILLATOR:
                    osc = oscillatorUnderMouse;
                    if (osc < 0)
                    {
                        break;
                    }
                    selectedOscillator = Oscillators[0][osc];
                    switch (ctrl.Id)
                    {
                        case (int)OscillatorControls.WAVE:
                            if ((WAVEFORM)ctrl.Selection == WAVEFORM.WAVE)
                            {
                                // If not selecting another waveform within one second,
                                // the select wavefiles folder pops up from this timer.
                                //selectWavefilesTimer.Start();
                            }
                            else if ((WAVEFORM)ctrl.Selection == WAVEFORM.DRUMSET)
                            {
                                // If not selecting another waveform within one second,
                                // the select drumset folder pops up from this timer.
                                //selectDrumsetTimer.Start();
                            }
                            else
                            {
                                //selectDrumsetTimer.Stop();
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].WaveForm = (WAVEFORM)ctrl.Selection;
                            }
                            if (DisplayOnOff.Selection == 0)
                            {
                                ((Graph)DisplayGUI.SubControls.ControlsList[(int)DisplayControls.OSCILLOGRAPH]).
                                    Draw(oscilloscope.ClearGraph());
                                ((Graph)DisplayGUI.SubControls.ControlsList[(int)DisplayControls.OSCILLOGRAPH]).
                                    Draw(oscilloscope.MakeSample(selectedOscillator));
                            }
                            break;
                        case (int)OscillatorControls.KEYBOARD_OR_FIXED:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].Keyboard = (Double)ctrl.Selection == 0;
                                Oscillators[poly][oscillatorUnderMouse].SetFrequency();
                                if (ctrl.Selection == 1)
                                {
                                    Oscillators[poly][oscillatorUnderMouse].Filter.FilterFunction = 0;
                                }
                            }
                            if (ctrl.Selection == 1)
                            {
                                ((Rotator)FilterGUIs[OscillatorToFilter(oscillatorUnderMouse)].SubControls.
                                    ControlsList[(int)FilterControls.FILTER_FUNCTION]).Selection = 0;
                            }
                            switch (((Rotator)ctrl).Selection)
                            {
                                case 0:
                                    ((DigitalDisplay)DisplayGUI
                                        .SubControls.ControlsList[(int)DisplayControls.DIGITS]).DisplayValue(currentOscillator.FinetunedFrequency);
                                    ((Knob)OscillatorGUIs[currentOscillator.Id].SubControls.ControlsList[(int)OscillatorControls.FREQUENCY]).Value =
                                        (int)currentOscillator.FinetunedFrequency / 10;
                                    ((Knob)OscillatorGUIs[currentOscillator.Id].SubControls.ControlsList[(int)OscillatorControls.FINE_TUNE]).Value =
                                        (int)(currentOscillator.FinetunedFrequency % 10);
                                    break;
                                case 1:
                                    ((DigitalDisplay)DisplayGUI
                                        .SubControls.ControlsList[(int)DisplayControls.DIGITS]).DisplayValue(currentOscillator.LfoFrequency);
                                    ((Knob)OscillatorGUIs[currentOscillator.Id].SubControls.ControlsList[(int)OscillatorControls.FREQUENCY]).Value =
                                        (int)currentOscillator.LfoFrequency * 10;
                                    ((Knob)OscillatorGUIs[currentOscillator.Id].SubControls.ControlsList[(int)OscillatorControls.FINE_TUNE]).Value =
                                        (int)(currentOscillator.LfoFrequency % 10);
                                    break;
                            }
                            break;
                        case (int)OscillatorControls.MODULATION_KNOB_TARGET:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = ((Rotator)ctrl).Selection;
                            }
                            switch (Oscillators[0][oscillatorUnderMouse].ModulationKnobTarget)
                            {
                                case 0:
                                    ((Knob)(OscillatorGUIs[oscillatorUnderMouse].
                                        SubControls.ControlsList[(int)OscillatorControls.MODULATION])).Value =
                                        (int)(Oscillators[0][oscillatorUnderMouse].AM_Sensitivity * 256.0);
                                    Oscillators[0][oscillatorUnderMouse].Usage = OscillatorUsage.MODULATION;
                                    break;
                                case 1:
                                    ((Knob)(OscillatorGUIs[oscillatorUnderMouse].
                                        SubControls.ControlsList[(int)OscillatorControls.MODULATION])).Value =
                                        (int)(Oscillators[0][oscillatorUnderMouse].FM_Sensitivity * 256.0);
                                    Oscillators[0][oscillatorUnderMouse].Usage = OscillatorUsage.FM;
                                    break;
                                case 2:
                                    ((Knob)(OscillatorGUIs[oscillatorUnderMouse].
                                        SubControls.ControlsList[(int)OscillatorControls.MODULATION])).Value =
                                        (int)(Oscillators[0][oscillatorUnderMouse].FM_Sensitivity * 256.0);
                                    Oscillators[0][oscillatorUnderMouse].Usage = OscillatorUsage.FM_PLUS_MINUS;
                                    break;
                                case 3:
                                    ((Knob)(OscillatorGUIs[oscillatorUnderMouse].
                                        SubControls.ControlsList[(int)OscillatorControls.MODULATION])).Value =
                                        (int)Oscillators[0][oscillatorUnderMouse].XM_Sensitivity;
                                    Oscillators[0][oscillatorUnderMouse].Usage = OscillatorUsage.MODULATION;
                                    break;
                            }
                            break;
                        case (int)OscillatorControls.MODULATION_WHEEL_TARGET:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].ModulationWheelTarget = ((Rotator)ctrl).Selection;
                            }
                            break;
                        case (int)OscillatorControls.ADSR_OR_PULSE:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].UseAdsr = (ctrl.Selection % 2) == 0;
                            }
                            break;
                        case (int)OscillatorControls.OUT_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            Source = new Socket(SocketType.OUT, connectorId);
                            if (Wiring.SourceConnected && !Wiring.DestinationConnected)
                            {
                                Wiring.SourceConnected = false;
                                ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                            }
                            else if (Wiring.DestinationConnected)
                            {
                                MakeConnection();
                            }
                            else
                            {
                                ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                    ((Rotator)ctrl).HitArea.Left + ((Rotator)ctrl).HitArea.Width - 27,
                                    ((Rotator)ctrl).HitArea.Top + ((Rotator)ctrl).HitArea.Height - 26));
                                ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                Wiring.SourceConnected = true;
                                srcOsc = -1;
                            }
                            break;
                        case (int)OscillatorControls.AM_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            wireId = FindConnectedWire(SocketType.AM, connectorId);
                            if (wireId > -1)
                            {
                                for (int poly = 0; poly < 6; poly++)
                                {
                                    Oscillators[poly][oscillatorUnderMouse].AM_Modulator = null;
                                }
                                Disconnect(wireId, SocketType.AM); // There is already a wire here, disconnect!
                            }
                            else
                            {
                                Destination = new Socket(SocketType.AM, connectorId);
                                if (Wiring.SourceConnected)
                                {
                                    MakeConnection();
                                }
                                else if (Wiring.DestinationConnected)
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                                    Wiring.DestinationConnected = false;
                                }
                                else if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                        ((Rotator)ctrl).HitArea.Left + ((Rotator)ctrl).HitArea.Width - 27,
                                        ((Rotator)ctrl).HitArea.Top + ((Rotator)ctrl).HitArea.Height - 26));
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                    Wiring.DestinationConnected = true;
                                    //srcOsc = osc;
                                }
                            }
                            ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 0;
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 0;
                            }
                            ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                                (int)Oscillators[0][oscillatorUnderMouse].AM_Sensitivity;
                            break;
                        case (int)OscillatorControls.FM_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            wireId = FindConnectedWire(SocketType.FM, connectorId);
                            if (wireId > -1)
                            {
                                for (int poly = 0; poly < 6; poly++)
                                {
                                    Oscillators[poly][oscillatorUnderMouse].FM_Modulator = null;
                                }
                                Disconnect(wireId, SocketType.FM);
                            }
                            else
                            {
                                Destination = new Socket(SocketType.FM, connectorId);
                                if (Wiring.SourceConnected)
                                {
                                    MakeConnection();
                                }
                                else if (Wiring.DestinationConnected)
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                                    Wiring.DestinationConnected = false;
                                }
                                else if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                        ((Rotator)ctrl).HitArea.Left + ((Rotator)ctrl).HitArea.Width - 27,
                                        ((Rotator)ctrl).HitArea.Top + ((Rotator)ctrl).HitArea.Height - 26));
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                    Wiring.DestinationConnected = true;
                                    //srcOsc = osc;
                                }
                            }
                            ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 1;
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 1;
                            }
                            ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                                (int)Oscillators[0][oscillatorUnderMouse].FM_Sensitivity;
                            break;
                        case (int)OscillatorControls.XM_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            wireId = FindConnectedWire(SocketType.XM, connectorId);
                            if (wireId > -1)
                            {
                                for (int poly = 0; poly < 6; poly++)
                                {
                                    Oscillators[poly][oscillatorUnderMouse].XM_Modulator = null;

                                    if (Oscillators[poly][oscillatorUnderMouse].WaveForm == WAVEFORM.SQUARE)
                                    {
                                        // We do not know what modulation the modulation knob represents,
                                        // so just set a clean square wave:
                                        Oscillators[poly][oscillatorUnderMouse].Phase = Math.PI;
                                    }
                                }
                                Disconnect(wireId, SocketType.XM);
                            }
                            else
                            {
                                Destination = new Socket(SocketType.XM, connectorId);
                                if (Wiring.SourceConnected)
                                {
                                    MakeConnection();
                                }
                                else if (Wiring.DestinationConnected)
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                                    Wiring.DestinationConnected = false;
                                }
                                else
                                {
                                    if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                                    {
                                        ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                        ((Rotator)ctrl).HitArea.Left + ((Rotator)ctrl).HitArea.Width - 27,
                                        ((Rotator)ctrl).HitArea.Top + ((Rotator)ctrl).HitArea.Height - 26));
                                        ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                        Wiring.DestinationConnected = true;
                                        //srcOsc = osc;
                                    }
                                }
                            }
                            ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 3;
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 3;
                            }
                            ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                                (int)Oscillators[0][oscillatorUnderMouse].XM_Sensitivity;
                            break;
                        case (int)OscillatorControls.VIEW_ME:
                            selectedOscillator = currentOscillator;
                            allowGuiUpdates = true;
                            break;
                    }
                    break;
                case _compoundType.FILTER:
                    osc = FilterToOscillator(filterUnderMouse);
                    selectedOscillator = Oscillators[0][osc];
                    for (int poly = 0; poly < 6; poly++)
                    {
                        switch (ctrl.Id)
                        {
                            case (int)FilterControls.FILTER_FUNCTION:
                                if (Oscillators[poly][osc].Keyboard)
                                {
                                    Oscillators[poly][osc].Filter.FilterFunction = (int)obj;
                                }
                                break;
                            case (int)FilterControls.MODULATION_WHEEL_TARGET:
                                Oscillators[poly][osc].Filter.ModulationWheelTarget = (int)obj;
                                break;
                        }
                    }
                    break;
                case _compoundType.PITCH_ENVELOPE:
                    osc = PitchEnvelopeToOscillator(pitchEnvelopeUnderMouse);
                    selectedOscillator = Oscillators[0][osc];

                    for (int poly = 0; poly < 6; poly++)
                    {
                        switch (ctrl.Id)
                        {
                            case (int)PitchEnvelopeControls.PITCH_ENV_MODULATION_WHEEL_USE:
                                Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvModulationWheelTarget = (int)obj; ;
                                break;
                            case (int)PitchEnvelopeControls.MOD_PITCH:
                                Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvPitch = (int)obj;
                                break;
                            case (int)PitchEnvelopeControls.MOD_AM:
                                Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvAm = (int)obj;
                                break;
                            case (int)PitchEnvelopeControls.MOD_FM:
                                Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvFm = (int)obj;
                                break;
                            case (int)PitchEnvelopeControls.MOD_XM:
                                Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvXm = (int)obj;
                                break;
                        }
                    }
                    switch (ctrl.Id)
                    {
                        case (int)PitchEnvelopeControls.MOD_AM:
                            if ((int)obj == 1)
                            {
                                ((Rotator)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.ADSR_AM_SENS]).Selection = 0;
                            }
                            break;
                        case (int)PitchEnvelopeControls.MOD_FM:
                            if ((int)obj == 1)
                            {
                                ((Rotator)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.ADSR_FM_SENS]).Selection = 0;
                            }
                            break;
                        case (int)PitchEnvelopeControls.MOD_XM:
                            if ((int)obj == 1)
                            {
                                ((Rotator)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.ADSR_XM_SENS]).Selection = 0;
                            }
                            break;
                    }
                    break;
                case _compoundType.ADSR:
                    osc = AdsrToOscillator(adsrUnderMouse);
                    selectedOscillator = Oscillators[0][osc];
                    switch (ctrl.Id)
                    {
                        case (int)AdsrControls.ADSR_MODULATION_WHEEL_USE:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrModulationWheelTarget = (int)obj;
                            }
                            break;
                        case (int)(AdsrControls.ADSR_AM_SENS):
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrAmSensitive = (int)obj > 0;
                            }
                            break;
                        case (int)(AdsrControls.ADSR_FM_SENS):
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrFmSensitive = (int)obj > 0;
                            }
                            break;
                        case (int)(AdsrControls.ADSR_XM_SENS):
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrXmSensitive = (int)obj > 0;
                            }
                            break;
                        case (int)AdsrControls.PEDAL_HOLD:
                            dispatcher[selectedOscillator.Id].PedalHold = ((Rotator)ctrl).Selection > 0;
                            for (int poly = 0; poly < 6; poly++)
                            {
                                for (int osc = 0; osc < 12; osc++)
                                {
                                    if (Oscillators[poly][osc].MidiChannel == selectedOscillator.MidiChannel)
                                    {
                                        dispatcher[osc].PedalHold = dispatcher[selectedOscillator.Id].PedalHold;
                                        if (!dispatcher[selectedOscillator.Id].PedalHold)
                                        {
                                            Oscillators[poly][osc].Adsr.AdsrRelease(Oscillators[poly][osc].UseAdsr);
                                        }
                                        if (Patch.OscillatorsInLayout == 4 && osc < 4)
                                        {
                                            ((Rotator)AdsrGUIs[AdsrToOscillator(osc)].SubControls.ControlsList[(int)AdsrControls.PEDAL_HOLD]).Selection =
                                                dispatcher[selectedOscillator.Id].PedalHold ? 1 : 0;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                    switch (ctrl.Id)
                    {
                        case (int)AdsrControls.ADSR_AM_SENS:
                            if ((int)obj == 1)
                            {
                                ((Rotator)PitchEnvelopeGUIs[OscillatorToPitchEnvelope(osc)].SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_AM]).Selection = 0;
                            }
                            break;
                        case (int)AdsrControls.ADSR_FM_SENS:
                            if ((int)obj == 1)
                            {
                                ((Rotator)PitchEnvelopeGUIs[OscillatorToPitchEnvelope(osc)].SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_FM]).Selection = 0;
                            }
                            break;
                        case (int)AdsrControls.ADSR_XM_SENS:
                            if ((int)obj == 1)
                            {
                                ((Rotator)PitchEnvelopeGUIs[OscillatorToPitchEnvelope(osc)].SubControls.ControlsList[(int)PitchEnvelopeControls.MOD_XM]).Selection = 0;
                            }
                            break;
                    }
                    break;
                case _compoundType.DISPLAY:
                    osc = -1;
                    switch (ctrl.Id)
                    {
                        case (int)DisplayControls.MILLIVOLTS_PER_CM:
                            switch((int)obj)
                            {
                                case 0:
                                    oscilloscope.VoltsPerCm = 0;
                                    break;
                                case 1:
                                    oscilloscope.VoltsPerCm = 1;
                                    break;
                                case 2:
                                    oscilloscope.VoltsPerCm = 2;
                                    break;
                                case 3:
                                    oscilloscope.VoltsPerCm = 5;
                                    break;
                                case 4:
                                    oscilloscope.VoltsPerCm = 10;
                                    break;
                                case 5:
                                    oscilloscope.VoltsPerCm = 20;
                                    break;
                                case 6:
                                    oscilloscope.VoltsPerCm = 50;
                                    break;
                                case 7:
                                    oscilloscope.VoltsPerCm = 100;
                                    break;
                                case 8:
                                    oscilloscope.VoltsPerCm = 200;
                                    break;
                                case 9:
                                    oscilloscope.VoltsPerCm = 500;
                                    break;
                                case 10:
                                    oscilloscope.VoltsPerCm = 1000;
                                    break;
                            }
                            break;
                        case (int)DisplayControls.MILLISECONDS_PER_CM:
                            switch ((int)obj)
                            {
                                case 0:
                                    oscilloscope.MillisecondsPerCm = .01;
                                    break;
                                case 1:
                                    oscilloscope.MillisecondsPerCm = .02;
                                    break;
                                case 2:
                                    oscilloscope.MillisecondsPerCm = .05;
                                    break;
                                case 3:
                                    oscilloscope.MillisecondsPerCm = .1;
                                    break;
                                case 4:
                                    oscilloscope.MillisecondsPerCm = .2;
                                    break;
                                case 5:
                                    oscilloscope.MillisecondsPerCm = .5;
                                    break;
                                case 6:
                                    oscilloscope.MillisecondsPerCm = 1;
                                    break;
                                case 7:
                                    oscilloscope.MillisecondsPerCm = 2;
                                    break;
                                case 8:
                                    oscilloscope.MillisecondsPerCm = 5;
                                    break;
                                case 9:
                                    oscilloscope.MillisecondsPerCm = 10;
                                    break;
                                case 10:
                                    oscilloscope.MillisecondsPerCm = 20;
                                    break;
                                case 11:
                                    oscilloscope.MillisecondsPerCm = 50;
                                    break;
                                case 12:
                                    oscilloscope.MillisecondsPerCm = 100;
                                    break;
                                case 13:
                                    oscilloscope.MillisecondsPerCm = 200;
                                    break;
                                case 14:
                                    oscilloscope.MillisecondsPerCm = 500;
                                    break;
                                case 15:
                                    oscilloscope.MillisecondsPerCm = 1000;
                                    break;
                            }
                            break;
                    }
                    break;
                case _compoundType.CONTROL_PANEL:
                    osc = -1;
                    switch (ctrl.Id)
                    {
                        case (int)ControlPanelControls.LOAD_FROM_FILE:
                            await LoadFromFile();
                            break;
                        case (int)ControlPanelControls.SAVE_TO_FILE:
                            await SaveToFile();
                            break;
                        case (int)ControlPanelControls.SAVE_AS_TO_FILE:
                            await SaveAsToFile();
                            break;
                        case (int)ControlPanelControls.FACTORY_PATCHES:
                            await factoryPatches.ShowAsync();
                            if (!String.IsNullOrEmpty(factoryPatches.PatchName))
                            {
                                await OpenFactoryPatch(factoryPatches.PatchName);
                            }
                            break;
                        case (int)ControlPanelControls.LAYOUT:
                            AllKeysOff();
                            oscId = currentOscillator == null ? Oscillators[0][0].Id : currentOscillator.Id % 12;
                            initDone = false;
                            Layout = (Layouts)obj;
                            Controls.ControlsList.RemoveRange(numberOfFixedControls, Controls.ControlsList.Count - numberOfFixedControls);
                            CreateLayout((Layouts)obj);
                            CreateControls();
                            CreateWiring();
                            oscId = oscId > Patch.OscillatorsInLayout - 1 ? 0 : oscId;
                            currentOscillator = Oscillators[0][oscId];

                            // Make sure all controls has the correct size and position:
                            Controls.ResizeControls(gridControls, Window.Current.Bounds);
                            Controls.SetControlsUniform(gridControls);
                            Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
                            Controls.SetControlsUniform(gridControlPanel);
                            initDone = true;
                            allowGuiUpdates = true;
                            break;
                        case (int)ControlPanelControls.REVERB:
                            if (Reverb.Selection == 0)
                            {
                                FrameServer.TurnOffReverb();
                            }
                            else
                            {
                                FrameServer.TurnOnReverb();
                            }
                            break;
                        case (int)ControlPanelControls.MIDI_SETTINGS:
                            AllKeysOff();
                            await MidiSettings.ShowAsync();
                            for (int osc = 0; osc < 12; osc++)
                            {
                                dispatcher[osc].Clear();
                            }
                            break;
                        case (int)ControlPanelControls.SETTINGS:
                            await Settings.ShowAsync();
                            Patch.SettingsData = new SettingsData(Settings);
                            for (int osc = 0; osc < 12; osc++)
                            {
                                dispatcher[osc].KeyPriority = Settings.KeyPriority;
                            }
                            //if (Settings.MidiDevice != "All midi inputs")
                            //{
                            //    for (int i = 0; i < midiIn.usedPorts.Count; i++)
                            //    {
                            //        if (Settings.MidiDevice == midiIn.portNames[i])
                            //        {
                            //            midiIn.usedPorts[i].Use = true;
                            //        }
                            //        else
                            //        {
                            //            midiIn.usedPorts[i].Use = false;
                            //        }
                            //    }
                            //}
                            //for (int ch = 0; ch < 12; ch++)
                            //{
                            //    for (int cm = 0; cm < 30; cm++)
                            //    {
                            //        Patch.SettingsData.CCMapping[cm][ch] = Settings.CCMapping[cm][ch];
                            //    }
                            //}
                            break;
                        case (int)ControlPanelControls.MANUAL:
                            await ShowManual();
                            break;
                    }
                    break;
            }
            try
            {
                CheckLoad(Oscillators[0][osc]);
            } catch { }
            allowGuiUpdates = true;
        }

        #endregion rotators
        #region helpers

        private void CheckLoad(Oscillator oscillator)
        {
            oscillator.WaveShape.SetWaveShapeUsage(oscillator);

            try
            {
                if (oscillator.Volume > 0 && srcOsc == -1)
                {
                    srcOsc = osc;
                }
                else if (oscillator.Volume == 0 && srcOsc > -1)
                {
                    osc = srcOsc;
                    srcOsc = -1;
                }
                if (osc > -1 && Oscillators[0][osc].Volume > 0)
                {
                    //Oscillators[0][osc].WaveShape.SetCanBeUsed(Oscillators[0][osc]);
                    Oscillators[0][osc].WaveShape.SetWaveShapeUsage(Oscillators[0][osc]);
                    if (Oscillators[0][osc].WaveShape.WaveShapeUsage == WaveShape.Usage.CREATE_ALWAYS)
                    {
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = false;
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING_LIGHT]).IsOn = false;
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING_HEAVY]).IsOn = true;
                    }
                    else if (Oscillators[0][osc].WaveShape.WaveShapeUsage == WaveShape.Usage.CREATE_ONCE)
                    {
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = false;
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING_LIGHT]).IsOn = true;
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING_HEAVY]).IsOn = false;
                    }
                    else
                    {
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = true;
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING_LIGHT]).IsOn = false;
                        ((Indicator)OscillatorGUIs[osc].SubControls.
                            ControlsList[(int)OscillatorControls.LEDSOUNDING_HEAVY]).IsOn = false;
                    }
                }
                else
                {
                    ((Indicator)OscillatorGUIs[osc].SubControls.
                        ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = false;
                    ((Indicator)OscillatorGUIs[osc].SubControls.
                        ControlsList[(int)OscillatorControls.LEDSOUNDING_LIGHT]).IsOn = false;
                    ((Indicator)OscillatorGUIs[osc].SubControls.
                        ControlsList[(int)OscillatorControls.LEDSOUNDING_HEAVY]).IsOn = false;
                }
            }
            catch { }

        }

        //private Oscillator FindOscillatorByModulator(Oscillator oscillator)
        //{
        //    if (oscillator.Volume > 0)
        //    {
        //        return oscillator;
        //    }
        //    else
        //    {
        //        if ()
        //        return FindOscillatorByModulator(ocillator.Modulating )
        //    }
        //}

        #endregion helpers
        #region areabuttons

        public void AreaButtonAction(AreaButton ctrl, Object obj)
        {
            //int connectorId = -1;
            //int wireId = -1;
            switch (selectedCompoundType)
            {
                case _compoundType.OSCILLATOR:
                    osc = oscillatorUnderMouse;
                    selectedOscillator = Oscillators[0][osc];
                    switch (ctrl.Id)
                    {
                        //case (int)OscillatorControls.OUT_SOCKET:
                        //    connectorId = oscillatorUnderMouse;
                        //    Source = new Socket(SocketType.OUT, connectorId);
                        //    if (Wiring.SourceConnected && !Wiring.DestinationConnected)
                        //    {
                        //        Wiring.SourceConnected = false;
                        //        ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                        //    }
                        //    else if (Wiring.DestinationConnected)
                        //    {
                        //        if (MakeConnection())
                        //        {
                        //            for (int poly = 0; poly < 6; poly++)
                        //            {
                        //                selectedOscillator.WaveShape.MakeWave();
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                        //            ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                        //            ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                        //        ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                        //        Wiring.SourceConnected = true;
                        //    }
                        //    break;
                        //case (int)OscillatorControls.AM_SOCKET:
                        //    connectorId = oscillatorUnderMouse;
                        //    wireId = FindConnectedWire(SocketType.AM, connectorId);
                        //    if (wireId > -1)
                        //    {
                        //        for (int poly = 0; poly < 6; poly++)
                        //        {
                        //            Oscillators[poly][oscillatorUnderMouse].AM_Modulator = null;
                        //            Oscillators[poly][oscillatorUnderMouse].FM_Modulator = null;
                        //            Oscillators[poly][oscillatorUnderMouse].XM_Modulator = null;
                        //        }
                        //        Disconnect(wireId, SocketType.AM); // There is already a wire here, disconnect!
                        //    }
                        //    else
                        //    {
                        //        Destination = new Socket(SocketType.AM, connectorId);
                        //        if (Wiring.SourceConnected)
                        //        {
                        //            if (MakeConnection())
                        //            {
                        //                for (int poly = 0; poly < 6; poly++)
                        //                {
                        //                    selectedOscillator.WaveShape.MakeWave();
                        //                }
                        //            }
                        //        }
                        //        else if (Wiring.DestinationConnected)
                        //        {
                        //            ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                        //            Wiring.DestinationConnected = false;
                        //        }
                        //        else if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                        //        {
                        //            ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                        //                ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                        //                ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                        //            ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                        //            Wiring.DestinationConnected = true;
                        //        }
                        //    }
                        //    ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                        //        SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 0;
                        //    for (int poly = 0; poly < 6; poly++)
                        //    {
                        //        Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 0;
                        //    }
                        //    ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                        //        SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = 
                        //        (int)Oscillators[0][oscillatorUnderMouse].AM_Sensitivity;
                        //    break;
                        //case (int)OscillatorControls.FM_SOCKET:
                        //    connectorId = oscillatorUnderMouse;
                        //    wireId = FindConnectedWire(SocketType.FM, connectorId);
                        //    if (wireId > -1)
                        //    {
                        //        for (int poly = 0; poly < 6; poly++)
                        //        {
                        //            Oscillators[poly][oscillatorUnderMouse].AM_Modulator = null;
                        //            Oscillators[poly][oscillatorUnderMouse].FM_Modulator = null;
                        //            Oscillators[poly][oscillatorUnderMouse].XM_Modulator = null;
                        //        }
                        //        Disconnect(wireId, SocketType.FM);
                        //    }
                        //    else
                        //    {
                        //        Destination = new Socket(SocketType.FM, connectorId);
                        //        if (Wiring.SourceConnected)
                        //        {
                        //            if (MakeConnection())
                        //            {
                        //                for (int poly = 0; poly < 6; poly++)
                        //                {
                        //                    selectedOscillator.WaveShape.MakeWave();
                        //                }
                        //            }
                        //        }
                        //        else if (Wiring.DestinationConnected)
                        //        {
                        //            ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                        //            Wiring.DestinationConnected = false;
                        //        }
                        //        else if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                        //        {
                        //            ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                        //                ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                        //                ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                        //            ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                        //            Wiring.DestinationConnected = true;
                        //        }
                        //    }
                        //    ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                        //        SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 1;
                        //    for (int poly = 0; poly < 6; poly++)
                        //    {
                        //        Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 1;
                        //    }
                        //    ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                        //        SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                        //        (int)Oscillators[0][oscillatorUnderMouse].FM_Sensitivity;
                        //    break;
                        //case (int)OscillatorControls.XM_SOCKET:
                        //    connectorId = oscillatorUnderMouse;
                        //    wireId = FindConnectedWire(SocketType.XM, connectorId);
                        //    if (wireId > -1)
                        //    {
                        //        for (int poly = 0; poly < 6; poly++)
                        //        {
                        //            Oscillators[poly][oscillatorUnderMouse].AM_Modulator = null;
                        //            Oscillators[poly][oscillatorUnderMouse].FM_Modulator = null;
                        //            Oscillators[poly][oscillatorUnderMouse].XM_Modulator = null;
                        //        }
                        //        Disconnect(wireId, SocketType.XM);
                        //    }
                        //    else
                        //    {
                        //        Destination = new Socket(SocketType.XM, connectorId);
                        //        if (Wiring.SourceConnected)
                        //        {
                        //            if (MakeConnection())
                        //            {
                        //                for (int poly = 0; poly < 6; poly++)
                        //                {
                        //                    selectedOscillator.WaveShape.MakeWave();
                        //                }
                        //            }
                        //        }
                        //        else if (Wiring.DestinationConnected)
                        //        {
                        //            ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                        //            Wiring.DestinationConnected = false;
                        //        }
                        //        else
                        //        {
                        //            if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                        //            {
                        //                ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                        //                ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                        //                ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                        //                ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                        //                Wiring.DestinationConnected = true;
                        //            }
                        //        }
                        //    }
                        //    ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                        //        SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 2;
                        //    for (int poly = 0; poly < 6; poly++)
                        //    {
                        //        Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 2;
                        //    }
                        //    ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                        //        SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                        //        (int)Oscillators[0][oscillatorUnderMouse].XM_Sensitivity;
                        //    break;
                        case (int)OscillatorControls.VIEW_ME:
                            selectedOscillator = currentOscillator;
                            allowGuiUpdates = true;
                            break;

                    }
                    break;
                case _compoundType.WIRE:
                    break;
                case _compoundType.OTHER:
                    break;
            }
            allowGuiUpdates = true;
        }

        #endregion areabuttons
        #region graphs

        public void GraphAction(Graph ctrl, Object obj)
        {
            osc = PitchEnvelopeToOscillator(pitchEnvelopeUnderMouse);
            int id = ((PointerData)obj).Id;
            int mouseButton = ((PointerData)obj).ButtonPressed;
            Point position = ((PointerData)obj).Position;
            Graph graph = (Graph)PitchEnvelopeGUIs[id].SubControls.ControlsList[(int)PitchEnvelopeControls.GRAPH];
            switch (selectedCompoundType)
            {
                case _compoundType.PITCH_ENVELOPE:
                    double widthFactor = (graph.ImageList[0].ActualWidth) / 271;
                    double heightFactor = (graph.ImageList[0].ActualHeight) / 173;
                    position.X /= widthFactor;
                    position.Y /= heightFactor;
                    if (mouseButton == 0)
                    {
                        if (graph.Points.Count == 0)
                        {
                            position.X = 0;
                            graph.AddPoint(new Point(position.X, position.Y));
                            //graph.AddPoint(new Point(269 * widthFactor, 86 * heightFactor));
                            graph.AddPoint(new Point(269, 86));
                        }
                        else
                        {
                            bool rightmost = true;
                            for (int i = 0; i < graph.Points.Count; i++)
                            {
                                if (graph.Points[i].X > position.X)
                                {
                                    rightmost = false;
                                }
                            }
                            if (rightmost)
                            {
                                graph.AddPoint(new Point(269, position.Y));
                            }
                            else
                            {
                                graph.AddPoint(new Point(position.X, position.Y));
                            }
                        }
                    }
                    else if (mouseButton == 1)
                    {
                        graph.RemovePoint(position);
                        if (graph.Points.Count == 2)
                        {
                            graph.Points[0] = new Point(0, graph.Points[0].Y);
                            graph.Points[1] = new Point(269 * widthFactor, graph.Points[1].Y);
                        }
                        else if (graph.Points.Count < 2)
                        {
                            graph.Points.Clear();
                        }
                    }

                    graph.SortByX();
                    for (int poly = 0; poly < 6; poly++)
                    {
                        Oscillators[poly][osc].PitchEnvelope.CopyPoints(graph.Points);
                    }
                    graph.Draw();
                    break;
            }
            allowGuiUpdates = true;
        }

        #endregion graphs
    }
}
