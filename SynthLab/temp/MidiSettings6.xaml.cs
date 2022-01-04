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
    public sealed partial class MidiSettings : ContentDialog
    {
        Patch patch;
        Image[] channelImages;

        public MidiSettings()
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
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ChannelTapped(object sender, TappedRoutedEventArgs e)
        {
            ((Image)sender).Source = imgChannel1.Source;
        }

        private void VelocityTapped(object sender, TappedRoutedEventArgs e)
        {
            ((Image)sender).Source = imgVelocityOn.Source;
        }
    }
}
