using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfKinectV2CustomButton.Services
{
    public sealed class ButtonBrushFactory
    {
        public ButtonBrushFactory()
        {
            DefaultBrush = CreateImageBrush("Images/violinicon.png");
            ActiveBrush = CreateImageBrush("Images/violinicon2.png");
        }

        public ImageBrush DefaultBrush { get; }

        public ImageBrush ActiveBrush { get; }

        private static ImageBrush CreateImageBrush(string relativePath)
        {
            return new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(relativePath, UriKind.Relative)),
            };
        }
    }
}
