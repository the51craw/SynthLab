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
        public List<DeviceInformation> MidiInDevices;
        public List<MidiInPort> portList;
        public List<string> portNames;
        public List<UsedPort> usedPorts;

        private MainPage mainPage;

        public MidiIn(MainPage mainPage)
        {
            this.mainPage = mainPage;
            portList = new List<MidiInPort>();
            portNames = new List<string>();
            usedPorts = new List<UsedPort>();
        }

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
                            System.Diagnostics.Debug.WriteLine("Unable to create MidiInPort from input device " + inDevice.Name);
                        }
                        else
                        {
                            portList.Add(MidiInPort);
                            MidiInPort.MessageReceived += mainPage.MidiInPort_MessageReceived;
                            portNames.Add(inDevice.Name);
                            usedPorts.Add(new UsedPort(inDevice.Id, true));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ContentDialog error = new Message("Unexpected error: " + exception.Message);
                _ = error.ShowAsync();
                return false;
            }
            return true;
        }
    }

    public class UsedPort
    {
        public string PortId;
        public bool Use;

        public UsedPort(string Id, bool Use)
        {
            PortId = Id;
            this.Use = Use;
        }
    }
}
