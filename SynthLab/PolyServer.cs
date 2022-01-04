using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UwpControlsLibrary;

namespace SynthLab
{
    /// <summary>
    /// The frame server is the one recieving requests from the AudioGraph system.
    /// The reason for the frame server is that the AudioGraph system calls oscillators
    /// that are supposed to produce output sound at different times. Since Addition
    /// synthesis is supposed to create a mix of sound from multiple oscillators, those
    /// oscillators need to run in sync.
    /// The frame server keeps a list of oscillators that are actually supposed
    /// to create output sound. It is the only object registered as an input node to be
    /// called from the AudioGraph system.
    /// When a request comes in it generates the sound by running the logic of the 
    /// oscillators that are set to create the sound, and multiplies all samples
    /// togehter to create the final sound, which is delivered to the AudioGraph system.
    /// It also has the reverb effects.
    /// Note that playing multiple keys constitutes playing different frequencies,
    /// and the buffer size is different fore each frequency. Ergo, we need one frame
    /// server for each played key. Since the polyphony is Patch.Polyphony, we need Patch.Polyphony frame servers.
    /// </summary>
    public class PolyServer
    {
        /// <summary>
        /// The frame server handles oscillators in Patch that is to be used as if they were
        /// running at the same time. The frame server performs their logic and adds
        /// their buffers together to deliver as the resulting frame to AudioGraph.
        /// </summary>

        /// <summary>
        /// Indicates whether the oscillator is currently active and generating
        /// wave data or not.
        /// </summary>
        public Boolean IsOn = false;
        private MainPage mainPage;
        public float[] WaveData;
        public byte Key;
        public double FrequencyInUse;

        public PolyServer(MainPage mainPage)
        {
            this.mainPage = mainPage;
            WaveData = new float[mainPage.SampleCount];
        }

        /// <summary>
        /// Assembles a frame from all actively sounding oscillators in current polyphony layer.
        /// </summary>
        /// <param name="requestedNumberOfSamples"></param>
        /// <returns></returns>
        public async void GenerateAudioData(uint requestedNumberOfSamples, int poly, int channel)
        {
            uint bufferSize = requestedNumberOfSamples;

            // Do not allow GUI activity to interrupt data generating, 
            // wait while GUI changes are performed
            while (mainPage.CurrentActivity == CURRENTACTIVITY.CHANGING_PARAMETERS)
            {
                await Task.Delay(1);
            }

            // Start with an empty data list:
            for (int i = 0; i < requestedNumberOfSamples; i++)
            {
                WaveData[i] = 0;
            }

            // Make a sample of data for each sounding oscillator in this poly:
            for (int osc = 0; osc < mainPage.Patch.OscillatorCount; osc++)
            {
                if (mainPage.Patch.Oscillators[poly][osc] != null && mainPage.Patch.Oscillators[poly][osc].Volume > 0
                    && mainPage.Patch.Oscillators[poly][osc].MidiChannel == channel)
                {
                    // Create sound from this oscillator:
                    mainPage.Patch.Oscillators[poly][osc].GenerateAudioData(requestedNumberOfSamples);

                    for (int i = 0; i < requestedNumberOfSamples; i++)
                    {
                        WaveData[i] += (float)mainPage.Patch.Oscillators[poly][osc].WaveData[i];
                    }
                }
            }
        }
    }
}
