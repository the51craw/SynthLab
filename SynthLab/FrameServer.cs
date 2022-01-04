using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.UI.Xaml.Controls;
//using AudioEffectComponent;
//using MathNet.Numerics.IntegralTransforms;

namespace SynthLab
{
    public enum CURRENTACTIVITY
    {
        NONE,
        GENERATING_WAVE_SHAPE,
        CHANGING_PARAMETERS,
    }

    public sealed partial class MainPage : Page
    {
        public int adjust = 0;
        public int fadjust = 0;
        public Boolean allowGuiUpdates = false;
        public FrameServer FrameServer;
        public CURRENTACTIVITY CurrentActivity = CURRENTACTIVITY.NONE;
    }
    public partial class FrameServer
    {
        MainPage mainPage;
        public PolyServer[] PolyServers;

        /// <summary>
        /// The frame server's FrameInputNode delivers the audio data to the AudioGraph system.
        /// </summary>
        public AudioFrameInputNode FrameInputNode;

        public ReverbEffectDefinition reverbEffectDefinition;

        private AudioFrame frame;

        public FrameServer(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }

        public async Task<bool> InitAudio()
        {
            if (await CreateAudioGraph())
            {
                CreateFrameInputNode();
                return true;
            }
            return false;
        }

        public void StartAudio()
        {
            StartAudioGraph();
        }

        public void Init()
        {
            //await Task.Delay(100);
            PolyServers = new PolyServer[32];
            for (int poly = 0; poly < 32; poly++)
            {
                PolyServers[poly] = new PolyServer(mainPage);
            }
        }

        /// <summary>
        /// Assembles a frame from all actively sounding oscillators
        /// in all used polyphony layer (for all pressed keys).
        /// </summary>

        public unsafe void FrameInputNode_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            if (mainPage.initDone && mainPage.SampleCount != 0 /*&& mainPage.dispatcher.NumberOfOscillatorsInUse() > 0*/)
            {
                mainPage.CurrentActivity = CURRENTACTIVITY.GENERATING_WAVE_SHAPE;
                mainPage.SampleCount = (uint)args.RequiredSamples;
                frame = new AudioFrame(mainPage.SampleCount * sizeof(float));

                using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
                using (IMemoryBufferReference reference = buffer.CreateReference())
                {
                    byte* inBytes;
                    uint capacity;
                    float* bufferPointer;

                    // Get the AudioFrame buffer
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out inBytes, out capacity);

                    // Make a float pointer for inserting wave data into buffer:
                    bufferPointer = (float*)inBytes;

                    // Since FrameSever adds all waveforms from the PolyServer
                    // together, we must first sett all samples to zero:
                    for (int i = 0; i < mainPage.SampleCount; i++)
                    {
                        bufferPointer[i] = 0;
                    }

                    // Generate and add together audio data for all channels and pressed keys:
                    for (int ch = 0; ch < 16; ch++)
                    {
                        for (int poly = 0; poly < mainPage.Patch.Polyphony; poly++)
                        {
                            if (mainPage.dispatcher[ch].PolyIsPlaying(poly)) // IsPressed[poly])
                            {
                                PolyServers[poly].GenerateAudioData(mainPage.SampleCount, poly, ch);
                                for (int i = 0; i < mainPage.SampleCount; i++)
                                {
                                    bufferPointer[i] += (float)PolyServers[poly].WaveData[i];
                                }
                            }
                        }
                    }
                }

                if (frame != null)
                {
                    FrameInputNode.AddFrame(frame);
                    if (mainPage.selectedOscillator.ModulationType == ModulationType.DX)
                    {
                        mainPage.allowGuiUpdates = true;
                    }
                }
                else
                {
                    Debug.WriteLine("Missed!");
                }
                mainPage.CurrentActivity = CURRENTACTIVITY.NONE;
                //mainPage.hold = false;
            }
        }
    }
}
