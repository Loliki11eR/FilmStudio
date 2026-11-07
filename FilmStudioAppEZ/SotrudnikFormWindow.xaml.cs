using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace FilmStudioAppEZ
{
    public partial class SotrudnikFormWindow : Window
    {
        // ссылка на контекст bd
        private FilmStudioEZEntities bd = new FilmStudioEZEntities();

        // текущий объект сотрудника
        private Сотрудник tekushiySotrudnik;

        // флаг режима изменения
        private bool etoRedaktirovanie = false;

        // конструктор для добавления нового сотрудника
        public SotrudnikFormWindow()
        {
            InitializeComponent();
            this.Title = "Добавление сотрудника";
            zagruzitSpecializacii();
            tekushiySotrudnik = new Сотрудник();
            etoRedaktirovanie = false;
        }

        // конструктор для редактирования существующего сотрудника
        public SotrudnikFormWindow(Сотрудник vybranniySotrudnik)
        {
            InitializeComponent();
            this.Title = "Редактирование сотрудника";
            zagruzitSpecializacii();

            // запись находится в текущем контексте по ключу
            tekushiySotrudnik = bd.Сотрудник.Find(vybranniySotrudnik.Код_сотрудника);
            etoRedaktirovanie = true;

            zagruzitDannye();
        }

        // список специализаций загружается в комбобокс
        private void zagruzitSpecializacii()
        {
            try
            {
                var specializacii = bd.Специализация.OrderBy(s => s.Наименование).ToList();
                cmbSpecializacia.DisplayMemberPath = "Наименование";
                cmbSpecializacia.SelectedValuePath = "Код_специализации";
                cmbSpecializacia.ItemsSource = specializacii;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки специализаций: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные сотрудника выводятся на экран
        private void zagruzitDannye()
        {
            if (tekushiySotrudnik != null)
            {
                txtFamilia.Text = tekushiySotrudnik.Фамилия;
                txtImya.Text = tekushiySotrudnik.Имя;
                cmbSpecializacia.SelectedValue = tekushiySotrudnik.Специализация;
                txtTelefon.Text = tekushiySotrudnik.Номер_телефона;
                txtEmail.Text = tekushiySotrudnik.Почта;
                txtStavka.Text = tekushiySotrudnik.Ставка.ToString();
            }
        }

        // данные проверяются на корректность заполнения
        private bool proveritDannye()
        {
            // проверяется заполнение фамилии
            if (string.IsNullOrWhiteSpace(txtFamilia.Text))
            {
                MessageBox.Show("Введите фамилию", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFamilia.Focus();
                return false;
            }

            // проверяется заполнение имени
            if (string.IsNullOrWhiteSpace(txtImya.Text))
            {
                MessageBox.Show("Введите имя", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtImya.Focus();
                return false;
            }

            // проверяется факт выбора специализации
            if (cmbSpecializacia.SelectedItem == null)
            {
                MessageBox.Show("Выберите специализацию", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbSpecializacia.Focus();
                return false;
            }

            // проверяется формат номера телефона
            if (!string.IsNullOrWhiteSpace(txtTelefon.Text))
            {
                var telefonPattern = @"^[\d\+\-\(\)\s]+$";
                if (!Regex.IsMatch(txtTelefon.Text, telefonPattern))
                {
                    MessageBox.Show("Некорректный формат телефона", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTelefon.Focus();
                    return false;
                }
            }

            // проверяется наличие знака собачки в почте
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                string vvedennyiEmail = txtEmail.Text.Trim();

                if (!vvedennyiEmail.Contains("@"))
                {
                    MessageBox.Show("Email должен содержать символ @", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return false;
                }

                // проверяется уникальность почты по всей базе данных
                bool emailZanyat = bd.Сотрудник.Any(s => s.Почта == vvedennyiEmail &&
                                                       s.Код_сотрудника != tekushiySotrudnik.Код_сотрудника);
                if (emailZanyat)
                {
                    MessageBox.Show("Сотрудник с такой почтой уже зарегистрирован в системе", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtEmail.Focus();
                    return false;
                }
            }

            // проверяется заполнение ставки
            if (string.IsNullOrWhiteSpace(txtStavka.Text))
            {
                MessageBox.Show("Введите ставку", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStavka.Focus();
                return false;
            }

            // проверяется значение ставки на число больше нуля
            if (!decimal.TryParse(txtStavka.Text, out decimal stavka) || stavka <= 0)
            {
                MessageBox.Show("Ставка должна быть числом больше нуля", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStavka.Focus();
                return false;
            }

            return true;
        }

        // данные сотрудника сохраняются в базу
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            if (!proveritDannye())
                return;

            try
            {
                int kodSpecializacii = (int)cmbSpecializacia.SelectedValue;

                // текстовые поля переносятся в объект модели
                tekushiySotrudnik.Фамилия = txtFamilia.Text.Trim();
                tekushiySotrudnik.Имя = txtImya.Text.Trim();
                tekushiySotrudnik.Специализация = kodSpecializacii;
                tekushiySotrudnik.Номер_телефона = txtTelefon.Text.Trim();
                tekushiySotrudnik.Почта = txtEmail.Text.Trim();
                tekushiySotrudnik.Ставка = decimal.Parse(txtStavka.Text);

                if (!etoRedaktirovanie)
                {
                    // новый первичный ключ рассчитывается при создании
                    int noviyKod = 1;
                    if (bd.Сотрудник.Any())
                    {
                        noviyKod = bd.Сотрудник.Max(s => s.Код_сотрудника) + 1;
                    }
                    tekushiySotrudnik.Код_сотрудника = noviyKod;

                    Random rnd = new Random();
                    tekushiySotrudnik.Табельный_номер = "ТН-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + rnd.Next(100, 999);

                    bd.Сотрудник.Add(tekushiySotrudnik);
                }

                // коммит транзакции в бд
                bd.SaveChanges();
                MessageBox.Show("Данные сохранены успешно", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                string podrobnayaOshibka = ex.Message;
                if (ex.InnerException != null)
                {
                    podrobnayaOshibka += "\nДетали: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        podrobnayaOshibka += "\nДополнительно: " + ex.InnerException.InnerException.Message;
                    }
                }

                MessageBox.Show("Ошибка сохранения: " + podrobnayaOshibka, "Ошибка",
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
