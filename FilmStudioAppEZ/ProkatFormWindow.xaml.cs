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
    /// Логика взаимодействия для ProkatFormWindow.xaml
    /// </summary>
    public partial class ProkatFormWindow : Window
    {
        // ссылка на контекст bd
        private FilmStudioEZEntities bd = new FilmStudioEZEntities();

        // текущий объект проката
        private Прокат tekushiyProkat;

        // флаг режима изменения
        private bool etoRedaktirovanie = false;

        public ProkatFormWindow()
        {
            InitializeComponent();
            zagruzitSpravochniki();
            tekushiyProkat = new Прокат();
            etoRedaktirovanie = false;
        }

        // constructor для изменения существующей записи о прокате
        public ProkatFormWindow(Прокат vybranniyProkat)
        {
            InitializeComponent();
            zagruzitSpravochniki();

            // запись находится в текущем контексте по ключу
            tekushiyProkat = bd.Прокат.Find(vybranniyProkat.Код_проката);
            etoRedaktirovanie = true;

            zagruzitDannye();
        }

        // список проектов загружается из базы данных
        private void zagruzitSpravochniki()
        {
            try
            {
                cbProekt.ItemsSource = bd.Проект.OrderBy(p => p.Название).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные проката выводятся на экранные элементы
        private void zagruzitDannye()
        {
            if (tekushiyProkat != null)
            {
                cbProekt.SelectedValue = tekushiyProkat.Проект;
                dpDataVyhoda.SelectedDate = tekushiyProkat.Дата_выхода;
                txtKinoteatry.Text = tekushiyProkat.Количество_кинотеатров.ToString();
                txtSbory.Text = tekushiyProkat.Сборы.ToString();
                txtReyting.Text = tekushiyProkat.Рейтинг.ToString();
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

            // проверяется заполнение даты выхода в прокат
            if (dpDataVyhoda.SelectedDate == null)
            {
                MessageBox.Show("Укажите дату выхода в прокат", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDataVyhoda.Focus();
                return false;
            }

            DateTime dataVyhoda = dpDataVyhoda.SelectedDate.Value.Date;

            // проверяется ограничение на текущую дату
            if (dataVyhoda == DateTime.Today)
            {
                MessageBox.Show("Дата выхода в прокат не может быть сегодняшним днём", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDataVyhoda.Focus();
                return false;
            }

            // проверяется количество кинотеатров на целое число больше нуля
            if (!int.TryParse(txtKinoteatry.Text, out int kinoteatry) || kinoteatry <= 0)
            {
                MessageBox.Show("Количество кинотеатров должно быть целым числом больше нуля", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtKinoteatry.Focus();
                return false;
            }

            // проверяется значение кассовых сборов только если оно введено
            if (!string.IsNullOrWhiteSpace(txtSbory.Text))
            {
                if (!decimal.TryParse(txtSbory.Text, out decimal sbory) || sbory < 0)
                {
                    MessageBox.Show("Кассовые сборы должны быть положительным числом", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtSbory.Focus();
                    return false;
                }
            }

            // проверяется значение рейтинга только если оно введено
            if (!string.IsNullOrWhiteSpace(txtReyting.Text))
            {
                if (!decimal.TryParse(txtReyting.Text, out decimal reyting) || reyting < 0 || reyting > 10)
                {
                    MessageBox.Show("Рейтинг должен быть числом в диапазоне от 0 до 10", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtReyting.Focus();
                    return false;
                }
            }

            int idProekta = (int)cbProekt.SelectedValue;

            // из базы данных извлекается выбранный проект для проверки сроков съемок
            var proekt = bd.Проект.Find(idProekta);
            if (proekt != null && proekt.Дата_окончания_съемок.HasValue)
            {
                DateTime minimalnayaData = proekt.Дата_окончания_съемок.Value.Date.AddMonths(3);

                // проверяется интервал между окончанием съемок и прокатом
                if (dataVyhoda < minimalnayaData)
                {
                    MessageBox.Show("Дата выхода в прокат должна быть минимум через 3 месяца после окончания съемок проекта: " +
                        minimalnayaData.ToString("dd.MM.yyyy"), "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpDataVyhoda.Focus();
                    return false;
                }
            }

            return true;
        }



        // данные проката сохраняются в базу данных
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            if (!proveritDannye())
                return;

            try
            {
                // значения элементов интерфейса переносятся в объект модели
                tekushiyProkat.Проект = (int)cbProekt.SelectedValue;
                tekushiyProkat.Дата_выхода = dpDataVyhoda.SelectedDate.Value.Date;
                tekushiyProkat.Количество_кинотеатров = int.Parse(txtKinoteatry.Text);

                // пустые текстовые поля переводятся в значения null для базы данных
                tekushiyProkat.Сборы = string.IsNullOrWhiteSpace(txtSbory.Text) ? (decimal?)null : decimal.Parse(txtSbory.Text);
                tekushiyProkat.Рейтинг = string.IsNullOrWhiteSpace(txtReyting.Text) ? (decimal?)null : decimal.Parse(txtReyting.Text);

                if (!etoRedaktirovanie)
                {
                    int noviyKod = 1;
                    if (bd.Прокат.Any())
                    {
                        noviyKod = bd.Прокат.Max(p => p.Код_проката) + 1;
                    }
                    tekushiyProkat.Код_проката = noviyKod;

                    bd.Прокат.Add(tekushiyProkat);
                }

                bd.SaveChanges();
                MessageBox.Show("Данные о прокате успешно сохранены", "Успех",
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