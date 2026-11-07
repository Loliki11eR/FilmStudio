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
    /// Логика взаимодействия для PloshchadkiWindow.xaml
    /// </summary>
    public partial class PloshchadkiWindow : Window
    {
        public PloshchadkiWindow()
        {
            InitializeComponent();
            zagruzitSpisok();
        }

        // данные площадок выводятся в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    // записи запрашиваются напрямую из базы со связанными типами площадок
                    var spisok = bd.Площадка
                        .Include("Тип_площадки")
                        .AsNoTracking()
                        .OrderBy(p => p.Адрес)
                        .ToList();
                    dgPloshadki.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // форма создания новой площадки открывается на экране
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            var forma = new PloshadkaFormWindow();

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // форма редактирования записи выводится на экран
        private void btnRedaktirovat_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранная площадка из таблицы
            var vybrannaya = dgPloshadki.SelectedItem as Площадка;

            // проверяется факт выбора строки в таблице
            if (vybrannaya == null)
            {
                MessageBox.Show("Выберите площадку для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // объект передается в конструктор формы
            var forma = new PloshadkaFormWindow(vybrannaya);

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // запись удаляется после проверки зависимостей
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранная площадка из таблицы
            var vybrannaya = dgPloshadki.SelectedItem as Площадка;

            // проверяется факт выбора строки в таблице
            if (vybrannaya == null)
            {
                MessageBox.Show("Выберите площадку для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить площадку по адресу: {vybrannaya.Адрес}?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        // проверяется наличие связанных записей в графиках съемок и проектах
                        bool estSvyazi = bd.График_съемок.Any(g => g.Площадка == vybrannaya.Код_площадки) ||
                                         bd.Проект.Any(p => p.Площадка == vybrannaya.Код_площадки);

                        if (estSvyazi)
                        {
                            MessageBox.Show("Нельзя удалить площадку, так как она задействована в проектах или графиках съемок",
                                "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var ploshadkaDlyaUdaleniya = bd.Площадка.Find(vybrannaya.Код_площадки);
                        bd.Площадка.Remove(ploshadkaDlyaUdaleniya);
                        bd.SaveChanges();

                        MessageBox.Show("Площадка успешно удалена", "Успех",
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
