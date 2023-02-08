using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Devices.Midi;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SynthLab
{
    /// <summary>
    /// Event handler allowing use of computer keyboard to play notes
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int xpose = 0;
        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            //Debug.WriteLine(args.KeyStatus.ScanCode.ToString());
            //return;
            if ((int)args.KeyStatus.ScanCode == 73 && xpose < 2)
            {
                xpose++;
            }
            else if ((int)args.KeyStatus.ScanCode == 81 && xpose > -2)
            {
                xpose--;
            }
            else
            {
                int note = KeyToNote((int)args.KeyStatus.ScanCode);
                if (note > -1)
                {
                    // HBE if (!dispatcher[0].KeyIsPlaying(note))
                    {
                        KeyOn((byte)note, 0, 64);
                    }
                }
            }
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            int note = KeyToNote((int)args.KeyStatus.ScanCode);
            if (note > -1)
            {
                KeyOff((byte)note, 0);
            }
        }

        private int KeyToNote(int key)
        {
            int[] codes = new int[]
            {
               44, 31, 45, 32, 46, 47, 34, 48, 35, 
                49, 36, 50, 51, 38, 52, 39, 53, 16, 
                3, 17, 4, 18, 5, 19, 20, 7, 21, 8, 
                22, 23, 10, 24, 11, 25, 12, 26, 27
            };
            for (int i = 0; i < codes.Length; i++)
            {
                if (codes[i] == key)
                {
                    return i + 48 + xpose * 12;
                }
            }
            return -1;
        }
    }
}
