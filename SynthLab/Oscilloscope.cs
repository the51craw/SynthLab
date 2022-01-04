using System;
using UwpControlsLibrary;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace SynthLab
{
    public sealed partial class MainPage : Page
    {
        public Oscilloscope oscilloscope;
    }

    public class Oscilloscope
    {
        private Brush brush = new SolidColorBrush(Windows.UI.Colors.Chartreuse);
        private MainPage mainPage;
        private Rect oscilloscopeBounds;
        private Double relativeSizeX = 1;
        private Double relativeSizeY = 1;
        public Point oscilloskopOffset { set { SetBounds(value); } }
        public int X;
        public int Y;

        public Oscilloscope(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }

        public void SetBounds(Point point)
        {
            oscilloscopeBounds = new Rect(point.X + 9, point.Y + 16, 200, 100);
        }

        public void Resize(Rect newSize)
        {
            relativeSizeX = newSize.Width / 1920;
            relativeSizeY = newSize.Height / 1040;
        }

        public void Draw(Oscillator oscillator)
        {
            Point[] graphData = oscillator.MakeGraphData(60);
            ((Graph)mainPage.DisplayGUI.SubControls.ControlsList[(int)DisplayControls.OSCILLOGRAPH]).Draw(graphData);
        }
    }
}
