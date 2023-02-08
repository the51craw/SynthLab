using System;
using System.Linq;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Storage;
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
            catch
            {
                ContentDialog error = new Message("It seems like you do not have Acrobate Reader installed.\n"
                    + "You need Acrobat reader to read the manual.\nGo to adobe.com to install it, or set your Microsoft\n"
                    + "Edge browser as default pdf reader.");
                await error.ShowAsync();
            }
        }

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
            switch ((Layouts)Layout)
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
            if (Layout == Layouts.FOUR_OSCILLATORS)
            {
                return Oscillators[0][pitchEnvelopeUnderMouse].Id;
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
            if (Layout == Layouts.FOUR_OSCILLATORS)
            {
                return Oscillators[0][adsrUnderMouse].Id;
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
            switch ((Layouts)Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    // Each oscillator has its own adsr with the same index:
                    if (oscillator < 4)
                    {
                        filter = oscillator;
                    }
                    else
                    {
                        filter = 3;
                    }
                    break;
                case Layouts.SIX_OSCILLATORS:
                    // Each oscillator has its own adsr with the same index:
                    if (oscillator < 6)
                    {
                        filter = oscillator;
                    }
                    else
                    {
                        filter = 5;
                    }
                    filter = oscillator;
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    if (oscillator < 8)
                    {
                        filter = oscillator % 4;
                    }
                    else
                    {
                        filter = 7;
                    }
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
            switch ((Layouts)Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    // Each oscillator has its own adsr with the same index:
                    if (Oscillator < 4)
                    {
                        adsr = Oscillator;
                    }
                    else
                    {
                        adsr = 3;
                    }
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
            switch ((Layouts)Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    if (Oscillator < 4)
                    {
                        pitchEnvelope = Oscillator;
                    }
                    else
                    {
                        pitchEnvelope = 3;
                    }
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
        //public int DisplayToOscillator(int Display)
        //{
        //    int oscillator = 0;
        //    switch ((Layouts)Layout)
        //    {
        //        case Layouts.FOUR_OSCILLATORS:
        //            if (Display < 4)
        //            {
        //                oscillator = Display;
        //            }
        //            else
        //            {
        //                oscillator = 3;
        //            }
        //            break;
        //        case Layouts.SIX_OSCILLATORS:
        //            // Each oscillator has its own adsr with the same index:
        //            if (Display < 6)
        //            {
        //                oscillator = Display;
        //            }
        //            else
        //            {
        //                oscillator = 5;
        //            }
        //            break;
        //        case Layouts.EIGHT_OSCILLATORS:
        //            // Four filters (0 - 3) belongs to 1 of two oscillators.
        //            // Filter 0 belongs to oscillators 0 and 4, filter 1 to
        //            // oscillators 1 and 5 etcetera.
        //            // The view buttons are used to select which one.
        //            if (((Rotator)OscillatorGUIs[Display].SubControls.
        //                ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
        //            {
        //                if (oscillator < 8)
        //                {
        //                    oscillator = Display;
        //                }
        //                else
        //                {
        //                    oscillator = 7;
        //                }
        //            }
        //            else if (((Rotator)OscillatorGUIs[Display + 4].SubControls.
        //                ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
        //            {
        //                oscillator = Display + 4;
        //            }
        //            else
        //            {
        //                // User has not yet selected an oscillator. Let's select
        //                // the one just above the filter.
        //                ((Rotator)OscillatorGUIs[Display + 4].SubControls.
        //                    ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 1;
        //                oscillator = Display + 4;
        //            }
        //        break;
        //        case Layouts.TWELVE_OSCILLATORS:
        //            // There is only one display and the oscillator with
        //            // the view button active uses the display:
        //            for (int i = 0; i < 12; i++)
        //            {
        //                if (((Rotator)OscillatorGUIs[i].SubControls.
        //                    ControlsList[(int)OscillatorControls.VIEW_ME]).Selection == 1)
        //                {
        //                    oscillator = i;
        //                    break;
        //                }
        //            }
        //            break;
        //    }
        //    return oscillator;
        //}

        //public int OscillatorToDisplay(int Oscillator)
        //{
        //    int[] firstFour = new int[] { 1, 2, 5, 6 };
        //    int display = 0;
        //    switch ((Layouts)Layout)
        //    {
        //        case Layouts.FOUR_OSCILLATORS:
        //            // Each oscillator has its own display with the same index:
        //            if (Oscillator > 4)
        //            {
        //                display = Oscillator;
        //            }
        //            else
        //            {
        //                display = 3;
        //            }
        //            break;
        //        case Layouts.SIX_OSCILLATORS:
        //            if (Oscillator > 6)
        //            {
        //                display = Oscillator;
        //            }
        //            else
        //            {
        //                display = 5;
        //            }
        //            break;
        //        case Layouts.EIGHT_OSCILLATORS:
        //            // Two displays belongs to 4 of 8 oscillators.
        //            // Display 0 belongs to oscillators 0, 1, 4 and 5, ADSR 1 to
        //            // oscillators 2, 3, 6 and 7.
        //            // The view buttons are used to select which one.
        //            for (int osc = 0; osc < 12; osc++)
        //            {
        //                if (firstFour.Contains(Oscillator))
        //                {
        //                    display = 0;
        //                }
        //                else
        //                {
        //                    display = 1;
        //                }
        //            }
        //            break;
        //        case Layouts.TWELVE_OSCILLATORS:
        //            // There is only one display and the oscillator with
        //            // the view button active uses the display:
        //            display = 0;
        //            break;
        //    }
        //    return display;
        //}

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
                Oscillators[0][oscillator.Id].ViewMe = false;
            }

            ((Rotator)OscillatorGUIs[0].SubControls.
                ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 1;
            Oscillators[0][0].ViewMe = true;

            return 0;
        }
    }
}
