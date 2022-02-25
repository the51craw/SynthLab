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

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        #region properties
        /// <summary>
        /// This is the oscillator automatically selected when mouse pointer
        /// is moving over a control in the GUI for the oscillator.
        /// It is valid when using a sub control in this control.
        /// It is set to Patch.Oscillators[0][0] at startup.
        /// </summary>
        public Oscillator currentOscillator;

        /// <summary>
        /// This is the latest selected oscillator. It was selected by clicking
        /// the View button of the interface or by changing a setting in a compound control that belongs to the oscillator.
        /// It is set to Patch.Oscillators[0][0] at startup and when changing layout.
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
        private Boolean select;
        int ch = 0;

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
                    else if (obj.GetType() == typeof(Keyboard))
                    {
                        KeyboardAction((Keyboard)obj, value);
                    }
                    else if (obj.GetType() == typeof(AreaButton))
                    {
                        AreaButtonAction((AreaButton)obj, value);
                    }
                    else if (obj.GetType() == typeof(Graph))
                    {
                        GraphAction((Graph)obj, value);
                    }
                    else if (obj.GetType() == typeof(StaticImage))
                    {
                        StaticImageAction((StaticImage)obj, value);
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
            }
        }

        #endregion main
        #region knobs

        public async Task KnobAction(Knob knob, Object obj)
        {
            switch (selectedCompoundType)
            {
                case _compoundType.OSCILLATOR:
                    ch = oscillatorUnderMouse;
                    switch (knob.Id)
                    {
                        case (int)OscillatorControls.MODULATION:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Oscillator oscillator = Patch.Oscillators[poly][ch];

                                switch (((Rotator)(OscillatorGUIs[ch].
                                    SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET])).Selection)
                                {
                                    case 0:
                                        oscillator.AM_Sensitivity = (float)knob.Value;
                                        break;
                                    case 1:
                                        oscillator.FM_Sensitivity = (float)knob.Value;
                                        if (oscillator.WaveForm == WAVEFORM.SINE && oscillator.FM_Modulator != null
                                            && oscillator.FM_Modulator.WaveForm == WAVEFORM.SINE)
                                        {
                                            CreateWaveform(oscillator);
                                        }
                                        break;
                                    case 2:
                                        oscillator.XM_Sensitivity = (float)knob.Value;

                                        // XM_Sensitivity for a squarewave should not be allowed to be zero in order to 
                                        // match the ration between the high and low to be the same as when PM_Sensitivity
                                        // is 127 (but, of course, inverted):
                                        if (oscillator.XM_Sensitivity == 0 && oscillator.WaveForm == WAVEFORM.SQUARE)
                                        {
                                            oscillator.XM_Sensitivity = 1;
                                        }
                                        if (oscillator.WaveForm == WAVEFORM.SQUARE)
                                        {
                                            Patch.Oscillators[poly][oscillatorUnderMouse].WaveShape.Phase = Math.PI * (1 + (knob.Value - 128) / 138f);
                                            Patch.Oscillators[poly][oscillatorUnderMouse].WaveShape.MakeWave();
                                        }
                                        break;
                                }
                            }
                            break;
                        case (int)OscillatorControls.FREQUENCY:
                            selectedOscillator = Patch.Oscillators[0][ch];
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                if (currentOscillator.Keyboard)
                                {
                                    Patch.Oscillators[poly][currentOscillator.Id].Frequency = knob.Value;
                                }
                                else
                                {
                                    Patch.Oscillators[poly][currentOscillator.Id].FrequencyLfo = knob.Value;
                                }

                                if (Patch.Oscillators[poly][currentOscillator.Id].Key != 0xff)
                                {
                                    Patch.Oscillators[poly][currentOscillator.Id].
                                        SetKeyboardAdjustedFrequency(Patch.Oscillators[poly][currentOscillator.Id].Key);
                                }

                                Patch.Oscillators[poly][currentOscillator.Id].Frequency = knob.Value * 10;
                                Patch.Oscillators[poly][currentOscillator.Id].FrequencyLfo = (float)Math.Truncate((float)knob.Value / 10);
                                Patch.Oscillators[poly][currentOscillator.Id].SetStepSize();
                            }
                            break;
                        case (int)OscillatorControls.FINE_TUNE:
                            selectedOscillator = Patch.Oscillators[0][ch];
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                if (currentOscillator.Keyboard)
                                {
                                    Patch.Oscillators[poly][currentOscillator.Id].FineTune = (float)knob.Value;
                                }
                                else
                                {
                                    Patch.Oscillators[poly][currentOscillator.Id].FineTuneLfo = (float)knob.Value;
                                }

                                if (Patch.Oscillators[poly][currentOscillator.Id].Key != 0xff)
                                {
                                    Patch.Oscillators[poly][currentOscillator.Id].
                                        SetKeyboardAdjustedFrequency(Patch.Oscillators[poly][currentOscillator.Id].Key);
                                }
                                Patch.Oscillators[poly][currentOscillator.Id].SetStepSize();
                            }
                            break;
                        case (int)OscillatorControls.VOLUME:
                            selectedOscillator = Patch.Oscillators[0][ch];
                            if (knob.Value == 0)
                            {
                                select = false;
                            }
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch].Volume = (byte)knob.Value;
                            }
                            try
                            {
                                ((Indicator)OscillatorGUIs[ch].
                                    SubControls.ControlsList[(int)OscillatorControls.LEDSOUNDING]).IsOn = 
                                    Patch.Oscillators[0][ch].Volume > 0 ? true : false;
                            }
                            catch (Exception exception)
                            {
                                ContentDialog error = new Error(exception.Message);
                                _ = error.ShowAsync();
                            }
                            break;
                    }
                    break;
                case _compoundType.FILTER:
                    ch = FilterToOscillator(filterUnderMouse);
                    selectedOscillator = Patch.Oscillators[0][ch];
                    ((Knob)((CompoundControl)FilterGUIs[filterUnderMouse]).
                    SubControls.ControlsList[knob.Id]).Value = (int)obj;
                    // Don't change parameters in the middle of generating wave shape:
                    while (CurrentActivity == CURRENTACTIVITY.GENERATING_WAVE_SHAPE)
                    {
                        await Task.Delay(1);
                    }
                    CurrentActivity = CURRENTACTIVITY.CHANGING_PARAMETERS;
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        switch (knob.Id)
                        {
                            case (int)FilterControls.Q:
                                Patch.Oscillators[poly][ch].Filter.Q = (byte)(int)obj;
                                break;
                            case (int)FilterControls.FREQUENCY_CENTER:
                                Patch.Oscillators[poly][ch].Filter.FrequencyCenter = (byte)(int)obj;
                                break;
                            case (int)FilterControls.KEYBOARD_FOLLOW:
                                Patch.Oscillators[poly][ch].Filter.KeyboardFollow = (byte)(int)obj;
                                break;
                            case (int)FilterControls.GAIN:
                                Patch.Oscillators[poly][ch].Filter.Gain = (byte)(int)obj;
                                break;
                            case (int)FilterControls.FILTER_MIX:
                                Patch.Oscillators[poly][ch].Filter.Mix = (byte)(int)obj;
                                break;
                        }
                        if (Patch.Oscillators[poly][ch].Filter.FilterFunction > 0)
                        {
                            Patch.Oscillators[poly][ch].WaveShape.ApplyFilter(Patch.Oscillators[poly][ch].Key);
                        }
                    }
                    CurrentActivity = CURRENTACTIVITY.NONE;
                    break;
                case _compoundType.PITCH_ENVELOPE:
                    ch = PitchEnvelopeToOscillator(pitchEnvelopeUnderMouse);
                    selectedOscillator = Patch.Oscillators[0][ch];
                    switch (knob.Id)
                    {
                        case (int)PitchEnvelopeControls.DEPTH:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch]
                                    .PitchEnvelope.Depth = (int)obj;
                            }
                            break;
                        case (int)PitchEnvelopeControls.SPEED:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch]
                                    .PitchEnvelope.Speed = (int)obj;
                            }
                            break;
                    }
                    break;
                case _compoundType.ADSR:
                    ch = AdsrToOscillator(adsrUnderMouse);
                    selectedOscillator = Patch.Oscillators[0][ch];
                    switch (knob.Id)
                    {
                        case ((int)AdsrControls.ADSR_A):
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch].Adsr.AdsrAttackTime = (byte)(int)obj;
                            }
                            break;
                        case (int)AdsrControls.ADSR_D:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch].Adsr.AdsrDecayTime = (byte)(int)obj;
                            }
                            break;
                        case (int)AdsrControls.ADSR_S:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch].Adsr.AdsrSustainLevel = (byte)(int)obj;
                            }
                            break;
                        case (int)AdsrControls.ADSR_R:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch].Adsr.AdsrReleaseTime = (byte)(int)obj;
                            }
                            break;
                    }
                    //if (Patch.AdsrTypeSelection == 1)
                    //{
                    //    FollowCommonAdsrGui(knob);
                    //}
                    break;
            }
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
            CurrentActivity = CURRENTACTIVITY.CHANGING_PARAMETERS;
            FollowModulationWheel((int)obj);
            CurrentActivity = CURRENTACTIVITY.NONE;
        }

        #endregion sliders
        #region rotators

        public async void RotatorAction(Rotator ctrl, Object obj)
        {
            int oscId = 0;
            switch (selectedCompoundType)
            {
                case _compoundType.OSCILLATOR:
                    ch = oscillatorUnderMouse;
                    if (ch < 0)
                    {
                        break;
                    }
                    selectedOscillator = Patch.Oscillators[0][ch];
                    select = true;
                    switch (ctrl.Id)
                    {
                        case (int)OscillatorControls.WAVE:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].WaveForm = (WAVEFORM)ctrl.Selection;
                                Patch.Oscillators[poly][oscillatorUnderMouse].WaveShape.Waveform = (WAVEFORM)ctrl.Selection;
                                CreateWaveform(Patch.Oscillators[poly][oscillatorUnderMouse]);
                                //if (Patch.Filter[poly][osc].FilterFunction > 1)
                                //{
                                //    Patch.Oscillators[poly][oscillatorUnderMouse].WaveShape.ApplyFilter(69);
                                //}
                            }
                            break;
                        case (int)OscillatorControls.KEYBOARD_OR_FIXED:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].Keyboard = (Double)ctrl.Selection == 0;
                                Patch.Oscillators[poly][oscillatorUnderMouse].SetFrequency();
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
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = ((Rotator)ctrl).Selection;
                            }
                            switch (Patch.Oscillators[0][oscillatorUnderMouse].ModulationKnobTarget)
                            {
                                case 0:
                                    ((Knob)(OscillatorGUIs[oscillatorUnderMouse].
                                        SubControls.ControlsList[(int)OscillatorControls.MODULATION])).Value =
                                        (int)Patch.Oscillators[0][oscillatorUnderMouse].AM_Sensitivity;
                                    break;
                                case 1:
                                    ((Knob)(OscillatorGUIs[oscillatorUnderMouse].
                                        SubControls.ControlsList[(int)OscillatorControls.MODULATION])).Value =
                                        (int)Patch.Oscillators[0][oscillatorUnderMouse].FM_Sensitivity;
                                    break;
                                case 2:
                                    ((Knob)(OscillatorGUIs[oscillatorUnderMouse].
                                        SubControls.ControlsList[(int)OscillatorControls.MODULATION])).Value =
                                        (int)Patch.Oscillators[0][oscillatorUnderMouse].XM_Sensitivity;
                                    break;
                            }
                            break;
                        case (int)OscillatorControls.MODULATION_WHEEL_TARGET:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].ModulationWheelTarget = ((Rotator)ctrl).Selection;
                            }
                            break;
                        case (int)OscillatorControls.ADSR_OR_PULSE:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].Adsr.Pulse = (ctrl.Selection % 2) == 1;
                            }
                            break;
                        case (int)OscillatorControls.VIEW_ME:
                            allowGuiUpdates = true;
                            break;
                    }
                    break;
                case _compoundType.FILTER:
                    ch = FilterToOscillator(filterUnderMouse);
                    selectedOscillator = Patch.Oscillators[0][ch];
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        switch (ctrl.Id)
                        {
                            case (int)FilterControls.FILTER_FUNCTION:
                                Patch.Oscillators[poly][ch].Filter.FilterFunction = (int)obj;
                                CreateWaveform(Patch.Oscillators[poly][ch]);
                                break;
                            case (int)FilterControls.MODULATION_WHEEL_TARGET:
                                Patch.Oscillators[poly][ch].Filter.ModulationWheelTarget = (int)obj;
                                break;
                        }
                    }
                    break;
                case _compoundType.PITCH_ENVELOPE:
                    ch = PitchEnvelopeToOscillator(pitchEnvelopeUnderMouse);
                    selectedOscillator = Patch.Oscillators[0][ch];

                    for (int poly = 0; poly < Patch.OscillatorCount; poly++)
                    {
                        switch (ctrl.Id)
                        {
                            case (int)PitchEnvelopeControls.PITCH_ENV_MODULATION_WHEEL_USE:
                                Patch.Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvModulationWheelTarget = (int)obj; ;
                                break;
                            case (int)PitchEnvelopeControls.MOD_PITCH:
                                Patch.Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvPitch = (int)obj > 0;
                                break;
                            case (int)PitchEnvelopeControls.MOD_AM:
                                Patch.Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvAm = (int)obj > 0;
                                break;
                            case (int)PitchEnvelopeControls.MOD_FM:
                                Patch.Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvFm = (int)obj > 0;
                                break;
                            case (int)PitchEnvelopeControls.MOD_XM:
                                Patch.Oscillators[poly][pitchEnvelopeUnderMouse].PitchEnvelope.PitchEnvXm = (int)obj > 0;
                                break;
                        }
                    }
                    break;
                case _compoundType.ADSR:
                    ch = AdsrToOscillator(adsrUnderMouse);
                    selectedOscillator = Patch.Oscillators[0][ch];
                    switch (ctrl.Id)
                    {
                        case (int)AdsrControls.ADSR_MODULATION_WHEEL_USE:
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][ch].Adsr.AdsrModulationWheelTarget = (int)obj;
                            }
                            break;
                        case (int)AdsrControls.PEDAL_HOLD:
                            dispatcher[selectedOscillator.MidiChannel].PedalHold = ((Rotator)ctrl).Selection > 0;
                            foreach (CompoundControl compoundControl in AdsrGUIs)
                            {
                                ((Rotator)compoundControl.SubControls.ControlsList[(int)AdsrControls.PEDAL_HOLD]).Selection = 
                                    dispatcher[selectedOscillator.MidiChannel].PedalHold ? 1 : 0;
                            }
                            break;
                    }
                    break;
                case _compoundType.CONTROL_PANEL:
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
                            oscId = currentOscillator == null ? Patch.Oscillators[0][0].Id : currentOscillator.Id % Patch.OscillatorCount;
                            initDone = false;
                            Patch.Layout = (Layouts)obj;
                            Controls.ControlsList.RemoveRange(numberOfFixedControls, Controls.ControlsList.Count - numberOfFixedControls);
                            CreateLayout((Layouts)obj);
                            CreateControls();
                            CreateWiring();
                            oscId = oscId > Patch.OscillatorCount - 1 ? 0 : oscId;
                            currentOscillator = Patch.Oscillators[0][oscId];

                            // Make sure all controls has the correct size and position:
                            Controls.ResizeControls(gridControls, Window.Current.Bounds);
                            Controls.SetControlsUniform(gridControls);
                            Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
                            Controls.SetControlsUniform(gridControlPanel);
                            initDone = true;
                            allowGuiUpdates = true;
                            break;
                        //case (int)ControlPanelControls.USING_GRAPHICS_CARD:
                        //    if (graphicsCardAvailable)
                        //    {
                        //        usingGraphicsCard = ((Rotator)ctrl).Selection == 1;
                        //    }
                        //    else
                        //    {
                        //        ((Rotator)ctrl).Selection = 0;
                        //    }
                        //    break;
                        case (int)ControlPanelControls.MIDI_SETTINGS:
                            AllKeysOff();
                            await midiSettings.ShowAsync();
                            for (int osc = 0; osc < 17; osc++)
                            {
                                dispatcher[osc].Clear();
                            }

                            //switch (Patch.Layout)
                            //{
                            //    case Layouts.FOUR_OSCILLATORS:
                            //        MidiSettings4 midiSettings4 = new MidiSettings4();
                            //        midiSettings4.Setup(Patch);
                            //        await midiSettings4.ShowAsync();
                            //        break;
                            //    case Layouts.SIX_OSCILLATORS:
                            //        MidiSettings6 midiSettings6 = new MidiSettings6();
                            //        midiSettings6.Setup(Patch);
                            //        await midiSettings6.ShowAsync();
                            //        break;
                            //    case Layouts.EIGHT_OSCILLATORS:
                            //        MidiSettings8 midiSettings8 = new MidiSettings8();
                            //        midiSettings8.Setup(Patch);
                            //        await midiSettings8.ShowAsync();
                            //        break;
                            //    case Layouts.TWELVE_OSCILLATORS:
                            //        MidiSettings midiSettings = new MidiSettings(this);
                            //        midiSettings.Setup(Patch);
                            //        await midiSettings.ShowAsync();
                            //        break;
                            //}
                            break;
                        case (int)ControlPanelControls.SETTINGS:
                            //settings.Polyphony = Patch.Polyphony;
                            await settings.ShowAsync();
                            for (int ch = 0; ch < 16; ch++)
                            {
                                dispatcher[ch].KeyPriority = settings.KeyPriority;
                            }
                            //if (settings.Polyphony != Patch.Polyphony)
                            //{
                            //    ChangePolyphony(settings.Polyphony);
                            //}
                            break;
                        case (int)ControlPanelControls.MANUAL:
                            await ShowManual();
                            break;
                    }
                    break;
            }
            allowGuiUpdates = true;
        }

        public void KeyboardAction(Keyboard keyboard, Object obj)
        {

        }

        #endregion rotators
        #region areabuttons

        public void AreaButtonAction(AreaButton ctrl, Object obj)
        {
            int connectorId = -1;
            int wireId = -1;
            switch (selectedCompoundType)
            {
                case _compoundType.OSCILLATOR:
                    ch = oscillatorUnderMouse;
                    selectedOscillator = Patch.Oscillators[0][ch];
                    switch (ctrl.Id)
                    {
                        case (int)OscillatorControls.OUT_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            Source = new Socket(SocketType.OUT, connectorId);
                            if (Patch.Wiring.SourceConnected && !Patch.Wiring.DestinationConnected)
                            {
                                Patch.Wiring.SourceConnected = false;
                                ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                            }
                            else if (Patch.Wiring.DestinationConnected)
                            {
                                if (MakeConnection())
                                {
                                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                                    {
                                        CreateWaveform(Patch.Oscillators[poly][oscillatorUnderMouse]);
                                    }
                                }
                            }
                            else
                            {
                                ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                    ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                                    ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                                ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                Patch.Wiring.SourceConnected = true;
                            }
                            break;
                        case (int)OscillatorControls.AM_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            wireId = FindConnectedWire(SocketType.AM, connectorId);
                            if (wireId > -1)
                            {
                                for (int poly = 0; poly < Patch.Polyphony; poly++)
                                {
                                    Patch.Oscillators[poly][oscillatorUnderMouse].ModulationType = ModulationType.NORMAL;
                                }
                                Disconnect(wireId, SocketType.AM); // There is already a wire here, disconnect!
                            }
                            else
                            {
                                Destination = new Socket(SocketType.AM, connectorId);
                                if (Patch.Wiring.SourceConnected)
                                {
                                    if (MakeConnection())
                                    {
                                        for (int poly = 0; poly < Patch.Polyphony; poly++)
                                        {
                                            CreateWaveform(Patch.Oscillators[poly][oscillatorUnderMouse].AM_Modulator);
                                        }
                                    }
                                }
                                else if (Patch.Wiring.DestinationConnected)
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                                    Patch.Wiring.DestinationConnected = false;
                                }
                                else if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                        ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                                        ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                    Patch.Wiring.DestinationConnected = true;
                                }
                            }
                            ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 0;
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 0;
                            }
                            ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = 
                                (int)Patch.Oscillators[0][oscillatorUnderMouse].AM_Sensitivity;
                            break;
                        case (int)OscillatorControls.FM_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            wireId = FindConnectedWire(SocketType.FM, connectorId);
                            if (wireId > -1)
                            {
                                for (int poly = 0; poly < Patch.Polyphony; poly++)
                                {
                                    Patch.Oscillators[poly][oscillatorUnderMouse].ModulationType = ModulationType.NORMAL;
                                }
                                Disconnect(wireId, SocketType.FM);
                            }
                            else
                            {
                                Destination = new Socket(SocketType.FM, connectorId);
                                if (Patch.Wiring.SourceConnected)
                                {
                                    if (MakeConnection())
                                    {
                                        for (int poly = 0; poly < Patch.Polyphony; poly++)
                                        {
                                            CreateWaveform(Patch.Oscillators[poly][oscillatorUnderMouse].FM_Modulator);
                                        }
                                    }
                                }
                                else if (Patch.Wiring.DestinationConnected)
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                                    Patch.Wiring.DestinationConnected = false;
                                }
                                else if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                        ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                                        ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                    Patch.Wiring.DestinationConnected = true;
                                }
                            }
                            ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 1;
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 1;
                            }
                            ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                                (int)Patch.Oscillators[0][oscillatorUnderMouse].FM_Sensitivity;
                            break;
                        case (int)OscillatorControls.XM_SOCKET:
                            connectorId = oscillatorUnderMouse;
                            wireId = FindConnectedWire(SocketType.XM, connectorId);
                            if (wireId > -1)
                            {
                                for (int poly = 0; poly < Patch.Polyphony; poly++)
                                {
                                    Patch.Oscillators[poly][oscillatorUnderMouse].ModulationType = ModulationType.NORMAL;
                                }
                                Disconnect(wireId, SocketType.XM);
                            }
                            else
                            {
                                Destination = new Socket(SocketType.XM, connectorId);
                                if (Patch.Wiring.SourceConnected)
                                {
                                    if (MakeConnection())
                                    {
                                        for (int poly = 0; poly < Patch.Polyphony; poly++)
                                        {
                                            CreateWaveform(Patch.Oscillators[poly][oscillatorUnderMouse].XM_Modulator);
                                        }
                                    }
                                }
                                else if (Patch.Wiring.DestinationConnected)
                                {
                                    ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                                    Patch.Wiring.DestinationConnected = false;
                                }
                                else
                                {
                                    if (oscillatorsWithInputs1D0L.Contains(oscillatorUnderMouse))
                                    {
                                        ((Indicator)Controls.ControlsList[HangingWire]).ControlSizing.SetPosition(new Point(
                                        ((AreaButton)ctrl).HitArea.Left + ((AreaButton)ctrl).HitArea.Width - 27,
                                        ((AreaButton)ctrl).HitArea.Top + ((AreaButton)ctrl).HitArea.Height - 26));
                                        ((Indicator)Controls.ControlsList[HangingWire]).IsOn = true;
                                        Patch.Wiring.DestinationConnected = true;
                                    }
                                }
                            }
                            ((Rotator)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 2;
                            for (int poly = 0; poly < Patch.Polyphony; poly++)
                            {
                                Patch.Oscillators[poly][oscillatorUnderMouse].ModulationKnobTarget = 2;
                            }
                            ((Knob)OscillatorGUIs[oscillatorUnderMouse].
                                SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value =
                                (int)Patch.Oscillators[0][oscillatorUnderMouse].XM_Sensitivity;
                            break;
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
            ch = PitchEnvelopeToOscillator(pitchEnvelopeUnderMouse);
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
                            graph.AddPoint(new Point(269 * widthFactor, 86 * heightFactor));
                        }
                        else
                        {
                            graph.AddPoint(new Point(position.X, position.Y));
                        }
                    }
                    else if (mouseButton == 1)
                    {
                        graph.RemovePoint(position);
                        if (graph.Points.Count < 2)
                        {
                            graph.Points.Clear();
                        }
                    }

                    graph.SortByX();
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        Patch.Oscillators[poly][ch].PitchEnvelope.CopyPoints(graph.Points);
                    }
                    graph.Draw();
                    break;
            }
            allowGuiUpdates = true;
        }

        #endregion graphs
        #region functions

        public void StaticImageAction(StaticImage ctrl, Object obj)
        {

        }

        #endregion functions
    }
}
