using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        FactoryPatches factoryPatches = new FactoryPatches();
    }


    public sealed partial class FactoryPatches : ContentDialog
    {
        public String PatchName;

        public FactoryPatches()
        {
            this.InitializeComponent();
            Init();
        }

        private async void Init()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFile> files = await storageFolder.GetFilesAsync();
            RowDefinition rowDefinition;
            Image button;
            TextBlock text;
            FontFamily ethnocentric = new FontFamily("Ethnocentric");
            int i = 0;
            //foreach (StorageFile file in patches)
            foreach (StorageFile file in files)
            {
                rowDefinition = new RowDefinition();
                rowDefinition.MinHeight = 26;
                rowDefinition.MaxHeight = 26;
                main.RowDefinitions.Add(rowDefinition);
                button = new Image();
                button.Source = new BitmapImage(new Uri("ms-appx://SynthLab/Images/TitleBackground.png"));
                Grid.SetRow(button, i);
                button.Tag = file.Name;
                button.Tapped += P_Tapped;
                main.Children.Add(button);
                text = new TextBlock();
                text.Text = file.DisplayName;
                text.FontFamily = ethnocentric;
                text.FontSize = 16;
                text.Margin = new Thickness(14, 6, 0, 0);
                text.Tag = file.Name;
                text.Tapped += P_Tapped;
                Grid.SetRow(text, i);
                main.Children.Add(text);
                i++;
            }
            rowDefinition = new RowDefinition();
            rowDefinition.MinHeight = 26;
            rowDefinition.MaxHeight = 26;
            main.RowDefinitions.Add(rowDefinition);
            button = new Image();
            button.Source = new BitmapImage(new Uri("ms-appx://SynthLab/Images/TitleBackground.png"));
            Grid.SetRow(button, i);
            button.Tapped += Cancel_Tapped;
            main.Children.Add(button);
            text = new TextBlock();
            text.Text = "                     Cancel";
            text.FontFamily = ethnocentric;
            text.FontSize = 16;
            text.Margin = new Thickness(14, 6, 0, 0);
            text.Tapped += Cancel_Tapped;
            Grid.SetRow(text, i);
            main.Children.Add(text);
        }

        private void Cancel_Tapped(object sender, TappedRoutedEventArgs args)
        {
            PatchName = "";
            Hide();
        }

        private void P_Tapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender.GetType() == typeof(TextBlock))
            {
                PatchName = (string)((TextBlock)sender).Tag;
                Hide();
            }
            else if (sender.GetType() == typeof(Image))
            {
                PatchName = (string)((Image)sender).Tag;
                Hide();
            }
        }
    }
}
