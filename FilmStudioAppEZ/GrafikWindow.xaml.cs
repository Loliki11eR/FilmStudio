using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для GrafikWindow.xaml
    /// </summary>
    public partial class GrafikWindow : Window
    {
        public GrafikWindow()
        {
            InitializeComponent();
            zagruzitSpravochniki();
            zagruzitSpisok();
        }

        // список проектов загружается в комбобокс для фильтрации
        private void zagruzitSpravochniki()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    cmbFiltrProekt.ItemsSource = bd.Проект.OrderBy(p => p.Название).ToList();
                    cmbFiltrProekt.DisplayMemberPath = "Название";
                    cmbFiltrProekt.SelectedValuePath = "Код_проекта";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки проектов: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные расписания съемок выводятся в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    var spisok = bd.График_съемок
                        .Include("Площадка1")
                        .AsNoTracking()
                        .OrderBy(g => g.Дата)
                        .ThenBy(g => g.Время)
                        .ToList();
                    dgGrafik.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // записи фильтруются по дате и по выбранному проекту одновременно
        private void cmbFiltr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    var query = bd.График_съемок.Include("Площадка1").AsQueryable();

                    if (cmbFiltrProekt.SelectedValue != null)
                    {
                        int idProekta = (int)cmbFiltrProekt.SelectedValue;
                        // фильтрация выполняется по условному совпадению группы или площадки проекта
                        query = query.Where(g => g.Набор_сотрудников == idProekta);
                    }

                    if (dpFiltrData.SelectedDate != null)
                    {
                        DateTime vybrannayaData = dpFiltrData.SelectedDate.Value.Date;
                        query = query.Where(g => g.Дата == vybrannayaData);
                    }

                    dgGrafik.ItemsSource = query.AsNoTracking().OrderBy(g => g.Дата).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // фильтры сбрасываются до начального состояния
        private void btnSbros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltrProekt.SelectedValue = null;
            dpFiltrData.SelectedDate = null;
            zagruzitSpisok();
        }

        // форма создания расписания открывается на экране
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            var forma = new GrafikFormWindow();

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // форма редактирования записи выводится на экран
        private void btnRedaktirovat_Click(object sender, RoutedEventArgs e)
        {
            var vybrannyi = dgGrafik.SelectedItem as График_съемок;

            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var forma = new GrafikFormWindow(vybrannyi);

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // запись удаляется из базы данных
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            var vybrannyi = dgGrafik.SelectedItem as График_съемок;

            if (vybrannyi == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Удалить выбранную запись из графика съёмок?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        var zapisDlyaUdaleniya = bd.График_съемок.Find(vybrannyi.Код_графика_съемок);
                        bd.График_съемок.Remove(zapisDlyaUdaleniya);
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