using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        public Oscilloscope oscilloscope;
    }

    public class Oscilloscope
    {
        private Brush brush = new SolidColorBrush(Windows.UI.Colors.Chartreuse);
        private MainPage mainPage;
        private Rect oscilloscopeBounds;
        private double relativeSizeX;
        private double relativeSizeY;
        public Point oscilloskopOffset { set { SetBounds(value); } }
        public int VoltsPerCm;
        public double MillisecondsPerCm;
        public int X;
        public int Y;
        public float[] waveFrames;
        private bool allowNewWaveData = true;

        /// <summary>
        /// We have a waveData buffers with 2400 floats, made to hold 12 times 200 floats,
        /// where the 12 corresponds to 12 generated 480 samples from one channel of one frame of samples.
        /// waveDataBufferWriteIndex is the current position to write new wave data to.
        /// waveDataBufferReadIndex is the current position to read wave data from.
        /// </summary>
        private int waveDataBufferWriteIndex = 0;
        private int waveDataBufferReadIndex = 0;

        public Oscilloscope(MainPage mainPage)
        {
            this.mainPage = mainPage;
            waveFrames = new float[2400];
        }

        public void SetBounds(Point point)
        {
            oscilloscopeBounds = new Rect(point.X + 9, point.Y + 16, 200, 100);
        }

        public void Resize(Rect newSize)
        {
            relativeSizeX = newSize.Width / 1920;
            relativeSizeY = newSize.Height / 1040;
        }

        /// <summary>
        /// Adds one frame of sound data, 480 doubles, to waveFrames at
        /// current write position.
        /// </summary>
        /// <param name="WaveData"></param>
        public async void AddWaveData(float[] WaveData, uint requestedNumberOfSamples)
        {
            while (!allowNewWaveData)
            {
                await Task.Delay(1);
            }

            // We only copy one channel here since the oscilloscope
            // is a single ray scope. We use left channel:
            if (WaveData != null && WaveData.Length == requestedNumberOfSamples * 2)
            {
                for (uint i = 0; i < requestedNumberOfSamples * 2; i += 2)
                {
                    if ((i / 2) + waveDataBufferWriteIndex >= waveFrames.Length)
                    {
                        waveDataBufferWriteIndex -= waveFrames.Length;
                    }
                    try
                    {
                        waveFrames[(i / 2) + waveDataBufferWriteIndex] = WaveData[i];
                    }
                    catch
                    {
                        waveDataBufferWriteIndex = 0;
                    }

                }

                // Advance waveDataBufferWriteIndex one frame to prepare for next frame:
                waveDataBufferWriteIndex +=  (int)requestedNumberOfSamples;
                if (waveDataBufferWriteIndex >= waveFrames.Length)
                {
                    waveDataBufferWriteIndex -= waveFrames.Length;
                }

                // Set waveDataBufferReadIndex to the oldest data,
                // which is the position where next writes starts:
                waveDataBufferReadIndex = waveDataBufferWriteIndex;
            }
        }

        /// <summary>
        /// When all keys are off, the waveFrames still contains wave data which
        /// will show momentary when a key is pressed. Therefore, when last playing
        /// key is released, the waveFrames are removed by calling this.
        /// </summary>
        public void RemoveWaveFrames()
        {
            if (waveFrames != null)
            {
                for (int i = 0; i < waveFrames.Length; i++)
                {
                    waveFrames[i] = 0;
                }
            }
        }

        /// <summary>
        /// Makes a temporary buffer representing a flat
        /// line to display by the oscilloscope.
        /// </summary>
        /// <returns></returns>
        //public async Task<List<Point>> ClearGraph()
        public List<Point> ClearGraph()
        {
            //while (!allowNewWaveData)
            //{
            //    await Task.Delay(1);
            //}

            List<double> localBuffer = new List<double>();
            for (int i = 0; i < 200; i++)
            {
                localBuffer.Add(0);
            }
            return BufferToPoints(localBuffer, 60);
        }

        public List<Point> MakeSample(Oscillator oscillator)
        {
            List<Point> samples = new List<Point>();
            oscillator.AngleLeft = 0;
            for (int i = 0; i < 200; i++)
            {
                // Use any channel to create graph.
                samples.Add(new Point(i + 10, 65 - 40 * oscillator.MakeWave(Channel.LEFT, OscillatorUsage.OUTPUT)));
                oscillator.AngleLeft += Math.PI / 50;
                if (oscillator.AngleLeft > 2 * Math.PI)
                {
                    oscillator.AngleLeft -= 2 * Math.PI;
                }
            }
            return samples;
        }

        /// <summary>
        /// Finds a positive zero pass in the waveFrames buffer and
        /// assemples a temporary buffer to display by the oscilloscope.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public List<Point> MakeGraph(uint requestedNumberOfSamples)
        {
            allowNewWaveData = false;
            List<double> localBuffer = new List<double>();
            double sourceStepSize = requestedNumberOfSamples / 200 * MillisecondsPerCm;

            // Finding a trigger point. Since Y-axis is positive down, the oscilloscope
            // graph is upside down, so we fix that by trigging at PI rather than 0.
            int count = 0;

            // Find first negative value:
            while (waveFrames[waveDataBufferReadIndex] > 0
                && waveDataBufferReadIndex + 1 != waveDataBufferWriteIndex && count++ < waveFrames.Length)
            {
                waveDataBufferReadIndex++;
                waveDataBufferReadIndex = (waveDataBufferReadIndex < waveFrames.Length ? 
                    waveDataBufferReadIndex : waveDataBufferReadIndex - waveFrames.Length);
            }

            // Find first trigger point (at first positive value):
            while (waveFrames[waveDataBufferReadIndex] <= 0
                && waveDataBufferReadIndex + 1 != waveDataBufferWriteIndex && count++ < waveFrames.Length)
            {
                waveDataBufferReadIndex++;
                waveDataBufferReadIndex = (waveDataBufferReadIndex < waveFrames.Length ?
                    waveDataBufferReadIndex : waveDataBufferReadIndex - waveFrames.Length);
            }

            // Back up two samples to include zero passage. That looks better
            // with square waves and sawtooth up waves:
            waveDataBufferReadIndex--;
            waveDataBufferReadIndex--;
            if (waveDataBufferReadIndex < 0)
            {
                waveDataBufferReadIndex += 2400;
            }

            // Set read point accordingly:
            double sourceReadPoint = waveDataBufferReadIndex;
            int destinationIndex;

            // Pick values from the wave data buffer and fill the local 
            // buffer with 200 doubles.
            for (int i = 0; i < 200; i++)
            {
                // The fetch point is moving forward by i * 480f / 200f * MillisecondsPerCm for each sample to draw.
                // If we by last increment of i passed the end fo the source buffer, move the read index one
                // read buffer size backwards. Note that this might cause the read index to become negative, but
                // since it is used in addition to  i * 480f / 200f * MillisecondsPerCm it still points 
                // into the source buffer.
                //waveDataBufferReadIndex = waveDataBufferReadIndex + i * 480f / 200f * MillisecondsPerCm < 24000 ? 
                //    waveDataBufferReadIndex : waveDataBufferReadIndex - 24000;
                //localBuffer.Add(waveFrames[(int)(waveDataBufferReadIndex + i * 480f / 200f * MillisecondsPerCm)]);
                //if (sourceReadPoint + sourceStepSize >= waveFrames.Length)
                //{
                //    sourceReadPoint -= waveFrames.Length;
                //}
                destinationIndex = (int)((float)sourceReadPoint + sourceStepSize);
                //if (destinationIndex % 2 > 0)
                //{
                //    destinationIndex++;
                //}
                if (destinationIndex >= waveFrames.Length)
                {
                    destinationIndex -= waveFrames.Length;
                }
                try
                {
                    localBuffer.Add(waveFrames[destinationIndex]);
                }
                catch
                {
                    destinationIndex = 0;
                }

                sourceReadPoint += sourceStepSize;
            }
            allowNewWaveData = true;
            return BufferToPoints(localBuffer, 58);
        }

        /// <summary>
        /// Used by MakeGraphData to translate the float values generated 
        /// by the wave-generating functions into integers with an amplitude of 80 pixels.
        /// </summary>
        /// <param name="inBuffer"></param>
        /// <returns></returns>
        public List<Point> BufferToPoints(List<double> inBuffer, double height)
        {
            double factor = 0;
            List<Point> outBuffer = new List<Point>();
            double max = 0;
            double min = 0;
            double offset = 0;

            // Each incoming sample represents a 1/48 ms.
            // The samples are re-sampled from 480 to 200 samples to display.
            // So, each displayed sample represents 2.4/48 = 0,05 ms.
            // There are 20 displayed samples/cm on screen, so one cm holds 1 ms.
            if (VoltsPerCm == 0)
            {
                // Measure largest amplitude:
                for (int i = 0; i < inBuffer.Count; i++)
                {
                    if (inBuffer[i] > 0 && max < inBuffer[i])
                    {
                        max = inBuffer[i];
                    }
                    else if (inBuffer[i] < 0 && min > inBuffer[i])
                    {
                        min = inBuffer[i];
                    }
                }
                factor = (max - min) * 0.7;
                offset = (max + min) / 2;

                // Convert to an amplitude of height peak-to-peak and invert signal
                // while creating points:
                for (int i = 0; i < inBuffer.Count; i++)
                {
                    if (factor > 0)
                    {
                        // Graph hight = 92 px. Center is 63 so span is from 17 - 109
                        outBuffer.Add(new Point(i + 9, (63 + (offset - inBuffer[i]) * height / factor)));
                    }
                    else
                    {
                        outBuffer.Add(new Point(i + 9, 63));
                    }
                }
            }
            else
            {
                // Convert to an amplitude considering VoltsPerCm peak-to-peak and invert signal
                for (int i = 0; i < inBuffer.Count; i++)
                {
                    // Graph hight = 92 px. Center is 63 so span is from 17 - 109
                    double y = (63 + inBuffer[i] * 1000 / VoltsPerCm);
                    y = y > 130 ? 130 : y;
                    y = y < -3 ? -3 : y;
                    outBuffer.Add(new Point(i + 9, y));
                }
            }

            return outBuffer;
        }
    }
}
