using System;
using Windows.UI.Xaml.Controls;

namespace SynthLab
{
    public sealed partial class Message : ContentDialog
    {
        public bool Result;
        public Message(String Text)
        {
            this.InitializeComponent();
            this.Text.Text = Text;
        }

        public Message(String Text, string Title)
        {
            this.InitializeComponent();
            this.Text.Text = Text;
            this.Title = Title;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = false;
        }
    }
}
