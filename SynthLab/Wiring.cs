using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UwpControlsLibrary;
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

        public void MakeConnections(bool force = false)
        {
            foreach (Oscillator osc in Oscillators[0])
            {
                if (osc.AM_ModulatorId > -1)
                {
                    Source = new Socket(SocketType.OUT, osc.AM_ModulatorId);
                    Destination = new Socket(SocketType.AM, osc.Id);
                    MakeConnection(force);
                }
                if (osc.FM_ModulatorId > -1)
                {
                    Source = new Socket(SocketType.OUT, osc.FM_ModulatorId);
                    Destination = new Socket(SocketType.FM, osc.Id);
                    MakeConnection(force);
                }
                if (osc.XM_ModulatorId > -1)
                {
                    Source = new Socket(SocketType.OUT, osc.XM_ModulatorId);
                    Destination = new Socket(SocketType.XM, osc.Id);
                    MakeConnection(force);
                }
            }
        }

        public int FindWire(Socket source, Socket destination)
        {
            for (int i = 0; i < Wiring.wires.Count; i++)
            {
                if (Wiring.wires[i] != null && 
                    Wiring.wires[i].Source.OscillatorId == source.OscillatorId &&
                    Wiring.wires[i].Destination.OscillatorId == destination.OscillatorId &&
                    Wiring.wires[i].Destination.SocketType == destination.SocketType)
                {
                    return i;
                }
            }
            return -1;
        }

        public int FindWire(int source, int destination, SocketType socketType)
        {
            for (int i = 0; i < Wiring.wires.Count; i++)
            {
                if (Wiring.wires[i] != null &&
                    Wiring.wires[i].Source.OscillatorId == source &&
                    Wiring.wires[i].Destination.OscillatorId == destination &&
                    Wiring.wires[i].Destination.SocketType == socketType)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool MakeConnection(bool force = false)
        {
            int wireNumber = FindWire(Source, Destination);
            Oscillator sourceOscillator = GetOscillatorById(Source.OscillatorId);
            Oscillator destinationOscillator = GetOscillatorById(Destination.OscillatorId);

            if (wireNumber > -1 || force)
            {
                ((Indicator)Controls.ControlsList[HangingWire]).IsOn = false;
                if (wireNumber > -1)
                {
                    Wiring.wires[wireNumber].Indicator.IsOn = true;
                }

                for (int poly = 0; poly < 6; poly++)
                {
                    switch (Destination.SocketType)
                    {
                        case SocketType.AM:
                            Oscillators[poly][destinationOscillator.Id].AM_Modulator =
                                Oscillators[poly][sourceOscillator.Id];
                            Oscillators[poly][destinationOscillator.Id].AM_ModulatorId =
                                sourceOscillator.Id;
                            break;
                        case SocketType.FM:
                            Oscillators[poly][destinationOscillator.Id].FM_Modulator =
                                Oscillators[poly][sourceOscillator.Id];
                            Oscillators[poly][destinationOscillator.Id].FM_ModulatorId =
                                sourceOscillator.Id;
                            break;
                        case SocketType.XM:
                            Oscillators[poly][destinationOscillator.Id].XM_Modulator =
                                Oscillators[poly][sourceOscillator.Id];
                            Oscillators[poly][destinationOscillator.Id].XM_ModulatorId =
                                sourceOscillator.Id;
                            Oscillators[poly][sourceOscillator.Id].Modulating.Add(
                            Oscillators[poly][destinationOscillator.Id]);
                            break;
                    }
                }
                Wiring.SourceConnected = false;
                Wiring.DestinationConnected = false;
            }

            if (force)
            {
                return true;
            }

            return wireNumber > -1;
        }

        public void Disconnect(int wireId, SocketType socketType)
        {
            for (int poly = 0; poly < 6; poly++)
            {
                switch (socketType)
                {
                    case SocketType.AM:
                        Disconnect(wireId, SocketType.AM, poly);
                        Wiring.wires[wireId].Indicator.IsOn = false;
                        break;
                    case SocketType.FM:
                        Disconnect(wireId, SocketType.FM, poly);
                        Wiring.wires[wireId].Indicator.IsOn = false;
                        break;
                    case SocketType.XM:
                        Disconnect(wireId, SocketType.XM, poly);
                        Wiring.wires[wireId].Indicator.IsOn = false;
                        break;
                }
            }
        }

        private void Disconnect(int id, SocketType socketType, int poly)
        {
            Oscillator destination = GetOscillatorById(Wiring.wires[id].Destination.OscillatorId);
            switch (socketType)
            {
                case SocketType.AM:
                    Oscillators[poly][destination.Id].AM_Modulator = null;
                    Oscillators[poly][destination.Id].AM_ModulatorId = -1;
                    break;
                case SocketType.FM:
                    Oscillators[poly][destination.Id].FM_Modulator = null;
                    Oscillators[poly][destination.Id].FM_ModulatorId = -1;
                    break;
                case SocketType.XM:
                    destination.Modulating.Remove(destination.XM_Modulator);
                    Oscillators[poly][destination.Id].XM_Modulator = null;
                    Oscillators[poly][destination.Id].XM_ModulatorId = -1;
                    break;
            }
        }

        private int FindConnectedWire(SocketType socketType, int oscillatorId)
        {
            for (int wireNumber = 0; wireNumber < Wiring.wires.Count; wireNumber++)
            {
                if (socketType == SocketType.OUT)
                {
                    if (Wiring.wires[wireNumber].Source != null
                        && Wiring.wires[wireNumber].Source.SocketType == socketType
                        && Wiring.wires[wireNumber].Source.OscillatorId == oscillatorId
                        && Wiring.wires[wireNumber].Indicator != null
                        && Wiring.wires[wireNumber].Indicator.IsOn)
                    {
                        return wireNumber;
                    }
                }
                else
                {
                    if (Wiring.wires[wireNumber].Destination != null
                        && Wiring.wires[wireNumber].Destination.SocketType == socketType
                        && Wiring.wires[wireNumber].Destination.OscillatorId == oscillatorId
                        && Wiring.wires[wireNumber].Indicator != null
                        && Wiring.wires[wireNumber].Indicator.IsOn)
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
