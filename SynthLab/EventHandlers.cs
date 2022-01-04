using System;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        // When the pointer is moved over the click-area, ask the Controls
        // object if and if so which control the pointer is over:
        private void imgClickArea_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            SetModifiers(e);
           
            if (initDone && Controls != null)
            {
                currentControl = -1;
                Controls.PointerMoved(sender, e);
                selectedCompoundType = _compoundType.NONE;
                oscillatorUnderMouse = -1;
                foreach (CompoundControl oscillator in OscillatorGUIs)
                {
                    if (oscillator.IsSelected)
                    {
                        selectedCompoundType = _compoundType.OSCILLATOR;
                        oscillatorUnderMouse = oscillator.Id;
                        currentOscillator = Patch.Oscillators[0][oscillator.Id];
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
                        }
                    }
                }

                if (selectedCompoundType == _compoundType.NONE)
                {
                    Object obj = null;
                    PointerPoint currentMousePoint = e.GetCurrentPoint(imgClickArea);
                    Double x = currentMousePoint.Position.X;
                    Double y = currentMousePoint.Position.Y;

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
                            obj = ((ControlBase)ctrl).Moved(new Point(x, y));
                            Action(ctrl, obj);
                        }
                    }
                }
                //if (currentOscillator != null && selectedOscillator != null)
                //{
                //    test.Text = "currentOscillator: " + currentOscillator.Id.ToString() + ", selectedOscillator: " + selectedOscillator.Id.ToString();
                //}
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
                            if (LeftButtonPressed || RightButtonPressed)
                            {
                                ((Rotator)OscillatorGUIs[oscillatorUnderMouse].SubControls.ControlsList[currentControl]).Tapped();
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
                            if (LeftButtonPressed || RightButtonPressed)
                            {
                                ((Rotator)FilterGUIs[filterUnderMouse].SubControls.ControlsList[currentControl]).Tapped();
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
                        }

                    }
                    else if (adsrUnderMouse > -1)
                    {
                        AdsrGUIs[adsrUnderMouse].SubControls.PointerPressed(
                            AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl], e);
                        if (AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl].GetType() == typeof(Rotator))
                        {
                            if (LeftButtonPressed || RightButtonPressed)
                            {
                                ((Rotator)AdsrGUIs[adsrUnderMouse].SubControls.ControlsList[currentControl]).Tapped();
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
                            if (LeftButtonPressed || RightButtonPressed)
                            {
                                ((Rotator)DisplayGUI.SubControls.ControlsList[currentControl]).Tapped();
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
                            ((Rotator)ControlPanel.SubControls.ControlsList[currentControl]).Tapped();
                            Action(ControlPanel.SubControls.ControlsList[currentControl]);
                        }
                    }
                    else
                    {
                        Object obj;
                        Object ctrl = Controls.ControlsList[currentControl];
                        if (ctrl.GetType() == typeof(Rotator))
                        {
                            ((Rotator)ctrl).Tapped();
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
            Controls.PointerReleased(currentControl, e);
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
                            Object obj = Controls.PointerWheelChanged(sender, e);
                            Action(((CompoundControl)ctrl).SubControls.ControlsList[currentControl], obj);
                        }
                    }
                }
            }
        }

        Boolean combValues = false;
        List<byte[]> log = new List<byte[]>();
        public async void MidiInPort_MessageReceivedAsync(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            if (initDone)
            {
                IMidiMessage receivedMidiMessage = args.Message;
                byte[] rawData = receivedMidiMessage.RawData.ToArray();

                byte[] msg = new byte[rawData.Length];
                for (int i = 0; i < rawData.Length; i++)
                {
                    msg[i] += rawData[i];
                }
                log.Add(msg);

                if (receivedMidiMessage.Type == MidiMessageType.NoteOn)
                {
                    if (rawData[2] > 0)
                    {
                        //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        //{
                            KeyOn(rawData[1], rawData[0] & 0x0f, rawData[2]);
                        //});
                    }
                    else
                    {
                        //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        //{
                            KeyOff(rawData[1], rawData[0] & 0x0f);
                        //});
                    }
                }
                else if (receivedMidiMessage.Type == MidiMessageType.NoteOff)
                {
                    //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                        KeyOff(rawData[1], rawData[0] & 0x0f);
                    //});
                }
                else if (receivedMidiMessage.Type == MidiMessageType.ControlChange)
                {
                    if (rawData[1] == 0x40) // Pedal hold
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (Patch.Layout == Layouts.FOUR_OSCILLATORS)
                            {
                                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                                {
                                    ((Rotator)AdsrGUIs[osc].SubControls.ControlsList[(int)AdsrControls.PEDAL_HOLD])
                                    .Selection = rawData[2] > 63 ? 1 : 0;
                                }
                            }
                            else
                            {
                                ((Rotator)AdsrGUIs[0].SubControls.ControlsList[(int)AdsrControls.PEDAL_HOLD])
                                .Selection = rawData[2] > 63 ? 1 : 0;
                            }
                            dispatcher[rawData[0] & 0x0f].PedalHold = rawData[2] > 63 ? true : false;
                        });
                    }
                    else if (rawData[1] == 0x01) // Modulator
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).ControlGraphicsFollowsValue = false;
                            ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).Value = rawData[2];
                            ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).ControlGraphicsFollowsValue = true;
                            adjust = rawData[2];
                            FollowModulationWheel((int)rawData[2]);
                        });
                    }
                    //else if (rawData[1] == 0x40) // Pedal hold
                    //{
                    //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //    {
                    //        dispatcher[rawData[0] & 0x0f].PedalHold = false;
                    //        fadjust = rawData[2];
                    //    });
                    //}
                    else if (rawData[1] == 0x07) // Volume
                    {
                    }
                    else if (rawData[1] == 21)
                    {
                        if (!combValues)
                        {
                            combValues = true;
                            // Don't change parameters in the middle of generating wave shape:
                            while (CurrentActivity == CURRENTACTIVITY.GENERATING_WAVE_SHAPE)
                            {
                                await Task.Delay(1);
                            }
                            CurrentActivity = CURRENTACTIVITY.CHANGING_PARAMETERS;
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                ((VerticalSlider)Controls.ControlsList[(int)OtherControls.MODULATION_WHEEL]).Value = rawData[2];
                                adjust = rawData[2];
                                FollowModulationWheel((int)rawData[2]);
                            });
                            CurrentActivity = CURRENTACTIVITY.NONE;
                            combValues = false;
                        }
                    }
                    else
                    {
                        Trace.WriteLine("Skipped CC message " + msg);
                    }
                }
                else if (receivedMidiMessage.Type == MidiMessageType.PitchBendChange)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        fadjust = rawData[2];
                    });
                }
                else
                {
                    Trace.WriteLine("Skipped message " + msg);
                }
            }
        }

        private TagType GetTagType(int id)
        {
            Object tag = GetTag(id);
            if (tag != null && tag.GetType() == typeof(BaseTag))
            {
                BaseTag baseTag = (BaseTag)GetTag(id);
                return baseTag == null ? TagType.OTHER : baseTag.TagType;
            }
            else
            {
                return TagType.OTHER;
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
            switch (Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Patch.Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(FilterTag))
                    {
                        return Patch.Oscillators[0][((FilterTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(AdsrTag))
                    {
                        return Patch.Oscillators[0][((AdsrTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
                case Layouts.SIX_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Patch.Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(FilterTag))
                    {
                        return Patch.Oscillators[0][((FilterTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Patch.Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    else if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(FilterTag))
                    {
                        return Patch.Oscillators[0][((FilterTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    if (((ControlBase)Controls.ControlsList[id]).Tag != null &&
                        ((ControlBase)Controls.ControlsList[id]).Tag.GetType() == typeof(OscillatorTag))
                    {
                        return Patch.Oscillators[0][((OscillatorTag)((ControlBase)Controls.ControlsList[id]).Tag).OscillatorId];
                    }
                    break;
            }
            return null;
        }

        public Oscillator GetOscillatorById(int id)
        {
            if (id > -1 && id < Patch.OscillatorCount)
            {
                return Patch.Oscillators[0][id];
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
