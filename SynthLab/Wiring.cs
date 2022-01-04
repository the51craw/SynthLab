using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public enum SocketType
    {
        AM,
        FM,
        XM,
        OUT,
    }

    public sealed partial class MainPage : Page
    {
        // Connection points:
        Socket Source = null;
        Socket Destination = null;

        public int HangingWire;

        public void MakeConnections()
        {
            foreach (Oscillator osc in Patch.Oscillators[0])
            {
                if (osc.AM_ModulatorId > -1)
                {
                    Source = new Socket(SocketType.OUT, osc.AM_ModulatorId);
                    Destination = new Socket(SocketType.AM, osc.Id);
                    if (MakeConnection())
                    {
                        for (int poly = 0; poly < Patch.Polyphony; poly++)
                        {
                            CreateWaveform(Patch.Oscillators[poly][osc.Id]);
                        }
                    }
                }
                if (osc.FM_ModulatorId > -1)
                {
                    Source = new Socket(SocketType.OUT, osc.FM_ModulatorId);
                    Destination = new Socket(SocketType.FM, osc.Id);
                    if (MakeConnection())
                    {
                        for (int poly = 0; poly < Patch.Polyphony; poly++)
                        {
                            CreateWaveform(Patch.Oscillators[poly][osc.Id]);
                        }
                    }
                }
                if (osc.XM_ModulatorId > -1)
                {
                    Source = new Socket(SocketType.OUT, osc.XM_ModulatorId);
                    Destination = new Socket(SocketType.XM, osc.Id);
                    if (MakeConnection())
                    {
                        for (int poly = 0; poly < Patch.Polyphony; poly++)
                        {
                            CreateWaveform(Patch.Oscillators[poly][osc.Id]);
                        }
                    }
                }
            }
        }

        public int FindWire(Socket source, Socket destination)
        {
            for (int i = 0; i < Patch.Wiring.wires.Count; i++)
            {
                if (Patch.Wiring.wires[i] != null && 
                    Patch.Wiring.wires[i].Source.OscillatorId == source.OscillatorId &&
                    Patch.Wiring.wires[i].Destination.OscillatorId == destination.OscillatorId &&
                    Patch.Wiring.wires[i].Destination.SocketType == destination.SocketType)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindWire(int source, int destination, SocketType socketType)
        {
            for (int i = 0; i < Patch.Wiring.wires.Count; i++)
            {
                if (Patch.Wiring.wires[i] != null &&
                    Patch.Wiring.wires[i].Source.OscillatorId == source &&
                    Patch.Wiring.wires[i].Destination.OscillatorId == destination &&
                    Patch.Wiring.wires[i].Destination.SocketType == socketType)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool MakeConnection()
        {
            int wireNumber = FindWire(Source, Destination);
            Oscillator sourceOscillator = GetOscillatorById(Source.OscillatorId);
            Oscillator destinationOscillator = GetOscillatorById(Destination.OscillatorId);

            if (wireNumber > -1)
            {
                ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                Patch.Wiring.wires[wireNumber].Indicator.IsOn = true;

                for (int poly = 0; poly < Patch.Polyphony; poly++)
                {
                    switch (Destination.SocketType)
                    {
                        case SocketType.AM:
                            Patch.Oscillators[poly][destinationOscillator.Id].AM_Modulator =
                                Patch.Oscillators[poly][sourceOscillator.Id];
                            Patch.Oscillators[poly][destinationOscillator.Id].AM_ModulatorId =
                                sourceOscillator.Id;
                            break;
                        case SocketType.FM:
                            Patch.Oscillators[poly][destinationOscillator.Id].FM_Modulator =
                                Patch.Oscillators[poly][sourceOscillator.Id];
                            Patch.Oscillators[poly][destinationOscillator.Id].FM_ModulatorId =
                                sourceOscillator.Id;
                            break;
                        case SocketType.XM:
                            Patch.Oscillators[poly][destinationOscillator.Id].XM_Modulator =
                                Patch.Oscillators[poly][sourceOscillator.Id];
                            Patch.Oscillators[poly][destinationOscillator.Id].XM_ModulatorId =
                                sourceOscillator.Id;
                            break;
                    }
                }
                Patch.Wiring.SourceConnected = false;
                Patch.Wiring.DestinationConnected = false;
            }

            return wireNumber > -1;
        }

        public void Disconnect(int wireId, SocketType socketType)
        {
            for (int poly = 0; poly < Patch.Polyphony; poly++)
            {
                switch (socketType)
                {
                    case SocketType.AM:
                        Disconnect(wireId, SocketType.AM, poly);
                        Patch.Wiring.wires[wireId].Indicator.IsOn = false;
                        break;
                    case SocketType.FM:
                        Disconnect(wireId, SocketType.FM, poly);
                        Patch.Wiring.wires[wireId].Indicator.IsOn = false;
                        break;
                    case SocketType.XM:
                        Disconnect(wireId, SocketType.XM, poly);
                        Patch.Wiring.wires[wireId].Indicator.IsOn = false;
                        break;
                }
            }
        }

        private void Disconnect(int id, SocketType socketType, int poly)
        {
            Oscillator source = GetOscillatorById(Patch.Wiring.wires[id].Source.OscillatorId);
            Oscillator destination = GetOscillatorById(Patch.Wiring.wires[id].Destination.OscillatorId);
            switch (socketType)
            {
                case SocketType.AM:
                    Patch.Oscillators[poly][destination.Id].AM_Modulator = null;
                    Patch.Oscillators[poly][destination.Id].AM_ModulatorId = -1;
                    break;
                case SocketType.FM:
                    Patch.Oscillators[poly][destination.Id].FM_Modulator = null;
                    Patch.Oscillators[poly][destination.Id].FM_ModulatorId = -1;
                    break;
                case SocketType.XM:
                    Patch.Oscillators[poly][destination.Id].XM_Modulator = null;
                    Patch.Oscillators[poly][destination.Id].XM_ModulatorId = -1;
                    break;
            }
        }

        private int FindConnectedWire(SocketType socketType, int oscillatorId)
        {
            for (int wireNumber = 0; wireNumber < Patch.Wiring.wires.Count; wireNumber++)
            {
                if (socketType == SocketType.OUT)
                {
                    if (Patch.Wiring.wires[wireNumber].Source != null
                        && Patch.Wiring.wires[wireNumber].Source.SocketType == socketType
                        && Patch.Wiring.wires[wireNumber].Source.OscillatorId == oscillatorId
                        && Patch.Wiring.wires[wireNumber].Indicator != null
                        && Patch.Wiring.wires[wireNumber].Indicator.IsOn)
                    {
                        return wireNumber;
                    }
                }
                else
                {
                    if (Patch.Wiring.wires[wireNumber].Destination != null
                        && Patch.Wiring.wires[wireNumber].Destination.SocketType == socketType
                        && Patch.Wiring.wires[wireNumber].Destination.OscillatorId == oscillatorId
                        && Patch.Wiring.wires[wireNumber].Indicator != null
                        && Patch.Wiring.wires[wireNumber].Indicator.IsOn)
                    {
                        return wireNumber;
                    }
                }
            }
            return -1;
        }

        private void RemoveConnection()
        {

        }
    }

    /// <summary>
    /// The Socket represents an endpoint of a wire.
    /// A wire has two endpoints, one Socket of type
    /// AM, FM or XM, and one Socket of type OUT.
    /// </summary>
    public class Socket
    {
        /// <summary>
        /// SocketType is one of AM, FM, XM or OUT
        /// </summary>
        public SocketType SocketType;

        /// <summary>
        /// The Id of the oscillator the Socket is on
        /// </summary>
        public int OscillatorId;

        public Socket(SocketType socketType, int oscillatorId)
        {
            SocketType = socketType;
            OscillatorId = oscillatorId;
        }
    }

    /// <summary>
    /// Wiring holds all wires a user can attach between oscillators.
    /// </summary>
    public class Wiring
    {
        /// <summary>
        /// List of all wires
        /// </summary>
        public List<Wire> wires = new List<Wire>();
        
        /// <summary>
        /// Array of all available inputs for current layout
        /// </summary>
        public AreaButton[] inputs = new AreaButton[36];

        /// <summary>
        /// Array of all available outputs for current layout
        /// </summary>
        public AreaButton[] outputs = new AreaButton[13];

        public Boolean SourceConnected = false;
        public Boolean DestinationConnected = false;
    }

    public class Wire
    {
        public Socket Source { get; }
        public Socket Destination { get; }
        public SocketType ModulationType { get; }
        public WireTag Tag;

        [JsonIgnore]
        public Indicator Indicator;

        public Wire (SocketType modulationType, Socket source, Socket destination, Indicator indicator)
        {
            ModulationType = modulationType;
            Source = source;
            Destination = destination;
            Indicator = indicator;
        }
    }
}
