using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Show manual. Requires pdf viewer.
        /// </summary>
        /// <returns></returns>
        public async Task ShowManual()
        {
            try
            {
                Windows.System.LauncherOptions options = new Windows.System.LauncherOptions();
                options.ContentType = "application/pdf";
                StorageFile manual = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///Documents/Manual.pdf", UriKind.Absolute));
                await Windows.System.Launcher.LaunchFileAsync(manual);
            }
            catch (Exception e)
            {
                ContentDialog error = new Error("It seems like you do not have Acrobate Reader installed.\n"
                    + "You need Acrobat reader to read the manual.\nGo to adobe.com to install it.");
                await error.ShowAsync();
            }
        }

        public void CreateWaveform(Oscillator oscillator)
        {
            for (int poly = 0; poly < Patch.Polyphony; poly++)
            {
                Patch.Oscillators[poly][oscillator.Id].WaveShape.MakeWave();
            }
        }

        //public void SetGraphicResponse(ControlBase control)
        //{
        //    control.ControlGraphicsFollowsValue = dispatcher.NumberOfOscillatorsInUse() == 0;
        //}

        public void RestoreGraphicResponse(ControlBase control)
        {
            control.ControlGraphicsFollowsValue = true;
        }

        /// <summary>
        /// Filters belongs to Oscillator according to rules
        /// that are depending on the current layout.
        /// This function returns the index of the oscillator
        /// the filter is connected to.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        public int FilterToOscillator(int Filter)
        {
            int oscillator = 0;
            switch ((Layouts)Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                case Layouts.SIX_OSCILLATORS:
                    // Each oscillator has its own filter with the same index:
                    oscillator = Filter;
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    // Four filters (0 - 3) belongs to 1 of two oscillators.
                    // Filter 0 belongs to oscillators 0 and 4, filter 1 to
                    // oscillators 1 and 5 etcetera.
                    // The view buttons are used to select which one.
                    if (((Rotator)OscillatorGUIs[Filter].SubControls.
                        ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
                    {
                        oscillator = Filter;
                    }
                    else if (((Rotator)OscillatorGUIs[Filter + 4].SubControls.
                        ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
                    {
                        oscillator = Filter + 4;
                    }
                    else
                    {
                        // User has not yet selected an oscillator. Let's select
                        // the one just above the filter.
                        ((Rotator)OscillatorGUIs[Filter + 4].SubControls.
                            ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 1;
                        oscillator = Filter + 4;
                    }
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    // There is only one filter GUI and the oscillator with
                    // the view button active uses the filter:
                    for (int i = 0; i < 12; i++)
                    {
                        if (((Rotator)OscillatorGUIs[i].SubControls.
                            ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
                        {
                            oscillator = i;
                            break;
                        }
                    }
                    break;
            }
            return oscillator;
        }

        /// <summary>
        /// Pitch envelopes belongs to Oscillators according to rules
        /// that are depending on the current layout.
        /// This function returns the index of the oscillator
        /// the pitch envelope is connected to.
        /// </summary>
        /// <param name="Adsr"></param>
        /// <returns></returns>
        public int PitchEnvelopeToOscillator(int PitchEnvelope)
        {
            if (Patch.Layout == Layouts.FOUR_OSCILLATORS)
            {
                return Patch.Oscillators[0][pitchEnvelopeUnderMouse].Id;
            }
            else
            {
                return FindOrSetViewedOscillator();
            }
        }

        /// <summary>
        /// ADSRs belongs to Oscillator according to rules
        /// that are depending on the current layout.
        /// This function returns the index of the oscillator
        /// the ADSR is connected to.
        /// </summary>
        /// <param name="Adsr"></param>
        /// <returns></returns>
        public int AdsrToOscillator(int Adsr)
        {
            if (Patch.Layout == Layouts.FOUR_OSCILLATORS)
            {
                return Patch.Oscillators[0][adsrUnderMouse].Id;
            }
            else
            {
                return FindOrSetViewedOscillator();
            }
        }

        public int OscillatorToFilter(int oscillator)
        {
            int[] firstFour = new int[] { 1, 2, 5, 6 };
            int filter = 0;
            switch ((Layouts)Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                case Layouts.SIX_OSCILLATORS:
                    // Each oscillator has its own adsr with the same index:
                    filter = oscillator;
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    filter = oscillator % 4;
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    filter = 0;
                    break;
            }
            return filter;
        }

        public int OscillatorToAdsr(int Oscillator)
        {
            int[] firstFour = new int[] { 1, 2, 5, 6 };
            int adsr = 0;
            switch ((Layouts)Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    // Each oscillator has its own adsr with the same index:
                    adsr = Oscillator;
                    break;
                case Layouts.SIX_OSCILLATORS:
                case Layouts.EIGHT_OSCILLATORS:
                case Layouts.TWELVE_OSCILLATORS:
                    // There is only one ADSR and the oscillator with
                    // the view button active uses the ADSR:
                    adsr = 0;
                    break;
            }
            return adsr;
        }

        public int OscillatorToPitchEnvelope(int Oscillator)
        {
            int pitchEnvelope = 0;
            switch ((Layouts)Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    pitchEnvelope = Oscillator;
                    break;
                case Layouts.SIX_OSCILLATORS:
                case Layouts.EIGHT_OSCILLATORS:
                case Layouts.TWELVE_OSCILLATORS:
                    pitchEnvelope = 0;
                    break;
            }
            return pitchEnvelope;
        }


        /// <summary>
        /// Displays belongs to Oscillators according to rules
        /// that are depending on the current layout.
        /// This function returns the index of the oscillator
        /// the display is connected to.
        /// </summary>
        /// <param name="Display"></param>
        /// <returns></returns>
        public int DisplayToOscillator(int Display)
        {
            int oscillator = 0;
            switch ((Layouts)Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                case Layouts.SIX_OSCILLATORS:
                    // Each oscillator has its own adsr with the same index:
                    oscillator = Display;
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    // Four ADSRs (0 - 3) belongs to 1 of two oscillators.
                    // ADSR 0 belongs to oscillators 0 and 4, ADSR 1 to
                    // oscillators 1 and 5 etcetera.
                    // The view buttons are used to select which one.
                    if (((Rotator)OscillatorGUIs[Display].SubControls.
                        ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
                    {
                        oscillator = Display;
                    }
                    else
                    {
                        oscillator = Display + 4;
                    }
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    // There is only one display and the oscillator with
                    // the view button active uses the display:
                    for (int i = 0; i < 12; i++)
                    {
                        if (((Rotator)OscillatorGUIs[i].SubControls.
                            ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
                        {
                            oscillator = i;
                            break;
                        }
                    }
                    break;
            }
            return oscillator;
        }

        public int OscillatorToDisplay(int Oscillator)
        {
            int[] firstFour = new int[] { 1, 2, 5, 6 };
            int display = 0;
            switch ((Layouts)Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                case Layouts.SIX_OSCILLATORS:
                    // Each oscillator has its own display with the same index:
                    display = Oscillator;
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    // Two displays belongs to 4 of 8 oscillators.
                    // Display 0 belongs to oscillators 0, 1, 4 and 5, ADSR 1 to
                    // oscillators 2, 3, 6 and 7.
                    // The view buttons are used to select which one.
                    for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                    {
                        if (firstFour.Contains(Oscillator))
                        {
                            display = 0;
                        }
                        else
                        {
                            display = 1;
                        }
                    }
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    // There is only one display and the oscillator with
                    // the view button active uses the display:
                    display = 0;
                    break;
            }
            return display;
        }

        private int FindOrSetViewedOscillator()
        {
            // See id any oscillator is viewed. If not, make oscillator 0 viewed:
            foreach (CompoundControl oscillator in OscillatorGUIs)
            {
                if (((Rotator)oscillator.SubControls.
                ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
                {
                    return oscillator.Id;
                }
            }

            foreach (CompoundControl oscillator in OscillatorGUIs)
            {
                ((Rotator)oscillator.SubControls.
                    ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 0;
                Patch.Oscillators[0][oscillator.Id].ViewMe = false;
            }

            ((Rotator)OscillatorGUIs[0].SubControls.
                ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 1;
            Patch.Oscillators[0][0].ViewMe = true;

            return 0;
        }

        //!!! Remove change polyphony!
        private async void ChangePolyphony(int polyphony)
        {
            allowGuiUpdates = false;
            Patch.Polyphony = polyphony;
            Patch.CreateOscillators(this);
            dispatcher = new KeyDispatcher[17];
            for (int ch = 0; ch < 17; ch++)
            {
                dispatcher[ch] = new KeyDispatcher(this, Patch.Polyphony);
            }
            //CreateWaveShapes();

            FrameServer.PolyServers = new PolyServer[Patch.Polyphony];
            for (int poly = 0; poly < Patch.Polyphony; poly++)
            {
                FrameServer.PolyServers[poly] = new PolyServer(this);
            }

            foreach (List<Oscillator> oscList in Patch.Oscillators)
            {
                foreach (Oscillator osc in oscList)
                {
                    osc.mainPage = this;
                    osc.PitchEnvelope.mainPage = this;
                    osc.Adsr.mainPage = this;
                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CreateLayout(Patch.Layout);
                //CreateFilters();
                CreateControls();
                CreateWiring();

                // Make sure all controls has the correct size and position:
                Controls.ResizeControls(gridControls, Window.Current.Bounds);
                Controls.SetControlsUniform(gridControls);
                Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
                Controls.SetControlsUniform(gridControlPanel);

                ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.LAYOUT]).Selection = (int)Patch.Layout;

                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                {
                    selectedOscillator = Patch.Oscillators[0][osc];
                    allowGuiUpdates = true;
                }
            });
        }
    }
}
