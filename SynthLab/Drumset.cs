using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    /// <summary>
    /// The type of sound in a synthesizer can be categorized into two types,
    /// keyboard sound and drumsets when considering how the midi note messages
    /// are used. A keyboard sound normally has the same sound on all keys, but 
    /// are transposed to a pitch corresponding to the frequency we expect to
    /// hear when pressing the key. Like a piano or an organ.
    /// A drumset is different because there can be different sounds produced
    /// from different keys, and normally the sounds can not change pitch.
    /// The drumset class in My Synthesizer Laboratory uses pre-recorded drum
    /// sounds connected to the different keys. There has been some vintage
    /// synthesizers that made drum sounds by synthesizing the 'old' way, mostly
    /// subtractive synthesis, but that is not on the agenda for Drumset. The
    /// user can synthesize drums the 'old' way anyhow, so I do not need to 
    /// include that in Drumset.
    /// A drumset is created from a folder containing sound files. The different
    /// sound files must have the key number added to the beginning of the file
    /// name. It recognizes the midi key number, but also accepts key/octave
    /// format. Between the key number and the rest of the file name there
    /// must be an underscore, even if the user elects to omit the name of
    /// the sound.
    /// 
    /// </summary>

    public sealed partial class MainPage : Page
    {
        public async Task<StorageFolder> GetDrumsetFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add(".wav");
            folderPicker.FileTypeFilter.Add(".mp3");
            folderPicker.CommitButtonText = "Ok";
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            StorageFolder filesLocation = await folderPicker.PickSingleFolderAsync();
            return filesLocation;
        }
    }

    public class Drumset
    {
        private MainPage mainPage;
        public StorageFolder FilesLocation;
        public int FileCount;
        public List<string> fileParsingResults;
        public AudioFileInputNode[] Keys;

        public Drumset(MainPage mainPage, StorageFolder filesLocation)
        {
            this.mainPage = mainPage;
            FilesLocation = filesLocation;
            Init();
        }

        public async void Init()
        {
            fileParsingResults = new List<string>();
            Keys = new AudioFileInputNode[128];
            try
            {
                IReadOnlyList<StorageFile> files = await FilesLocation.GetFilesAsync();
                FileCount = 0;
                foreach (StorageFile file in files)
                {
                    await GetDrumFile(file);
                }
            }
            catch
            {
            }
        }

        private async Task GetDrumFile(StorageFile file)
        {
            string[] parts = file.Name.Split('_');
            int key = -1;
            if (parts.Length > 1)
            {
                key = UnpackKeyNumber(parts[0]);
            }

            if (key < 0)
            { 
                fileParsingResults.Add("The file " + file.Name + " does not follow the format k_name.ext nor the format K#o_name.ext " +
                    "where k = MIDI key number (0 - 127), K = Key name (C/D/E/F/G/A/B) # is optional, o is octave number 0 - 7, name " +
                    "is any text describing the sound or nothing and ext is one of wav or mp3!");
                return;
            }
            CreateAudioFileInputNodeResult fileInputResult = await mainPage.FrameServer.theAudioGraph.CreateFileInputNodeAsync(file);
            if (fileInputResult.Status != AudioFileNodeCreationStatus.Success)
            {
                fileParsingResults.Add("The file " + file.Name + " could not be opened by the AudioGraph.");
            }
            else
            {
                Keys[key] = fileInputResult.FileInputNode;
                Keys[key].AddOutgoingConnection(mainPage.FrameServer.Mixer);
                Keys[key].Stop();
            }
        }

        private int UnpackKeyNumber(string s)
        {
            string keyNames = "CDEFGAB";
            int key = 0;
            try
            {
                int n = int.Parse(s);
                if (n > -1 && n < 128)
                {
                    return n;
                }
            }
            catch
            {
                s = s.ToUpper();
                int n = keyNames.IndexOf(s);
                if (n > -1)
                {
                    key += n;
                    if (s.Contains("#"))
                    {
                        key++;
                        s = s.Replace("#", "");
                        s = s.Remove(0, 1);
                        n = int.Parse(s);
                        if (n > -1 && n < 8)
                        {
                            return key + 12 * n;
                        }
                    }
                }
            }
            return -1;
        }
    }
}
