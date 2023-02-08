using Newtonsoft.Json;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        public Patch Patch;
    }

    public class Patch
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Properties that are stored with a Patch
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Key priority is initially set to 'stay dead' but can be changed
        /// in the settings to be 'replace oldest key'.
        /// </summary>
        //public int KeyPriority = 0;

        /// <summary>
        /// OscillatorsInLayout limits the use of oscillators and is set when selecting the Layout.
        /// </summary>
        public int OscillatorsInLayout = 4;

        /// <summary>
        /// List of oscillators. Only first poly is present here since all polys have the same settings.
        /// </summary>
        public List<Oscillator> OscillatorSettings { get; set; }

        public SettingsData SettingsData { get; set; }

        public MidiSettingsData MidiSettingsData { get; set; }

        public int ChorusSetting { get; set; }

        public int ReverbSwitch { get; set; }

        public int ReverbValue { get; set; }

        //public string MidiDevice;// { get { return Settings.MidiDevice; } set { Settings.MidiDevice = value; } }
        //public int KeyPriority;// { get { return Settings.KeyPriority; } set { Settings.KeyPriority = value; } }
        //public int Polyphony;// { get { return Settings.Polyphony; } set { Settings.Polyphony = value; } }
        //public int LowSineWaveFactor;// { get { return Settings.LowSineWaveFactor + 1; } set { Settings.LowSineWaveFactor = value - 1; } }
        //public int ChorusSpeed1;// { get { return (int)Settings.ChorusSpeed1; } set { Settings.ChorusSpeed1 = value; } }
        //public int ChorusSpeed2;// { get { return (int)Settings.ChorusSpeed2; } set { Settings.ChorusSpeed2 = value; } }
        //public int ChorusSpeed3;// { get { return (int)Settings.ChorusSpeed3; } set { Settings.ChorusSpeed3 = value; } }
        //public int ChorusDepth1;// { get { return (int)Settings.ChorusDepth1; } set { Settings.ChorusDepth1 = value; } }
        //public int ChorusDepth2;// { get { return (int)Settings.ChorusDepth2; } set { Settings.ChorusDepth2 = value; } }
        //public int ChorusDepth3;// { get { return (int)Settings.ChorusDepth3; } set { Settings.ChorusDepth3 = value; } }
        //public int[][] CCMapping;// { get { return Settings.CCMapping; } set { Settings.CCMapping = value; } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Public prperties that are not stored with a Patch
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //[JsonIgnore]
        //public int[][] CCMapping;

        //[JsonIgnore]
        //public MainPage mainPage;

        [JsonConstructor]
        public Patch()
        {
            SettingsData = new SettingsData(new Settings());
        }

        /// <summary>
        /// This constructor is used at start-up in LoadSettings(), however no settings is used.
        /// Future version of LoadSettings() might be used to use saved settings between sessions.
        /// </summary>
        /// <param name="mainPage"></param>
        //public Patch(MainPage mainPage)
        //{
        //    this.mainPage = mainPage;

        //    //CCMapping = new int[30][];
        //    //for (int i = 0; i < CCMapping.Length; i++)
        //    //{
        //    //    CCMapping[i] = new int[17];
        //    //    for (int ch = 0; ch < 17; ch++)
        //    //    {
        //    //        CCMapping[i][ch] = i + 2;
        //    //    }
        //    //}
        //}

        /// <summary>
        /// This copy constructor is used when loading a patch.
        /// </summary>
        /// <param name="mainPage"></param>
        /// <param name="patch"></param>
        public Patch(MainPage mainPage, Patch patch)
        {

        }
    }

    public class SettingsData
    {
        public string MidiDevice;
        public int MidiInPort;
        public int KeyPriority;
        public int LowSineWaveFactor;
        public int ChorusSpeed1;
        public int ChorusSpeed2;
        public int ChorusSpeed3;
        public int ChorusDepth1;
        public int ChorusDepth2;
        public int ChorusDepth3;
        public int[][] CCMapping;

        public SettingsData(Settings settings)
        {
            if (settings != null)
            {
                MidiDevice = settings.MidiDevice;
                KeyPriority = settings.KeyPriority;
                LowSineWaveFactor = settings.LowSineWaveFactor;
                ChorusSpeed1 = settings.ChorusSpeed1;
                ChorusSpeed2 = settings.ChorusSpeed2;
                ChorusSpeed3 = settings.ChorusSpeed3;
                ChorusDepth1 = settings.ChorusDepth1;
                ChorusDepth2 = settings.ChorusDepth2;
                ChorusDepth3 = settings.ChorusDepth3;
                MidiInPort = settings.MidiInPort;
                CCMapping = new int[30][];
                for (int i = 0; i < settings.CCMapping.Length; i++)
                {
                    CCMapping[i] = new int[12];
                    for (int osc = 0; osc < 12; osc++)
                    {
                        CCMapping[i][osc] = settings.CCMapping[i][osc];
                    }
                }
            }
            else
            {
                CCMapping = new int[30][];
                for (int i = 0; i < 30; i++)
                {
                    CCMapping[i] = new int[12];
                }
            }
        }
    }

    public class MidiSettingsData
    {
        public int[] Channel;
        public bool[] VelocitySensitivity;
        public bool[] VelocityModulationSensitivity;

        public MidiSettingsData(MidiSettings midiSettings)
        {
            Channel = new int[12];
            VelocitySensitivity = new bool[12];
            VelocityModulationSensitivity = new bool[12];

            for (int osc = 0; osc < 12; osc++)
            {
                if (midiSettings != null)
                {
                    Channel[osc] = midiSettings.Channel[osc];
                    VelocitySensitivity[osc] = midiSettings.VelocitySensitivity[osc];
                    VelocityModulationSensitivity[osc] = midiSettings.VelocityModulationSensitivity[osc];
                }
            }
        }
    }
}
