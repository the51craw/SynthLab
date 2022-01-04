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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SynthLab
{
    public sealed partial class MidiSettings6 : ContentDialog
    {
        private Patch patch;
        private Image[] channelImages;
        private int oscillator;
        private Settings settings;
        private class Settings
        {
            public int[] channel;
            public Boolean[] velositySensitivity;

            public Settings()
            {
                channel = new int[6];
                velositySensitivity = new Boolean[6];
            }
        }
        public MidiSettings6()
        {
            this.InitializeComponent();
        }

        public void Setup(Patch patch)
        {
            this.patch = patch;
            channelImages = new Image[] {
                /*imgChannelAll,*/ imgChannel1, imgChannel2, imgChannel3,
                imgChannel4, imgChannel5, imgChannel6, imgChannel7,
                imgChannel8, imgChannel9, imgChannel10, imgChannel11,
                imgChannel12, imgChannel13, imgChannel14, imgChannel15, imgChannel16
            };

            settings = new Settings();
            settings.channel[0] = patch.Oscillators[0][0].MidiChannel;
            settings.channel[1] = patch.Oscillators[0][1].MidiChannel;
            settings.channel[2] = patch.Oscillators[0][2].MidiChannel;
            settings.channel[3] = patch.Oscillators[0][3].MidiChannel;
            settings.channel[4] = patch.Oscillators[0][4].MidiChannel;
            settings.channel[5] = patch.Oscillators[0][5].MidiChannel;

            ch1.Source = channelImages[settings.channel[0]].Source;
            ch2.Source = channelImages[settings.channel[1]].Source;
            ch3.Source = channelImages[settings.channel[2]].Source;
            ch4.Source = channelImages[settings.channel[3]].Source;
            ch5.Source = channelImages[settings.channel[4]].Source;
            ch6.Source = channelImages[settings.channel[5]].Source;

            settings.velositySensitivity[0] = patch.Oscillators[0][0].VelocitySensitive;
            settings.velositySensitivity[1] = patch.Oscillators[0][1].VelocitySensitive;
            settings.velositySensitivity[2] = patch.Oscillators[0][2].VelocitySensitive;
            settings.velositySensitivity[3] = patch.Oscillators[0][3].VelocitySensitive;
            settings.velositySensitivity[4] = patch.Oscillators[0][4].VelocitySensitive;
            settings.velositySensitivity[5] = patch.Oscillators[0][5].VelocitySensitive;

            vs1.Source = settings.velositySensitivity[0] ? imgVelocityOn.Source : imgVelocityOff.Source;
            vs2.Source = settings.velositySensitivity[1] ? imgVelocityOn.Source : imgVelocityOff.Source;
            vs3.Source = settings.velositySensitivity[2] ? imgVelocityOn.Source : imgVelocityOff.Source;
            vs4.Source = settings.velositySensitivity[3] ? imgVelocityOn.Source : imgVelocityOff.Source;
            vs5.Source = settings.velositySensitivity[4] ? imgVelocityOn.Source : imgVelocityOff.Source;
            vs6.Source = settings.velositySensitivity[5] ? imgVelocityOn.Source : imgVelocityOff.Source;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (main.Visibility == Visibility.Collapsed)
            {
                main.Visibility = Visibility.Visible;
                channels.Visibility = Visibility.Collapsed;
                args.Cancel = true;
            }
            else
            {
                for (int poly = 0; poly < patch.Polyphony; poly++)
                {
                    for (int oscillator = 0; oscillator < patch.OscillatorCount; oscillator++)
                    {
                        patch.Oscillators[poly][oscillator].MidiChannel = settings.channel[oscillator];
                        patch.Oscillators[poly][oscillator].VelocitySensitive = settings.velositySensitivity[oscillator];
                    }
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (main.Visibility == Visibility.Collapsed)
            {
                main.Visibility = Visibility.Visible;
                channels.Visibility = Visibility.Collapsed;
                args.Cancel = true;
            }
        }

        private void ChannelTapped(object sender, TappedRoutedEventArgs e)
        {
            oscillator = int.Parse((String)((Image)sender).Tag);
            main.Visibility = Visibility.Collapsed;
            channels.Visibility = Visibility.Visible;
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
            main.Visibility = Visibility.Visible;
            channels.Visibility = Visibility.Collapsed;
        }

        private void VelocityTapped(object sender, TappedRoutedEventArgs e)
        {
            oscillator = int.Parse((String)((Image)sender).Tag);
            settings.velositySensitivity[oscillator] = !settings.velositySensitivity[oscillator];
            ((Image)sender).Source = settings.velositySensitivity[oscillator] ?
                imgVelocityOn.Source : imgVelocityOff.Source;
        }
    }
}
