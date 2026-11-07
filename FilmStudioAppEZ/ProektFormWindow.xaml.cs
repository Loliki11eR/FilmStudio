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
    /// Логика взаимодействия для ProektFormWindow.xaml
    /// </summary>
    public partial class ProektFormWindow : Window
    {
        // ссылка на контекст bd
        private FilmStudioEZEntities bd = new FilmStudioEZEntities();

        // текущий объект проекта из бд
        private Проект tekushiyProect;

        // флаг режима изменения
        private bool etoRedaktirovanie = false;
        public ProektFormWindow()
        {
            InitializeComponent();
            ZapolnitSpravochniki();
            tekushiyProect = new Проект();
            etoRedaktirovanie = false;
        }

        // конструктор для открытия на редактирование существующего проекта
        public ProektFormWindow(Проект vybranniyProect)
        {
            InitializeComponent();
            ZapolnitSpravochniki();

            // находим запись в текущем контексте по первичному ключу
            tekushiyProect = bd.Проект.Find(vybranniyProect.Код_проекта);
            etoRedaktirovanie = true;

            ZapolnitPolyaFormy();
        }

        // загрузка данных для выпадающих списков из базы
        private void ZapolnitSpravochniki()
        {
            try
            {
                cbZhanr.ItemsSource = bd.Жанр.ToList();
                cbStatus.ItemsSource = bd.Статус_проекта.ToList();
                cbAuditoriya.ItemsSource = bd.Целевая_аудитория.ToList();
            }
            catch (Exception oshibka)
            {
                MessageBox.Show("ошибка загрузки справочников: " + oshibka.Message, "ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // перенос данных из объекта бд в элементы интерфейса
        private void ZapolnitPolyaFormy()
        {
            txtNazvanie.Text = tekushiyProect.Название;
            txtBudzhet.Text = tekushiyProect.Бюджет.ToString();
            dpDataNachala.SelectedDate = tekushiyProect.Дата_начала_съемок;
            dpDataOkonchaniya.SelectedDate = tekushiyProect.Дата_окончания_съемок;

            // устанавливаем выбранные значения по внешним ключам int
            cbZhanr.SelectedValue = tekushiyProect.Жанр;
            cbStatus.SelectedValue = tekushiyProect.Статус;
            cbAuditoriya.SelectedValue = tekushiyProect.Целевая_аудитория;
        }

        // валидация полей и сохранение изменений
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            // валидация обязательного названия
            if (string.IsNullOrWhiteSpace(txtNazvanie.Text))
            {
                MessageBox.Show("название проекта обязательно для заполнения", "ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // валидация бюджета на число больше нуля
            if (!decimal.TryParse(txtBudzhet.Text, out decimal budzhet) || budzhet <= 0)
            {
                MessageBox.Show("бюджет должен быть числом больше нуля", "ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // проверка на заполненность обеих дат съемок
            if (dpDataNachala.SelectedDate == null || dpDataOkonchaniya.SelectedDate == null)
            {
                MessageBox.Show("необходимо выбрать обе даты съемок", "ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime dataNachala = dpDataNachala.SelectedDate.Value;
            DateTime dataOkonchaniya = dpDataOkonchaniya.SelectedDate.Value;

            // проверка логики дат окончания и начала
            if (dataOkonchaniya < dataNachala)
            {
                MessageBox.Show("дата окончания не может быть раньше даты начала", "ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // проверка обязательного выбора статуса из списка
            if (cbStatus.SelectedValue == null)
            {
                MessageBox.Show("необходимо выбрать статус проекта из списка", "ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // перенос проверенных значений интерфейса в объект модели
            tekushiyProect.Название = txtNazvanie.Text.Trim();
            tekushiyProect.Бюджет = budzhet;
            tekushiyProect.Дата_начала_съемок = dataNachala;
            tekushiyProect.Дата_окончания_съемок = dataOkonchaniya;

            // присваиваем внешние ключи int
            tekushiyProect.Жанр = (int?)cbZhanr.SelectedValue;
            tekushiyProect.Статус = (int)cbStatus.SelectedValue;
            tekushiyProect.Целевая_аудитория = (int?)cbAuditoriya.SelectedValue;

            try
            {
                if (!etoRedaktirovanie)
                {
                    // генерация нового первичного ключа
                    int noviyKod = 1;
                    if (bd.Проект.Any())
                    {
                        noviyKod = bd.Проект.Max(p => p.Код_проекта) + 1;
                    }
                    tekushiyProect.Код_проекта = noviyKod;

                    bd.Проект.Add(tekushiyProect);
                }

                // сообщение транзакции в бд
                bd.SaveChanges();
                MessageBox.Show("данные проекта успешно сохранены", "успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception oshibka)
            {
                MessageBox.Show("ошибка при записи изменений в бд: " + oshibka.Message, "ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // закрытие окна без сохранения
        private void btnOtmena_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}