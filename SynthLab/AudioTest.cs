using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
//using AudioEffectComponent;
using Windows.Media.Effects;
using Windows.Foundation.Collections;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Windows.UI.Core;

namespace SynthLab
{
    /// <summary>
    /// AudioTest is used to obtain the buffer size that will be required
    /// during sound generation. It is first mention when starting to 
    /// create a sound. Here we only obtain the requirement.
    /// The AudioTest object can then be disposed.
    /// The buffer size is stored in MainPage.SampleBufferSize.
    /// </summary>
    public class AudioTest
    {
        private AudioFrameInputNode FrameInputNode;
        private MainPage mainPage;

        public AudioTest(MainPage mainPage)
        {
            this.mainPage = mainPage;
            AudioEncodingProperties nodeEncodingProperties = mainPage.FrameServer.theAudioGraph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 1;
            nodeEncodingProperties.SampleRate = 48000;
            nodeEncodingProperties.BitsPerSample = 32;
            nodeEncodingProperties.Bitrate = 48000 * 32;
            FrameInputNode = mainPage.FrameServer.theAudioGraph.CreateFrameInputNode(nodeEncodingProperties);
            FrameInputNode
                .AddOutgoingConnection
                (
                    mainPage
                    .FrameServer
                    .Mixer
                );
            FrameInputNode.QuantumStarted += FrameInputNode_QuantumStarted;
            FrameInputNode.Start();
        }

        private void FrameInputNode_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            FrameInputNode.Stop();
            mainPage.SampleCount = (uint)args.RequiredSamples;
        }
    }
}
