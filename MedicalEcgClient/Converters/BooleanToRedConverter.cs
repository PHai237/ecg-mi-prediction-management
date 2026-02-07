using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MedicalEcgClient.Converters
{
    public class BooleanToRedConverter : IValueConverter
    {
        // Chuyển đổi từ bool (IsRecording) sang Brush (Màu nền)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRecording && isRecording)
            {
                // Đang ghi -> Màu đỏ (hoặc màu cảnh báo)
                return new SolidColorBrush(Colors.Red);
            }
            // Không ghi -> Màu xanh (hoặc màu mặc định của nút)
            return new SolidColorBrush(Colors.LimeGreen);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}