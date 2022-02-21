using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SynthLab
{
    public sealed partial class MidiSettings12 : ContentDialog
    {
        private Patch patch;
        private Image[] channelImages;
        private Image[][] velocitySensitivityImages;
        private int oscillator;
        private Settings settings;

        private class Settings
        {
            public int[] channel;
            //public Image[] imgChannel;
            public bool[] velositySensitivity;
            //public Image[] imgVelositySensitivity;

            public Settings()
            {
                channel = new int[12];
                velositySensitivity = new Boolean[12];
            }
        }

        public MidiSettings12()
        {
            this.InitializeComponent();
        }

        public void Setup(Patch patch)
        {
            this.patch = patch;
            channelImages = new Image[] {
                imgChannelAll, imgChannel1, imgChannel2, imgChannel3,
                imgChannel4, imgChannel5, imgChannel6, imgChannel7,
                imgChannel8, imgChannel9, imgChannel10, imgChannel11,
                imgChannel12, imgChannel13, imgChannel14, imgChannel15, imgChannel16
            };
            velocitySensitivityImages = new Image[12][];// {imgVelocityOff, imgVelocityOn};

            settings = new Settings();
            for (int i = 0; i < 12; i++)
            {
                settings.channel[i] = patch.Oscillators[0][i].MidiChannel;
                settings.velositySensitivity[i] = patch.Oscillators[0][i].VelocitySensitive;
                velocitySensitivityImages[i] = new Image[2];
                velocitySensitivityImages[i][0] = new Image();
                velocitySensitivityImages[i][0].Source = imgVelocityOff.Source;
                velocitySensitivityImages[i][1] = new Image();
                velocitySensitivityImages[i][1].Source = imgVelocityOn.Source;
            }

            ch1.Source = channelImages[settings.channel[0]].Source;
            ch2.Source = channelImages[settings.channel[1]].Source;
            ch3.Source = channelImages[settings.channel[2]].Source;
            ch4.Source = channelImages[settings.channel[3]].Source;
            ch5.Source = channelImages[settings.channel[4]].Source;
            ch6.Source = channelImages[settings.channel[5]].Source;
            ch7.Source = channelImages[settings.channel[6]].Source;
            ch8.Source = channelImages[settings.channel[7]].Source;
            ch9.Source = channelImages[settings.channel[8]].Source;
            ch10.Source = channelImages[settings.channel[9]].Source;
            ch11.Source = channelImages[settings.channel[10]].Source;
            ch12.Source = channelImages[settings.channel[11]].Source;

            //vs1.Source = settings.velositySensitivity[0] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs2.Source = settings.velositySensitivity[1] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs3.Source = settings.velositySensitivity[2] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs4.Source = settings.velositySensitivity[3] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs5.Source = settings.velositySensitivity[4] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs6.Source = settings.velositySensitivity[5] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs7.Source = settings.velositySensitivity[6] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs8.Source = settings.velositySensitivity[7] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs9.Source = settings.velositySensitivity[8] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs10.Source = settings.velositySensitivity[9] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs11.Source = settings.velositySensitivity[10] ? imgVelocityOn.Source : imgVelocityOff.Source;
            //vs12.Source = settings.velositySensitivity[11] ? imgVelocityOn.Source : imgVelocityOff.Source;

            vsOn1 = velocitySensitivityImages[0][1];
            vsOn2 = velocitySensitivityImages[1][1];
            vsOn3 = velocitySensitivityImages[2][1];
            vsOn4 = velocitySensitivityImages[3][1];
            vsOn5 = velocitySensitivityImages[4][1];
            vsOn6 = velocitySensitivityImages[5][1];
            vsOn7 = velocitySensitivityImages[6][1];
            vsOn8 = velocitySensitivityImages[7][1];
            vsOn9 = velocitySensitivityImages[8][1];
            vsOn10 = velocitySensitivityImages[9][1];
            vsOn11 = velocitySensitivityImages[10][1];
            vsOn12 = velocitySensitivityImages[11][1];

            vsOff1 = velocitySensitivityImages[0][0];
            vsOff2 = velocitySensitivityImages[1][0];
            vsOff3 = velocitySensitivityImages[2][0];
            vsOff4 = velocitySensitivityImages[3][0];
            vsOff5 = velocitySensitivityImages[4][0];
            vsOff6 = velocitySensitivityImages[5][0];
            vsOff7 = velocitySensitivityImages[6][0];
            vsOff8 = velocitySensitivityImages[7][0];
            vsOff9 = velocitySensitivityImages[8][0];
            vsOff10 = velocitySensitivityImages[9][0];
            vsOff11 = velocitySensitivityImages[10][0];
            vsOff12 = velocitySensitivityImages[11][0];
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //if (main.Visibility == Visibility.Collapsed)
            //{
            //    main.Visibility = Visibility.Visible;
            //    channels.Visibility = Visibility.Collapsed;
            //    args.Cancel = true;
            //}
            //else
            //{
            for (int poly = 0; poly < patch.Polyphony; poly++)
            {
                for (int oscillator = 0; oscillator < patch.OscillatorCount; oscillator++)
                {
                    patch.Oscillators[poly][oscillator].MidiChannel = settings.channel[oscillator];
                    patch.Oscillators[poly][oscillator].VelocitySensitive = settings.velositySensitivity[oscillator];
                }
            }
            //}
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //if (main.Visibility == Visibility.Collapsed)
            //{
                main.Visibility = Visibility.Visible;
                args.Cancel = true;
            //}
        }

        private void ChannelTapped(object sender, TappedRoutedEventArgs e)
        {
            oscillator = int.Parse((String)((Image)sender).Tag);
            //main.Visibility = Visibility.Collapsed;
            //channels.Visibility = Visibility.Visible;
        }

        private void ChannelSelected(object sender, TappedRoutedEventArgs e)
        {
            settings.channel[oscillator] = int.Parse((String)((Image)sender).Tag);
            ch1.Source = channelImages[settings.channel[0]].Source;
            ch2.Source = channelImages[settings.channel[1]].Source;
            ch3.Source = channelImages[settings.channel[2]].Source;
            ch4.Source = channelImages[settings.channel[3]].Source;
            ch5.Source = channelImages[settings.channel[4]].Source;
            ch6.Source = channelImages[settings.channel[5]].Source;
            ch7.Source = channelImages[settings.channel[6]].Source;
            ch8.Source = channelImages[settings.channel[7]].Source;
            ch9.Source = channelImages[settings.channel[8]].Source;
            ch10.Source = channelImages[settings.channel[9]].Source;
            ch11.Source = channelImages[settings.channel[10]].Source;
            ch12.Source = channelImages[settings.channel[11]].Source;
            //main.Visibility = Visibility.Visible;
            //channels.Visibility = Visibility.Collapsed;
        }

        private void VelocityOnTapped(object sender, TappedRoutedEventArgs e)
        {
            oscillator = int.Parse((String)((Image)sender).Tag);
            settings.velositySensitivity[oscillator] = true;
            velocitySensitivityImages[oscillator][0].Visibility = Visibility.Collapsed;
            velocitySensitivityImages[oscillator][1].Visibility = Visibility.Visible;
        }

        private void VelocityOffTapped(object sender, TappedRoutedEventArgs e)
        {
            oscillator = int.Parse((String)((Image)sender).Tag);
            settings.velositySensitivity[oscillator] = false;
            velocitySensitivityImages[oscillator][0].Visibility = Visibility.Visible;
            velocitySensitivityImages[oscillator][1].Visibility = Visibility.Collapsed;
        }
    }
}
