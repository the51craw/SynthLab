using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace SynthLab
{
    public class GCSupressor
    {
        List<Object> supressedObjects;
        Object lockObject;
        Boolean inUse = true;

        public GCSupressor()
        {
            supressedObjects = new List<Object>();
            lockObject = new Object();
        }

        public void Supress(Object o)
        {
            if (inUse)
            {
                if (o != null)
                {
                    GC.SuppressFinalize(o);
                    supressedObjects.Add(o);
                }
                ulong available = MemoryManager.ExpectedAppMemoryUsageLimit;
                if ((ulong)GC.GetTotalMemory(false) > available - 1000000000)
                {
                    ReleaseAll();
                }
            }
        }

        public void Release(object o)
        {
            if (o != null)
            {
                GC.ReRegisterForFinalize(o);
                GC.Collect();
            }
        }

        public void ReleaseAll()
        {
            if (inUse)
            {
                lock (lockObject)
                {
                    while (supressedObjects.Count > 0)
                    {
                        if (supressedObjects[0] != null)
                        {
                            GC.ReRegisterForFinalize(supressedObjects[0]);
                        }
                        supressedObjects.RemoveAt(0);
                    }
                }
                GC.Collect();
            }
        }

        public void SupressPatch(Patch patch)
        {
            foreach (List<Oscillator> oscillators in patch.Oscillators)
            {
                foreach (Oscillator oscillator in oscillators)
                {
                    GC.SuppressFinalize(oscillator);
                    if (oscillator.WaveShape != null)
                    {
                        GC.SuppressFinalize(oscillator.WaveShape);
                        if (oscillator.WaveShape.OriginalWaveData != null)
                        {
                            GC.SuppressFinalize(oscillator.WaveShape.OriginalWaveData);
                        }
                    }
                    if (oscillator.Filter != null)
                    {
                        GC.SuppressFinalize(oscillator.Filter);
                    }
                    if (oscillator.PitchEnvelope != null)
                    {
                        GC.SuppressFinalize(oscillator.PitchEnvelope);
                    }
                    if (oscillator.Adsr != null)
                    {
                        GC.SuppressFinalize(oscillator.Adsr);
                    }
                    if (oscillator.WaveData != null)
                    {
                        GC.SuppressFinalize(oscillator.WaveData);
                    }
                    if (oscillator.fftData != null)
                    {
                        GC.SuppressFinalize(oscillator.fftData);
                    }
                }
            }
        }

        public void ReleasePatch(Patch patch)
        {
            if (patch != null && patch.Oscillators != null)
            {
                foreach (List<Oscillator> oscillators in patch.Oscillators)
                {
                    foreach (Oscillator oscillator in oscillators)
                    {
                        GC.ReRegisterForFinalize(oscillator);
                        if (oscillator.WaveShape != null)
                        {
                            GC.ReRegisterForFinalize(oscillator.WaveShape);
                            if (oscillator.WaveShape.OriginalWaveData != null)
                            {
                                GC.ReRegisterForFinalize(oscillator.WaveShape.OriginalWaveData);
                            }
                        }
                        if (oscillator.Filter != null)
                        {
                            GC.ReRegisterForFinalize(oscillator.Filter);
                        }
                        if (oscillator.PitchEnvelope != null)
                        {
                            GC.ReRegisterForFinalize(oscillator.PitchEnvelope);
                        }
                        if (oscillator.Adsr != null)
                        {
                            GC.ReRegisterForFinalize(oscillator.Adsr);
                        }
                        if (oscillator.WaveData != null)
                        {
                            GC.ReRegisterForFinalize(oscillator.WaveData);
                        }
                        if (oscillator.fftData != null)
                        {
                            GC.ReRegisterForFinalize(oscillator.fftData);
                        }
                    }
                }
                GC.Collect();
            }
        }
    }
}
