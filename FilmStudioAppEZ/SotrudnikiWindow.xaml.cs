using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FilmStudioAppEZ
{
    public partial class SotrudnikiWindow : Window
    {
        public System.Collections.Generic.List<string> test_p { get; set; }
        public SotrudnikiWindow()
        {
            InitializeComponent();
            zagruzitSpisok();
        }

        // данные сотрудников загружаются из базы в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    // данные запрашиваются из базы вместе со связанными специализациями
                    var spisok = bd.Сотрудник
                        .Include("Специализация1")
                        .AsNoTracking()
                        .OrderBy(s => s.Фамилия)
                        .ToList();
                    dgSotrudniki.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // записи фильтруются по фамилии при изменении строки поиска
        private void txtPoisk_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    var tekst = txtPoisk.Text.Trim().ToLower();

                    // актуальные связанные данные подтягиваются при поиске по буквам
                    var spisok = bd.Сотрудник
                        .Include("Специализация1")
                        .AsNoTracking()
                        .Where(s => s.Фамилия.ToLower().Contains(tekst))
                        .OrderBy(s => s.Фамилия)
                        .ToList();
                    dgSotrudniki.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // форма добавления нового сотрудника выводится на экран
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            var forma = new SotrudnikFormWindow();

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // форма редактирования выбранной записи выводится на экран
        private void btnRedaktirovat_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранный сотрудник из таблицы
            var vybrannyi = dgSotrudniki.SelectedItem as Сотрудник;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // объект передается в конструктор формы
            var forma = new SotrudnikFormWindow(vybrannyi);

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // запись удаляется из базы после проверки внешних зависимостей
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранный сотрудник из таблицы
            var vybrannyi = dgSotrudniki.SelectedItem as Сотрудник;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить сотрудника {vybrannyi.Фамилия} {vybrannyi.Имя}?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        // проверяется наличие связей в съемочных группах
                        var estSvyazi = bd.Состав_группы.Any(sg => sg.Сотрудник == vybrannyi.Код_сотрудника);

                        if (estSvyazi)
                        {
                            MessageBox.Show("Нельзя удалить сотрудника, он участвует в проектах",
                                "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var sotrudnikDlyaUdaleniya = bd.Сотрудник.Find(vybrannyi.Код_сотрудника);
                        bd.Сотрудник.Remove(sotrudnikDlyaUdaleniya);
                        bd.SaveChanges();

                        MessageBox.Show("Сотрудник успешно удален", "Успех",
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
