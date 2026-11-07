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
    /// Логика взаимодействия для ZatratyFormWindow.xaml
    /// </summary>
    public partial class ZatratyFormWindow : Window
    {
        // ссылка на контекст bd
        private FilmStudioEZEntities bd = new FilmStudioEZEntities();

        // текущий объект финансовой затраты
        private Затрата tekushayaZatrata;

        // флаг режима изменения
        private bool etoRedaktirovanie = false;

        public ZatratyFormWindow()
        {
            InitializeComponent();
            zagruzitSpravochniki();
            tekushayaZatrata = new Затрата();
            etoRedaktirovanie = false;
        }

        // конструктор для изменения существующего финансового расхода
        public ZatratyFormWindow(Затрата vybrannayaZatrata)
        {
            InitializeComponent();
            zagruzitSpravochniki();

            // запись находится в текущем контексте по ключу
            tekushayaZatrata = bd.Затрата.Find(vybrannayaZatrata.Код_затраты);
            etoRedaktirovanie = true;

            zagruzitDannye();
        }

        // списки проектов и категорий затрат загружаются из базы
        private void zagruzitSpravochniki()
        {
            try
            {
                cbProekt.ItemsSource = bd.Проект.OrderBy(p => p.Название).ToList();
                cbTipZatrat.ItemsSource = bd.Тип_затраты.OrderBy(t => t.Наименование).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные затраты выводятся на экранные элементы
        private void zagruzitDannye()
        {
            if (tekushayaZatrata != null)
            {
                cbProekt.SelectedValue = tekushayaZatrata.Проект;
                cbTipZatrat.SelectedValue = tekushayaZatrata.Тип_затрат;
                txtSumma.Text = tekushayaZatrata.Сумма.ToString();
                dpData.SelectedDate = tekushayaZatrata.Дата;
            }
        }

        // данные проверяются на соответствие условиям валидации
        private bool proveritDannye()
        {
            // проверяется факт выбора проекта
            if (cbProekt.SelectedValue == null)
            {
                MessageBox.Show("Выберите проект из списка", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbProekt.Focus();
                return false;
            }

            // проверяется факт выбора типа расхода
            if (cbTipZatrat.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип финансовой затраты", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbTipZatrat.Focus();
                return false;
            }

            // проверяется числовое значение суммы на корректность и положительность
            if (!decimal.TryParse(txtSumma.Text, out decimal summa) || summa <= 0)
            {
                MessageBox.Show("Сумма финансового расхода должна быть числом больше нуля", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSumma.Focus();
                return false;
            }

            // проверяется заполнение даты проведения операции
            if (dpData.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату проведения расхода", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpData.Focus();
                return false;
            }

            int idProekta = (int)cbProekt.SelectedValue;
            DateTime dataZatraty = dpData.SelectedDate.Value.Date;

            // из базы данных извлекается выбранный проект для проверки его сроков
            var proekt = bd.Проект.Find(idProekta);
            if (proekt != null && proekt.Дата_окончания_съемок.HasValue)
            {
                // проверяется ограничение даты затраты датой окончания самого проекта
                if (dataZatraty > proekt.Дата_окончания_съемок.Value.Date)
                {
                    MessageBox.Show("Дата расхода не может быть позже даты окончания съемок проекта: " +
                        proekt.Дата_окончания_съемок.Value.ToString("dd.MM.yyyy"), "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpData.Focus();
                    return false;
                }
            }

            return true;
        }


        // данные расхода сохраняются в базу данных
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            if (!proveritDannye())
                return;

            try
            {
                // значения элементов интерфейса переносятся в объект модели
                tekushayaZatrata.Проект = (int)cbProekt.SelectedValue;
                tekushayaZatrata.Тип_затрат = (int)cbTipZatrat.SelectedValue;
                tekushayaZatrata.Сумма = decimal.Parse(txtSumma.Text);
                tekushayaZatrata.Дата = dpData.SelectedDate.Value.Date;

                if (!etoRedaktirovanie)
                {
                    // новый первичный ключ рассчитывается при создании
                    int noviyKod = 1;
                    if (bd.Затрата.Any())
                    {
                        noviyKod = bd.Затрата.Max(z => z.Код_затраты) + 1;
                    }
                    tekushayaZatrata.Код_затраты = noviyKod;

                    bd.Затрата.Add(tekushayaZatrata);
                }

                // коммит транзакции в бд
                bd.SaveChanges();
                MessageBox.Show("Данные о затратах успешно сохранены", "Успех",
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

        // окно закрывается без сохранения изменений
        private void btnOtmena_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}