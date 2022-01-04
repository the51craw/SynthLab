using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
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
    public sealed partial class Settings : ContentDialog
    {
        public int KeyPriority = 0;
        public int Polyphony = 6;
        private int polyphonyButton = 0;

        public Settings()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void KeyPriorityStayDead_Tapped(object sender, TappedRoutedEventArgs e)
        {
            KeyPriority++;
            KeyPriority %= 2;
            KeyPriorityStayDead.Visibility = KeyPriority == 0 ? Visibility.Visible : Visibility.Collapsed;
            KeyPriorityReleaseFirst.Visibility = KeyPriority == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void KeyPriorityStayDead_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            PointerPointProperties ppp = pp.Properties;
            KeyPriority = ppp.MouseWheelDelta > 0 ? 1 : 0;
            KeyPriorityStayDead.Visibility = KeyPriority == 0 ? Visibility.Visible : Visibility.Collapsed;
            KeyPriorityReleaseFirst.Visibility = KeyPriority == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Polyphony_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int clicked = int.Parse((string)((Image)sender).Tag);
            int next = (clicked + 1) % 14;
            Polyphony = 6 + 2 * next;
            SetPolyphonyImage(next);
        }

        private void Polyphony_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            PointerPointProperties ppp = pp.Properties;
            int delta = ppp.MouseWheelDelta > 0 ? 1 : -1;
            int next = (polyphonyButton + delta);
            next = next > 13 ? 13 : next;
            next = next < 0 ? 0 : next;
            polyphonyButton = next;
            Polyphony = 6 + 2 * next;
            SetPolyphonyImage(next);
        }

        private void SetPolyphonyImage(int imageNumber)
        {
            Polyphony6.Visibility = Visibility.Collapsed;
            Polyphony8.Visibility = Visibility.Collapsed;
            Polyphony10.Visibility = Visibility.Collapsed;
            Polyphony12.Visibility = Visibility.Collapsed;
            Polyphony14.Visibility = Visibility.Collapsed;
            Polyphony16.Visibility = Visibility.Collapsed;
            Polyphony18.Visibility = Visibility.Collapsed;
            Polyphony20.Visibility = Visibility.Collapsed;
            Polyphony22.Visibility = Visibility.Collapsed;
            Polyphony24.Visibility = Visibility.Collapsed;
            Polyphony26.Visibility = Visibility.Collapsed;
            Polyphony28.Visibility = Visibility.Collapsed;
            Polyphony30.Visibility = Visibility.Collapsed;
            Polyphony32.Visibility = Visibility.Collapsed;
            switch (imageNumber)
            {
                case 0:
                    Polyphony6.Visibility = Visibility.Visible;
                    break;
                case 1:
                    Polyphony8.Visibility = Visibility.Visible;
                    break;
                case 2:
                    Polyphony10.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Polyphony12.Visibility = Visibility.Visible;
                    break;
                case 4:
                    Polyphony14.Visibility = Visibility.Visible;
                    break;
                case 5:
                    Polyphony16.Visibility = Visibility.Visible;
                    break;
                case 6:
                    Polyphony18.Visibility = Visibility.Visible;
                    break;
                case 7:
                    Polyphony20.Visibility = Visibility.Visible;
                    break;
                case 8:
                    Polyphony22.Visibility = Visibility.Visible;
                    break;
                case 9:
                    Polyphony24.Visibility = Visibility.Visible;
                    break;
                case 10:
                    Polyphony26.Visibility = Visibility.Visible;
                    break;
                case 11:
                    Polyphony28.Visibility = Visibility.Visible;
                    break;
                case 12:
                    Polyphony30.Visibility = Visibility.Visible;
                    break;
                case 13:
                    Polyphony32.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
