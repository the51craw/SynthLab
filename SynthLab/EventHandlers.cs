using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        // Globally accessed modifiers:
        Boolean LeftButtonPressed = false;
        Boolean RightButtonPressed = false;
        int oscillatorUnderMouse = -1;
        int filterUnderMouse = -1;
        int adsrUnderMouse = -1;
        int pitchEnvelopeUnderMouse = -1;
        int displayUnderMouse = -1;
        IMidiMessage[] midiInMessageQueue;
        int midiInWriteIndex = 0;
        int midiInReadIndex = 0;

        // When the pointer is moved over the click-area, ask the Controls
        // object if and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            SetModifiers(e);
           
            if (initDone && Controls != null)
            {
                currentControl = -1;
                //Debug.WriteLine("EventHAndlers.cs 39");
                Controls.PointerMoved(sender, e);
                selectedCompoundType = _compoundType.NONE;
                oscillatorUnderMouse = -1;
                foreach (CompoundControl oscillator in OscillatorGUIs)
                {
                    if (oscillator.IsSelected)
                    {
                        selectedCompoundType = _compoundType.OSCILLATOR;
                        oscillatorUnderMouse = oscillator.Id;
                        currentOscillator = Oscillators[0][oscillator.Id];
                        foreach (object subControl in oscillator.SubControls.ControlsList)
                        {
                            if (((ControlBase)subControl).IsSelected)
                            {
                                currentControl = ((ControlBase)subControl).Id;
                                if (LeftButtonPressed || RightButtonPressed)
                                {
                                    Action(subControl);
                                }
                            }
                        }
                    }
                }
                filterUnderMouse = -1;
                foreach (CompoundControl filter in FilterGUIs)
                {
                    if (filter.IsSelected)
                    {
                        selectedCompoundType = _compoundType.FILTER;
                        filterUnderMouse = filter.Id;
                        foreach (object subControl in filter.SubControls.ControlsList)
                        {
                            if (((ControlBase)subControl).IsSelected)
                            {
                                currentControl = ((ControlBase)subControl).Id;
                                if (LeftButtonPressed || RightButtonPressed)
                                {
                                    Action(subControl);
                                }
                            }
                        }
                    }
                }
                adsrUnderMouse = -1;
                foreach (CompoundControl adsr in AdsrGUIs)
                {
                    if (adsr.IsSelected)
                    {
                        selectedCompoundType = _compoundType.ADSR;
                        adsrUnderMouse = adsr.Id;
                        foreach (object subControl in adsr.SubControls.ControlsList)
                        {
                            if (((ControlBase)subControl).IsSelected)
                            {
                                currentControl = ((ControlBase)subControl).Id;
                                if (LeftButtonPressed || RightButtonPressed)
                                {
                                    Action(subControl);
                                }
                            }
                        }
                    }
                }
                pitchEnvelopeUnderMouse = -1;
                foreach (CompoundControl pitchEnvelope in PitchEnvelopeGUIs)
                {
                    if (pitchEnvelope.IsSelected)
                    {
                        selectedCompoundType = _compoundType.PITCH_ENVELOPE;
                        pitchEnvelopeUnderMouse = pitchEnvelope.Id;
                        foreach (object subControl in pitchEnvelope.SubControls.ControlsList)
                        {
                            if (((ControlBase)subControl).IsSelected)
                            {
                                currentControl = ((ControlBase)subControl).Id;
                                if (LeftButtonPressed || RightButtonPressed)
                                {
                                    Action(subControl); // TODO: Implement move point in Action!
                                }
                            }
                        }
                    }
                }
                displayUnderMouse = -1;
                if (DisplayGUI.IsSelected)
                {
                    selectedCompoundType = _compoundType.DISPLAY;
                    displayUnderMouse = DisplayGUI.Id;
                    foreach (object subControl in DisplayGUI.SubControls.ControlsList)
                    {
                        if (((ControlBase)subControl).IsSelected)
                        {
                            currentControl = ((ControlBase)subControl).Id;
                            if (LeftButtonPressed || RightButtonPressed)
                            {
                                Action(subControl);
                            }
                        }
                    }
                }
                if (ControlPanel.IsSelected)
                {
                    selectedCompoundType = _compoundType.CONTROL_PANEL;
                    foreach (object subControl in ControlPanel.SubControls.ControlsList)
                    {
                        if (((ControlBase)subControl).IsSelected)
                        {
                            currentControl = ((ControlBase)subControl).Id;
                            Object ctrl = Controls.ControlsList[currentControl];
                            if (subControl.GetType() == typeof(VerticalSlider))
                            {
                                var obj = ((VerticalSlider)subControl).Value;
                                Action(subControl, obj);
                            }
                        }
                    }
                }

                if (selectedCompoundType == _compoundType.NONE)
                {
                    Object obj = null;
                    PointerPoint currentMousePoint = e.GetCurrentPoint(imgClickArea);
                    Double x = currentMousePoint.Position.X;
                    Double y = currentMousePoint.Position.Y;

                    //Debug.WriteLine("EventHAndlers.cs 159");
                    currentControl = Controls.FindControl(currentMousePoint.Position);
                    if (currentControl > -1)
                    {
                        Object tag = GetTag(currentControl);
                        currentOscillator = GetOscillatorByControlId(currentControl) == null ?
                            currentOscillator : GetOscillatorByControlId(currentControl);
                        PointerPointProperties props = currentMousePoint.Properties;
                        Object ctrl = Controls.ControlsList[currentControl];
                        if (ctrl != null && props.IsLeftButtonPressed)
                        {
                            try
                            {
                                // This fails if mouse pointer is pressed on keyboard and
                                // moved outside keyboard and released over imgClickArea instead.
                                obj = ((ControlBase)ctrl).Moved(new Point(x, y));
                                Action(ctrl, obj);
                            } catch { }
                        }
                    }
                }
            }
        }

        private void imgClickArea_Tapped(object sender, TappedRoutedEventArgs e)
        {
        }

        private void imgClickArea_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SetModifiers(e);
            if (Controls != null)
            {
                //Debug.WriteLine("EventHAndlers.cs 187");
                Controls.PointerPressed(currentControl, e);

                // Calls to Rotator controls Tapped() are added in order to make toggle happen on mouse down for immediate response.

                if (currentControl > -1)
                {
                    if (oscillatorUnderMouse > -1 && currentControl != (int)OscillatorControls.NUMBER)
                    {
                        OscillatorGUIs[oscillatorUnderMouse].SubControls.PointerPressed(
                            OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl], e);
                        if (OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl].GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed)
                            {
                                ((Rotator)OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl]).Tapped();
                                Action(OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                            else if (RightButtonPressed)
                            {
                                ((Rotator)OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl]).RightTapped();
                                Action(OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                        }
                        else if (OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl].GetType() == typeof(AreaButton))
                        {
                            if (LeftButtonPressed || RightButtonPressed)
                            {
                                Action(OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                        }
                    }
                    else if (filterUnderMouse > -1)
                    {
                        FilterGUIs[filterUnderMouse].SubControls.PointerPressed(
                            FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl], e);
                        if (FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl].GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed)
                            {
                                ((Rotator)FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl]).Tapped();
                                Action(FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                            else if (RightButtonPressed)
                            {
                                ((Rotator)FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl]).RightTapped();
                                Action(FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                        }
                    }
                    else if (pitchEnvelopeUnderMouse > -1)
                    {
                        PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.PointerPressed(
                            PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl], e);
                        if (PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl].GetType() == typeof(Graph))
                        {
                            if (LeftButtonPressed || RightButtonPressed)
                            {
                                Point position = e.GetCurrentPoint(((Graph)((CompoundControl)PitchEnvelopeGUIs[pitchEnvelopeUnderMouse]).
                                    SubControls.ControlsList[(int)PitchEnvelopeControls.GRAPH]).ImageList[0]).Position;
                                Action(PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl],
                                    new PointerData(LeftButtonPressed, RightButtonPressed, position, pitchEnvelopeUnderMouse));
                            }
                        }
                        else if (PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl].GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed)
                            {
                                ((Rotator)PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl]).Tapped();
                                Action(PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                            else if (RightButtonPressed)
                            {
                                ((Rotator)PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl]).RightTapped();
                                Action(PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                        }

                    }
                    else if (adsrUnderMouse > -1)
                    {
                        AdsrGUIs[adsrUnderMouse].SubControls.PointerPressed(
                            AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl], e);
                        if (AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl].GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed)
                            {
                                ((Rotator)AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl]).Tapped();
                                Action(AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                            else if (RightButtonPressed)
                            {
                                ((Rotator)AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl]).RightTapped();
                                Action(AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl]);
                            }
                        }
                    }
                    else if (displayUnderMouse > -1)
                    {
                        DisplayGUI.SubControls.PointerPressed(
                            DisplayGUI.SubControls.ControlsList[currentControl], e);
                        if (DisplayGUI.SubControls.ControlsList[currentControl].GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed)
                            {
                                ((Rotator)DisplayGUI.SubControls.ControlsList[currentControl]).Tapped();
                                Action(DisplayGUI.SubControls.ControlsList[currentControl]);
                            }
                            else if (RightButtonPressed)
                            {
                                ((Rotator)DisplayGUI.SubControls.ControlsList[currentControl]).RightTapped();
                                Action(DisplayGUI.SubControls.ControlsList[currentControl]);
                            }
                        }
                    }
                    else if (ControlPanel.IsSelected)
                    {
                        ControlPanel.SubControls.PointerPressed(
                            ControlPanel.SubControls.ControlsList[currentControl], e);
                        if (ControlPanel.SubControls.ControlsList[currentControl].GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed)
                            {
                                ((Rotator)ControlPanel.SubControls.ControlsList[currentControl]).Tapped();
                                Action(ControlPanel.SubControls.ControlsList[currentControl]);
                            }
                            else if (RightButtonPressed)
                            {
                                ((Rotator)ControlPanel.SubControls.ControlsList[currentControl]).RightTapped();
                                Action(ControlPanel.SubControls.ControlsList[currentControl]);
                            }
                        }
                    }
                    else
                    {
                        Object obj;
                        Object ctrl = Controls.ControlsList[currentControl];
                        if (ctrl.GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed)
                            {
                                ((Rotator)ctrl).Tapped();
                            }
                            else if (RightButtonPressed)
                            {
                                ((Rotator)ctrl).RightTapped();
                            }
                            obj = ((Rotator)ctrl).Selection;
                            Action(ctrl, obj);
                        }
                    }
                }
            }
        }

        private void imgClickArea_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            SetModifiers(e);
            if (keyboard.IsSelected)
            {
                //Debug.WriteLine("EventHAndlers.cs 347");
                try
                {
                    Controls.PointerReleased(currentControl, e);
                }
                catch { }
            }
            pitchBenderReleased = true;
            pitchBenderReleasedTimer.Start();
        }

        private void imgClickArea_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            SetModifiers(e);
            if (Controls != null)
            {
                if (currentControl > -1)
                {
                    if (oscillatorUnderMouse > -1 && OscillatorGUIs[oscillatorUnderMouse]
                        .SubControls.ControlsList[currentControl].GetType() != typeof(AreaButton))
                    {
                        OscillatorGUIs[oscillatorUnderMouse].SubControls.PointerWheelChanged(
                            OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl], e);
                        Action(OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl]);
                    }
                    else if (filterUnderMouse > -1)
                    {
                        FilterGUIs[filterUnderMouse].SubControls.PointerWheelChanged(
                            FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl], e);
                        Action(FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl]);
                    }
                    else if (pitchEnvelopeUnderMouse > -1)
                    {
                        PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.PointerWheelChanged(
                            PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl], e);
                        Action(PitchEnvelopeGUIs[pitchEnvelopeUnderMouse].SubControls.ControlsList[currentControl]);
                    }
                    else if (adsrUnderMouse > -1)
                    {
                        AdsrGUIs[adsrUnderMouse].SubControls.PointerWheelChanged(
                            AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl], e);
                        Action(AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl]);
                    }
                    else if (displayUnderMouse > -1)
                    {
                        DisplayGUI.SubControls.PointerWheelChanged(
                            DisplayGUI.SubControls.ControlsList[currentControl], e);
                        Action(DisplayGUI.SubControls.ControlsList[currentControl]);
                    }
                    else
                    {
                        if (selectedCompoundType == _compoundType.CONTROL_PANEL)
                        {
                            Object ctrl = Controls.ControlsList[0];
                            //Debug.WriteLine("EventHAndlers.cs 397");
                            Object obj = Controls.PointerWheelChanged(sender, e);
                            Action(((CompoundControl)ctrl).SubControls.ControlsList[currentControl], obj);
                        }
                        else if (selectedCompoundType == _compoundType.NONE
                            && Controls.ControlsList[currentControl].GetType() == typeof(VerticalSlider))
                        {
                            Object ctrl = Controls.ControlsList[currentControl];
                            //Debug.WriteLine("EventHAndlers.cs 405");
                            Object obj = Controls.PointerWheelChanged(sender, e);
                            Action(Controls.ControlsList[currentControl], obj);
                        }
                    }
                }
            }
        }

        //Boolean combValues = false;
        //List<byte[]> log = new List<byte[]>();

        public void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            if (initDone)
            {
                bool useInput = true;
                if (Settings.MidiDevice != "All midi inputs")
                {
                    useInput = false;
                    for (int i = 0; i < midiIn.usedPorts.Count; i++)
                    {
                        if (sender.DeviceId == midiIn.usedPorts[i].PortId)
                        {
                            useInput = midiIn.usedPorts[i].Use;
                            break;
                        }
                    }
                }
                byte[] rawData = args.Message.RawData.ToArray();
                if (useInput && 
                    (args.Message.Type == MidiMessageType.NoteOn
                    || args.Message.Type == MidiMessageType.NoteOff
                    || args.Message.Type == MidiMessageType.ChannelPressure
                    || args.Message.Type == MidiMessageType.PitchBendChange
                    || (args.Message.Type == MidiMessageType.ControlChange
                        && (rawData[1] < 0x20 || rawData[1] == 0x40))))
                {
                    midiInMessageQueue[midiInWriteIndex++] = args.Message;
                }
                if (midiInWriteIndex > 8191)
                {
                    midiInWriteIndex = 0;
                }
            }
        }

        private void MidiInTimer_Tick(object sender, object e)
        {
            while (midiInReadIndex != midiInWriteIndex)
            {
                HandleMidiInMessage(midiInMessageQueue[midiInReadIndex]);
                midiInReadIndex++;
                if (midiInReadIndex > 8191)
                {
                    midiInReadIndex = 0;
                }
            }
        }

        private bool handlingMidiInMessage = false;
        public async void HandleMidiInMessage(IMidiMessage receivedMidiMessage)
        {
            while (handlingMidiInMessage)
            {
                await Task.Delay(1);
            }
            if (receivedMidiMessage != null)
            {
                handlingMidiInMessage = true;
                byte[] rawData = receivedMidiMessage.RawData.ToArray();

                //string s = "";
                //for (int i = 0; i < rawData.Length; i++)
                //{
                //    s += rawData[i].ToString("X2") + " ";
                //}
                //Debug.WriteLine(s);

                if (receivedMidiMessage.Type == MidiMessageType.NoteOn)
                {
                    if (rawData[2] > 0)
                    {
                        KeyOn(rawData[1], rawData[0] & 0x0f, rawData[2]);
                    }
                    else
                    {
                        KeyOff(rawData[1], rawData[0] & 0x0f);
                    }
                }
                else if (receivedMidiMessage.Type == MidiMessageType.NoteOff)
                {
                    KeyOff(rawData[1], rawData[0] & 0x0f);
                }
                else if (receivedMidiMessage.Type == MidiMessageType.ControlChange)
                {
                    if (rawData[1] > 0x01 && rawData[1] < 0x20)
                    {
                        MapCC(rawData);
                    }
                    else if (rawData[1] == 0x40) // Pedal hold
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (Layout == Layouts.FOUR_OSCILLATORS)
                            {
                                for (int osc = 0; osc < 4; osc++)
                                {
                                    if (Oscillators[0][osc].MidiChannel == (int)(rawData[0] & 0x0f))
                                    {
                                        ((Rotator)AdsrGUIs[osc].SubControls.ControlsList[(int)AdsrControls.PEDAL_HOLD])
                                            .Selection = rawData[2] > 63 ? 1 : 0;
                                    }
                                }
                            }
                            else
                            {
                                ((Rotator)AdsrGUIs[0].SubControls.ControlsList[(int)AdsrControls.PEDAL_HOLD])
                                    .Selection = rawData[2] > 63 ? 1 : 0;
                            }
                            for (int osc = 0; osc < 12; osc++)
                            {
                                for (int poly = 0; poly < 6; poly++)
                                {
                                    if (Oscillators[poly][osc].MidiChannel == (int)(rawData[0] & 0x0f))
                                    {
                                        dispatcher[osc].PedalHold = rawData[2] > 63 ? true : false;
                                        if (!dispatcher[osc].PedalHold)
                                        {
                                            Oscillators[poly][osc].Adsr.AdsrRelease(Oscillators[poly][osc].UseAdsr);
                                        }
                                    }
                                }
                            }
                        });
                    }
                    else if (rawData[1] == 0x01) // Modulator
                    {
                        ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).Value = rawData[2];
                        FollowModulationWheel((int)rawData[2]);
                    }
                    else if (rawData[1] == 21) // LaunchKey CC 21 acting as modulation wheel
                    {
                        ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).Value = rawData[2];
                        FollowModulationWheel((int)rawData[2]);
                    }
                    else if (rawData[1] == 22) // LaunchKey CC 22 acting as pitch bender
                    {
                        ((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value = rawData[2] * 128;
                        if (rawData[2] > 0x40)
                        {
                            PitchBend[rawData[0] & 0x0f] = (double)(1 + (rawData[2] - 64) / 63f);
                        }
                        else if (rawData[2] < 0x40)
                        {
                            PitchBend[rawData[0] & 0x0f] = (0.5 + (rawData[2] / 128f));
                        }
                        else
                        {
                            PitchBend[rawData[0] & 0x0f] = 1f;
                        }
                    }
                }
                else if (receivedMidiMessage.Type == MidiMessageType.PitchBendChange)
                {
                    int value = rawData[1] + rawData[2] * 128;
                    ((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value = value;
                    if (value > 8192)
                    {
                        PitchBend[rawData[0] & 0x0f] = (1 + (value - 8192f) / 8191f);
                    }
                    else if (value < 8192)
                    {
                        PitchBend[rawData[0] & 0x0f] = 0.5 + (value / 16383f);
                    }
                    else
                    {
                        PitchBend[rawData[0] & 0x0f] = 1f;
                    }
                }
                else if (receivedMidiMessage.Type == MidiMessageType.ChannelPressure) // Also called 'After touch'
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).ControlGraphicsFollowsValue = false;
                        ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).Value = rawData[1];
                        ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).ControlGraphicsFollowsValue = true;
                    //adjust = rawData[1];
                    FollowModulationWheel((int)rawData[1]);
                    });
                }
                handlingMidiInMessage = false;
            }
        }

        private void MapCC(byte[] rawData)
        {
            byte cc = rawData[1];
            byte channel = (byte)(rawData[0] & 0x0f);
            byte value = rawData[2];

            for (int osc = 0; osc < 12; osc++)
            {
                if (Oscillators[0][osc].MidiChannel == channel
                    || Oscillators[0][osc].MidiChannel == 16)
                {
                    switch (Patch.SettingsData.CCMapping[cc - 2][osc] - 1) // Minus 1 because first selection is 'Not mapped'.
                    {
                        case 0: // Osc AM modulation depth
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                // Set control:
                                //((Rotator)((Oscillator)(Oscillators[0][0]).
                                ((Rotator)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 0;
                                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = value * 2;
                                // Times 2 because CC value is 0 - 127 and knob is 0 - 255.
                            }
                            // Set value for all polys of oscillators even if not in layout:
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].AM_Sensitivity = value * 2;
                            }
                            break;
                        case 1: // Osc FM modulation depth
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Rotator)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 1;
                                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = value * 2;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].FM_Sensitivity = value / 32.0;
                            }
                            break;
                        case 2:  // Osc XM modulation depth
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Rotator)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION_KNOB_TARGET]).Selection = 2;
                                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.MODULATION]).Value = value * 2;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].XM_Sensitivity = value * 2;
                                if (Oscillators[poly][osc].XM_Sensitivity == 0 && Oscillators[poly][osc].WaveForm == WAVEFORM.SQUARE)
                                {
                                    Oscillators[poly][osc].XM_Sensitivity = 1;
                                }
                                if (Oscillators[poly][osc].WaveForm == WAVEFORM.SQUARE)
                                {
                                    Oscillators[poly][osc].Phase = Math.PI * (1 + (value * 2 - 128) / 138f);
                                }
                            }
                            break;
                        case 3: // Osc frequency
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.FREQUENCY]).Value = value; // (int)(value * 1001 / 128.0);
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Frequency = (value * 1001 / 128.0) * 10;
                                Oscillators[poly][osc].FrequencyLfo = (value * 1000 / 128.0) / 10;
                            }
                            break;
                        case 4: // Osc fine tune
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.FINE_TUNE]).Value = value; // (int)(value * 1000 / 128.0);
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].FineTune = (value * 1001 / 128.0);
                                Oscillators[poly][osc].FineTuneLfo = (value * 1000 / 128.0);
                            }
                            break;
                        case 5: // Osc volume
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)OscillatorGUIs[osc].SubControls.ControlsList[(int)OscillatorControls.VOLUME]).Value = value; // (int)(value * 999 / 128.0);
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Volume = (byte)(value * 999 / 128.0);
                            }
                            break;
                        case 6: // Filter Q
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)FilterGUIs[OscillatorToFilter(osc)].SubControls.ControlsList[(int)FilterControls.Q]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Filter.Q = value;
                            }
                            break;
                        case 7: // Filter freq
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)FilterGUIs[OscillatorToFilter(osc)].SubControls.ControlsList[(int)FilterControls.FREQUENCY_CENTER]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Filter.FrequencyCenter = value;
                            }
                            break;
                        case 8: // Filter key follow
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)FilterGUIs[OscillatorToFilter(osc)].SubControls.ControlsList[(int)FilterControls.KEYBOARD_FOLLOW]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Filter.KeyboardFollow = value;
                            }
                            break;
                        case 9: // Filter gain
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)FilterGUIs[OscillatorToFilter(osc)].SubControls.ControlsList[(int)FilterControls.GAIN]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Filter.Gain = value;
                            }
                            break;
                        case 10: // Filter mix
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)FilterGUIs[OscillatorToFilter(osc)].SubControls.ControlsList[(int)FilterControls.FILTER_MIX]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Filter.Mix = value;
                            }
                            break;
                        case 11:// Pitch bender depth
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)PitchEnvelopeGUIs[OscillatorToPitchEnvelope(osc)].SubControls.ControlsList[(int)PitchEnvelopeControls.DEPTH]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].PitchEnvelope.Depth = value;
                            }
                            break;
                        case 12:// Pitch bender speed
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)PitchEnvelopeGUIs[OscillatorToPitchEnvelope(osc)].SubControls.ControlsList[(int)PitchEnvelopeControls.SPEED]).Value = (int)(value * 299 / 128.0);
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].PitchEnvelope.Speed = (int)((value + 1) * 299 / 128.0);
                            }
                            break;
                        case 13:// ADSR attack time
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.ADSR_A]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrAttackTime = value;
                            }
                            break;
                        case 14:// ADSR decay time
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.ADSR_D]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrDecayTime = value;
                            }
                            break;
                        case 15:// ADSR sustain level
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.ADSR_S]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrSustainLevel = value;
                            }
                            break;
                        case 16:// ADSR release time
                            if (osc < Patch.OscillatorsInLayout)
                            {
                                ((Knob)AdsrGUIs[OscillatorToAdsr(osc)].SubControls.ControlsList[(int)AdsrControls.ADSR_R]).Value = value;
                            }
                            for (int poly = 0; poly < 6; poly++)
                            {
                                Oscillators[poly][osc].Adsr.AdsrReleaseTime = value;
                            }
                            break;
                        case 17: // Pitch bend
                            if (rawData[2] > 0x40)
                            {
                                PitchBend[rawData[0] & 0x0f] = (double)(1 + (rawData[2] - 64) / 63f);
                            }
                            else if (rawData[2] < 0x40)
                            {
                                PitchBend[rawData[0] & 0x0f] = (0.5 + (rawData[2] / 128f));
                            }
                            else
                            {
                                PitchBend[rawData[0] & 0x0f] = 1f;
                            }
                            break;
                    }

                    // If oscillator uses WaveShape it will need a new one to be generated:
                    //for (int poly = 0; poly < 6; poly++)
                    //{
                    //    Oscillators[poly][osc].WaveShape.SetNeedsToBeCreated();
                    //}
                }
            }
        }

        private Object GetTag(int id)
        {
            if (Controls.ControlsList[id].GetType() == typeof(Knob))
            {
                return ((Knob)Controls.ControlsList[id]).Tag;
            }
            if (Controls.ControlsList[id].GetType() == typeof(VerticalSlider))
            {
                return ((VerticalSlider)Controls.ControlsList[id]).Tag;
            }
            else if (Controls.ControlsList[id].GetType() == typeof(AreaButton))
            {
                return ((AreaButton)Controls.ControlsList[id]).Tag;
            }
            else if (Controls.ControlsList[id].GetType() == typeof(Rotator))
            {
                return ((Rotator)Controls.ControlsList[id]).Tag;
            }
            else if (Controls.ControlsList[id].GetType() == typeof(Rotator))
            {
                return ((Rotator)Controls.ControlsList[id]).Tag;
            }
            else if (Controls.ControlsList[id].GetType() == typeof(MomentaryButton))
            {
                return ((MomentaryButton)Controls.ControlsList[id]).Tag;
            }
            return null;
        }

        private Oscillator GetOscillatorByControlId(int id)
        {
            switch (Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(FilterTag))
                    {
                        return Oscillators[0][((FilterTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(AdsrTag))
                    {
                        return Oscillators[0][((AdsrTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
                case Layouts.SIX_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(FilterTag))
                    {
                        return Oscillators[0][((FilterTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(FilterTag))
                    {
                        return Oscillators[0][((FilterTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
            }
            return null;
        }

        public Oscillator GetOscillatorById(int id)
        {
            if (id > -1 && id < 12)
            {
                return Oscillators[0][id];
            }
            else
            {
                return null;
            }
        }
        public Object GetOscillatorControl(OscillatorControls control)
        {
            if (currentOscillator != null)
            {
                return Controls.ControlsList[currentOscillator.Id * numberOfOscillatorControlEnums + (int)control];
            }
            return null;
        }

        private void SetModifiers(PointerRoutedEventArgs e)
        {
            PointerPoint currentMousePoint = e.GetCurrentPoint(imgClickArea);
            PointerPointProperties props = currentMousePoint.Properties;
            LeftButtonPressed = props.IsLeftButtonPressed;
            RightButtonPressed = props.IsRightButtonPressed;
        }

        private class PointerData
        {
            public int ButtonPressed;
            public Point Position;
            public int Id;

            public PointerData(Boolean leftButtonPressed, Boolean rightButtonPressed, Point position, int id)
            {
                if (leftButtonPressed)
                {
                    ButtonPressed = 0;
                }
                else if (rightButtonPressed)
                {
                    ButtonPressed = 1;
                }
                else
                {
                    ButtonPressed = -1;
                }
                int x = (int)position.X - 9;
                int y = (int)position.Y - 1;
                x = x < 0 ? 0 : x;
                y = y < 0 ? 0 : y;
                Position = new Point((double)x, (double)y);
                Id = id;
            }
        }
    }
}
