using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        public enum Layouts
        {
            FOUR_OSCILLATORS,
            SIX_OSCILLATORS,
            EIGHT_OSCILLATORS,
            TWELVE_OSCILLATORS,
        }

        public MainLayout MainLayout;
        public OscillatorsLayout OscillatorLayout;
        public FiltersLayout FilterLayout;
        public AdsrLayout AdsrLayout;
        public DisplayLayout DisplayLayout;
        public ControlPanelLayout ControlPanelLayout;
        public StaticImage Logotype;
        public Rotator LayoutSelector;
        public Rotator AdsrTypeSelector;

        private void CreateLayout(Layouts layout)
        {
            MainLayout = new MainLayout(layout);
            OscillatorLayout = new OscillatorsLayout(this);
            FilterLayout = new FiltersLayout(this);
            AdsrLayout = new AdsrLayout(this);
            DisplayLayout = new DisplayLayout(this);
            ControlPanelLayout = new ControlPanelLayout(this);
        }
    }

    /// <summary>
    /// Main layout is all about how many oscillators are
    /// present, and how they are arranged. Also about 
    /// the filter arrangement. Only certain combinations
    /// are possible.
    /// </summary>
    public class MainLayout
    {
        // Number of columns and rows:
        public int Columns;
        public int Rows;

        // Layout of panels (oscillator, filter, ADSR and display background images):
        public int x_spacing;
        public int x1;
        public int x2;
        public int x3;
        public int x4;
        public int y_spacing;
        public int y1;
        public int y2;
        public int y3;
        public int y4;
        public int y5;

        public MainLayout(MainPage.Layouts layout)
        {
            // Layout of panels (oscillator, filter, ADSR and display background images):
            x_spacing = 476;
            x1 = 12;
            x2 = x1 + x_spacing;
            x3 = x1 + x_spacing * 2;
            x4 = x1 + x_spacing * 3;
            y_spacing = 207;
            y1 = 41;
            y2 = y1 + y_spacing;
            y3 = y1 + y_spacing * 2;
            y4 = y1 + y_spacing * 3;
            y5 = y1 + y_spacing * 4;

            switch (layout)
            {
                case MainPage.Layouts.FOUR_OSCILLATORS:
                    Columns = 1;
                    Rows = 4;
                    break;
                case MainPage.Layouts.SIX_OSCILLATORS:
                    Columns = 2;
                    Rows = 3;
                    break;
                case MainPage.Layouts.EIGHT_OSCILLATORS:
                    Columns = 4;
                    Rows = 2;
                    break;
                case MainPage.Layouts.TWELVE_OSCILLATORS:
                    Columns = 4;
                    Rows = 3;
                    break;
            }
        }
    }

    public class OscillatorsLayout
    {
        // Layout of oscillators within the application main window:
        public int X;
        public int Y;
        public int X_Spacing;
        public int Y_Spacing;

        // Layout of controls within an oscillator, knobs:
        public int modulationKnobX;
        public int oscillatorKnobY;
        public int frequencyKnobX;
        public int frequencyFineKnobX;
        public int volumeKnobX;
        public int oscillatorKnobX_Spacing;

        // Selectors:
        public int selectorsX;
        public int waveSelectorY;
        public int kbrdLfoSelectorY;
        public int adsrSelectorY;
        public int viewSelectorY;
        public int selectorSpacingY;
        public int modulationKnobFunctionSelectorX;

        public OscillatorsLayout(MainPage mainPage)
        {
            switch (mainPage.Patch.Layout)
            {
                case MainPage.Layouts.FOUR_OSCILLATORS:
                    X = mainPage.MainLayout.x1;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.SIX_OSCILLATORS:
                    X = mainPage.MainLayout.x1;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing * 2;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.EIGHT_OSCILLATORS:
                    X = mainPage.MainLayout.x1;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.TWELVE_OSCILLATORS:
                    X = mainPage.MainLayout.x1;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
            }

            // Layout of controls within an oscillator, knobs:
            modulationKnobX = 47;
            oscillatorKnobY = 120;
            oscillatorKnobX_Spacing = 81;
            frequencyKnobX = modulationKnobX + oscillatorKnobX_Spacing;
            frequencyFineKnobX = modulationKnobX + oscillatorKnobX_Spacing * 2;
            volumeKnobX = modulationKnobX + oscillatorKnobX_Spacing * 3;

            // Selectors:
            selectorsX = 333;
            selectorSpacingY = 31;
            waveSelectorY = 12;
            kbrdLfoSelectorY = waveSelectorY + selectorSpacingY;
            adsrSelectorY = waveSelectorY + selectorSpacingY * 2;
            viewSelectorY = waveSelectorY + selectorSpacingY * 3;
            modulationKnobFunctionSelectorX = 10;
        }
    }

    /// <summary>
    /// Places Filter controls.
    /// If Single, one set is placed at the coordinates
    /// and used for selected oscillator (View is On),
    /// or placed at coordinates relative to corresponding oscillator.
    /// </summary>
    public class FiltersLayout
    {
        // Denotes single filter
        public Boolean Single;

        // Position within main window:
        public int X;
        public int Y;
        public int X_Spacing;
        public int Y_Spacing;

        public int filterKnobX;
        public int filterKnobY;
        public int filterKnobX_Spacing;

        public int filterFunctionX;
        public int filterFunctionY;
        public int filterModWheelFunctionX;

        public FiltersLayout(MainPage mainPage)
        {
            filterKnobX = 50;
            filterKnobY = 120;
            filterKnobX_Spacing = 84;
            filterFunctionX = 10;
            filterFunctionY = 12;
            filterModWheelFunctionX = 207;

            switch (mainPage.Patch.Layout)
            {
                case MainPage.Layouts.FOUR_OSCILLATORS:
                    X = mainPage.MainLayout.x2;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.SIX_OSCILLATORS:
                    X = mainPage.MainLayout.x2;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing * 2;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.EIGHT_OSCILLATORS:
                    X = mainPage.MainLayout.x1;
                    Y = mainPage.MainLayout.y2;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.TWELVE_OSCILLATORS:
                    X = mainPage.MainLayout.x3;
                    Y = mainPage.MainLayout.y4;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
            }
        }
    }

    public class AdsrLayout
    {
        public int X;
        public int Y;
        public int X_Spacing;
        public int Y_Spacing;
        public int ModWheelFunctionSelectorX = 333;
        public int ModWheelFunctionSelectorY = 12;
        public int PedalX = 56;
        public int PedalY = 26;
        public int GraphX1 = 18;
        public int GraphY1 = 18;
        public int GraphX2 = 318;
        public int GraphY2 = 90;

        public AdsrLayout(MainPage mainPage)
        {
            switch (mainPage.Patch.Layout)
            {
                case MainPage.Layouts.FOUR_OSCILLATORS:
                    X = mainPage.MainLayout.x4;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.SIX_OSCILLATORS:
                    X = mainPage.MainLayout.x4;
                    Y = mainPage.MainLayout.y4;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.EIGHT_OSCILLATORS:
                    X = mainPage.MainLayout.x4;
                    Y = mainPage.MainLayout.y4;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.TWELVE_OSCILLATORS:
                    X = mainPage.MainLayout.x4;
                    Y = mainPage.MainLayout.y4;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
            }
        }
    }

    public class DisplayLayout
    {
        public int X;
        public int Y;
        public int X_Spacing;
        public int Y_Spacing;

        public DisplayLayout(MainPage mainPage)
        {
            switch (mainPage.Patch.Layout)
            {
                case MainPage.Layouts.FOUR_OSCILLATORS:
                    X = mainPage.MainLayout.x4;
                    Y = mainPage.MainLayout.y1;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.SIX_OSCILLATORS:
                    X = mainPage.MainLayout.x2;
                    Y = mainPage.MainLayout.y4;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.EIGHT_OSCILLATORS:
                    X = mainPage.MainLayout.x2;
                    Y = mainPage.MainLayout.y4;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
                case MainPage.Layouts.TWELVE_OSCILLATORS:
                    X = mainPage.MainLayout.x2;
                    Y = mainPage.MainLayout.y4;
                    X_Spacing = mainPage.MainLayout.x_spacing;
                    Y_Spacing = mainPage.MainLayout.y_spacing;
                    break;
            }
        }
    }

    public class ControlPanelLayout
    {
        public int X;
        public int Y;
        public int X_Spacing;
        public int Y_Spacing;

        public ControlPanelLayout(MainPage mainPage)
        {
            X = 138;
            Y = 13;
            X_Spacing = 0;
            Y_Spacing = 26;
        }
    }
}
