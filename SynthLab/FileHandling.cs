using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
                    String fileContent = JsonConvert.SerializeObject(Patch, Formatting.Indented);
                    await FileIO.WriteTextAsync(outFile, fileContent);
                    currentFilePath = outFile.Path;
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Error(exception.Message);
                _ = error.ShowAsync();
            }
        }

        private async Task SaveToFile()
        {
            try
            {
                StorageFile outFile = await StorageFile.GetFileFromPathAsync(currentFilePath);

                if (outFile != null)
                {
                    ContentDialog save = new Error("Save this to " + currentFilePath + "?");
                    if (await save.ShowAsync() == ContentDialogResult.Primary)
                    {
                        String fileContent = JsonConvert.SerializeObject(Patch, Formatting.Indented);
                        await FileIO.WriteTextAsync(outFile, fileContent);
                    }
                }
                else
                {
                    await SaveAsToFile();
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Error(exception.Message);
                _ = error.ShowAsync();
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
                    //GCSupressor.ReleasePatch(Patch);
                    Patch = JsonConvert.DeserializeObject<Patch>(content);

                    if (Patch != null)
                    {
                        //GCSupressor.SupressPatch(Patch);
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
                ContentDialog error = new Error(errorText);
                await error.ShowAsync();
                var result = await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");

                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    errorText = "Restart Failed!\nPlease close and restart the application manually.";
                    error = new Error(errorText);
                    await error.ShowAsync();
                }                  
            }
        }

        private async Task<int> OpenFactoryPatch(String PatchName)
        {
            initDone = false;
            StorageFolder storageFolder =
                ApplicationData.Current.LocalFolder;
            StorageFile patchFile =
                await storageFolder.GetFileAsync(PatchName);
            try
            {
                String content = await FileIO.ReadTextAsync(patchFile);
                //GCSupressor.ReleasePatch(Patch);
                Patch = JsonConvert.DeserializeObject<Patch>(content);

                if (Patch != null)
                {
                    //GCSupressor.SupressPatch(Patch);
                    Patch.Name = patchFile.Name;
                    await LoadPatch(this, Patch);
                }
                UpdateOscillatorGuis();
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
                ContentDialog error = new Error(errorText);
                await error.ShowAsync();
                var result = await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");

                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    errorText = "Restart Failed!\nPlease close and restart the application manually.";
                    error = new Error(errorText);
                    await error.ShowAsync();
                }
            }
            return -1;
        }

        private async Task<bool> LoadPatch(MainPage mainPage, Patch Patch)
        {
            Patch.mainPage = mainPage;
            //StorageItemMostRecentlyUsedList mru = StorageApplicationPermissions.MostRecentlyUsedList;
            //mostRecentlyUsedToken = mru.Add(patchFile, "current file");

            // Create oscillator objects read from patch:
            for (int poly = 0; poly < 32; poly++)
            {
                if (poly > Patch.Oscillators.Count - 1)
                {
                    Patch.Oscillators.Add(new List<Oscillator>());
                }

                for (int osc = 0; osc < 12; osc++)
                {
                    if (osc < Patch.Oscillators[poly].Count)
                    {
                        Patch.Oscillators[poly][osc].mainPage = this;
                        Patch.Oscillators[poly][osc].WaveData = new double[mainPage.SampleCount];
                        Patch.Oscillators[poly][osc].Filter.mainPage = this;
                        Patch.Oscillators[poly][osc].Filter.oscillator = Patch.Oscillators[poly][osc];
                        Patch.Oscillators[poly][osc].Filter.PostCreationInit();
                        Patch.Oscillators[poly][osc].WaveShape.mainPage = this;
                        Patch.Oscillators[poly][osc].WaveShape.PostCreationInit(this, Patch.Oscillators[poly][osc]);
                        Patch.Oscillators[poly][osc].PitchEnvelope.mainPage = this;
                        Patch.Oscillators[poly][osc].PitchEnvelope.oscillator = Patch.Oscillators[poly][osc];
                        Patch.Oscillators[poly][osc].Adsr.mainPage = this;
                        Patch.Oscillators[poly][osc].Adsr.Init(Patch.Oscillators[poly][osc]);
                        //Patch.Oscillators[poly][osc].Adsr.Pulse.adsr = Patch.Oscillators[poly][osc].Adsr;
                    }
                    else
                    {
                        Patch.Oscillators[poly].Add(new Oscillator(this));
                        Patch.Oscillators[poly][osc].Init(this);
                        Patch.Oscillators[poly][osc].Id = osc;
                        Patch.Oscillators[poly][osc].PolyId = poly;
                        Patch.Oscillators[poly][osc].Filter = new Filter(this, Patch.Oscillators[poly][osc]);
                        Patch.Oscillators[poly][osc].Filter.oscillator = Patch.Oscillators[poly][osc];
                        Patch.Oscillators[poly][osc].Filter.PostCreationInit();
                        Patch.Oscillators[poly][osc].WaveShape = new WaveShape();
                        Patch.Oscillators[poly][osc].WaveShape.mainPage = this;
                        Patch.Oscillators[poly][osc].WaveShape.PostCreationInit(this, Patch.Oscillators[poly][osc]);
                        Patch.Oscillators[poly][osc].PitchEnvelope = new PitchEnvelope(this, Patch.Oscillators[poly][osc]);
                        Patch.Oscillators[poly][osc].PitchEnvelope.oscillator = Patch.Oscillators[poly][osc];
                        Patch.Oscillators[poly][osc].Adsr = new ADSR(this);
                        Patch.Oscillators[poly][osc].Adsr.Init(Patch.Oscillators[poly][osc]);
                        //Patch.Oscillators[poly][osc].Adsr.Pulse.adsr = Patch.Oscillators[poly][osc].Adsr;
                    }
                }
            }

            // Create GUI:
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CreateLayout(Patch.Layout);
                CreateControls();
                CreateWiring();
                MakeConnections();

                // Make sure all controls has the correct size and position:
                Controls.ResizeControls(gridControls, Window.Current.Bounds);
                Controls.SetControlsUniform(gridControls);
                Controls.ResizeControls(gridControlPanel, Window.Current.Bounds);
                Controls.SetControlsUniform(gridControlPanel);

                ((Rotator)ControlPanel.SubControls.ControlsList[(int)ControlPanelControls.LAYOUT]).Selection = (int)Patch.Layout;

                for (int osc = 0; osc < Patch.OscillatorCount; osc++)
                {
                    selectedOscillator = Patch.Oscillators[0][osc];
                    for (int poly = 0; poly < Patch.Polyphony; poly++)
                    {
                        //Patch.Oscillators[poly][osc].Init(this);
                        //Patch.Oscillators[poly][osc].mainPage = this;
                        Patch.Oscillators[poly][osc].WaveData = new double[SampleCount];
                        Patch.Oscillators[poly][osc].Filter.PostCreationInit();
                        Patch.Oscillators[poly][osc].PitchEnvelope.mainPage = this;
                        Patch.Oscillators[poly][osc].Adsr.mainPage = this;
                        //Patch.Oscillators[poly][osc].Adsr.Pulse.mainPage = this;
                        //Patch.Oscillators[poly][osc].WaveShape.PostCreationInit(this, os);
                    }
                }
            });
            return true;
        }

        /// <summary>
        /// Localsettings contains almost all data for the Patch.
        /// The Patch contains all data for a working setup except the
        /// modulator and modulating oscillators. Those are referenced
        /// by Id number in settings, and are referenced when the Patch
        /// has been successfully read.
        /// </summary>
        /// <returns></returns>
        private async Task LoadLocalState()
        {
            Patch = new Patch(this);
            //GCSupressor.SupressPatch(Patch);
            return; //!!!
            // TODO This is not used. Remove it and possibly replace with a setting to remember the full state between sessions.
            // Rather than splitting the data into LocalSettings.Values entries, save it as a file in LocalState instead.

            String settings = "";
            String temp;
            if (ApplicationData.Current.LocalSettings.Values["Setup0"] == null)
            {
                //GCSupressor.ReleasePatch(Patch);
                Patch = new Patch(this);
                //GCSupressor.SupressPatch(Patch);
            }
            else
            {
                int i = 0;
                do
                {
                    temp = (string)ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()];
                    settings += temp;
                    i++;
                } while (temp != null);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //GCSupressor.ReleasePatch(Patch);
                    Patch = new Patch(this);
                    //GCSupressor.SupressPatch(Patch);
                });
            }
        }

        private void StoreSettings()
        {
            //Patch = new Patch(oscillatorsById, adsr[0], filterGUI, currentOscillator.Id);
            try
            {
                String settings = JsonConvert.SerializeObject(Patch, Formatting.Indented);

                // Localsettings values are limited in length. Chop it up in parts of
                // permitted length and store as separate numbered values:
                List<String> settingsParts = new List<String>();
                int i = 0;
                while (settings.Length > 0)
                {
                    if (settings.Length > 2048)
                    {
                        ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] = settings.Remove(2048);
                        settings = settings.Remove(0, 2048);
                    }
                    else
                    {
                        ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] = settings;
                        settings = "";
                    }
                    i++;
                }

                // If a longer json string was stored earlier, we must remove the extra
                // parts. Otherwise LoadSettings would add them too, and deliver an invalid json.
                while (ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] != null)
                {
                    ApplicationData.Current.LocalSettings.Values["Setup" + i.ToString()] = null;
                    i++;
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Error(exception.Message);
                _ = error.ShowAsync();
            }
        }
    }
}
