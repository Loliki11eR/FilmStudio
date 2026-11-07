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
    /// Логика взаимодействия для GruppyWindow.xaml
    /// </summary>
    public partial class GruppyWindow : Window
    {
        public GruppyWindow()
        {
            InitializeComponent();
            zagruzitSpisok();
        }

        // данные съемочных групп выгружаются в таблицу
        private void zagruzitSpisok()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    var spisok = bd.Съемочная_группа
                        .AsNoTracking()
                        .OrderBy(g => g.Название_группы)
                        .ToList();
                    dgGruppy.ItemsSource = spisok;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // новая съемочная группа записывается в базу
        private void btnDobavit_Click(object sender, RoutedEventArgs e)
        {
            // проверяется заполнение текстового поля названия
            if (string.IsNullOrWhiteSpace(txtNazvanieGruppy.Text))
            {
                MessageBox.Show("Введите название съёмочной группы", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    string nazvanie = txtNazvanieGruppy.Text.Trim();

                    // проверяется отсутствие группы с таким же именем
                    if (bd.Съемочная_группа.Any(g => g.Название_группы == nazvanie))
                    {
                        MessageBox.Show("Группа с таким названием уже существует", "Ошибка валидации",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // новый код рассчитывается автоматически
                    int noviyKod = 1;
                    if (bd.Съемочная_группа.Any())
                    {
                        noviyKod = bd.Съемочная_группа.Max(g => g.Код_съемочной_группы) + 1;
                    }

                    var novayaGruppa = new Съемочная_группа
                    {
                        Код_съемочной_группы = noviyKod,
                        Название_группы = nazvanie
                    };

                    bd.Съемочная_группа.Add(novayaGruppa);
                    bd.SaveChanges();

                    txtNazvanieGruppy.Clear();
                    zagruzitSpisok();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка создания группы: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // выбранная группа удаляется при отсутствии связей
        private void btnUdalit_Click(object sender, RoutedEventArgs e)
        {
            var vybrannaya = dgGruppy.SelectedItem as Съемочная_группа;

            if (vybrannaya == null)
            {
                MessageBox.Show("Выберите группу для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить группу {vybrannaya.Название_группы}?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var bd = new FilmStudioEZEntities())
                    {
                        // проверяется наличие привязанных сотрудников или проектов
                        bool estSvyazi = bd.Состав_группы.Any(sg => sg.Съемочная_группа == vybrannaya.Код_съемочной_группы) ||
                                         bd.Проект.Any(p => p.Съемочная_группа == vybrannaya.Код_съемочной_группы);

                        if (estSvyazi)
                        {
                            MessageBox.Show("Нельзя удалить группу, так как в нее добавлены сотрудники или она привязана к проекту",
                                "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var gruppaDlyaUdaleniya = bd.Съемочная_группа.Find(vybrannaya.Код_съемочной_группы);
                        bd.Съемочная_группа.Remove(gruppaDlyaUdaleniya);
                        bd.SaveChanges();

                        MessageBox.Show("Группа успешно удалена", "Успех",
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