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
    /// Логика взаимодействия для ZatratyWindow.xaml
    /// </summary>
    public partial class ZatratyWindow : Window
    {
        public ZatratyWindow()
        {
            InitializeComponent();
            zagruzitSpravochniki();
            zagruzitSpisok();
        }

        // списки проектов и типов затрат загружаются в комбобоксы
        private void zagruzitSpravochniki()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    cmbFiltrProekt.ItemsSource = bd.Проект.OrderBy(p => p.Название).ToList();
                    cmbFiltrTip.ItemsSource = bd.Тип_затраты.OrderBy(t => t.Наименование).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные финансовых расходов выводятся в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    var spisok = bd.Затрата
                        .Include("Проект1")
                        .Include("Тип_затраты")
                        .AsNoTracking()
                        .OrderByDescending(z => z.Дата)
                        .ToList();
                    dgZatraty.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // записи фильтруются по двум критериям одновременно
        private void cmbFiltr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    var query = bd.Затрата.Include("Проект1").Include("Тип_затраты").AsQueryable();

                    if (cmbFiltrProekt.SelectedValue != null)
                    {
                        int idProekta = (int)cmbFiltrProekt.SelectedValue;
                        query = query.Where(z => z.Проект == idProekta);
                    }

                    if (cmbFiltrTip.SelectedValue != null)
                    {
                        int idTipa = (int)cmbFiltrTip.SelectedValue;
                        query = query.Where(z => z.Тип_затрат == idTipa);
                    }

                    dgZatraty.ItemsSource = query.AsNoTracking().OrderByDescending(z => z.Дата).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка фильтрации: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // значения фильтров сбрасываются до начального состояния
        private void btnSbros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltrProekt.SelectedValue = null;
            cmbFiltrTip.SelectedValue = null;
            zagruzitSpisok();
        }

        // форма создания расхода открывается на экране
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            var forma = new ZatratyFormWindow();

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // форма редактирования расхода выводится на экран
        private void btnRedaktirovat_Click(object sender, RoutedEventArgs e)
        {
            var vybrannaya = dgZatraty.SelectedItem as Затрата;

            if (vybrannaya == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var forma = new ZatratyFormWindow(vybrannaya);

            // окно открывается модально и обновляется список
            if (forma.ShowDialog() == true)
            {
                zagruzitSpisok();
            }
        }

        // выбранная запись удаляется из базы данных
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            var vybrannaya = dgZatraty.SelectedItem as Затрата;

            if (vybrannaya == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Удалить выбранную запись о затратах?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        var zapisDlyaUdaleniya = bd.Затрата.Find(vybrannaya.Код_затраты);
                        bd.Затрата.Remove(zapisDlyaUdaleniya);
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