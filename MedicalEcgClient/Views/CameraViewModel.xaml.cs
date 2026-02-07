using MedicalEcgClient.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MedicalEcgClient.Views
{
    public partial class CameraView : UserControl
    {
        private Point _origin;
        private Point _start;

        public CameraView()
        {
            InitializeComponent();
        }

        private void ImageContainer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;

                var st = ImgScale;
                var tt = ImgTranslate;

                double zoom = e.Delta > 0 ? .2 : -.2;

                if (!(st.ScaleX < .4 && zoom < 0) && !(st.ScaleX > 10 && zoom > 0))
                {
                    Point relative = e.GetPosition(DisplayImage);
                    double absoluteX = relative.X * st.ScaleX + tt.X;
                    double absoluteY = relative.Y * st.ScaleY + tt.Y;

                    st.ScaleX += zoom;
                    st.ScaleY += zoom;

                    tt.X = absoluteX - relative.X * st.ScaleX;
                    tt.Y = absoluteY - relative.Y * st.ScaleY;
                }
            }
        }

        private void ImageContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ImgScale.ScaleX > 1.0 || (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var tt = ImgTranslate;
                _start = e.GetPosition(ImageContainer);
                _origin = new Point(tt.X, tt.Y);
                ImageContainer.Cursor = Cursors.Hand;
                DisplayImage.CaptureMouse();
            }
        }

        private void ImageContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisplayImage.ReleaseMouseCapture();
            ImageContainer.Cursor = Cursors.Arrow;
        }

        private void ImageContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (DisplayImage.IsMouseCaptured)
            {
                var tt = ImgTranslate;
                Vector v = _start - e.GetPosition(ImageContainer);
                tt.X = _origin.X - v.X;
                tt.Y = _origin.Y - v.Y;
            }
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ImgScale.ScaleX = 1;
            ImgScale.ScaleY = 1;
            ImgTranslate.X = 0;
            ImgTranslate.Y = 0;
        }
    }
}