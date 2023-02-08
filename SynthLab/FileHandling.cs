using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        private string currentFilePath = "";

        private async Task SaveAsToFile()
        {
            try
            {
                FileSavePicker fileSavePicker = new FileSavePicker();
                fileSavePicker.FileTypeChoices.Add("Json file", new List<string>() { ".json" });
                StorageFile outFile = await fileSavePicker.PickSaveFileAsync();

                if (outFile != null)
                {
                    OscillatorsToOscillatorsettings();
                    Patch.SettingsData = new SettingsData(Settings);
                    Patch.MidiSettingsData = new MidiSettingsData(MidiSettings);
                    Patch.ChorusSetting = Chorus.Selection;
                    Patch.ReverbSwitch = Reverb.Selection;
                    Patch.ReverbValue = ReverbSlider.Value;
                    String fileContent = JsonConvert.SerializeObject(Patch, Formatting.Indented);
                    await FileIO.WriteTextAsync(outFile, fileContent);
                    currentFilePath = outFile.Path;
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Message(exception.Message);
                _ = error.ShowAsync();
            }
        }

        private async Task SaveToFile()
        {
            if (String.IsNullOrEmpty(currentFilePath))
            {
                await SaveAsToFile();
            }
            else
            {
                try
                {
                    StorageFile outFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync("currentFilePath");

                    if (outFile != null)
                    {
                        Message save = new Message("Save this to " + currentFilePath + "?", "Save file");
                        if (await save.ShowAsync() == ContentDialogResult.Primary)
                        {
                            if (save.Result)
                            {
                                OscillatorsToOscillatorsettings();
                                Patch.SettingsData = new SettingsData(Settings);
                                Patch.MidiSettingsData = new MidiSettingsData(MidiSettings);
                                Patch.ChorusSetting = Chorus.Selection;
                                Patch.ReverbSwitch = Reverb.Selection;
                                Patch.ReverbValue = ReverbSlider.Value;
                                String fileContent = JsonConvert.SerializeObject(Patch, Formatting.Indented);
                                await FileIO.WriteTextAsync(outFile, fileContent);
                            }
                        }
                    }
                    else
                    {
                        await SaveAsToFile();
                    }
                }
                catch (Exception exception)
                {
                    ContentDialog error = new Message(exception.Message);
                    _ = error.ShowAsync();
                }
            }
        }

        private async Task LoadFromFile()
        {
            initDone = false;
            try
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.FileTypeFilter.Add(".json");
                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    String content = await FileIO.ReadTextAsync(file);
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("currentFilePath", file);
                    Patch = JsonConvert.DeserializeObject<Patch>(content);

                    if (Patch != null)
                    {
                        currentFilePath = file.Path;
                        await LoadPatch(this, Patch);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                allowGuiUpdates = true;
                initDone = true;
            }
            catch (Exception exeption)
            {
                string errorText = "Patch file seems to be corrupt.";
                if (!String.IsNullOrEmpty(exeption.Message))
                {
                    errorText += "\n" + exeption.Message;
                }
                errorText += "\n" + "Application will now restart.";
                ContentDialog error = new Message(errorText);
                await error.ShowAsync();
                var result = await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");

                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    errorText = "Restart Failed!\nPlease close and restart the application manually.";
                    error = new Message(errorText);
                    await error.ShowAsync();
                }                  
            }
        }

        private async Task<int> OpenFactoryPatch(String PatchName)
        {
            initDone = false;
            var packagePath = Package.Current.InstalledLocation;
            var folderPath = Path.Combine(packagePath.Path, "Patches");
            StorageFolder storageFolder =
                await StorageFolder.GetFolderFromPathAsync(folderPath);
            //StorageFolder storageFolder =
            //    ApplicationData.Current.LocalFolder;
            StorageFile patchFile =
                await storageFolder.GetFileAsync(PatchName);
            try
            {
                String content = await FileIO.ReadTextAsync(patchFile);
                Patch = JsonConvert.DeserializeObject<Patch>(content);

                if (Patch != null)
                {
                    await LoadPatch(this, Patch);
                }
                allowGuiUpdates = true;
                initDone = true;
                return 0;
            }
            catch (Exception exeption)
            {
                string errorText = "Patch file seems to be corrupt.";
                if (!String.IsNullOrEmpty(exeption.Message))
                {
                    errorText += "\n" + exeption.Message;
                }
                errorText += "\n" + "Application will now restart.";
                ContentDialog error = new Message(errorText);
                await error.ShowAsync();
                var result = await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");

                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    errorText = "Restart Failed!\nPlease close and restart the application manually.";
                    error = new Message(errorText);
                    await error.ShowAsync();
                }
            }
            return -1;
        }

        private Task LoadPatch(MainPage mainPage, Patch Patch)
        {
            OscillatorsettingsToOscillators();
            Settings = new Settings(Patch.SettingsData);
            MidiSettings = new MidiSettings(this, Patch.MidiSettingsData);
            Settings.MidiInPorts.Items.Add("All midi inputs");
            foreach (string port in midiIn.portNames)
            {
                Settings.MidiInPorts.Items.Add(port);
            }
            if (Settings.MidiInPort >= midiIn.portList.Count)
            {
                Settings.MidiInPort = 0;
            }
            else
            {
                Settings.MidiInPort = Patch.SettingsData.MidiInPort;
            }

            allowGuiUpdates = false;
            initDone = false;
            Controls.ControlsList.RemoveRange(numberOfFixedControls, Controls.ControlsList.Count - numberOfFixedControls);
            switch (Patch.OscillatorsInLayout)
            {
                case 4:
                    ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.LAYOUT]).Selection = 0;
                    Layout = Layouts.FOUR_OSCILLATORS;
                    break;
                case 6:
                    ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.LAYOUT]).Selection = 1;
                    Layout = Layouts.SIX_OSCILLATORS;
                    break;
                case 8:
                    ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.LAYOUT]).Selection = 2;
                    Layout = Layouts.EIGHT_OSCILLATORS;
                    break;
                case 12:
                    ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.LAYOUT]).Selection = 3;
                    Layout = Layouts.TWELVE_OSCILLATORS;
                    break;
            }
            CreateLayout(Layout);
            CreateControls();
            CreateWiring();
            currentOscillator = Oscillators[0][0];
            Chorus.Selection = Patch.ChorusSetting;
            Reverb.Selection = Patch.ReverbSwitch;
            ReverbSlider.Value = Patch.ReverbValue;
            if (Reverb.Selection == 0)
            {
                FrameServer.TurnOffReverb();
            }
            else
            {
                FrameServer.TurnOnReverb();
            }

            // Make sure all controls has the correct size and position:
            Controls.ResizeControls(gridControls, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControls);
            Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
            Controls.SetControlsUniform(gridControlPanel);

            return Task.Run (() =>
            {
                initDone = true;
                allowGuiUpdates = true;
            });
        }

        ///// <summary>
        ///// Localsettings contains almost all data for the Patch.
        ///// The Patch contains all data for a working setup except the
        ///// modulator and modulating oscillators. Those are referenced
        ///// by Id number in settings, and are referenced when the Patch
        ///// has been successfully read.
        ///// </summary>
        ///// <returns></returns>
        //private async Task LoadLocalState()
        //{
        //    Patch = new Patch(this);
        //    return; //!!!
        //    // TODO This is not used. Remove it and possibly replace with a setting to remember the full state between sessions.
        //    // Rather than splitting the data into LocalSettings.Values entries, save it as a file in LocalState instead.

        //    String settings = "";
        //    String temp;
        //    if (ApplicationData.Current.LocalSettings.Values["Setup0"] == null)
        //    {
        //        Patch = new Patch(this);
        //    }
        //    else
        //    {
        //        int i = 0;
        //        do
        //        {
        //            temp = (string)ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()];
        //            settings += temp;
        //            i++;
        //        } while (temp != null);

        //        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //        {
        //            Patch = new Patch(this);
        //        });
        //    }
        //}

        //private void StoreSettings()
        //{
        //    try
        //    {
        //        String settings = JsonConvert.SerializeObject(Patch, Formatting.Indented);

        //        // Localsettings values are limited in length. Chop it up in parts of
        //        // permitted length and store as separate numbered values:
        //        List<String> settingsParts = new List<String>();
        //        int i = 0;
        //        while (settings.Length > 0)
        //        {
        //            if (settings.Length > 2048)
        //            {
        //                ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] = settings.Remove(2048);
        //                settings = settings.Remove(0, 2048);
        //            }
        //            else
        //            {
        //                ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] = settings;
        //                settings = "";
        //            }
        //            i++;
        //        }

        //        // If a longer json string was stored earlier, we must remove the extra
        //        // parts. Otherwise LoadSettings would add them too, and deliver an invalid json.
        //        while (ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] != null)
        //        {
        //            ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] = null;
        //            i++;
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        ContentDialog error = new Message(exception.Message);
        //        _ = error.ShowAsync();
        //    }
        //}

        private void OscillatorsToOscillatorsettings()
        {
            Patch.OscillatorSettings = new List<Oscillator>();
            for (int osc = 0; osc < 12; osc++)
            {
                Oscillator oscillator = new Oscillator(this, Oscillators[0][osc]);
                oscillator.Filter = new Filter(Oscillators[0][osc]);
                oscillator.PitchEnvelope = new PitchEnvelope(Oscillators[0][osc], Oscillators[0][osc].PitchEnvelope);
                oscillator.Adsr = new ADSR(Oscillators[0][osc].Adsr, oscillator);
                oscillator.WaveShape = new WaveShape(Oscillators[0][osc], Oscillators[0][osc].WaveShape);
                oscillator.Phase = Oscillators[0][osc].Phase;
                oscillator.Filter.Q = Oscillators[0][osc].Filter.Q;
                oscillator.Filter.FrequencyCenter = Oscillators[0][osc].Filter.FrequencyCenter;
                oscillator.Filter.KeyboardFollow = Oscillators[0][osc].Filter.KeyboardFollow;
                oscillator.Filter.Gain = Oscillators[0][osc].Filter.Gain;
                oscillator.Filter.Mix = Oscillators[0][osc].Filter.Mix;
                oscillator.Filter.ModulationWheelTarget = Oscillators[0][osc].Filter.ModulationWheelTarget;
                oscillator.PitchEnvelope.Points = new List<Point>();
                foreach (Point point in Oscillators[0][osc].PitchEnvelope.Points)
                {
                    oscillator.PitchEnvelope.Points.Add(point);
                }
                oscillator.PitchEnvelope.PitchEnvModulationWheelTarget = Oscillators[0][osc].PitchEnvelope.PitchEnvModulationWheelTarget;
                Patch.OscillatorSettings.Add(oscillator);
                oscillator.PitchEnvelope.PitchEnvModulationWheelTarget = Oscillators[0][osc].PitchEnvelope.PitchEnvModulationWheelTarget;
                oscillator.PitchEnvelope.PitchEnvPitch = Oscillators[0][osc].PitchEnvelope.PitchEnvPitch;
                oscillator.PitchEnvelope.PitchEnvAm = Oscillators[0][osc].PitchEnvelope.PitchEnvAm;
                oscillator.PitchEnvelope.PitchEnvFm = Oscillators[0][osc].PitchEnvelope.PitchEnvFm;
                oscillator.PitchEnvelope.PitchEnvXm = Oscillators[0][osc].PitchEnvelope.PitchEnvXm;
                oscillator.PitchEnvelope.Depth = Oscillators[0][osc].PitchEnvelope.Depth;
                oscillator.PitchEnvelope.Speed = Oscillators[0][osc].PitchEnvelope.Speed;
                oscillator.Adsr.AdsrAttackTime = Oscillators[0][osc].Adsr.AdsrAttackTime;
                oscillator.Adsr.AdsrDecayTime = Oscillators[0][osc].Adsr.AdsrDecayTime;
                oscillator.Adsr.AdsrSustainLevel = Oscillators[0][osc].Adsr.AdsrSustainLevel;
                oscillator.Adsr.AdsrReleaseTime = Oscillators[0][osc].Adsr.AdsrReleaseTime;
                oscillator.Adsr.AdsrAmSensitive = Oscillators[0][osc].Adsr.AdsrAmSensitive;
                oscillator.Adsr.AdsrFmSensitive = Oscillators[0][osc].Adsr.AdsrFmSensitive;
                oscillator.Adsr.AdsrXmSensitive = Oscillators[0][osc].Adsr.AdsrXmSensitive;
                oscillator.Adsr.AdsrModulationWheelTarget = Oscillators[0][osc].Adsr.AdsrModulationWheelTarget;
            }
        }

        private void OscillatorsettingsToOscillators()
        {
            Oscillators = new List<List<Oscillator>>();
            for (int poly = 0; poly < 6; poly++)
            {
                Oscillators.Add(new List<Oscillator>());
                for (int osc = 0; osc < 12; osc++)
                {
                    Oscillators[poly].Add(new Oscillator(this, Patch.OscillatorSettings[osc]));
                    Oscillators[poly][osc].mainPage = this;
                    Oscillators[poly][osc].Filter = new Filter(Oscillators[poly][osc]);
                    Oscillators[poly][osc].PitchEnvelope = new PitchEnvelope(Oscillators[poly][osc], Patch.OscillatorSettings[osc].PitchEnvelope);
                    Oscillators[poly][osc].Adsr = new ADSR(Patch.OscillatorSettings[osc].Adsr, Oscillators[poly][osc]);
                    Oscillators[poly][osc].WaveShape = new WaveShape(Oscillators[0][osc], Patch.OscillatorSettings[osc].WaveShape);
                    Oscillators[poly][osc].Filter.Q = Patch.OscillatorSettings[osc].Filter.Q;
                    Oscillators[poly][osc].Filter.FrequencyCenter = Patch.OscillatorSettings[osc].Filter.FrequencyCenter;
                    Oscillators[poly][osc].Filter.KeyboardFollow = Patch.OscillatorSettings[osc].Filter.KeyboardFollow;
                    Oscillators[poly][osc].Filter.Gain = Patch.OscillatorSettings[osc].Filter.Gain;
                    Oscillators[poly][osc].Filter.Mix = Patch.OscillatorSettings[osc].Filter.Mix;
                    Oscillators[poly][osc].Filter.ModulationWheelTarget = Patch.OscillatorSettings[osc].Filter.ModulationWheelTarget;
                    Oscillators[poly][osc].PitchEnvelope.Points = new List<Point>();
                    foreach (Point point in Patch.OscillatorSettings[osc].PitchEnvelope.Points)
                    {
                        Oscillators[poly][osc].PitchEnvelope.Points.Add(point);
                    }
                    Oscillators[poly][osc].PitchEnvelope.PitchEnvModulationWheelTarget = Patch.OscillatorSettings[osc].PitchEnvelope.PitchEnvModulationWheelTarget;
                    Oscillators[poly][osc].PitchEnvelope.PitchEnvPitch = Patch.OscillatorSettings[osc].PitchEnvelope.PitchEnvPitch;
                    Oscillators[poly][osc].PitchEnvelope.PitchEnvAm = Patch.OscillatorSettings[osc].PitchEnvelope.PitchEnvAm;
                    Oscillators[poly][osc].PitchEnvelope.PitchEnvFm = Patch.OscillatorSettings[osc].PitchEnvelope.PitchEnvFm;
                    Oscillators[poly][osc].PitchEnvelope.PitchEnvXm = Patch.OscillatorSettings[osc].PitchEnvelope.PitchEnvXm;
                    Oscillators[poly][osc].PitchEnvelope.Depth = Patch.OscillatorSettings[osc].PitchEnvelope.Depth;
                    Oscillators[poly][osc].PitchEnvelope.Speed = Patch.OscillatorSettings[osc].PitchEnvelope.Speed;
                    Oscillators[poly][osc].Adsr.SetMainPage(this);
                    Oscillators[poly][osc].Adsr.AdsrAttackTime = Patch.OscillatorSettings[osc].Adsr.AdsrAttackTime;
                    Oscillators[poly][osc].Adsr.AdsrDecayTime = Patch.OscillatorSettings[osc].Adsr.AdsrDecayTime;
                    Oscillators[poly][osc].Adsr.AdsrSustainLevel = Patch.OscillatorSettings[osc].Adsr.AdsrSustainLevel;
                    Oscillators[poly][osc].Adsr.AdsrReleaseTime = Patch.OscillatorSettings[osc].Adsr.AdsrReleaseTime;
                    Oscillators[poly][osc].Adsr.AdsrAmSensitive = Patch.OscillatorSettings[osc].Adsr.AdsrAmSensitive;
                    Oscillators[poly][osc].Adsr.AdsrFmSensitive = Patch.OscillatorSettings[osc].Adsr.AdsrFmSensitive;
                    Oscillators[poly][osc].Adsr.AdsrXmSensitive = Patch.OscillatorSettings[osc].Adsr.AdsrXmSensitive;
                    Oscillators[poly][osc].Adsr.AdsrModulationWheelTarget = Patch.OscillatorSettings[osc].Adsr.AdsrModulationWheelTarget;
                }
            }
            MakeConnections(true);
        }
    }
}
