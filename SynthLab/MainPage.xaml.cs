using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.ApplicationModel.Core;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UnhandledExceptionEventHandler = System.UnhandledExceptionEventHandler;

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
        public Settings settings;

        //[JsonIgnore]
        //public GCSupressor GCSupressor;

        [JsonIgnore]
        public EarCompensation EarCompensation;

        private const int rowCount = 3;
        private const int colCount = 4;
        private Controls Controls;
        public CompoundControl ControlPanel;

        private Boolean oscillatorsCreated = false;
        private MidiIn midiIn;
        public KeyDispatcher[] dispatcher;
        public Boolean initDone = false;
        public DispatcherTimer updateTimer;
        private int currentControl;
        private Size newSize;
        private Boolean windowShapeIsGood = false;
        public Boolean usingGraphicsCard = false;
        public Boolean graphicsCardAvailable = false;

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
        private Rect hitArea;

        public MainPage()
        {
            this.InitializeComponent();
            //Application.Current.Suspending += Current_Suspending;
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            StoreSettings();
        }

        private void UpdateTimer_Tick(object sender, object e)
        {
            int retryCount = 0;
            if (allowGuiUpdates && initDone && currentOscillator != null && !hold)
            {
                updateTimer.Stop();
                if (!windowShapeIsGood && retryCount > 0)
                {
                    windowShapeIsGood = ApplicationView.GetForCurrentView().TryResizeView(newSize);
                    retryCount--;
                }

                if (Window.Current.Bounds.Width != Window.Current.Bounds.Height * 1920 / 1040)
                {
                    newSize = new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Width * 1040 / 1920);
                    windowShapeIsGood = ApplicationView.GetForCurrentView().TryResizeView(newSize);
                    retryCount = 10;
                }

                if (selectedOscillator == null || selectedOscillator.Id > Patch.OscillatorCount - 1)
                {
                    if (currentOscillator != null)
                    {
                        selectedOscillator = currentOscillator;
                    }
                    else
                    {
                        selectedOscillator = Patch.Oscillators[0][0];
                    }
                }

                //if (selectedOscillator.Id < Patch.OscillatorCount)
                //{
                //    if (Patch.Oscillators[0][currentOscillator.Id].ViewMe == false)
                //    {
                //        // Turn off all view leds:
                //        for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                //        {
                //            ((Rotator)OscillatorGUIs[osc].SubControls
                //                .ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 0;
                //            for (int poly = 0; poly < Patch.Polyphony; poly++)
                //            {
                //                Patch.Oscillators[poly][osc].ViewMe = false;
                //            }
                //        }

                //        try
                //        {
                //            // Turn on view led for selected oscillator;
                //            ((Rotator)OscillatorGUIs[selectedOscillator.Id].SubControls
                //                .ControlsList[(int)OscillatorControls.VIEW_ME]).Selection = 1;
                //            for (int poly = 0; poly < Patch.Polyphony; poly++)
                //            {
                //                Patch.Oscillators[poly][selectedOscillator.Id].ViewMe = true;
                //            }
                //        }
                //        catch (Exception exception)
                //        {
                //            ContentDialog error = new Error("Unexpected error using FFT: " + exception.Message);
                //            _ = error.ShowAsync();
                //        }
                //    }
                //}

                if (Patch.OscillatorsInLayout > 4)
                {
                    DrawAdsr(0, selectedOscillator.Id);
                    ((Graph)PitchEnvelopeGUIs[0].SubControls.ControlsList[(int)PitchEnvelopeControls.GRAPH])
                        .Draw(selectedOscillator.PitchEnvelope.Points);
                }
                else
                {
                    for (int osc = 0; osc < Patch.OscillatorsInLayout; osc++)
                    {
                        //selectedOscillator = Patch.Oscillators[0][osc];
                        DrawAdsr(OscillatorToAdsr(selectedOscillator.Id), selectedOscillator.Id);
                        ((Graph)PitchEnvelopeGUIs[selectedOscillator.Id].SubControls.ControlsList[(int)PitchEnvelopeControls.GRAPH]).Draw();
                    }
                    //selectedOscillator = currentOscillator;
                }

                oscilloscope.Draw(selectedOscillator);

                ((DigitalDisplay)DisplayGUI.SubControls.ControlsList[(int)DisplayControls.DIGITS])
                    .DisplayValue(currentOscillator.FrequencyInUse);
                UpdateGui();
                allowGuiUpdates = false;
                updateTimer.Start();
            }
        }

        private async void imgClickArea_ImageOpened(object sender, RoutedEventArgs e)
        {
            // Fix the title bar:
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Color.FromArgb(0, 128, 128, 128);
            titleBar.ButtonForegroundColor = Color.FromArgb(0, 128, 128, 128);

            //GCSupressor = new GCSupressor();

            // Read local store and initiate Patch:
            //await LoadLocalState(); // This function has to be updated, see comment in function

            settings = new Settings();

            // Create the frame server:

            // Create MidiIn:
            midiIn = new MidiIn(this);
            FrameServer = new FrameServer(this);
            if (!await FrameServer.InitAudio())
            {
                Application.Current.Exit();
            }
            await midiIn.Init();

            await LoadLocalState(); // This function has to be updated, see comment in function

            CreateLayout(Layouts.FOUR_OSCILLATORS);
            FrameServer.Init();

            foreach (string port in midiIn.portNames)
            {
                settings.MidiInPorts.Items.Add(port);
            }

            // Create dispatchers:
            dispatcher = new KeyDispatcher[16];
            for (int ch = 0; ch < 16; ch++)
            {
                dispatcher[ch] = new KeyDispatcher(this, Patch.Polyphony);
            }

            // Init keyboard frequencies:
            InitFrequencies();

            // Create ear linearity object
            EarCompensation = new EarCompensation(this);

            // Create oscillators:
            Patch.CreateOscillators(this);
            selectedOscillator = Patch.Oscillators[0][0];

            //// See if we can use the graphics card to speed up oscillators:
            //await Task.Delay(1);

            currentOscillator = Patch.Oscillators[0][0];
            oscillatorsCreated = true;
            //allowGuiUpdates = true;

            //// Create updateTimer:
            //updateTimer = new DispatcherTimer();
            //updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            //updateTimer.Tick += UpdateTimer_Tick;
            //updateTimer.Start();

            //while (!oscillatorsCreated)
            //{
            //    await Task.Delay(10);
            //}

            // Createfilters:
            //CreateFilters();
            //Patch.Filter = new Filter[Patch.Polyphony][];
            //for (int poly = 0; poly < Patch.Polyphony; poly++)
            //{
            //    Patch.Filter[poly] = new Filter[12];
            //    for (int osc = 0; osc < 12; osc++)
            //    {
            //        Patch.Filter[poly][osc] = new Filter(this, poly, osc, SampleCount);
            //    }
            //}

            // Create WaveShapes:
            //CreateWaveShapes();
            //WaveShape = new WaveShape[Patch.Polyphony][];
            //for (int poly = 0; poly < Patch.Polyphony; poly++)
            //{
            //    WaveShape[poly] = new WaveShape[12];
            //    for (int osc = 0; osc < 12; osc++)
            //    {
            //        WaveShape[poly][osc] = new WaveShape(this, poly, osc);
            //    }
            //}

            CreateKeyboardAndControlPanel();
            CreateControls();
            CreateWiring();

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControlPanel);

            //if (usingGraphicsCard)
            //{
            //    ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.USING_GRAPHICS_CARD]).Selection = 1;
            //}

            Controls.HideOriginalControls();
            imgHangingWire.Visibility = Visibility.Collapsed;
            newSize = new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Width * 1040 / 1920);

            for (int poly = 0; poly < Patch.Polyphony; poly++)
            {
                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                {
                    Patch.Oscillators[poly][osc].InitOscillator(69, 64);
                    Patch.Oscillators[poly][osc].CreateWaveData(SampleCount);
                    Patch.Oscillators[poly][osc].MakeGraphData(66);
                }
            }

            // Create updateTimer:
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            allowGuiUpdates = true;
            initDone = true;
            //GCSupressor.SupressPatch(Patch);
            FrameServer.StartAudio();
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
                    windowShapeIsGood = false;
                }
                if (e.NewSize.Height != e.PreviousSize.Height)
                {
                    newSize = new Size(e.NewSize.Width * 1920 / 1040, e.NewSize.Height);
                    windowShapeIsGood = false;
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
