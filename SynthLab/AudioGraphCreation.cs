using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public partial class FrameServer
    {
        public AudioGraph theAudioGraph;
        public AudioDeviceOutputNode DeviceOutputNode;
        public AudioSubmixNode Mixer;
        //public ReverbEffectDefinition reverbEffectDefinition;
        //public EchoEffectDefinition echoEffectDefinition;

        private async Task<bool> CreateAudioGraph()
        {
            // Create an AudioGraph with default settings
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media);
            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Cannot create AudioGraph
                ContentDialog error = new Error("Failed to create connection with audio hardware!");
                await error.ShowAsync();
                return false;
            }
            theAudioGraph = result.Graph;

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await theAudioGraph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                ContentDialog error = new Error("Failed to create connection with audio hardware!");
                await error.ShowAsync();
                return false;
            }
            DeviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;

            // Create mixer:
            Mixer = theAudioGraph.CreateSubmixNode();
            Mixer.OutgoingGain = 1;
            Mixer.AddOutgoingConnection(DeviceOutputNode);

            //// Create reverb:
            //reverbEffectDefinition = new ReverbEffectDefinition(theAudioGraph);
            //reverbEffectDefinition.WetDryMix = 50;
            //reverbEffectDefinition.ReflectionsDelay = 120;
            //reverbEffectDefinition.ReverbDelay = 30;
            //reverbEffectDefinition.RearDelay = 3;
            //reverbEffectDefinition.DecayTime = 2;
            //Mixer.EffectDefinitions.Add(reverbEffectDefinition);
            //TurnOnReverb();

            // Initiate all oscillator AudioGraph related objects:
            //CreateOscillatorsInputNodes();

            //Start the AudioGraph since we will only start/stop the frame input nodes
            //theAudioGraph.Start();
            return true;
        }



        // System.Audio.ChannelCount
        // https://docs.microsoft.com/en-us/windows/win32/properties/audio-bumper


        public void CreateFrameInputNode()
        {
            //if (!mainPage.usingGraphicsCard)
            {
                // Set the NodeEncodingPorperties as the same format as the graph, except explicitly set mono:
                AudioEncodingProperties nodeEncodingProperties = theAudioGraph.EncodingProperties;
                nodeEncodingProperties.ChannelCount = 1;
                nodeEncodingProperties.Bitrate = nodeEncodingProperties.SampleRate * nodeEncodingProperties.BitsPerSample;
                mainPage.SampleRate = nodeEncodingProperties.SampleRate;
                mainPage.SampleCount = nodeEncodingProperties.SampleRate / 100;

                // Create the FrameInputNode:
                FrameInputNode = theAudioGraph.CreateFrameInputNode(nodeEncodingProperties);
                FrameInputNode.AddOutgoingConnection(DeviceOutputNode);

                //// Create reverbs:
                //reverbEffectDefinition = new ReverbEffectDefinition(mainPage.theAudioGraph);
                //reverbEffectDefinition.WetDryMix = 50;
                //reverbEffectDefinition.ReflectionsDelay = 120;
                //reverbEffectDefinition.ReverbDelay = 30;
                //reverbEffectDefinition.RearDelay = 3;
                //reverbEffectDefinition.DecayTime = 2;
                //FrameInputNode.EffectDefinitions.Add(reverbEffectDefinition);

                //// Reverbs are default off (no need for reverb on modulators).
                //// Reverbs will be set on/off with output volume.
                //TurnOffReverb();
            }
        }

        public void StartAudioGraph()
        {
            FrameInputNode.QuantumStarted += FrameInputNode_QuantumStarted;
            theAudioGraph.Start();
        }

        //public void TurnOnReverb()
        //{
        //    FrameInputNode.EnableEffectsByDefinition(reverbEffectDefinition);
        //}

        //public void TurnOffReverb()
        //{
        //    FrameInputNode.DisableEffectsByDefinition(reverbEffectDefinition);
        //}
    }
}
