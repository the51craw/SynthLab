using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthLab
{
    /**
     * Polyphony:
     * Synthlab can play up to Patch.Polyphony tones at the same time, including tones in release!
     * Prioritizer keeps track of which oscillators are available
     * for playing next key when multiple keys are pressed.
     * 
     * KeyOn:
     *  Key[poly] = key#
     *  IsPressed[poly] = true
     *  
     * KeyOff:
     *  if !PedalHold:
     *      poly 0 -> 9
     *          adsr release
     *  else
     *      adsr release
     *      
     * PedalHold off:
     *      poly 0 -> 9
     *          if not IsPressed[poly]
     *              adsr release
     */
    public class KeyDispatcher
    {
        public Boolean PedalHold { get { return pedalHold; } set { SetPedalHold(value); } }
        public int KeyPriority = 0;
        private MainPage mainPage;
        private int polyphony;

        /// <summary>
        /// Index = poly-number, values are the keys assinged to the polys/oscillators.
        /// </summary>
        public int[] Key;

        /// <summary>
        /// Keys that were not released before pedalHold goes off are marked here to stay alive.
        /// </summary>
        public Boolean[] IsPressed;

        /// <summary>
        /// Since key-off only hands the responsibility over to ADSR that knows not of
        /// channels, the dispatcher keeps track of channel.
        /// </summary>
        //public int[] Channel;

        public int[] KeyOrder;

        private Boolean pedalHold = false;

        public KeyDispatcher(MainPage mainPage, int Polyphony)
        {
            this.mainPage = mainPage;
            polyphony = Polyphony;
            Key = new int[32];
            IsPressed = new bool[Polyphony];
            //IsPressed = new bool[16][];
            //for (int ch = 0; ch < 16; ch++)
            //{
            //    IsPressed[ch] = new bool[Polyphony];
            //}
            //Channel = new int[polyphony];
            pedalHold = false;
            ResetLists();
            KeyOrder = new int[polyphony];
            for (int i = 0; i < polyphony; i++)
            {
                KeyOrder[i] = -1;
            }
        }

        public void SetPedalHold(Boolean value)
        {
            pedalHold = value;
            if (!pedalHold)
            {
                for (int poly = 0; poly < polyphony; poly++)
                {
                    if (!IsPressed[poly])
                    {
                        for (int osc = 0; osc < mainPage.Patch.OscillatorCount; osc++)
                        {
                            //!!!
                            mainPage.KeyOff((byte)Key[poly], mainPage.Patch.Oscillators[poly][osc].MidiChannel);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If there is a free poly it will be returned, else -1.
        /// Also sets Key, Channel and IsPressed for the poly.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Channel"></param>
        /// <returns></returns>
        public int TryAssignPoly(int Key)
        {
            //if (KeyIsPlaying(Key))
            //{
            //    return -1;
            //}
            if (NumberOfOscillatorsInUse() == polyphony)
            {
                if (KeyPriority > 0)
                {
                    int freePoly = -1;

                    // All polys are taken. Find the oldest one:
                    for (int i = 0; i < polyphony; i++)
                    {
                        if (KeyOrder[i] == 0)
                        {
                            // Mark it as the newest one:
                            KeyOrder[i] = polyphony;
                            // Remember which poly to use:
                            freePoly = i;
                        }
                        // Re-number the order to still be 0 - polyphony - 1:
                        KeyOrder[i]--;
                    }

                    if (pedalHold)
                    {
                        // Remember that this key was pressed while pedal was down:
                        IsPressed[freePoly] = true;
                    }
                    if (freePoly > -1)
                    {
                        this.Key[freePoly] = Key;
                    }
                    return freePoly;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                // Since not all polys are in use, there must be one free:
                for (int poly = 0; poly < polyphony; poly++)
                {
                    if (this.Key[poly] == -1)
                    {
                        // Find the KeyOrder that is newest and make this one newer:
                        int newest = -1;
                        for (int i = 0; i < polyphony; i++)
                        {
                            if (KeyOrder[i] > newest)
                            {
                                newest = KeyOrder[i];
                            }
                        }
                        KeyOrder[poly] = newest + 1;

                        this.Key[poly] = Key;
                        //this.Channel[poly] = Channel;
                        if (pedalHold)
                        {
                            // Remember that this key was pressed while pedal was down:
                            IsPressed[poly] = true;
                        }
                        return poly;
                    }
                }
                // This should never happen:
                throw new Exception("Expected poly was not found!");
            }
        }

        //public int ChannelOfPoly(int poly)
        //{
        //    return Channel[poly];
        //}

        public int TryGetPolyFromKey(int Key)
        {
            for (int poly = 0; poly < polyphony; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    return poly;
                }
            }
            return -1;
        }

        public Boolean PolyIsPlaying(int poly)
        {
            return Key[poly] > -1;
        }

        public void ReleaseOscillator(int Key)
        {
            for (int poly = 0; poly < polyphony; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    //ReleasePoly(poly);
                    KeyOrder[poly] = -1;
                    this.Key[poly] = -1;
                    //this.Channel[poly] = -1;
                    if (pedalHold)
                    {
                        IsPressed[poly] = false;
                    }
                    //Debug.WriteLine("Released key = " + Key.ToString() + " poly =  " + poly.ToString());
                }
            }
        }

        public void ReleasePoly(int poly)
        {
            KeyOrder[poly] = -1;
            Key[poly] = -1;
            //this.Channel[poly] = -1;
            if (pedalHold)
            {
                IsPressed[poly] = false;
            }
        }

        public int RePress(int Key)
        {
            int foundPoly = -1;
            for (int poly = 0; poly < polyphony; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    foundPoly = poly;
                    break;
                }
            }

            //!!!if (foundPoly > -1)
            //{
            //    KeyOrder.Remove(foundPoly);
            //    KeyOrder.Add(foundPoly);
            //}

            return foundPoly;
        }

        public Boolean KeyIsPlaying(int Key)
        {
            //Debug.Write("Check if key " + Key.ToString() + " is playing, ");
            for (int poly = 0; poly < polyphony; poly++)
            {
                if (this.Key[poly] == Key)
                {
                    //Debug.WriteLine("found poly = " + poly.ToString());
                    return true;
                }
            }
            //Debug.WriteLine("did not find it");
            return false;
        }


        private void ResetLists()
        {
            for (int ch = 0; ch < 16; ch++)
            {
                for (int poly = 0; poly < polyphony; poly++)
                {
                    Key[poly] = -1;
                    IsPressed[poly] = false;
                }
            }
        }

        public int NumberOfOscillatorsInUse()
        {
            int count = 0;
            //for (int ch = 0; ch < 16; ch++)
            {
                for (int poly = 0; poly < polyphony; poly++)
                {
                    if (Key[poly] > -1)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
