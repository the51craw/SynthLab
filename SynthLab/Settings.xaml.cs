using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SynthLab
{
    public sealed partial class Settings : ContentDialog
    {
        public string MidiDevice;
        public int KeyPriority { get { return keyPriority; } set { keyPriority = value; } }
        public int MidiInPort { get { return midiInPort; } set { midiInPort = value; } }    
        public int LowSineWaveFactor { get { return lowSineWaveFactor + 1; } set { lowSineWaveFactor = value - 1; } }
        public int ChorusSpeed1 { get { return (int)chorusSpeed1Value; } set { chorusSpeed1Value = value; } }
        public int ChorusSpeed2 { get { return (int)chorusSpeed2Value; } set { chorusSpeed2Value = value; } }
        public int ChorusSpeed3 { get { return (int)chorusSpeed3Value; } set { chorusSpeed3Value = value; } }
        public int ChorusDepth1 { get { return (int)chorusDepth1Value; } set { chorusDepth1Value = value; } }
        public int ChorusDepth2 { get { return (int)chorusDepth2Value; } set { chorusDepth2Value = value; } }
        public int ChorusDepth3 { get { return (int)chorusDepth3Value; } set { chorusDepth3Value = value; } }
        public int[][] CCMapping {  get { return ccMapping; } set { ccMapping = value; } }

        private int keyPriority = 0;
        private int midiInPort = 0;
        //private int polyphony = 6;
        private int polyphonyButton = 0;
        private int lowSineWaveFactor = 0;
        private double chorusDepth1Value;
        private double chorusDepth2Value;
        private double chorusDepth3Value;
        private double chorusSpeed1Value;
        private double chorusSpeed2Value;
        private double chorusSpeed3Value;

        private int factorButton = 0;
        private Image[] factorButtons;

        private int[][] ccMapping;  // Map to function [Map index][MIDI channel or all MIDI channels]
        private bool initialize = true;


        public Settings()
        {
            this.InitializeComponent();
            chorusSpeed1.Value = 3;
            chorusSpeed2.Value = 5;
            chorusSpeed3.Value = 8;
            chorusDepth1.Value = 3;
            chorusDepth2.Value = 5;
            chorusDepth3.Value = 8;
            factorButtons = new Image[8];
            factorButtons[0] = imgFactor1;
            factorButtons[1] = imgFactor2;
            factorButtons[2] = imgFactor3;
            factorButtons[3] = imgFactor4;
            factorButtons[4] = imgFactor5;
            factorButtons[5] = imgFactor6;
            factorButtons[6] = imgFactor7;
            factorButtons[7] = imgFactor8;
            MidiInPorts.SelectedIndex = 0;
            ccMapping = new int[30][];
            for (int i = 0; i < ccMapping.Length; i++)
            {
                ccMapping[i] = new int[12];
                for (int osc = 0; osc < 12; osc++)
                {
                    ccMapping[i][osc] = 0;
                }
            }
        }

        public Settings(SettingsData data)
        {
            this.InitializeComponent();
            chorusSpeed1.Value = data.ChorusSpeed1;
            chorusSpeed2.Value = data.ChorusSpeed2;
            chorusSpeed3.Value = data.ChorusSpeed3;
            chorusDepth1.Value = data.ChorusDepth1;
            chorusDepth2.Value = data.ChorusDepth2;
            chorusDepth3.Value = data.ChorusDepth3;
            factorButtons = new Image[8];
            factorButtons[0] = imgFactor1;
            factorButtons[1] = imgFactor2;
            factorButtons[2] = imgFactor3;
            factorButtons[3] = imgFactor4;
            factorButtons[4] = imgFactor5;
            factorButtons[5] = imgFactor6;
            factorButtons[6] = imgFactor7;
            factorButtons[7] = imgFactor8;
            if (data.MidiInPort >= MidiInPorts.Items.Count)
            {
                MidiInPorts.SelectedIndex = 0;
            }
            else
            {
                MidiInPorts.SelectedIndex = data.MidiInPort;
            }
            ccMapping = new int[30][];
            for (int i = 0; i < ccMapping.Length; i++)
            {
                ccMapping[i] = new int[12];
                for (int osc = 0; osc < 12; osc++)
                {
                    ccMapping[i][osc] = data.CCMapping[i][osc];
                }
            }
            cbCCNumber.SelectedIndex = 0;
            cbAffectedOscillator.SelectedIndex = 0;
            initialize = false;
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (initialize)
            {
                cbCCNumber.SelectedIndex = 0;
                cbCCMapping.SelectedIndex = 0;
                cbAffectedOscillator.SelectedIndex = 0;
                initialize = false;
            }
            KeyPriorityReleaseFirst.Visibility = keyPriority > 0 ? Visibility.Visible : Visibility.Collapsed;
            MidiDevice = (string)MidiInPorts.SelectedValue;
            SetFactorImage(lowSineWaveFactor);
            cbCCMapping.SelectedIndex = ccMapping[cbCCNumber.SelectedIndex][cbAffectedOscillator.SelectedIndex];
            //ccMapping[cbCCNumber.SelectedIndex][cbAffectedOscillator.SelectedIndex] = cbCCMapping.SelectedIndex;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MidiDevice = (string)MidiInPorts.SelectedValue;
            for (int i = 0; i < ccMapping.Length; i++)
            {
                for (int osc = 0; osc < 12; osc++)
                {
                    CCMapping[i][osc] = ccMapping[i][osc];
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void KeyPriorityStayDead_Tapped(object sender, TappedRoutedEventArgs e)
        {
            keyPriority++;
            keyPriority %= 2;
            KeyPriorityStayDead.Visibility = keyPriority == 0 ? Visibility.Visible : Visibility.Collapsed;
            KeyPriorityReleaseFirst.Visibility = keyPriority == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void KeyPriorityStayDead_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            keyPriority++;
            keyPriority %= 2;
            KeyPriorityStayDead.Visibility = keyPriority == 0 ? Visibility.Visible : Visibility.Collapsed;
            KeyPriorityReleaseFirst.Visibility = keyPriority == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void KeyPriorityStayDead_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            PointerPointProperties ppp = pp.Properties;
            keyPriority = ppp.MouseWheelDelta < 0 ? 1 : 0;
            KeyPriorityStayDead.Visibility = keyPriority == 0 ? Visibility.Visible : Visibility.Collapsed;
            KeyPriorityReleaseFirst.Visibility = keyPriority == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Polyphony_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            PointerPointProperties ppp = pp.Properties;
            int delta = ppp.MouseWheelDelta < 0 ? 1 : -1;
            int next = (polyphonyButton + delta);
            next = next > 13 ? 13 : next;
            next = next < 0 ? 0 : next;
            polyphonyButton = next;
            //polyphony = 6 + 2 * next;
        }

        private void Factor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (lowSineWaveFactor < 7)
            {
                lowSineWaveFactor++;
            }
            else
            {
                lowSineWaveFactor = 0;
            }
            SetFactorImage(lowSineWaveFactor);
        }

        private void Factor_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (lowSineWaveFactor > 0)
            {
                lowSineWaveFactor--;
            }
            else
            {
                lowSineWaveFactor = 7;
            }
            SetFactorImage(lowSineWaveFactor);
        }

        private void Factor_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint((Image)sender);
            PointerPointProperties ppp = pp.Properties;
            int delta = ppp.MouseWheelDelta < 0 ? 1 : -1;
            int next = (factorButton + delta);
            next = next > 7 ? 7 : next;
            next = next < 0 ? 0 : next;
            factorButton = next;
            lowSineWaveFactor = next;
            SetFactorImage(next);
        }

        private void SetFactorImage(int imageNumber)
        {
            for (int i = 0; i < factorButtons.Length; i++)
            {
                factorButtons[i].Visibility = Visibility.Collapsed;
            }
            factorButtons[lowSineWaveFactor].Visibility = Visibility.Visible;
        }

        private void cbAffectedOscillator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialize)
            {
                cbCCMapping.SelectedIndex = ccMapping[cbCCNumber.SelectedIndex][cbAffectedOscillator.SelectedIndex];
            }
        }

        private void cbCCNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialize)
            {
                cbCCMapping.SelectedIndex = ccMapping[cbCCNumber.SelectedIndex][cbAffectedOscillator.SelectedIndex];
            }
        }

        private void cbCCMapping_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!initialize)
            {
                ccMapping[cbCCNumber.SelectedIndex][cbAffectedOscillator.SelectedIndex] = cbCCMapping.SelectedIndex;
            }
        }

        private void chorusSpeed1_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            chorusSpeed1Value = chorusSpeed1.Value;
        }

        private void chorusDepth1_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            chorusDepth1Value = chorusDepth1.Value;
        }

        private void chorusSpeed2_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            chorusSpeed2Value = chorusSpeed2.Value;
        }

        private void chorusDepth2_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            chorusDepth2Value = chorusDepth2.Value;
        }

        private void chorusSpeed3_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            chorusSpeed3Value = chorusSpeed3.Value;
        }

        private void chorusDepth3_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            chorusDepth3Value = chorusDepth3.Value;
        }
    }
}
