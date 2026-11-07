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
    /// Логика взаимодействия для GrafikFormWindow.xaml
    /// </summary>
    public partial class GrafikFormWindow : Window
    {
        // ссылка на контекст bd
        private FilmStudioEZEntities bd = new FilmStudioEZEntities();

        // текущий объект графика съемок
        private График_съемок tekushiyGrafik;

        // флаг режима изменения
        private bool etoRedaktirovanie = false;

        public GrafikFormWindow()
        {
            InitializeComponent();
            zagruzitSpravochniki();
            tekushiyGrafik = new График_съемок();
            etoRedaktirovanie = false;
        }

        // конструктор для изменения существующей записи
        public GrafikFormWindow(График_съемок vybranniyGrafik)
        {
            InitializeComponent();
            zagruzitSpravochniki();

            // запись находится в текущем контексте по ключу
            tekushiyGrafik = bd.График_съемок.Find(vybranniyGrafik.Код_графика_съемок);
            etoRedaktirovanie = true;

            zagruzitDannye();
        }

        // списки проектов и площадок загружаются из базы
        private void zagruzitSpravochniki()
        {
            try
            {
                cbProekt.ItemsSource = bd.Проект.OrderBy(p => p.Название).ToList();
                cbPloshadka.ItemsSource = bd.Площадка.OrderBy(p => p.Адрес).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные графика выводятся на экран
        private void zagruzitDannye()
        {
            if (tekushiyGrafik != null)
            {
                dpData.SelectedDate = tekushiyGrafik.Дата;
                txtVremya.Text = tekushiyGrafik.Время?.ToString(@"hh\:mm");
                cbPloshadka.SelectedValue = tekushiyGrafik.Площадка;
                cbProekt.SelectedValue = tekushiyGrafik.Набор_сотрудников;
                txtScena.Text = tekushiyGrafik.Сцена;
            }
        }

        // данные проверяются на корректность заполнения
        private bool proveritDannye()
        {
            // проверяется заполнение даты
            if (dpData.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату съёмки", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpData.Focus();
                return false;
            }

            // проверяется формат и корректность времени
            if (string.IsNullOrWhiteSpace(txtVremya.Text) || !TimeSpan.TryParse(txtVremya.Text, out TimeSpan vremya))
            {
                MessageBox.Show("Введите корректное время в формате чч:мм", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtVremya.Focus();
                return false;
            }

            // проверяется выбор проекта
            if (cbProekt.SelectedValue == null)
            {
                MessageBox.Show("Выберите проект из списка", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbProekt.Focus();
                return false;
            }

            // проверяется выбор площадки
            if (cbPloshadka.SelectedValue == null)
            {
                MessageBox.Show("Выберите площадку из списка", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbPloshadka.Focus();
                return false;
            }

            return true;
        }

        // данные расписания сохраняются в базу
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            if (!proveritDannye())
                return;

            try
            {
                // значения элементов интерфейса переносятся в объект модели
                tekushiyGrafik.Дата = dpData.SelectedDate.Value.Date;
                tekushiyGrafik.Время = TimeSpan.Parse(txtVremya.Text);
                tekushiyGrafik.Площадка = (int)cbPloshadka.SelectedValue;
                tekushiyGrafik.Набор_сотрудников = (int)cbProekt.SelectedValue;
                tekushiyGrafik.Сцена = txtScena.Text.Trim();

                if (!etoRedaktirovanie)
                {
                    // новый первичный ключ рассчитывается при создании
                    int noviyKod = 1;
                    if (bd.График_съемок.Any())
                    {
                        noviyKod = bd.График_съемок.Max(g => g.Код_графика_съемок) + 1;
                    }
                    tekushiyGrafik.Код_графика_съемок = noviyKod;

                    bd.График_съемок.Add(tekushiyGrafik);
                }

                // коммит транзакции в бд
                bd.SaveChanges();
                MessageBox.Show("Запись графика успешно сохранена", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // окно закрывается без сохранения
        private void btnOtmena_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}