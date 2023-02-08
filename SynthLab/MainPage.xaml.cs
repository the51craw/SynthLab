using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UwpControlsLibrary;
using Windows.ApplicationModel.Core;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {

        [JsonIgnore]
        public CompoundControl[] OscillatorGUIs;

        [JsonIgnore]
        public CompoundControl[] PitchEnvelopeGUIs;

        [JsonIgnore]
        public CompoundControl[] FilterGUIs;

        [JsonIgnore]
        public CompoundControl DisplayGUI;

        [JsonIgnore]
        public CompoundControl[] AdsrGUIs;

        [JsonIgnore]
        public Rotator DisplayOnOff;

        /// <summary>
        /// Settings dialog data.
        /// </summary>
        [JsonIgnore]
        public Settings Settings;

        /// <summary>
        /// List of all oscillators, including polyphony (1'st dimension).
        /// Since we have the same settings for all polys of an oscillator
        /// this is not stored with a Patch. Instead a list OscillatorSettings
        /// is stored with the Patch. 
        /// </summary>
        [JsonIgnore]
        public List<List<Oscillator>> Oscillators;

        /// <summary>
        /// MIDI settings dialog data.
        /// </summary>
        [JsonIgnore]
        public MidiSettings MidiSettings;

        /// <summary>
        /// Wiring contains all wires connected between oscillators. Wires differs between layouts
        /// since connection between oscillators are affected by the different positions of the oscillators.
        /// The wiring is set up using fields in the oscillators, so we do not need to store them in Patch
        /// files.
        /// </summary>
        [JsonIgnore]
        public Wiring Wiring = new Wiring();

        [JsonIgnore]
        public MainPage.Layouts Layout { get { return layout; } set { layout = value; ChangeLayout(); } }
        private MainPage.Layouts layout;

        public int chorusSpeed1;
        public int chorusSpeed2;
        public int chorusSpeed3;
        public int chorusDepth1;
        public int chorusDepth2;
        public int chorusDepth3;


        [JsonIgnore]
        public EarCompensation EarCompensation;

        private const int rowCount = 3;
        private const int colCount = 4;
        public Controls Controls;
        public CompoundControl ControlPanel;

        private MidiIn midiIn;

        /// <summary>
        /// Each visible oscillator actually consist of 6 oscillators
        /// in order to make it polyphonic up to 6 notes.
        /// </summary>
        public KeyDispatcher[] dispatcher;

        public Boolean initDone = false;
        public DispatcherTimer updateTimer;
        public DispatcherTimer midiInTimer;
        //public DispatcherTimer selectDrumsetTimer;
        //public DispatcherTimer selectWavefilesTimer;
        public DispatcherTimer pitchBenderReleasedTimer;
        private bool pitchBenderReleased;
        private int currentControl;
        private Size newSize;
        //private Boolean windowShapeIsGood = false;
        public Boolean usingGraphicsCard = false;
        public Boolean graphicsCardAvailable = false;
        public Wave[] wave;
        public Drumset[] drumset;
        public Rotator Chorus;
        public Rotator Reverb;
        public VerticalSlider ReverbSlider;
        public Keyboard keyboard;

                [JsonIgnore]
        public Canvas adsrCanvas;

        [JsonIgnore]
        public Canvas oscilloscopeCanvas;

        [JsonIgnore]
        public Canvas[] pitchEnvelopeCanvases;
        private int numberOfFixedControls = 0;
        public uint SampleRate;
        public uint SampleCount;
        public _compoundType selectedCompoundType;
        public bool hold = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        //private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        //{
        //    StoreSettings();
        //}

        private async void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Fix the title bar:
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 128, 128, 128);
            titleBar.ButtonForegroundColor = Color.FromArgb(0, 128, 128, 128);

            // Read local store and initiate Patch:
            //await LoadLocalState(); // This function has to be updated, see comment in function

            Patch = new Patch();
            Settings = new Settings();
            Settings.MidiInPorts.Items.Add("All midi inputs");
            MidiSettings = new MidiSettings(this);

            // Create MidiIn:
            midiIn = new MidiIn(this);
            FrameServer = new FrameServer(this);
            if (!await FrameServer.InitAudio())
            {
                Application.Current.Exit();
            }
            await midiIn.Init();

            //await LoadLocalState(); // This function has to be updated, see comment in function

            CreateLayout(Layouts.FOUR_OSCILLATORS);
            //FrameServer.Init();

            foreach (string port in midiIn.portNames)
            {
                Settings.MidiInPorts.Items.Add(port);
            }

            //// Create dispatchers:
            dispatcher = new KeyDispatcher[12];
            for (int osc = 0; osc < 12; osc++)
            {
                dispatcher[osc] = new KeyDispatcher(this);
            }

            // Create wave and drumset lists:
            wave = new Wave[12];
            drumset = new Drumset[12];

            // Init keyboard frequencies:
            InitFrequencies();

            // Create ear linearity object
            EarCompensation = new EarCompensation(this);

            // Create oscillators:
            CreateOscillators(this);
            selectedOscillator = Oscillators[0][0];
            currentOscillator = Oscillators[0][0];

            CreateKeyboardAndControlPanel();

            oscilloscope = new Oscilloscope(this);
            oscilloscope.VoltsPerCm = 0;
            oscilloscope.MillisecondsPerCm = .5;

            CreateControls();
            CreateWiring();

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControlPanel);

            Controls.HideOriginalControls();
            imgHangingWire.Visibility = Visibility.Collapsed;
            newSize = new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Width * 1040 / 1920);

            for (int poly = 0; poly < 6; poly++)
            {
                for (int osc = 0; osc < Patch.OscillatorsInLayout; osc++)
                {
                    Oscillators[poly][osc].InitOscillator(0, 69, 64);
                    //Oscillators[poly][osc].CreateWaveData(SampleCount);
                    //Oscillators[poly][osc].MakeGraphData(66);
                }
            }

            // Create updateTimer:
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // Create midiInTimer:
            midiInTimer = new DispatcherTimer();
            midiInTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            midiInTimer.Tick += MidiInTimer_Tick;
            midiInTimer.Start();

            //// Create selectWavefilesTimer:
            //selectWavefilesTimer = new DispatcherTimer();
            //selectWavefilesTimer.Interval = new TimeSpan(0, 0, 0, 1);
            //selectWavefilesTimer.Tick += SelectWavefilesTimer_Tick;

            //// Create selectDrumsetTimer:
            //selectDrumsetTimer = new DispatcherTimer();
            //selectDrumsetTimer.Interval = new TimeSpan(0, 0, 0, 1);
            //selectDrumsetTimer.Tick += SelectDrumsetTimer_Tick;

            // Create pitch bender release timer:
            pitchBenderReleasedTimer = new DispatcherTimer();
            pitchBenderReleasedTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            pitchBenderReleasedTimer.Tick += PitchBenderReleasedTimer_Tick;

            allowGuiUpdates = true;
            initDone = true;
            //GCSupressor.SupressPatch(Patch);
            FrameServer.StartAudio();

            //MidiSettings = new MidiSettings(this);
            midiInMessageQueue = new IMidiMessage[8192]; // MIDI sends less than 1000 messages per second,
                                                         // so this is an eight second long buffer. Note
                                                         // that when using MIDI merge and multiple keyboards,
                                                         // or an e.g. DAW, this is divided by keyboard or
                                                         // track count.

            // Hook up keyboard entries:
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
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
            for (int poly = 0; poly < 6; poly++)
            {
                Oscillators.Add(new List<Oscillator>());
                for (int osc = 0; osc < 12; osc++)
                {
                    Oscillators[poly].Add(new Oscillator(mainPage));
                    Oscillators[poly][osc].Init(mainPage);
                    Oscillators[poly][osc].Id = osc;
                    //Oscillators[poly][osc].PolyId = poly;
                    Oscillators[poly][osc].WaveForm = WAVEFORM.SQUARE;
                    //Oscillators[poly][osc].Filter = new Filter();
                    //Oscillators[poly][osc].Filter.PostCreationInit(0);
                }
            }
        }

        /// <summary>
        /// ChangeLayout updates GUI
        /// </summary>
        private void ChangeLayout()
        {
            switch (layout)
            {
                case MainPage.Layouts.FOUR_OSCILLATORS:
                    Patch.OscillatorsInLayout = 4;
                    break;
                case MainPage.Layouts.SIX_OSCILLATORS:
                    Patch.OscillatorsInLayout = 6;
                    break;
                case MainPage.Layouts.EIGHT_OSCILLATORS:
                    Patch.OscillatorsInLayout = 8;
                    break;
                case MainPage.Layouts.TWELVE_OSCILLATORS:
                    Patch.OscillatorsInLayout = 12;
                    break;
            }
        }

        private void UpdateTimer_Tick(object sender, object e)
        {
            //int retryCount = 0;
            if (allowGuiUpdates && initDone && currentOscillator != null && !hold)
            {
                updateTimer.Stop();
                //if (!windowShapeIsGood && retryCount > 0)
                //{
                //    windowShapeIsGood = ApplicationView.GetForCurrentView().TryResizeView(newSize);
                //    retryCount--;
                //}

                //if (Window.Current.Bounds.Width != Window.Current.Bounds.Height * 1920 / 1040)
                //{
                //    newSize = new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Width * 1040 / 1920);
                //    windowShapeIsGood = ApplicationView.GetForCurrentView().TryResizeView(newSize);
                //    retryCount = 10;
                //}

                if (selectedOscillator == null || selectedOscillator.Id < 0 || selectedOscillator.Id > 11)
                {
                    if (currentOscillator != null)
                    {
                        selectedOscillator = currentOscillator;
                    }
                    else
                    {
                        selectedOscillator = Oscillators[0][0];
                    }
                }

                UpdateGui();
            }
            if (DisplayOnOff.Selection > 0 && allowUpdateOscilloscope && oscilloscope.waveFrames != null)
            {
                if (KeyDispatcher.AnyOscillatorInUse(this))
                {
                    //oscilloscope.MakeGraph(60);
                    // TODO: Findout how to use correct value from oscillator and which oscillator!
                    ((Graph)DisplayGUI.SubControls.ControlsList[(int)DisplayControls.OSCILLOGRAPH]).Draw(oscilloscope.MakeGraph(SampleCount));
                }
                else
                {
                    //oscilloscope.ClearGraph();
                    ((Graph)DisplayGUI.SubControls.ControlsList[(int)DisplayControls.OSCILLOGRAPH]).Draw(oscilloscope.ClearGraph());
                }
            }
            allowUpdateOscilloscope = false;
            allowGuiUpdates = false;

            updateTimer.Start();
        }

        private void PitchBenderReleasedTimer_Tick(object sender, object e)
        {
            if (pitchBenderReleased)
            {
                if (((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value > 8192 + 1024)
                {
                    ((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value -= 1024;
                }
                else if (((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value < 8192 - 1024)
                {
                    ((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value += 1024;
                }
                else
                {
                    ((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value = 8192;
                    pitchBenderReleased = false;
                    pitchBenderReleasedTimer.Stop();
                }

                for (int ch = 0; ch < 16; ch++)
                {
                    if (((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value > 8192)
                    {
                        PitchBend[ch] = (1 + (((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value - 8192f) / 8191f);
                    }
                    else if (((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value < 8192)
                    {
                        PitchBend[ch] = 0.5 + (((VerticalSlider)Controls.ControlsList[(int)OtherControls.PITCH_BENDER_WHEEL]).Value / 16383f);
                    }
                    else
                    {
                        PitchBend[ch] = 1f;
                    }
                }
            }
        }

        // When app size is changed, all controls must also be resized,
        // ask the Controls object to do it:
        private void gridMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Controls != null)
            {
                if (e.NewSize.Width != e.PreviousSize.Width)
                {
                    newSize = new Size(e.NewSize.Width, e.NewSize.Width * 1040 / 1920);
                    //windowShapeIsGood = false;
                }
                if (e.NewSize.Height != e.PreviousSize.Height)
                {
                    newSize = new Size(e.NewSize.Width * 1920 / 1040, e.NewSize.Height);
                    //windowShapeIsGood = false;
                }
                Controls.ResizeControls(gridControls, Window.Current.Bounds);
                Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
                UpdateGui();
            }
        }

        private void Page_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Shift:
                    break;
                case Windows.System.VirtualKey.Control:
                    break;
            }
        }

        private void Page_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {

        }
    }
}
