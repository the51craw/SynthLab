using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        public Patch Patch;
    }

    public class Patch
    {
        /// <summary>
        /// There are four layouts. Idea is to supply separate GUI's for Filter,
        /// PitchEnvelope and ADSR when not using all oscillators. E.g. when using
        /// four oscillators the grid has room for all GUI's but when using twelve
        /// oscillators all those GUI's must be shared among the oscillators since
        /// ther is only space for one of the GUI's of each type.
        /// 
        /// Enum for layout in use:
        ///     FOUR_OSCILLATORS,
        ///     SIX_OSCILLATORS,
        ///     EIGHT_OSCILLATORS,
        ///     TWELVE_OSCILLATORS,
        /// </summary>
        public MainPage.Layouts Layout { get { return layout; } set { layout = value; ChangeLayout(); } }
        private MainPage.Layouts layout;

        /// <summary>
        /// Polyphony is originally set to six, but can be changed in the settings.
        /// </summary>
        public int Polyphony = 32;

        /// <summary>
        /// Key priority is initially set to 'stay dead' but can be changed
        /// in the settings to be 'replace oldest key'.
        /// </summary>
        public int KeyPriority = 0;

        /// <summary>
        /// OscillatorsInLayout limits the use of oscillators and is set when selecting the Layout.
        /// </summary>
        public int OscillatorsInLayout = 4;

        /// <summary>
        /// OscillatosCount is the actual number of oscillators, always 12
        /// </summary>
        public int OscillatorCount = 12;

        public string Name = "";

        /// <summary>
        /// List of all oscillators, including polyphony (1'st dimension).
        /// </summary>
        public List<List<Oscillator>> Oscillators;

        /// <summary>
        /// 0 => Each ADSR can be set to individual values.
        /// 1 => All ADSR are set to the same values.
        /// </summary>
        public int AdsrTypeSelection;

        /// <summary>
        /// Wiring contains all wires connected between oscillators. Wires differs between layouts
        /// since connection between oscillators are affected by the different positions of the oscillators.
        /// </summary>
        [JsonIgnore]
        public Wiring Wiring = new Wiring();

        [JsonIgnore]
        public MainPage mainPage;

        [JsonConstructor]
        public Patch()
        {
        }

        /// <summary>
        /// This constructor is used at start-up in LoadSettings(), however no settings is used.
        /// Future version of LoadSettings() might be used to use saved settings between sessions.
        /// </summary>
        /// <param name="mainPage"></param>
        public Patch(MainPage mainPage)
        {
            this.mainPage = mainPage;
            CreateOscillators(mainPage);
        }

        /// <summary>
        /// This copy constructor is used when loading a patch.
        /// </summary>
        /// <param name="mainPage"></param>
        /// <param name="patch"></param>
        public Patch(MainPage mainPage, Patch patch)
        {

        }

        /// <summary>
        /// ChangeLayout updates GUI
        /// </summary>
        private void ChangeLayout()
        {
            AdsrTypeSelection = 0;

            switch (layout)
            {
                case MainPage.Layouts.FOUR_OSCILLATORS:
                    AdsrTypeSelection = 1;
                    OscillatorsInLayout = 4;
                    Init(mainPage, 4);
                    break;
                case MainPage.Layouts.SIX_OSCILLATORS:
                    OscillatorsInLayout = 6;
                    Init(mainPage, 6);
                    break;
                case MainPage.Layouts.EIGHT_OSCILLATORS:
                    OscillatorsInLayout = 8;
                    Init(mainPage, 8);
                    break;
                case MainPage.Layouts.TWELVE_OSCILLATORS:
                    OscillatorsInLayout = 12;
                    Init(mainPage, 12);
                    break;
            }
        }

        public void Init(MainPage mainPage, int oscillatorCount)
        {
            this.mainPage = mainPage;
            OscillatorCount = oscillatorCount;
        }

        /// <summary>
        /// CreateOscillators creates 12 sets of oscillators regardless of layout, and is called only at startup and patch load.
        /// Switching layout does not re-create oscillators, thus keeps all oscillator settings including envelopes and filters.
        /// The default settings are also set here, thus it can be used each time the GUI's are created.
        /// Loading a patch uses the copy constructor to copy settings into the oscillators.
        /// </summary>
        /// <param name="mainPage"></param>
        public void CreateOscillators(MainPage mainPage)
        {
            Oscillators = new List<List<Oscillator>>();
            for (int poly = 0; poly < Polyphony; poly++)
            {
                Oscillators.Add(new List<Oscillator>());
                for (int osc = 0; osc < 12; osc++)
                {
                    Oscillators[poly].Add(new Oscillator(mainPage));
                    Oscillators[poly][osc].Init(mainPage);
                    Oscillators[poly][osc].Id = osc;
                    Oscillators[poly][osc].PolyId = poly;
                    Oscillators[poly][osc].WaveForm = WAVEFORM.SQUARE;
                    Oscillators[poly][osc].Filter.PostCreationInit();
                    Oscillators[poly][osc].WaveShape.PostCreationInit(mainPage, Oscillators[poly][osc]);
                    Oscillators[poly][osc].WaveShape.Init();
                }
            }
        }
    }
}
