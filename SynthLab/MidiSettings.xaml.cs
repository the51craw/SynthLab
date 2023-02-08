using System;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SynthLab
{
    public sealed partial class MidiSettings : ContentDialog
    {
        public int[] Channel;
        public bool[] VelocitySensitivity;
        public bool[] VelocityModulationSensitivity;

        MainPage mainPage;
        Image[][] channelImage;
        Image[][] velocityImage;
        Image[][] velocityUsageImage;

        /// <summary>
        /// 1 = velocity, 2 = modulation, 3 = velocity + modulation
        /// </summary>
        int[] velocityUsage;
        bool initialize = true;

        public MidiSettings(MainPage mainPage)
        {
            this.InitializeComponent();
            this.mainPage = mainPage;
            Channel = new int[12];
            velocityUsage = new int[12];
            VelocitySensitivity = new bool[12];
            VelocityModulationSensitivity = new bool[12];
            Setup();
        }

        public MidiSettings(MainPage mainPage, MidiSettingsData data)
        {
            this.InitializeComponent();
            this.mainPage = mainPage;
            Channel = new int[12];
            velocityUsage = new int[12];
            VelocitySensitivity = new bool[12];
            VelocityModulationSensitivity = new bool[12];

            for (int osc = 0; osc < 12; osc++)
            {
                Channel[osc] = data.Channel[osc];
                VelocitySensitivity[osc] = data.VelocitySensitivity[osc];
                VelocityModulationSensitivity[osc] = data.VelocityModulationSensitivity[osc];
            }
            Setup();
            initialize = false;
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (initialize)
            {
                for (int osc = 0; osc < 12; osc++)
                {
                    for (int poly = 0; poly < 6; poly++)
                    {
                        Channel[osc] = 0;
                        VelocitySensitivity[osc] = false;
                    }

                    for (int ch = 0; ch < 17; ch++)
                    {
                        channelImage[osc][ch].Visibility =
                            ch == Channel[osc] ? Visibility.Visible : Visibility.Collapsed;
                    }

                    velocityUsageImage[osc][0].Visibility = Visibility.Visible;
                    velocityUsageImage[osc][1].Visibility = Visibility.Collapsed;
                    velocityUsageImage[osc][2].Visibility = Visibility.Collapsed;
                }
                initialize = false;
            }

            //for (int osc = 0; osc < 12; osc++)
            //{
            //    for (int poly = 0; poly < 6; poly++)
            //    {
            //        Channel[osc] = mainPage.Oscillators[poly][osc].MidiChannel;
            //        VelocitySensitivity[osc] = mainPage.Oscillators[poly][osc].VelocitySensitive;
            //    }

            //    for (int ch = 0; ch < 17; ch++)
            //    {
            //        channelImage[osc][ch].Visibility =
            //            ch == Channel[osc] ? Visibility.Visible : Visibility.Collapsed;
            //    }

            //    velocityImage[osc][0].Visibility =
            //        mainPage.Oscillators[0][osc].VelocitySensitive > 0 ? Visibility.Collapsed : Visibility.Visible;
            //    velocityImage[osc][1].Visibility =
            //        mainPage.Oscillators[0][osc].VelocitySensitive > 0 ? Visibility.Visible : Visibility.Collapsed;

            //    if (mainPage.Oscillators[0][osc].VelocitySensitive > 0)
            //    {
            //        velocityUsageImage[osc][0].Visibility = Visibility.Collapsed;
            //        velocityUsageImage[osc][1].Visibility = Visibility.Collapsed;
            //        velocityUsageImage[osc][2].Visibility = Visibility.Collapsed;

            //        switch (mainPage.Oscillators[0][osc].VelocitySensitive)
            //        {
            //            case 1:
            //                velocityUsageImage[osc][0].Visibility = Visibility.Visible;
            //                break;
            //            case 2:
            //                velocityUsageImage[osc][1].Visibility = Visibility.Visible;
            //                break;
            //            case 3:
            //                velocityUsageImage[osc][2].Visibility = Visibility.Visible;
            //                break;
            //        }
            //    }
            //    else
            //    {
            //        velocityUsageImage[osc][0].Visibility = Visibility.Visible;
            //        velocityUsageImage[osc][1].Visibility = Visibility.Collapsed;
            //        velocityUsageImage[osc][2].Visibility = Visibility.Collapsed;
            //    }
            //}
        }

        public void Setup()
        {
            channelImage = new Image[12][];

            for (int osc = 0; osc < 12; osc++)
            {
                channelImage[osc] = new Image[17];
            }

            channelImage[0][0] = Channel1_1;
            channelImage[0][1] = Channel1_2;
            channelImage[0][2] = Channel1_3;
            channelImage[0][3] = Channel1_4;
            channelImage[0][4] = Channel1_5;
            channelImage[0][5] = Channel1_6;
            channelImage[0][6] = Channel1_7;
            channelImage[0][7] = Channel1_8;
            channelImage[0][8] = Channel1_9;
            channelImage[0][9] = Channel1_10;
            channelImage[0][10] = Channel1_11;
            channelImage[0][11] = Channel1_12;
            channelImage[0][12] = Channel1_13;
            channelImage[0][13] = Channel1_14;
            channelImage[0][14] = Channel1_15;
            channelImage[0][15] = Channel1_16;
            channelImage[0][16] = Channel1_All;

            channelImage[1][0] = Channel2_1;
            channelImage[1][1] = Channel2_2;
            channelImage[1][2] = Channel2_3;
            channelImage[1][3] = Channel2_4;
            channelImage[1][4] = Channel2_5;
            channelImage[1][5] = Channel2_6;
            channelImage[1][6] = Channel2_7;
            channelImage[1][7] = Channel2_8;
            channelImage[1][8] = Channel2_9;
            channelImage[1][9] = Channel2_10;
            channelImage[1][10] = Channel2_11;
            channelImage[1][11] = Channel2_12;
            channelImage[1][12] = Channel2_13;
            channelImage[1][13] = Channel2_14;
            channelImage[1][14] = Channel2_15;
            channelImage[1][15] = Channel2_16;
            channelImage[1][16] = Channel2_All;

            channelImage[2][0] = Channel3_1;
            channelImage[2][1] = Channel3_2;
            channelImage[2][2] = Channel3_3;
            channelImage[2][3] = Channel3_4;
            channelImage[2][4] = Channel3_5;
            channelImage[2][5] = Channel3_6;
            channelImage[2][6] = Channel3_7;
            channelImage[2][7] = Channel3_8;
            channelImage[2][8] = Channel3_9;
            channelImage[2][9] = Channel3_10;
            channelImage[2][10] = Channel3_11;
            channelImage[2][11] = Channel3_12;
            channelImage[2][12] = Channel3_13;
            channelImage[2][13] = Channel3_14;
            channelImage[2][14] = Channel3_15;
            channelImage[2][15] = Channel3_16;
            channelImage[2][16] = Channel3_All;

            channelImage[3][0] = Channel4_1;
            channelImage[3][1] = Channel4_2;
            channelImage[3][2] = Channel4_3;
            channelImage[3][3] = Channel4_4;
            channelImage[3][4] = Channel4_5;
            channelImage[3][5] = Channel4_6;
            channelImage[3][6] = Channel4_7;
            channelImage[3][7] = Channel4_8;
            channelImage[3][8] = Channel4_9;
            channelImage[3][9] = Channel4_10;
            channelImage[3][10] = Channel4_11;
            channelImage[3][11] = Channel4_12;
            channelImage[3][12] = Channel4_13;
            channelImage[3][13] = Channel4_14;
            channelImage[3][14] = Channel4_15;
            channelImage[3][15] = Channel4_16;
            channelImage[3][16] = Channel4_All;

            channelImage[4][0] = Channel5_1;
            channelImage[4][1] = Channel5_2;
            channelImage[4][2] = Channel5_3;
            channelImage[4][3] = Channel5_4;
            channelImage[4][4] = Channel5_5;
            channelImage[4][5] = Channel5_6;
            channelImage[4][6] = Channel5_7;
            channelImage[4][7] = Channel5_8;
            channelImage[4][8] = Channel5_9;
            channelImage[4][9] = Channel5_10;
            channelImage[4][10] = Channel5_11;
            channelImage[4][11] = Channel5_12;
            channelImage[4][12] = Channel5_13;
            channelImage[4][13] = Channel5_14;
            channelImage[4][14] = Channel5_15;
            channelImage[4][15] = Channel5_16;
            channelImage[4][16] = Channel5_All;

            channelImage[5][0] = Channel6_1;
            channelImage[5][1] = Channel6_2;
            channelImage[5][2] = Channel6_3;
            channelImage[5][3] = Channel6_4;
            channelImage[5][4] = Channel6_5;
            channelImage[5][5] = Channel6_6;
            channelImage[5][6] = Channel6_7;
            channelImage[5][7] = Channel6_8;
            channelImage[5][8] = Channel6_9;
            channelImage[5][9] = Channel6_10;
            channelImage[5][10] = Channel6_11;
            channelImage[5][11] = Channel6_12;
            channelImage[5][12] = Channel6_13;
            channelImage[5][13] = Channel6_14;
            channelImage[5][14] = Channel6_15;
            channelImage[5][15] = Channel6_16;
            channelImage[5][16] = Channel6_All;

            channelImage[6][0] = Channel7_1;
            channelImage[6][1] = Channel7_2;
            channelImage[6][2] = Channel7_3;
            channelImage[6][3] = Channel7_4;
            channelImage[6][4] = Channel7_5;
            channelImage[6][5] = Channel7_6;
            channelImage[6][6] = Channel7_7;
            channelImage[6][7] = Channel7_8;
            channelImage[6][8] = Channel7_9;
            channelImage[6][9] = Channel7_10;
            channelImage[6][10] = Channel7_11;
            channelImage[6][11] = Channel7_12;
            channelImage[6][12] = Channel7_13;
            channelImage[6][13] = Channel7_14;
            channelImage[6][14] = Channel7_15;
            channelImage[6][15] = Channel7_16;
            channelImage[6][16] = Channel7_All;

            channelImage[7][0] = Channel8_1;
            channelImage[7][1] = Channel8_2;
            channelImage[7][2] = Channel8_3;
            channelImage[7][3] = Channel8_4;
            channelImage[7][4] = Channel8_5;
            channelImage[7][5] = Channel8_6;
            channelImage[7][6] = Channel8_7;
            channelImage[7][7] = Channel8_8;
            channelImage[7][8] = Channel8_9;
            channelImage[7][9] = Channel8_10;
            channelImage[7][10] = Channel8_11;
            channelImage[7][11] = Channel8_12;
            channelImage[7][12] = Channel8_13;
            channelImage[7][13] = Channel8_14;
            channelImage[7][14] = Channel8_15;
            channelImage[7][15] = Channel8_16;
            channelImage[7][16] = Channel8_All;

            channelImage[8][0] = Channel9_1;
            channelImage[8][1] = Channel9_2;
            channelImage[8][2] = Channel9_3;
            channelImage[8][3] = Channel9_4;
            channelImage[8][4] = Channel9_5;
            channelImage[8][5] = Channel9_6;
            channelImage[8][6] = Channel9_7;
            channelImage[8][7] = Channel9_8;
            channelImage[8][8] = Channel9_9;
            channelImage[8][9] = Channel9_10;
            channelImage[8][10] = Channel9_11;
            channelImage[8][11] = Channel9_12;
            channelImage[8][12] = Channel9_13;
            channelImage[8][13] = Channel9_14;
            channelImage[8][14] = Channel9_15;
            channelImage[8][15] = Channel9_16;
            channelImage[8][16] = Channel9_All;

            channelImage[9][0] = Channel10_1;
            channelImage[9][1] = Channel10_2;
            channelImage[9][2] = Channel10_3;
            channelImage[9][3] = Channel10_4;
            channelImage[9][4] = Channel10_5;
            channelImage[9][5] = Channel10_6;
            channelImage[9][6] = Channel10_7;
            channelImage[9][7] = Channel10_8;
            channelImage[9][8] = Channel10_9;
            channelImage[9][9] = Channel10_10;
            channelImage[9][10] = Channel10_11;
            channelImage[9][11] = Channel10_12;
            channelImage[9][12] = Channel10_13;
            channelImage[9][13] = Channel10_14;
            channelImage[9][14] = Channel10_15;
            channelImage[9][15] = Channel10_16;
            channelImage[9][16] = Channel10_All;

            channelImage[10][0] = Channel11_1;
            channelImage[10][1] = Channel11_2;
            channelImage[10][2] = Channel11_3;
            channelImage[10][3] = Channel11_4;
            channelImage[10][4] = Channel11_5;
            channelImage[10][5] = Channel11_6;
            channelImage[10][6] = Channel11_7;
            channelImage[10][7] = Channel11_8;
            channelImage[10][8] = Channel11_9;
            channelImage[10][9] = Channel11_10;
            channelImage[10][10] = Channel11_11;
            channelImage[10][11] = Channel11_12;
            channelImage[10][12] = Channel11_13;
            channelImage[10][13] = Channel11_14;
            channelImage[10][14] = Channel11_15;
            channelImage[10][15] = Channel11_16;
            channelImage[10][16] = Channel11_All;

            channelImage[11][0] = Channel12_1;
            channelImage[11][1] = Channel12_2;
            channelImage[11][2] = Channel12_3;
            channelImage[11][3] = Channel12_4;
            channelImage[11][4] = Channel12_5;
            channelImage[11][5] = Channel12_6;
            channelImage[11][6] = Channel12_7;
            channelImage[11][7] = Channel12_8;
            channelImage[11][8] = Channel12_9;
            channelImage[11][9] = Channel12_10;
            channelImage[11][10] = Channel12_11;
            channelImage[11][11] = Channel12_12;
            channelImage[11][12] = Channel12_13;
            channelImage[11][13] = Channel12_14;
            channelImage[11][14] = Channel12_15;
            channelImage[11][15] = Channel12_16;
            channelImage[11][16] = Channel12_All;

            for (int osc = 0; osc < 12; osc++)
            {
                for (int ch = 0; ch < 17; ch++)
                {
                    channelImage[osc][ch].Tag = new MidiChannelTag(osc, ch);
                    channelImage[osc][ch].Visibility =
                        ch == Channel[osc] ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            velocityImage = new Image[12][];
            for (int osc = 0; osc < 12; osc++)
            {
                velocityImage[osc] = new Image[2];
            }

            velocityImage[0][0] = vsOff1;
            velocityImage[1][0] = vsOff2;
            velocityImage[2][0] = vsOff3;
            velocityImage[3][0] = vsOff4;
            velocityImage[4][0] = vsOff5;
            velocityImage[5][0] = vsOff6;
            velocityImage[6][0] = vsOff7;
            velocityImage[7][0] = vsOff8;
            velocityImage[8][0] = vsOff9;
            velocityImage[9][0] = vsOff10;
            velocityImage[10][0] = vsOff11;
            velocityImage[11][0] = vsOff12;

            velocityImage[0][1] = vsOn1;
            velocityImage[1][1] = vsOn2;
            velocityImage[2][1] = vsOn3;
            velocityImage[3][1] = vsOn4;
            velocityImage[4][1] = vsOn5;
            velocityImage[5][1] = vsOn6;
            velocityImage[6][1] = vsOn7;
            velocityImage[7][1] = vsOn8;
            velocityImage[8][1] = vsOn9;
            velocityImage[9][1] = vsOn10;
            velocityImage[10][1] = vsOn11;
            velocityImage[11][1] = vsOn12;

            for (int osc = 0; osc < 12; osc++)
            {
                velocityImage[osc][0].Visibility = VelocitySensitivity[osc] || VelocityModulationSensitivity[osc] ? Visibility.Collapsed : Visibility.Visible;
                velocityImage[osc][1].Visibility = VelocitySensitivity[osc] || VelocityModulationSensitivity[osc] ? Visibility.Visible : Visibility.Collapsed;
            }

            velocityUsageImage = new Image[12][];
            for (int osc = 0; osc < 12; osc++)
            {
                velocityUsageImage[osc] = new Image[3];
            }

            velocityUsageImage[0][0] = vsToVol1;
            velocityUsageImage[1][0] = vsToVol2;
            velocityUsageImage[2][0] = vsToVol3;
            velocityUsageImage[3][0] = vsToVol4;
            velocityUsageImage[4][0] = vsToVol5;
            velocityUsageImage[5][0] = vsToVol6;
            velocityUsageImage[6][0] = vsToVol7;
            velocityUsageImage[7][0] = vsToVol8;
            velocityUsageImage[8][0] = vsToVol9;
            velocityUsageImage[9][0] = vsToVol10;
            velocityUsageImage[10][0] = vsToVol11;
            velocityUsageImage[11][0] = vsToVol12;

            velocityUsageImage[0][1] = vsToMod1;
            velocityUsageImage[1][1] = vsToMod2;
            velocityUsageImage[2][1] = vsToMod3;
            velocityUsageImage[3][1] = vsToMod4;
            velocityUsageImage[4][1] = vsToMod5;
            velocityUsageImage[5][1] = vsToMod6;
            velocityUsageImage[6][1] = vsToMod7;
            velocityUsageImage[7][1] = vsToMod8;
            velocityUsageImage[8][1] = vsToMod9;
            velocityUsageImage[9][1] = vsToMod10;
            velocityUsageImage[10][1] = vsToMod11;
            velocityUsageImage[11][1] = vsToMod12;

            velocityUsageImage[0][2] = vsToVM1;
            velocityUsageImage[1][2] = vsToVM2;
            velocityUsageImage[2][2] = vsToVM3;
            velocityUsageImage[3][2] = vsToVM4;
            velocityUsageImage[4][2] = vsToVM5;
            velocityUsageImage[5][2] = vsToVM6;
            velocityUsageImage[6][2] = vsToVM7;
            velocityUsageImage[7][2] = vsToVM8;
            velocityUsageImage[8][2] = vsToVM9;
            velocityUsageImage[9][2] = vsToVM10;
            velocityUsageImage[10][2] = vsToVM11;
            velocityUsageImage[11][2] = vsToVM12;

            for (int osc = 0; osc < 12; osc++)
            {
                if (!VelocitySensitivity[osc] && !VelocityModulationSensitivity[osc])
                {
                    velocityUsageImage[osc][0].Visibility = Visibility.Visible;
                    velocityUsageImage[osc][1].Visibility = Visibility.Collapsed;
                    velocityUsageImage[osc][2].Visibility = Visibility.Collapsed;
                }
                else if (VelocitySensitivity[osc] && !VelocityModulationSensitivity[osc])
                {
                    velocityUsageImage[osc][0].Visibility = Visibility.Visible;
                    velocityUsageImage[osc][1].Visibility = Visibility.Collapsed;
                    velocityUsageImage[osc][2].Visibility = Visibility.Collapsed;
                }
                else if (!VelocitySensitivity[osc] && VelocityModulationSensitivity[osc])
                {
                    velocityUsageImage[osc][0].Visibility = Visibility.Collapsed;
                    velocityUsageImage[osc][1].Visibility = Visibility.Visible;
                    velocityUsageImage[osc][2].Visibility = Visibility.Collapsed;
                }
                else if (VelocitySensitivity[osc] && VelocityModulationSensitivity[osc])
                {
                    velocityUsageImage[osc][0].Visibility = Visibility.Collapsed;
                    velocityUsageImage[osc][1].Visibility = Visibility.Collapsed;
                    velocityUsageImage[osc][2].Visibility = Visibility.Visible;
                }
            }
        }


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            for (int poly = 0; poly < 6; poly++)
            {
                for (int osc = 0; osc < 12; osc++)
                {
                    mainPage.Oscillators[poly][osc].MidiChannel = Channel[osc];
                    mainPage.Oscillators[poly][osc].Adsr.channel = Channel[osc];
                    if (!VelocitySensitivity[osc] && !VelocityModulationSensitivity[osc])
                    {
                        mainPage.Oscillators[poly][osc].VelocitySensitive = 0;
                    }
                    else if (VelocitySensitivity[osc] && !VelocityModulationSensitivity[osc])
                    {
                        mainPage.Oscillators[poly][osc].VelocitySensitive = 1;
                    }
                    else if (!VelocitySensitivity[osc] && VelocityModulationSensitivity[osc])
                    {
                        mainPage.Oscillators[poly][osc].VelocitySensitive = 2;
                    }
                    else if (VelocitySensitivity[osc] && VelocityModulationSensitivity[osc])
                    {
                        mainPage.Oscillators[poly][osc].VelocitySensitive = 3;
                    }
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ChannelTapped(object sender, TappedRoutedEventArgs e)
        {
            int osc = ((MidiChannelTag)((Image)sender).Tag).Oscillator;
            int ch = ((MidiChannelTag)((Image)sender).Tag).Channel;
            ((Image)sender).Visibility = Visibility.Collapsed;
            ch++;
            ch = ch < 17 ? ch : 0;
            channelImage[osc][ch].Visibility = Visibility.Visible;
            Channel[osc] = ch;
        }

        private void Channel1_1_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            int osc = ((MidiChannelTag)((Image)sender).Tag).Oscillator;
            int ch = ((MidiChannelTag)((Image)sender).Tag).Channel;
            ((Image)sender).Visibility = Visibility.Collapsed;
            ch--;
            ch = ch > -1 ? ch : 16;
            channelImage[osc][ch].Visibility = Visibility.Visible;
            Channel[osc] = ch;
        }

        private void Channel1_1_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            int osc = ((MidiChannelTag)((Image)sender).Tag).Oscillator;
            int ch = ((MidiChannelTag)((Image)sender).Tag).Channel;
            ((Image)sender).Visibility = Visibility.Collapsed;

            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            PointerPointProperties ppp = pp.Properties;

            int delta = ppp.MouseWheelDelta > 0 ? 1 : -1;
            ch += delta;
            ch = ch > -1 ? ch : 0;
            ch = ch < 17 ? ch : 16;
            channelImage[osc][ch].Visibility = Visibility.Visible;
            Channel[osc] = ch;
        }

        private void VelocityOnTapped(object sender, TappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag);
            velocityImage[osc][1].Visibility = Visibility.Collapsed;
            velocityImage[osc][0].Visibility = Visibility.Visible;
            VelocitySensitivity[osc] = false;
            VelocityModulationSensitivity[osc] = false;
        }

        private void VelocityOffTapped(object sender, TappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag);
            velocityImage[osc][0].Visibility = Visibility.Collapsed;
            velocityImage[osc][1].Visibility = Visibility.Visible;
            if (VelocitySensitivity[osc] == false && VelocityModulationSensitivity[osc] == false)
            {
                VelocitySensitivity[osc] = true;
            }
        }

        private void VelocityToVolTapped(object sender, TappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag) - 100;
            int state = int.Parse((String)((Image)sender).Tag) / 100 - 1;
            velocityUsageImage[osc][state].Visibility = Visibility.Collapsed;
            state = ++state % 3;
            velocityUsageImage[osc][state].Visibility = Visibility.Visible;
            velocityUsage[osc] = 2;
            VelocitySensitivity[osc] = false;
            VelocityModulationSensitivity[osc] = true;
        }

        private void VelocityToModTapped(object sender, TappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag) - 200;
            int state = int.Parse((String)((Image)sender).Tag) / 200;
            velocityUsageImage[osc][state].Visibility = Visibility.Collapsed;
            state = ++state % 3;
            velocityUsageImage[osc][state].Visibility = Visibility.Visible;
            velocityUsage[osc] = 3;
            VelocitySensitivity[osc] = true;
            VelocityModulationSensitivity[osc] = true;
        }

        private void VelocityToVMTapped(object sender, TappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag) - 300;
            int state = int.Parse((String)((Image)sender).Tag) / 300 + 1;
            velocityUsageImage[osc][state].Visibility = Visibility.Collapsed;
            state = ++state % 3;
            velocityUsageImage[osc][state].Visibility = Visibility.Visible;
            velocityUsage[osc] = 1;
            VelocitySensitivity[osc] = true;
            VelocityModulationSensitivity[osc] = false;
        }

        private void VelocityToVolRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag) - 100;
            int state = int.Parse((String)((Image)sender).Tag) / 100 - 1;
            velocityUsageImage[osc][state].Visibility = Visibility.Collapsed;
            state--;
            state = state < 0 ? 2 : state;
            velocityUsageImage[osc][state].Visibility = Visibility.Visible;
            velocityUsage[osc] = 2;
            VelocitySensitivity[osc] = false;
            VelocityModulationSensitivity[osc] = true;
        }

        private void VelocityToModRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag) - 200;
            int state = int.Parse((String)((Image)sender).Tag) / 200;
            velocityUsageImage[osc][state].Visibility = Visibility.Collapsed;
            state--;
            state = state < 0 ? 2 : state;
            velocityUsageImage[osc][state].Visibility = Visibility.Visible;
            velocityUsage[osc] = 3;
            VelocitySensitivity[osc] = true;
            VelocityModulationSensitivity[osc] = true;
        }

        private void VelocityToVMRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            int osc = int.Parse((String)((Image)sender).Tag) - 300;
            int state = int.Parse((String)((Image)sender).Tag) / 300 + 1;
            velocityUsageImage[osc][state].Visibility = Visibility.Collapsed;
            state--;
            state = state < 0 ? 2 : state;
            velocityUsageImage[osc][state].Visibility = Visibility.Visible;
            velocityUsage[osc] = 1;
            VelocitySensitivity[osc] = true;
            VelocityModulationSensitivity[osc] = false;
        }
    }

    public class MidiChannelTag
    {
        public int Oscillator;
        public int Channel;

        public MidiChannelTag(int Oscillator, int Channel)
        {
            this.Oscillator = Oscillator;
            this.Channel = Channel;
        }
    }
}
