using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static SynthLab.Oscillator;

namespace SynthLab
{

    public enum _compoundType
    {
        OSCILLATOR,
        PITCH_ENVELOPE,
        ADSR,
        DISPLAY,
        WIRE,
        OTHER,
        FILTER,
        CONTROL_PANEL,
        NONE,
    }

    // NOTE! These declarations _must_ be in the same order as they are created!
    public enum OscillatorControls
    {
        MODULATION,
        FREQUENCY,
        FINE_TUNE,
        VOLUME,
        WAVE,
        KEYBOARD_OR_FIXED,
        ADSR_OR_PULSE,
        VIEW_ME,
        LEDSOUNDING,
        MODULATION_KNOB_TARGET,
        MODULATION_WHEEL_TARGET,
        AM_SOCKET,
        FM_SOCKET,
        XM_SOCKET,
        OUT_SOCKET,
        NUMBER,
        WIRE,
        NONE
    }

    public enum FilterControls
    {
        Q,
        FREQUENCY_CENTER,
        KEYBOARD_FOLLOW,
        GAIN,
        FILTER_MIX,
        FILTER_FUNCTION,
        MODULATION_WHEEL_TARGET,
        NONE,
    }

    public enum PitchEnvelopeControls
    {
        PITCH_ENV_MODULATION_WHEEL_USE,
        DEPTH,
        SPEED,
        MOD_PITCH,
        MOD_AM,
        MOD_FM,
        MOD_XM,
        GRAPH,
        NONE,
    }

    public enum AdsrControls
    {
        ADSR_A,
        ADSR_D,
        ADSR_S,
        ADSR_R,
        ADSR_MODULATION_WHEEL_USE,
        PEDAL_HOLD,
        GRAPH,
        NONE,
    }

    public enum DisplayControls
    {
        FREQUENCY,
        OSCILLOGRAPH,
        DIGITS,
    }

    public enum type
    {
        OSCILLATOR,
        PITCH_ENVELOPE,
        ADSR,
        DISPLAY,
        FILTER,
        CONTROL_PANEL,
    }

    public enum ControlPanelControls
    {
        SAVE_TO_FILE,
        SAVE_AS_TO_FILE,
        LOAD_FROM_FILE,
        FACTORY_PATCHES,
        LAYOUT,
        SETTINGS,
        MIDI_SETTINGS,
        MANUAL,
        USING_GRAPHICS_CARD,
        NONE
    }

    public enum OtherControls
    {
        MODULATION_WHEEL = 1,
        DISPLAY,
        NONE
    }

    public enum WAVEFORM
    {
        SQUARE,
        SAW_DOWN,
        SAW_UP,
        TRIANGLE,
        SINE,
        RANDOM,
        NOISE,
        NONE,
    }

    public enum MODULATION
    {
        NONE,
        AM,
        FM_SINE,
        FM,
        PM,
    }

    public sealed partial class MainPage : Page
    {
        Brush color;

        int numberOfOscillatorControlEnums = 15;

        /// <summary>
        /// Horizontal start point of grid for layout of controls
        /// </summary>
        int gridStartX = 12;

        /// <summary>
        /// Vertical start point of grid for layout of controls
        /// </summary>
        int gridStartY = 41;

        /// <summary>
        /// Horizontal spacing in grid for layout of controls
        /// </summary>
        int gridSpacingX = 475;

        /// <summary>
        /// Vertical spacing in grid for layout of controls
        /// </summary>
        int gridSpacingY = 207;

        // Oscillators layout:

        /// <summary>
        /// Number of oscillators:
        /// </summary>
        int oscillatorCount = 4;

        /// <summary>
        /// Column of leftmost oscillator(s) in grid for layout of controls
        /// </summary>
        int oscillatorsX = 0;

        /// <summary>
        /// Row of topmost oscillator(s) in grid for layout of controls
        /// </summary>
        int oscillatorsY = 0;

        /// <summary>
        /// Number of columns to skip for oscillators in layout of controls
        /// </summary>
        int oscillatorsSkipX = 1;

        /// <summary>
        /// Number of rows to skip for oscillators in layout of controls
        /// </summary>
        int oscillatorsSkipY = 0;

        // Filters layout:

        /// <summary>
        /// Number of filters:
        /// </summary>
        int filterCount = 2;

        /// <summary>
        /// Column of leftmost filter(s) in grid for layout of controls
        /// </summary>
        int filtersX = 2;

        /// <summary>
        /// Row of topmost filter(s) in grid for layout of controls
        /// </summary>
        int filtersY = 2;

        /// <summary>
        /// Number of columns to skip for filters in layout of controls
        /// </summary>
        int filtersSkipX = 0;

        /// <summary>
        /// Number of rows to skip for filters in layout of controls
        /// </summary>
        int filtersSkipY = 0;

        // Pitch ADSR layout:

        /// <summary>
        /// Number of pitch ADSRs:
        /// </summary>
        int pitchEnvelopesCount = 2;

        /// <summary>
        /// Column of leftmost display(s) in grid for layout of controls
        /// </summary>
        int pitchEnvelopesX = 0;

        /// <summary>
        /// Row of topmost display(s) in grid for layout of controls
        /// </summary>
        int pitchEnvelopesY = 2;

        /// <summary>
        /// Number of columns to skip for displays in layout of controls
        /// </summary>
        int pitchEnvelopesSkipX = 0;

        /// <summary>
        /// Number of rows to skip for displays in layout of controls
        /// </summary>
        int pitchEnvelopesSkipY = 0;

        // Adsrs layout:

        /// <summary>
        /// Number of adsrs:
        /// </summary>
        int adsrCount = 4;

        /// <summary>
        /// Column of leftmost adsr(s) in grid for layout of controls
        /// </summary>
        int adsrsX = 1;

        /// <summary>
        /// Row of topmost adsr(s) in grid for layout of controls
        /// </summary>
        int adsrsY = 0;

        /// <summary>
        /// Number of columns to skip for adsrs in layout of controls
        /// </summary>
        int adsrsSkipX = 1;

        /// <summary>
        /// Number of rows to skip for adsrs in layout of controls
        /// </summary>
        int adsrsSkipY = 0;

        /// <summary>
        /// Tre av layouterna lämnar lite tomrum Jag har gjort ett par
        /// bilder för att fylla det. En bild som fyller en panel och 
        /// en bild som fyller två paneler.
        /// </summary>
        int fillerSize = 0;

        /// <summary>
        /// List of oscillators that should have an active output socket
        /// for wiring to another oscillator with lower Id. Note that the order of
        /// oscillator Id's must be set to indicate what input can be connected to.
        /// The numeric suffixes indicates the number of columns/rows the cable goes.
        /// </summary>
        int[] oscillatorsWithOutputs1D0L;
        int[] oscillatorsWithOutputs2D0L;
        int[] oscillatorsWithOutputs3D0L;
        int[] oscillatorsWithOutputs0D1R;
        int[] oscillatorsWithOutputs0D2R;
        int[] oscillatorsWithOutputs0D3R;
        int[] oscillatorsWithOutputs1D1R;
        int[] oscillatorsWithOutputs1D2R;
        int[] oscillatorsWithOutputs1D3R;
        int[] oscillatorsWithOutputs2D1R;
        int[] oscillatorsWithOutputs2D2R;
        int[] oscillatorsWithOutputs2D3R;
        int[] oscillatorsWithOutputs1D1L;
        int[] oscillatorsWithOutputs1D2L;
        int[] oscillatorsWithOutputs1D3L;
        int[] oscillatorsWithOutputs2D1L;
        int[] oscillatorsWithOutputs2D2L;
        int[] oscillatorsWithOutputs2D3L;

        /// <summary>
        /// List of oscillators that should have active input sockets
        /// </summary>
        int[] oscillatorsWithInputs1D0L;
        int[] oscillatorsWithInputs2D0L;
        int[] oscillatorsWithInputs3D0L;
        int[] oscillatorsWithInputs0D1R;
        int[] oscillatorsWithInputs0D2R;
        int[] oscillatorsWithInputs0D3R;
        int[] oscillatorsWithInputs1D1R;
        int[] oscillatorsWithInputs1D2R;
        int[] oscillatorsWithInputs1D3R;
        int[] oscillatorsWithInputs2D1R;
        int[] oscillatorsWithInputs2D2R;
        int[] oscillatorsWithInputs2D3R;
        int[] oscillatorsWithInputs1D1L;
        int[] oscillatorsWithInputs1D2L;
        int[] oscillatorsWithInputs1D3L;
        int[] oscillatorsWithInputs2D1L;
        int[] oscillatorsWithInputs2D2L;
        int[] oscillatorsWithInputs2D3L;

        /// <summary>
        /// Sets up the grid for the controls that are depending on what layout
        /// is selected. The grid is the entire application area minus small
        /// margins and the lower part that contains the fixed controls. The
        /// layout controls where to put the different components oscillators,
        /// filters, envelope generators (ADSR), displays and wiring positions.
        /// </summary>
        private void SetupGrid()
        {
            switch (Patch.Layout)
            {
                case Layouts.FOUR_OSCILLATORS:
                    oscillatorCount = 4;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 3;
                    oscillatorsSkipY = 1;
                    oscillatorsWithOutputs1D0L = new int[] { 0, 1, 2, -1 };
                    oscillatorsWithInputs1D0L = new int[] { 1, 2, 3, -1 };
                    oscillatorsWithOutputs2D0L = new int[] { 0, 1, -1, -1 };
                    oscillatorsWithInputs2D0L = new int[] { 2, 3, -1, -1 };
                    oscillatorsWithOutputs3D0L = new int[] { 0, -1, -1, -1 };
                    oscillatorsWithInputs3D0L = new int[] { 3, -1, -1, -1 };
                    oscillatorsWithOutputs0D1R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs0D1R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs0D2R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs0D2R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs0D3R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs0D3R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D1R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs1D2R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D3R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3R = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D1L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs1D2L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D3L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3L = new int[] { -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3L = new int[] { -1, -1, -1, -1 };

                    filterCount = 4;
                    filtersX = 1;
                    filtersY = 0;
                    filtersSkipX = 3;
                    filtersSkipY = 1;
                    //displayCount = 4;
                    pitchEnvelopesCount = 4;
                    pitchEnvelopesX = 2;
                    pitchEnvelopesY = 0;
                    pitchEnvelopesSkipX = 3;
                    pitchEnvelopesSkipY = 1;
                    adsrCount = 4;
                    adsrsX = 3;
                    adsrsY = 0;
                    adsrsSkipX = 3;
                    adsrsSkipY = 1;
                    fillerSize = 0;
                    break;
                case Layouts.SIX_OSCILLATORS:
                    oscillatorCount = 6;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 1;
                    oscillatorsSkipY = 0;
                    oscillatorsWithOutputs1D0L = new int[] { 0, 1, 2, 3, -1, -1 };
                    oscillatorsWithInputs1D0L  = new int[] { 2, 3, 4, 5, -1, -1 };
                    oscillatorsWithOutputs2D0L = new int[] { 0, 1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D0L  = new int[] { 4, 5, -1, -1, -1, -1 };
                    oscillatorsWithOutputs3D0L = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs3D0L  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs0D1R = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs0D1R  = new int[] { 1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs0D2R = new int[] { 0, -1, 2, -1, 4, -1 };
                    oscillatorsWithInputs0D2R  = new int[] { 1, -1, 3, -1, 5, -1 };
                    oscillatorsWithOutputs0D3R = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs0D3R  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D1R = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1R  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2R = new int[] { 0, -1, 2, -1, -1, -1 };
                    oscillatorsWithInputs1D2R  = new int[] { 3, -1, 5, -1, -1, -1 };
                    oscillatorsWithOutputs1D3R = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3R  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1R = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1R  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2R = new int[] { 0, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2R  = new int[] { 5, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3R = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3R  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D1L = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1L  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2L = new int[] { -1, 1, -1, 3, -1, -1 };
                    oscillatorsWithInputs1D2L  = new int[] { -1, 2, -1, 4, -1, -1 };
                    oscillatorsWithOutputs1D3L = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3L  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1L = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1L  = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2L = new int[] { -1, 1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2L  = new int[] { -1, 4, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3L = new int[] { -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3L  = new int[] { -1, -1, -1, -1, -1, -1 };

                    filterCount = 6;
                    filtersX = 1;
                    filtersY = 0;
                    filtersSkipX = 1;
                    filtersSkipY = 0;
                    pitchEnvelopesCount = 1;
                    pitchEnvelopesX = 2;
                    pitchEnvelopesY = 3;
                    pitchEnvelopesSkipX = 0;
                    pitchEnvelopesSkipY = 0;
                    adsrCount = 1;
                    adsrsX = 3;
                    adsrsY = 3;
                    adsrsSkipX = 0;
                    adsrsSkipY = 0;
                    fillerSize = 2;
                    break;
                case Layouts.EIGHT_OSCILLATORS:
                    oscillatorCount = 8;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 0;
                    oscillatorsSkipY = 1;
                    oscillatorsWithOutputs1D0L = new int[] { 0, 1, 2, 3, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D0L  = new int[] { 4, 5, 6, 7, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D0L = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D0L  = new int[] { 2, 3, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs3D0L = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs3D0L  = new int[] { 3, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs0D1R = new int[] { 0, 1, 2, -1, 4, 5, 6, -1 };
                    oscillatorsWithInputs0D1R  = new int[] { 1, 2, 3, -1, 5, 6, 7, -1 };
                    oscillatorsWithOutputs0D2R = new int[] { 0, 1, -1, -1, 4, 5, -1, -1 };
                    oscillatorsWithInputs0D2R  = new int[] { 2, 3, -1, -1, 6, 7, -1, -1 };
                    oscillatorsWithOutputs0D3R = new int[] { 0, -1, -1, -1, 4, -1, -1, -1 };
                    oscillatorsWithInputs0D3R  = new int[] { 3, -1, -1, -1, 7, -1, -1, -1 };
                    oscillatorsWithOutputs1D1R = new int[] { 0, 1, 2, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1R  = new int[] { 5, 6, 7, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2R = new int[] { 0, 1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D2R  = new int[] { 6, 7, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D3R = new int[] { 0, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3R  = new int[] { 7, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1R = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1R  = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2R = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2R  = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3R = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3R  = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D1L = new int[] { -1, 1, 2, 3, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1L  = new int[] { -1, 4, 5, 6, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2L = new int[] { -1, -1, 2, 3, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D2L  = new int[] { -1, -1, 4, 5, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D3L = new int[] { -1, -1, -1, 3, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3L  = new int[] { -1, -1, -1, 4, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1L = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1L  = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2L = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2L  = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3L = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3L  = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };

                    filterCount = 4;
                    filtersX = 0;
                    filtersY = 2;
                    filtersSkipX = 0;
                    filtersSkipY = 0;
                    pitchEnvelopesCount = 1;
                    pitchEnvelopesX = 2;
                    pitchEnvelopesY = 3;
                    pitchEnvelopesSkipX = 0;
                    pitchEnvelopesSkipY = 0;
                    adsrCount = 1;
                    adsrsX = 3;
                    adsrsY = 3;
                    adsrsSkipX = 0;
                    adsrsSkipY = 0;
                    fillerSize = 2;
                    break;
                case Layouts.TWELVE_OSCILLATORS:
                    oscillatorCount = 12;
                    oscillatorsX = 0;
                    oscillatorsY = 0;
                    oscillatorsSkipX = 0;
                    oscillatorsSkipY = 0;
                    oscillatorsWithOutputs1D0L = new int[] {  0,   1,  2,  3,  4,  5,  6,  7, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D0L  = new int[] {  4,   5,  6,  7,  8,  9, 10, 11, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D0L = new int[] {  0,   1,  2,  3, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D0L  = new int[] {  8,   9, 10, 11, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs3D0L = new int[] { -1,  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs3D0L  = new int[] { -1,  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs0D1R = new int[] {  0,   1,  2, -1,  4,  5,  6, -1,  8,  9, 10, -1 };
                    oscillatorsWithInputs0D1R  = new int[] {  1,   2,  3, -1,  5,  6,  7, -1,  9, 10, 11, -1 };
                    oscillatorsWithOutputs0D2R = new int[] {  0,   1, -1, -1,  4,  5, -1, -1,  8,  9, -1, -1 };
                    oscillatorsWithInputs0D2R  = new int[] {  2,   3, -1, -1,  6,  7, -1, -1, 10, 11, -1, -1 };
                    oscillatorsWithOutputs0D3R = new int[] {  0,  -1, -1, -1,  4, -1, -1, -1,  8, -1, -1, -1 };
                    oscillatorsWithInputs0D3R  = new int[] {  3,  -1, -1, -1,  7, -1, -1, -1, 11, -1, -1, -1 };
                    oscillatorsWithOutputs1D1R = new int[] {  0,   1,  2, -1,  4,  5,  6, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1R  = new int[] {  5,   6,  7, -1,  9, 10, 11, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2R = new int[] {  0,   1, -1, -1,  4,  5, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D2R  = new int[] {  6,   7, -1, -1, 10, 11, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D3R = new int[] {  0,  -1, -1, -1,  4, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3R  = new int[] {  7,  -1, -1, -1, 11, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1R = new int[] {  0,   1,  2, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1R  = new int[] {  9,  10, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2R = new int[] {  0,   1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2R  = new int[] {  10, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3R = new int[] {  0,  -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3R  = new int[] {  11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

                    oscillatorsWithOutputs1D1L = new int[] { -1,  1,  2,  3, -1, 5, 6, 7, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D1L  = new int[] { -1,  4,  5,  6, -1, 8, 9, 10, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D2L = new int[] { -1, -1,  2,  3, -1, -1, 6, 7, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D2L  = new int[] { -1, -1,  4,  5, -1, -1, 8, 9, -1, -1, -1, -1 };
                    oscillatorsWithOutputs1D3L = new int[] { -1, -1, -1,  3, -1, -1, -1, 7, -1, -1, -1, -1 };
                    oscillatorsWithInputs1D3L  = new int[] { -1, -1, -1,  4, -1, -1, -1, 8, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D1L = new int[] { -1,  1,  2,  3, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D1L  = new int[] { -1,  8,  9, 10, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D2L = new int[] { -1, -1,  2,  3, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D2L  = new int[] { -1, -1,  8,  9, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithOutputs2D3L = new int[] { -1, -1, -1,  3, -1, -1, -1, -1, -1, -1, -1, -1 };
                    oscillatorsWithInputs2D3L  = new int[] { -1, -1, -1,  8, -1, -1, -1, -1, -1, -1, -1, -1 };

                    filterCount = 1;
                    filtersX = 0;
                    filtersY = 3;
                    filtersSkipX = 0;
                    filtersSkipY = 0;
                    pitchEnvelopesCount = 1;
                    pitchEnvelopesX = 2;
                    pitchEnvelopesY = 3;
                    pitchEnvelopesSkipX = 0;
                    pitchEnvelopesSkipY = 0;
                    adsrCount = 1;
                    adsrsX = 3;
                    adsrsY = 3;
                    adsrsSkipX = 0;
                    adsrsSkipY = 0;
                    fillerSize = 1;
                    break;
            }
        }

        //public void CreateFilters()
        //{

        //    Patch.Filter = new Filter[32][];
        //    for (int poly = 0; poly < 32; poly++)
        //    {
        //        Patch.Filter[poly] = new Filter[12];
        //        for (int osc = 0; osc < 12; osc++)
        //        {
        //            Patch.Filter[poly][osc] = new Filter(this, poly, osc, SampleCount);
        //        }
        //    }
        //}

        //public void CreateWaveShapes()
        //{
        //    WaveShape = new WaveShape[32][];
        //    for (int poly = 0; poly < 32; poly++)
        //    {
        //        WaveShape[poly] = new WaveShape[12];
        //        for (int osc = 0; osc < 12; osc++)
        //        {
        //            WaveShape[poly][osc] = new WaveShape(this, poly, osc);
        //        }
        //    }
        //}

        /// <summary>
        /// Creates the fixed controls, Modulation wheel, keyboard, logotype, patch select buttons
        /// and the lower right corner buttons. These controls are created once and used during the
        /// entire session.
        /// </summary>
        public void CreateKeyboardAndControlPanel()
        {
            Controls = new Controls(Window.Current.Bounds, imgClickArea);
            Controls.InitControls(gridControls);

            // Control panel:
            ControlPanel = Controls.AddCompoundControl(
                new Rect(0, 0, imgBackground.ActualWidth, imgBackground.ActualHeight),
                        imgClickArea, 0, (int)type.CONTROL_PANEL, gridControlPanel, new Image[] { imgControlPanelBackground },
                        new Rect(1437, 869, imgControlPanelBackground.ActualWidth, imgControlPanelBackground.ActualHeight));
            ControlPanel.AddRotator((int)ControlPanelControls.SAVE_TO_FILE, gridControlPanel,
                new Image[] { imgSaveToFile },
                new Point(ControlPanelLayout.X, ControlPanelLayout.Y));
            ControlPanel.AddRotator((int)ControlPanelControls.SAVE_AS_TO_FILE, gridControlPanel,
                new Image[] { imgSaveAsToFile },
                new Point(ControlPanelLayout.X + imgSaveToFile.ActualWidth, ControlPanelLayout.Y));
            ControlPanel.AddRotator((int)ControlPanelControls.LOAD_FROM_FILE, gridControlPanel,
                new Image[] { imgLoadFromFile },
                new Point(ControlPanelLayout.X + imgSaveToFile.ActualWidth + imgSaveAsToFile.ActualWidth, ControlPanelLayout.Y));
            ControlPanel.AddRotator((int)ControlPanelControls.FACTORY_PATCHES, gridControlPanel,
                new Image[] { imgFactoryPatches },
                new Point(ControlPanelLayout.X, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 1));
            ControlPanel.AddRotator((int)ControlPanelControls.LAYOUT, gridControlPanel,
                new Image[] { imgLayout4, imgLayout6, imgLayout8, imgLayout12 },
                new Point(ControlPanelLayout.X, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 2));
            //ControlPanel.AddRotator((int)ControlPanelControls.MIDI_SETTINGS, gridControlPanel,
            //    new Image[] { imgMidiSettings },
            //    new Point(ControlPanelLayout.X, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 3));
            //ControlPanel.AddRotator((int)ControlPanelControls.SETTINGS, gridControlPanel,
            //    new Image[] { imgSettings },
            //    new Point(ControlPanelLayout.X + imgMidiSettings.ActualWidth, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 3));
            //ControlPanel.AddRotator((int)ControlPanelControls.MANUAL, gridControlPanel,
            //    new Image[] { imgManual },
            //    new Point(ControlPanelLayout.X + imgMidiSettings.ActualWidth + imgSettings.ActualWidth, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 3));
            ControlPanel.AddRotator((int)ControlPanelControls.SETTINGS, gridControlPanel,
                new Image[] { imgSettings },
                new Point(ControlPanelLayout.X, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 3));
            ControlPanel.AddRotator((int)ControlPanelControls.MIDI_SETTINGS, gridControlPanel,
                new Image[] { imgMidiSettings },
                new Point(ControlPanelLayout.X + imgSettings.ActualWidth, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 3));
            ControlPanel.AddRotator((int)ControlPanelControls.MANUAL, gridControlPanel,
                new Image[] { imgManual },
                new Point(ControlPanelLayout.X, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 4));
            //ControlPanel.AddRotator((int)ControlPanelControls.USING_GRAPHICS_CARD, gridControlPanel,
            //    new Image[] { imgUsingGraphicsCardOff, imgUsingGraphicsCardOn },
            //    new Point(ControlPanelLayout.X, ControlPanelLayout.Y + ControlPanelLayout.Y_Spacing * 4));
            //if (usingGraphicsCard)
            //{
            //    ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.USING_GRAPHICS_CARD]).Selection = 1;
            //}
            ControlPanel.AddStaticImage(0, gridControlPanel, new Image[] { imgLogotype }, new Point(30, 11));
            numberOfFixedControls++;

            // Modulator wheel:
            Controls.AddVerticalSlider((int)OtherControls.MODULATION_WHEEL, gridControlPanel,
                new Image[] { imgModulatorWheelBackground, imgModulatorWheelHandle }, new Rect(gridStartX + gridSpacingX - 6, gridStartY + gridSpacingY * 4, 43, 157), false, 0, 127);
            numberOfFixedControls++;

            // Keyboard:
            Keyboard keyboard = Controls.AddKeyBoard(Controls.ControlsList.Count, gridMain,
                imgWhiteKey, imgBlackKey, new Point(gridStartX + gridSpacingX + 44, gridStartY + gridSpacingY * 4), 36, 84);
            foreach (Octave octave in keyboard.Octaves)
            {
                foreach (Key key in octave.Keys)
                {
                    key.Image.PointerMoved += Keyboard_PointerMoved;
                    key.Image.PointerPressed += Keyboard_PointerPressed;
                    key.Image.PointerReleased += Keyboard_PointerReleased;
                }
            }
        }

        /// <summary>
        /// Creates the controls that are different depending on layout. Those controls
        /// occupy the area above the fixed controls, and are replaced whenever the
        /// layout is changed.
        /// </summary>
        public void CreateControls()
        {
            SetupGrid();
            gridCanvases.Children.Clear();
            adsrCanvas = new Canvas();
            oscilloscopeCanvas = new Canvas();
            gridCanvases.Children.Add(adsrCanvas);
            gridCanvases.Children.Add(oscilloscopeCanvas);
            Controls.InitControls(gridControls);
            gridControls.Children.Clear();
            OscillatorGUIs = new CompoundControl[oscillatorCount];
            FilterGUIs = new CompoundControl[filterCount];
            PitchEnvelopeGUIs = new CompoundControl[pitchEnvelopesCount];
            AdsrGUIs = new CompoundControl[adsrCount];

            int displayDigitsBackgroundX = 13;
            int displayDigitsBackgroundY = 13;
            int displayDigitsX = 4;
            int displayDigitsY = 60;
            int displayOscilloscopeBackgroundX = 238;
            int displayOscilloscopeBackgroundY = 13;

            int knobOscillatorX = 47;
            int knobOscillatorY = 120;
            int knobOscillatorSpacingX = 81;
            int buttonOscillatorX = 333;
            int buttonOscillatorY = 12;
            int buttonOscillatorSpacingY = 31;
            int modButtonOscillatorX = 10;
            int indicatorOscillatorX = 431;
            int indicatorOscillatorY = 137;

            // Input buttons offsets and spacing:
            int inputOffsetX = 71;
            int inputOffsetY = 11;
            int inputSpacing = 102;

            // Output button offset:
            int outputOffsetX = 426;
            int outputOffsetY = 160;

            // Oscillator number:
            int oscillatorNumberX = 368;
            int oscillatorNumberY = 145;

            int knobFilterX = 51;
            int knobFilterY = 121;
            int knobFiltersSpacingX = 83;
            int buttonFunctioFilterX = 10;
            int buttonModFilterX = 206;
            int buttonFilterY = 12;

            int knobPitchEnvelopeX = 332;
            int knobPitchEnvelopeSpacingX = 81;
            int knobPitchEnvelopeY = 133;
            int buttonPitchEnvelopeSpacingY = 26;
            int buttonPitchEnvelopeModSelectorX = 290;
            int buttonPitchEnvelopeModSpacingX = 84;
            int buttonPitchEnvelopeModSelectorY = 12;

            int knobAdsrX = 47;
            int knobAdsrY = 132;
            int knobAdsrSpacingX = 81;
            int buttonAdsrX = 333;
            int buttonAdsrY = 12;
            int buttonAdsrSpacingY = 31;
            int modButtonAdsrX = 10;

            // Display:
            Rect hitArea =
                new Rect(gridStartX, gridStartY + gridSpacingY * 4,
                    imgGraphDisplayBackground.ActualWidth, imgGraphDisplayBackground.ActualHeight);
            DisplayGUI = Controls.AddCompoundControl(new Rect(gridStartX, gridStartY + gridSpacingY * 4,
                    imgDisplayBackground.ActualWidth, imgDisplayBackground.ActualHeight), imgClickArea,
                (int)OtherControls.DISPLAY, (int)type.DISPLAY, gridControls, new Image[] { imgDisplayBackground },
                hitArea);
            DisplayGUI.AddGraph((int)DisplayControls.FREQUENCY, gridControls, new Image[] { imgGraphDisplayBackground },
                new Point(displayDigitsBackgroundX, displayDigitsBackgroundY), new SolidColorBrush(Windows.UI.Colors.Chartreuse));
            DisplayGUI.AddGraph((int)DisplayControls.OSCILLOGRAPH, gridControls, new Image[] { imgGraphDisplayBackground },
                new Point(displayOscilloscopeBackgroundX, displayOscilloscopeBackgroundY), new SolidColorBrush(Windows.UI.Colors.Chartreuse));
            DisplayGUI.AddDigitalDisplay((int)DisplayControls.DIGITS, gridControls,
                new Image[] { img0, img1, img2, img3, img4, img5, img6, img7, img8, img9, img0Dot,
                            img1Dot, img2Dot, img3Dot, img4Dot, img5Dot, img6Dot, img7Dot, img8Dot, img9Dot },
                new Point(displayDigitsX, displayDigitsY), 7, 2);
            ((DigitalDisplay)DisplayGUI.SubControls.ControlsList[(int)DisplayControls.DIGITS]).DisplayValue(440);
            oscilloscope = new Oscilloscope(this);
            oscilloscope.oscilloskopOffset = new Point(gridStartX + displayOscilloscopeBackgroundX,
                gridStartY + displayOscilloscopeBackgroundY + gridSpacingY * 4);
            numberOfFixedControls++;

            // Oscillators:
            int i = 0;
            for (int y = 0; y < LimitY(oscillatorCount, oscillatorsSkipX, oscillatorsSkipY); y++)
            {
                for (int x = 0; x < LimitX(oscillatorCount, oscillatorsSkipX, oscillatorsSkipY); x++)
                {
                    OscillatorGUIs[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackground.ActualWidth, imgBackground.ActualHeight),
                        imgClickArea, i, (int)type.OSCILLATOR, gridControls, new Image[] { imgOscillatorBackground },
                        AssembleHitarea(type.OSCILLATOR, x, y, 0, 0));
                    OscillatorGUIs[i].AddKnob((int)OscillatorControls.MODULATION, gridControls, new Image[] { imgKnob },
                        new Point(knobOscillatorX, knobOscillatorY), false, 0, 256, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].AM_Sensitivity;
                    OscillatorGUIs[i].AddKnob((int)OscillatorControls.FREQUENCY, gridControls, new Image[] { imgKnob },
                        new Point(knobOscillatorX + knobOscillatorSpacingX, knobOscillatorY), false, 0, 1000, 30, 330, 1)
                        .Value = (int)Patch.Oscillators[0][i].Frequency / 10;
                    OscillatorGUIs[i].AddKnob((int)OscillatorControls.FINE_TUNE, gridControls, new Image[] { imgKnob },
                        new Point(knobOscillatorX + knobOscillatorSpacingX * 2, knobOscillatorY), false, 0, 999, 30, 330, 1)
                        .Value = (int)Patch.Oscillators[0][i].FineTune;
                    OscillatorGUIs[i].AddKnob((int)OscillatorControls.VOLUME, gridControls, new Image[] { imgKnob },
                        new Point(knobOscillatorX + knobOscillatorSpacingX * 3, knobOscillatorY), false, 0, 127, 30, 330)
                        .Value = (int)Patch.Oscillators[0][i].Volume;
                    OscillatorGUIs[i].AddRotator((int)OscillatorControls.WAVE, gridControls,
                        new Image[] { imgSelectSquare, imgSelectSawDwn, imgSelectSawUp, imgSelectTriangle, imgSelectSine, imgSelectRandom, imgSelectNoise },
                        new Point(buttonOscillatorX, buttonOscillatorY))
                        .Selection = (int)Patch.Oscillators[0][i].WaveForm;
                    OscillatorGUIs[i].AddRotator((int)OscillatorControls.KEYBOARD_OR_FIXED, gridControls,
                        new Image[] { imgSelectKeyboard, imgSelectFixFreq },
                        new Point(buttonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY))
                        .Selection = Patch.Oscillators[0][i].Keyboard ? 0 : 1;
                    OscillatorGUIs[i].AddRotator((int)OscillatorControls.ADSR_OR_PULSE, gridControls,
                        new Image[] { imgSelectorAdsr, imgSelectorPulse/*, imgSelectorAdsrFm, imgSelectorPulseFm*/ },
                        new Point(buttonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY * 2))
                        .Selection = Patch.Oscillators[0][i].Adsr.Pulse ? 1 : 0;
                    OscillatorGUIs[i].AddRotator((int)OscillatorControls.VIEW_ME, gridControls,
                        new Image[] { imgSelectorViewOff, imgSelectorViewOn },
                        new Point(buttonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY * 3))
                        .Selection = Patch.Oscillators[0][i].ViewMe ? 1 : 0;
                    OscillatorGUIs[i].AddIndicator((int)OscillatorControls.LEDSOUNDING, gridControls, new Image[] { imgLedOn },
                        new Point(indicatorOscillatorX, indicatorOscillatorY))
                        .IsOn = Patch.Oscillators[0][i].Volume > 0;
                    OscillatorGUIs[i].AddRotator((int)OscillatorControls.MODULATION_KNOB_TARGET, gridControls,
                        new Image[] { imgModAM, imgModFM, imgModPM },
                        new Point(modButtonOscillatorX, buttonOscillatorY + buttonOscillatorSpacingY))
                        .Selection = Patch.Oscillators[0][i].ModulationKnobTarget;
                    OscillatorGUIs[i].AddRotator((int)OscillatorControls.MODULATION_WHEEL_TARGET, gridControls,
                        new Image[] { imgOscillatorModWheelOff, imgOscillatorModWheelAM, imgOscillatorModWheelFM,
                            imgOscillatorModWheelPM, imgOscillatorModWheelFrequency, imgOscillatorModWheelFinetune,
                            imgOscillatorModWheelVolume },
                        new Point(modButtonOscillatorX + 130, buttonOscillatorY + buttonOscillatorSpacingY))
                        .Selection = Patch.Oscillators[0][i].ModulationWheelTarget;
                    OscillatorGUIs[i].AddAreaButton((int)OscillatorControls.AM_SOCKET, gridControls,
                        new Rect(inputOffsetX, inputOffsetY, 30, 30));
                    OscillatorGUIs[i].AddAreaButton((int)OscillatorControls.FM_SOCKET, gridControls,
                        new Rect(inputOffsetX + inputSpacing, inputOffsetY, 30, 30));
                    OscillatorGUIs[i].AddAreaButton((int)OscillatorControls.XM_SOCKET, gridControls,
                        new Rect(inputOffsetX + inputSpacing * 2, inputOffsetY, 30, 30));
                    OscillatorGUIs[i].AddAreaButton((int)OscillatorControls.OUT_SOCKET, gridControls,
                        new Rect(outputOffsetX, outputOffsetY, 30, 30));
                    OscillatorGUIs[i].AddRotator((int)OscillatorControls.NUMBER, gridControls,
                        new Image[] { imgOscNumber1, imgOscNumber2, imgOscNumber3, imgOscNumber4, imgOscNumber5, imgOscNumber6, 
                            imgOscNumber7, imgOscNumber8, imgOscNumber9, imgOscNumber10, imgOscNumber11, imgOscNumber12 },
                        new Point(oscillatorNumberX, oscillatorNumberY)).Selection = i;
                    i++;
                }
            }

            // Filters:
            i = 0;
            for (int y = 0; y < LimitY(filterCount, filtersSkipX, filtersSkipY); y++)
            {
                for (int x = 0; x < LimitX(filterCount, filtersSkipX, filtersSkipY); x++)
                {
                    FilterGUIs[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackground.ActualWidth, imgBackground.ActualHeight), imgClickArea,
                        i, (int)type.FILTER,
                        gridControls, new Image[] { imgFilterBackground },
                        AssembleHitarea(type.FILTER, x, y, 0, 0));
                    FilterGUIs[i].AddKnob((int)FilterControls.Q, gridControls, new Image[] { imgKnob },
                        new Point(knobFilterX, knobFilterY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].Filter.Q;
                    FilterGUIs[i].AddKnob((int)FilterControls.FREQUENCY_CENTER, gridControls, new Image[] { imgKnob },
                        new Point(knobFilterX + knobFiltersSpacingX, knobFilterY), false, 0, 127, 30, 330, 1)
                        .Value = (int)Patch.Oscillators[0][i].Filter.FrequencyCenter;
                    FilterGUIs[i].AddKnob((int)FilterControls.KEYBOARD_FOLLOW, gridControls, new Image[] { imgKnob },
                        new Point(knobFilterX + knobFiltersSpacingX * 2, knobFilterY), false, 0, 127, 30, 330, 1)
                        .Value = (int)Patch.Oscillators[0][i].Filter.KeyboardFollow;
                    FilterGUIs[i].AddKnob((int)FilterControls.GAIN, gridControls, new Image[] { imgKnob },
                        new Point(knobFilterX + knobFiltersSpacingX * 3, knobFilterY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].Filter.Gain;
                    FilterGUIs[i].AddKnob((int)FilterControls.FILTER_MIX, gridControls, new Image[] { imgKnob },
                        new Point(knobFilterX + knobFiltersSpacingX * 4, knobFilterY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].Filter.Mix;
                    FilterGUIs[i].AddRotator((int)FilterControls.FILTER_FUNCTION, gridControls,
                        new Image[] { imgFilterOff, imgFilterFixed, imgFilterAdsrPositive, imgFilterAdsrNegative,
                            imgFilterPitchEnv, imgFilterXmFreq },
                        new Point(buttonFunctioFilterX, buttonFilterY))
                        .Selection = Patch.Oscillators[0][i].Filter.FilterFunction;
                    FilterGUIs[i].AddRotator((int)FilterControls.MODULATION_WHEEL_TARGET, gridControls,
                        new Image[] { imgModFilterOff, imgModFilterQ, imgModFilterFreq, imgModFilterKeyFollow, 
                            imgModFilterGain, imgModFilterMix },
                        new Point(buttonModFilterX, buttonFilterY))
                        .Selection = Patch.Oscillators[0][i].Filter.ModulationWheelTarget;
                    ((Knob)FilterGUIs[i].SubControls.ControlsList[(int)FilterControls.KEYBOARD_FOLLOW]).Value = 63;
                    i++;
                }
            }

            // Pitch Envelopes:
            i = 0;
            for (int y = 0; y < LimitY(pitchEnvelopesCount, pitchEnvelopesSkipX, pitchEnvelopesSkipY); y++)
            {
                for (int x = 0; x < LimitX(pitchEnvelopesCount, pitchEnvelopesSkipX, pitchEnvelopesSkipY); x++)
                {
                    PitchEnvelopeGUIs[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackground.ActualWidth, imgBackground.ActualHeight), imgClickArea,
                        i, (int)type.PITCH_ENVELOPE,
                        gridControls, new Image[] { imgPitchEnvelopeBackground },
                        AssembleHitarea(type.PITCH_ENVELOPE, x, y, 0, 0));
                    PitchEnvelopeGUIs[i].AddRotator((int)PitchEnvelopeControls.PITCH_ENV_MODULATION_WHEEL_USE, gridControls,
                        new Image[] { imgModWheelOff, imgModWheelDepth, imgModWheelSpeed },
                        new Point(buttonPitchEnvelopeModSelectorX, buttonPitchEnvelopeModSelectorY))
                        .Selection = Patch.Oscillators[0][i].PitchEnvelope.PitchEnvModulationWheelTarget;
                    PitchEnvelopeGUIs[i].AddKnob((int)PitchEnvelopeControls.DEPTH, gridControls, new Image[] { imgKnob },
                        new Point(knobPitchEnvelopeX, knobPitchEnvelopeY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].PitchEnvelope.Depth;
                    PitchEnvelopeGUIs[i].AddKnob((int)PitchEnvelopeControls.SPEED, gridControls, new Image[] { imgKnob },
                        new Point(knobPitchEnvelopeX + knobPitchEnvelopeSpacingX, knobPitchEnvelopeY), false, 1, 300, 30, 330)
                        .Value = (int)Patch.Oscillators[0][i].PitchEnvelope.Speed;
                    PitchEnvelopeGUIs[i].AddRotator((int)PitchEnvelopeControls.MOD_PITCH, gridControls,
                        new Image[] { imgOscillatorFreqOff, imgOscillatorFreqOn },
                        new Point(buttonPitchEnvelopeModSelectorX,
                        buttonPitchEnvelopeModSelectorY + buttonPitchEnvelopeSpacingY))
                        .Selection = Patch.Oscillators[0][i].PitchEnvelope.PitchEnvPitch ? 1 : 0;
                    PitchEnvelopeGUIs[i].AddRotator((int)PitchEnvelopeControls.MOD_AM, gridControls,
                        new Image[] { imgOscillatorAMOff, imgOscillatorAMOn },
                        new Point(buttonPitchEnvelopeModSelectorX + buttonPitchEnvelopeModSpacingX,
                        buttonPitchEnvelopeModSelectorY + buttonPitchEnvelopeSpacingY))
                        .Selection = Patch.Oscillators[0][i].PitchEnvelope.PitchEnvAm ? 1 : 0;
                    PitchEnvelopeGUIs[i].AddRotator((int)PitchEnvelopeControls.MOD_FM, gridControls,
                        new Image[] { imgOscillatorFMOff, imgOscillatorFMOn },
                        new Point(buttonPitchEnvelopeModSelectorX,
                        buttonPitchEnvelopeModSelectorY + buttonPitchEnvelopeSpacingY * 2))
                        .Selection = Patch.Oscillators[0][i].PitchEnvelope.PitchEnvFm ? 1 : 0;
                    PitchEnvelopeGUIs[i].AddRotator((int)PitchEnvelopeControls.MOD_XM, gridControls,
                        new Image[] { imgOscillatorPMOff, imgOscillatorPMOn },
                        new Point(buttonPitchEnvelopeModSelectorX + buttonPitchEnvelopeModSpacingX,
                        buttonPitchEnvelopeModSelectorY + buttonPitchEnvelopeSpacingY * 2))
                        .Selection = Patch.Oscillators[0][i].PitchEnvelope.PitchEnvXm ? 1 : 0;
                    PitchEnvelopeGUIs[i].AddGraph((int)PitchEnvelopeControls.GRAPH, gridControls,
                        new Image[] { imgPitchEnvelopeGraphBackground }, new Point(13, 13),
                        new SolidColorBrush(Windows.UI.Colors.Chartreuse), 2)
                        .CopyPoints(Patch.Oscillators[0][i].PitchEnvelope.Points);
                    i++;
                }
            }

            for (int poly = 0; poly < Patch.Polyphony; poly++)
            {
                for (int osc = 0; osc < oscillatorCount; osc++)
                {
                    if (Patch.Layout == Layouts.FOUR_OSCILLATORS)
                    {
                        Patch.Oscillators[poly][osc].PitchEnvelope.SetBounds(
                            ((Graph)PitchEnvelopeGUIs[osc].SubControls
                            .ControlsList[(int)PitchEnvelopeControls.GRAPH]).HitArea);
                    }
                    else
                    {
                        Patch.Oscillators[poly][osc].PitchEnvelope.SetBounds(
                                ((Graph)PitchEnvelopeGUIs[0].SubControls
                                .ControlsList[(int)PitchEnvelopeControls.GRAPH]).HitArea);
                    }
                }
            }

            // Envelope generators (ADSR):
            i = 0;
            for (int y = 0; y < LimitY(adsrCount, adsrsSkipX, adsrsSkipY); y++)
            {
                for (int x = 0; x < LimitX(adsrCount, adsrsSkipX, adsrsSkipY); x++)
                {
                    AdsrGUIs[i] = Controls.AddCompoundControl(
                        new Rect(0, 0, imgBackground.ActualWidth, imgBackground.ActualHeight),
                        imgClickArea, i, (int)type.ADSR, gridControls, new Image[] { imgAdsrBackground },
                        AssembleHitarea(type.ADSR, x, y, 0, 0));
                    AdsrGUIs[i].AddKnob((int)AdsrControls.ADSR_A, gridControls, new Image[] { imgKnob },
                        new Point(knobAdsrX, knobAdsrY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].Adsr.AdsrAttackTime;
                    AdsrGUIs[i].AddKnob((int)AdsrControls.ADSR_D, gridControls, new Image[] { imgKnob },
                        new Point(knobAdsrX + knobAdsrSpacingX, knobAdsrY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].Adsr.AdsrDecayTime;
                    AdsrGUIs[i].AddKnob((int)AdsrControls.ADSR_S, gridControls, new Image[] { imgKnob },
                        new Point(knobAdsrX + knobAdsrSpacingX * 2, knobAdsrY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].Adsr.AdsrSustainLevel;
                    AdsrGUIs[i].AddKnob((int)AdsrControls.ADSR_R, gridControls, new Image[] { imgKnob },
                        new Point(knobAdsrX + knobAdsrSpacingX * 3, knobAdsrY), false, 0, 127, 30, 330, 2)
                        .Value = (int)Patch.Oscillators[0][i].Adsr.AdsrReleaseTime;
                    AdsrGUIs[i].AddRotator((int)AdsrControls.ADSR_MODULATION_WHEEL_USE, gridControls,
                        new Image[] { imgAdsrModOff, imgAdsrModA, imgAdsrModD, imgAdsrModS, imgAdsrModR },
                        new Point(buttonOscillatorX, buttonOscillatorY));
                    AdsrGUIs[i].AddRotator((int)AdsrControls.PEDAL_HOLD, gridControls,
                        new Image[] { imgSelectHoldOff, imgSelectHoldOn }, new Point(342, 145));
                    ((Knob)AdsrGUIs[i].SubControls.ControlsList[(int)AdsrControls.ADSR_S]).Value = 127;
                    AdsrGUIs[i].AddGraph((int)AdsrControls.GRAPH, gridControls,
                        new Image[] { imgAdsrGraphBackground }, new Point(15, 13),
                        new SolidColorBrush(Windows.UI.Colors.Chartreuse));
                    i++;
                }
            }

            switch (fillerSize)
            {
                case 1:
                    Controls.AddStaticImage(0, gridControls, new Image[] { imgFiller1 },
                        new Point(gridStartX + gridSpacingX, gridStartY + gridSpacingY * 3));
                    break;
                case 2:
                    Controls.AddStaticImage(0, gridControls, new Image[] { imgFiller2 },
                        new Point(gridStartX, gridStartY + gridSpacingY * 3));
                    break;
            }
        }

        public void CreateWiring()
        {
            // Positions:
            int x = 77;
            int yDown = 164;
            int ySame = 16;
            int xStep = 475;
            int yStep = 207;

            // Hanging wire:
            HangingWire = Controls.ControlsList.Count;
            Controls.AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgHangingWire }, new Point(100, 100));

            // Wires:
            Patch.Wiring.wires.Clear();
            for (int osc = 0; osc < oscillatorCount; osc++)
            {
                // Connections within the same column one row down
                Indicator indicator;
                if (oscillatorsWithOutputs1D0L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM1D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs1D0L[osc]), indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D0L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM1D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs1D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D0L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM1D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs1D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D0L[osc]].XM_ModulatorId == osc;
                }

                // Connections within the same column two rows down
                if (oscillatorsWithOutputs2D0L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM2D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs2D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D0L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM2D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs2D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D0L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM2D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs2D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D0L[osc]].XM_ModulatorId == osc;
                }

                // Connections within the same column three rows down
                if (oscillatorsWithOutputs3D0L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM3D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs3D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs3D0L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM3D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs3D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs3D0L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM3D0L },
                        new Point(x, yDown));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs3D0L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs3D0L[osc]].XM_ModulatorId == osc;
                }

                // Connections to one column right in the same row
                if (oscillatorsWithOutputs0D1R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM0D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs0D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D1R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM0D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs0D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D1R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM0D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs0D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D1R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs0D3R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM0D3R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs0D3R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D3R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM0D3R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs0D3R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D3R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM0D3R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs0D3R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D3R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs1D1R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM1D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs1D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D1R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM1D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs1D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D1R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM1D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs1D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D1R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs1D2R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM1D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs1D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM1D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs1D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM1D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs1D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs1D3R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM1D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs1D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM1D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs1D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM1D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs1D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs2D1R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM2D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs2D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D1R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM2D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs2D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D1R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM2D1R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs2D1R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D1R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs2D2R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM2D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs2D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D2R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM2D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs2D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D2R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM2D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs2D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D2R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs2D3R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM2D3R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs2D3R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D3R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM2D3R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs2D3R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D3R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM2D3R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs2D3R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D3R[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs1D1L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM1D1L },
                        new Point(x - xStep, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs1D1L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D1L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM1D1L },
                        new Point(x - xStep, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs1D1L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D1L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM1D1L },
                        new Point(x - xStep, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs1D1L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D1L[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs1D2L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM1D2L },
                        new Point(x - xStep * 2, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs1D2L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM1D2L },
                        new Point(x - xStep * 2, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs1D2L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM1D2L },
                        new Point(x - xStep * 2, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs1D2L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D2L[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs1D3L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM1D3L },
                        new Point(x - xStep * 3, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs1D3L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D3L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM1D3L },
                        new Point(x - xStep * 3, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs1D3L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D3L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM1D3L },
                        new Point(x - xStep * 3, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs1D3L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs1D3L[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs2D1L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM2D1L },
                        new Point(x - xStep, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs2D1L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D1L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM2D1L },
                        new Point(x - xStep, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs2D1L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D1L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM2D1L },
                        new Point(x - xStep, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs2D1L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D1L[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs2D2L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM2D2L },
                        new Point(x - xStep * 2, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs2D2L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D2L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM2D2L },
                        new Point(x - xStep * 2, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs2D2L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D2L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM2D2L },
                        new Point(x - xStep * 2, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs2D2L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D2L[osc]].XM_ModulatorId == osc;
                }

                if (oscillatorsWithOutputs2D3L[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM2D3L },
                        new Point(x - xStep * 3, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs2D3L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D3L[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM2D3L },
                        new Point(x - xStep * 3, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs2D3L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D3L[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM2D3L },
                        new Point(x - xStep * 3, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs2D3L[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs2D3L[osc]].XM_ModulatorId == osc;
                }

                // Connections to two columns right in the same row
                if (oscillatorsWithOutputs0D2R[osc] > -1)
                {
                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgAM0D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.AM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.AM, oscillatorsWithInputs0D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D2R[osc]].AM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgFM0D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.FM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.FM, oscillatorsWithInputs0D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D2R[osc]].FM_ModulatorId == osc;

                    indicator = OscillatorGUIs[osc].AddIndicator(Controls.ControlsList.Count, gridControls, new Image[] { imgXM0D2R },
                        new Point(x, ySame));
                    Patch.Wiring.wires.Add(new Wire(SocketType.XM,
                        new Socket(SocketType.OUT, osc),
                        new Socket(SocketType.XM, oscillatorsWithInputs0D2R[osc]),
                        indicator));
                    Patch.Wiring.wires[Patch.Wiring.wires.Count - 1].Indicator.IsOn =
                        Patch.Oscillators[0][oscillatorsWithInputs0D2R[osc]].XM_ModulatorId == osc;
                }
            }
        }

        private int LimitY(int count, int skipX, int skipY)
        {
            if (count == 1)
            {
                return 1;
            }
            else if (count == 2)
            {
                if (skipY > 0)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return count / (4 - (skipX > 2 ? 3 : skipX > 0 ? 2 : 0));
            }
        }

        private int LimitX(int count, int skipX, int skipY)
        {
            if (count == 1)
            {
                return 1;
            }
            else if (count == 2)
            {
                if (skipY > 0)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                return 4 - (skipX > 2 ? 3 : skipX > 0 ? 2 : 0);
            }
        }

        private Rect AssembleHitarea(type type, int x, int y, int width, int height)
        {
            Rect rect = new Rect(0, 0, 0, 0);
            switch (type)
            {
                case type.OSCILLATOR:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + oscillatorsX + oscillatorsSkipX * x),
                        gridStartY + gridSpacingY * (y + oscillatorsY),
                        width, height);
                    break;
                case type.PITCH_ENVELOPE:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + pitchEnvelopesX + pitchEnvelopesSkipX * x),
                        gridStartY + gridSpacingY * y + pitchEnvelopesY * gridSpacingY,
                        width, height);
                    break;
                case type.FILTER:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + filtersX + filtersSkipX * x),
                        gridStartY + gridSpacingY * y + filtersY * gridSpacingY,
                        width, height);
                    break;
                case type.ADSR:
                    rect = new Rect(
                        gridStartX + gridSpacingX * (x + adsrsX + adsrsSkipX * x),
                        gridStartY + gridSpacingY * y + adsrsY * gridSpacingY,
                        width, height);
                    break;
            }
            return rect;
        }
    }
}
