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
    /// Логика взаимодействия для SemkaWindow.xaml
    /// </summary>
    public partial class SemkaWindow : Window
    {
        public SemkaWindow()
        {
            InitializeComponent();
            zagruzitSpisok();
        }

        // данные участников групп выводятся в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    // записи состава группы запрашиваются из базы со всеми связями
                    var spisok = bd.Состав_группы
                        .Include("Съемочная_группа1")
                        .Include("Сотрудник1")
                        .Include("Сотрудник1.Специализация1")
                        .AsNoTracking()
                        .OrderBy(sg => sg.Съемочная_группа1.Название_группы)
                        .ToList();
                    dgSostavGruppy.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // форма добавления нового сотрудника в группу открывается на экране
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            var forma = new SemkaGroupFormWindow();

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // форма редактирования участия выводится на экран
        private void btnRedaktirovat_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранная строка состава из таблицы
            var vybrannyi = dgSostavGruppy.SelectedItem as Состав_группы;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // объект передается в constructor формы
            var forma = new SemkaGroupFormWindow(vybrannyi);

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // сотрудник удаляется из состава группы
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            // извлекается выбранная строка состава из таблицы
            var vybrannyi = dgSostavGruppy.SelectedItem as Состав_группы;

            // проверяется факт выбора строки в таблице
            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Удалить сотрудника из данной съёмочной группы?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        var zapisDlyaUdaleniya = bd.Состав_группы.Find(vybrannyi.Код_состава_группы);
                        bd.Состав_группы.Remove(zapisDlyaUdaleniya);
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

        // окно управления съёмочными группами открывается на экране
        private void btnUpravlenieGruppami_Click(object sender, RoutedEventArgs e)
        {
            var forma = new GruppyWindow();
            forma.ShowDialog();

            // данные таблицы обновляются после закрытия окна
            zagruzitSpisok();
        }

    }
}