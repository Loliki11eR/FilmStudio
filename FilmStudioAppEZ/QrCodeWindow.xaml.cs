using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FilmStudioAppEZ
{
    /// <summary>
    /// Логика взаимодействия для QrCodeWindow.xaml
    /// </summary>
    public partial class QrCodeWindow : Window
    {
        public QrCodeWindow(Проект proekt)
        {
            InitializeComponent();
            generirovatZnakLokalno(proekt);
        }


        // графический знак создается внутри приложения без интернета
        private void generirovatZnakLokalno(Проект proekt)
        {
            try
            {
                string tekstDlyaShifrovaniya = $"Проект: {proekt.Название}\nБюджет: {proekt.Бюджет:N2} руб.";

                // генерация матрицы кода средствами библиотеки
                using (QRCodeGenerator generator = new QRCodeGenerator())
                using (QRCodeData dannye = generator.CreateQrCode(tekstDlyaShifrovaniya, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(dannye))
                {
                    // получение массива байт готовой картинки
                    byte[] grafikaBayty = qrCode.GetGraphic(20);

                    // перевод массива байт в понятный для wpf формат BitmapImage
                    BitmapImage kartinka = new BitmapImage();
                    using (MemoryStream potok = new MemoryStream(grafikaBayty))
                    {
                        kartinka.BeginInit();
                        kartinka.CacheOption = BitmapCacheOption.OnLoad;
                        kartinka.StreamSource = potok;
                        kartinka.EndInit();
                    }

                    // изображение выводится на форму
                    imgQrCode.Source = kartinka;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания знака: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnZakryt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}