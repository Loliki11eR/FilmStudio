using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для ProkatWindow.xaml
    /// </summary>
    public partial class ProkatWindow : Window
    {
        public ProkatWindow()
        {
            InitializeComponent();
            zagruzitSpisok();
        }

        // данные кинопроката выводятся в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    // записи проката запрашиваются из базы со связанными проектами
                    var spisok = bd.Прокат
                        .Include("Проект1")
                        .AsNoTracking()
                        .OrderByDescending(p => p.Дата_выхода)
                        .ToList();
                    dgProkat.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // форма добавления новой записи проката открывается на экране
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            var forma = new ProkatFormWindow();

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // форма редактирования записи выводится на экран
        private void btnRedaktirovat_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранная строка проката из таблицы
            var vybrannyi = dgProkat.SelectedItem as Прокат;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // объект передается в конструктор формы
            var forma = new ProkatFormWindow(vybrannyi);

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // запись о прокате удаляется из базы данных
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранная строка проката из таблицы
            var vybrannyi = dgProkat.SelectedItem as Прокат;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Удалить выбранную запись о прокате?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        var zapisDlyaUdaleniya = bd.Прокат.Find(vybrannyi.Код_проката);
                        bd.Прокат.Remove(zapisDlyaUdaleniya);
                        bd.SaveChanges();

                        MessageBox.Show("Запись успешно удалена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        zagruzitSpisok();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}