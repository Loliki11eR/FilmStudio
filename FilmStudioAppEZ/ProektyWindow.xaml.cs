using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FilmStudioAppEZ
{
    public partial class ProektyWindow : Window
    {
        public ProektyWindow()
        {
            InitializeComponent();
            zagruzitStatusyProektov();
            zagruzitSpisok();
        }

        // список статусов загружается в комбобокс
        private void zagruzitStatusyProektov()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    var statusy = bd.Статус_проекта.OrderBy(s => s.Наименование).ToList();
                    cmbFiltrStatus.DisplayMemberPath = "Наименование";
                    cmbFiltrStatus.SelectedValuePath = "Код_статуса_проекта";
                    cmbFiltrStatus.ItemsSource = statusy;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статусов: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные проектов выводятся в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    // список проектов запрашивается из базы без использования локального кэша
                    var spisok = bd.Проект
                        .Include("Статус_проекта")
                        .Include("Жанр1")
                        .Include("Целевая_аудитория1")
                        .AsNoTracking()
                        .OrderBy(p => p.Название)
                        .ToList();

                    dgProekty.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // записи фильтруются по выбранному статусу
        private void cmbFiltrStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFiltrStatus.SelectedValue == null) return;

            try
            {
                int idStatusa = (int)cmbFiltrStatus.SelectedValue;
                using (var bd = new FilmStudioEZEntities())
                {
                    // связанные таблицы подтягиваются без использования локального кэша
                    var filtrSpisok = bd.Проект
                        .Include("Статус_проекта")
                        .Include("Жанр1")
                        .Include("Целевая_аудитория1")
                        .AsNoTracking()
                        .Where(p => p.Статус == idStatusa)
                        .OrderBy(p => p.Название)
                        .ToList();
                    dgProekty.ItemsSource = filtrSpisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // фильтр сбрасывается до начального состояния
        private void btnSbros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltrStatus.SelectedValue = null;
            zagruzitSpisok();
        }

        // форма создания нового проекта открывается на экране
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            var forma = new ProektFormWindow();

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // форма редактирования записи выводится на экран
        private void btnRedaktirovat_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранный проект из таблицы
            var vybrannyi = dgProekty.SelectedItem as Проект;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите проект для редактирования", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // объект передается в конструктор формы
            var forma = new ProektFormWindow(vybrannyi);

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // запись удаляется после проверки зависимостей
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранный проект из таблицы
            var vybrannyi = dgProekty.SelectedItem as Проект;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите проект для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить проект {vybrannyi.Название}?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        // проверяется наличие связанных записей
                        bool estSvyazi = bd.Затрата.Any(z => z.Проект == vybrannyi.Код_проекта) ||
                                         bd.Прокат.Any(p => p.Проект == vybrannyi.Код_проекта);

                        if (estSvyazi)
                        {
                            MessageBox.Show("Нельзя удалить проект, по нему есть связанные записи в затратах или прокате",
                                "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var proektDlyaUdaleniya = bd.Проект.Find(vybrannyi.Код_проекта);
                        bd.Проект.Remove(proektDlyaUdaleniya);
                        bd.SaveChanges();

                        MessageBox.Show("Проект удален успешно", "Успех",
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

        // обработчик кнопки генерации qr-кода проекта
        private void btnQRCode_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранный проект из таблицы
            var vybrannyi = dgProekty.SelectedItem as Проект;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите проект для генерации QR-кода", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // окно создания знака открывается на экране модально
            var oknoKoda = new QrCodeWindow(vybrannyi);
            oknoKoda.Owner = this;
            oknoKoda.ShowDialog();
        }
    }
}
