using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    class MidiIn
    {
        public MidiInPort MidiInPort;
        public byte MidiInPortChannel;
        public List<DeviceInformation> MidiInDevices;
        public List<MidiInPort> portList;
        public List<string> portNames;

        private MainPage mainPage;

        public MidiIn(MainPage mainPage)
        {
            this.mainPage = mainPage;
            portList = new List<MidiInPort>();
            portNames = new List<string>();
        }

        //~MidiIn()
        //{
        //    foreach (MidiInPort port in portList)
        //    {
        //        MidiInPort.MessageReceived -= mainPage.MidiInPort_MessageReceivedAsync;
        //    }
        //    MidiInPort.Dispose();
        //}

        public async Task<bool> Init()
        {
            try
            {
                MidiInDevices = new List<DeviceInformation>();
                DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(MidiInPort.GetDeviceSelector());
                foreach (DeviceInformation inDevice in midiInputDevices)
                {
                    if (!inDevice.Name.Contains("CTRL"))
                    {
                        MidiInDevices.Add(inDevice);
                        MidiInPort = await MidiInPort.FromIdAsync(inDevice.Id);
                        if (MidiInPort == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Unable to create MidiInPort from input device");
                        }
                        else
                        {
                            portList.Add(MidiInPort);
                            MidiInPort.MessageReceived += mainPage.MidiInPort_MessageReceivedAsync;
                            portNames.Add(inDevice.Name);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Error("Unexpected error using FFT: " + exception.Message);
                _ = error.ShowAsync();
                return false;
            }
            return true;
        }
    }
}
