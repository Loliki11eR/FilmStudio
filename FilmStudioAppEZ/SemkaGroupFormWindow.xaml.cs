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
    /// Логика взаимодействия для SemkaGroupFormWindow.xaml
    /// </summary>
    public partial class SemkaGroupFormWindow : Window
    {
        // ссылка на контекст bd
        private FilmStudioEZEntities bd = new FilmStudioEZEntities();

        // текущий объект состава группы
        private Состав_группы tekushiySostav;

        // флаг режима изменения
        private bool etoRedaktirovanie = false;

        public SemkaGroupFormWindow()
        {
            InitializeComponent();
            zagruzitSpravochniki();
            tekushiySostav = new Состав_группы();
            etoRedaktirovanie = false;
        }

        // конструктор для изменения существующей привязки
        public SemkaGroupFormWindow(Состав_группы vybranniySostav)
        {
            InitializeComponent();
            zagruzitSpravochniki();

            // запись находится в текущем контексте по ключу
            tekushiySostav = bd.Состав_группы.Find(vybranniySostav.Код_состава_группы);
            etoRedaktirovanie = true;

            zagruzitDannye();
        }

        // списки групп и сотрудников загружаются из базы
        private void zagruzitSpravochniki()
        {
            try
            {
                cbGruppa.ItemsSource = bd.Съемочная_группа.OrderBy(g => g.Название_группы).ToList();

                // список сотрудников формируется с выводом фамилии и имени
                var sotrudnikiSpisok = bd.Сотрудник
                    .OrderBy(s => s.Фамилия)
                    .ToList()
                    .Select(s => new {
                        s.Код_сотрудника,
                        FIO = s.Фамилия + " " + s.Имя
                    }).ToList();

                cbSotrudnik.DisplayMemberPath = "FIO";
                cbSotrudnik.ItemsSource = sotrudnikiSpisok;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные состава выводятся на экран
        private void zagruzitDannye()
        {
            if (tekushiySostav != null)
            {
                cbGruppa.SelectedValue = tekushiySostav.Съемочная_группа;
                cbSotrudnik.SelectedValue = tekushiySostav.Сотрудник;
            }
        }

        // данные проверяются на корректность заполнения
        private bool proveritDannye()
        {
            // проверяется выбор съёмочной группы
            if (cbGruppa.SelectedValue == null)
            {
                MessageBox.Show("Выберите съёмочную группу", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbGruppa.Focus();
                return false;
            }

            // проверяется выбор сотрудника
            if (cbSotrudnik.SelectedValue == null)
            {
                MessageBox.Show("Выберите сотрудника", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbSotrudnik.Focus();
                return false;
            }

            return true;
        }

        // данные состава группы сохраняются в базу
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            if (!proveritDannye())
                return;

            try
            {
                int idGruppy = (int)cbGruppa.SelectedValue;
                int idSotrudnika = (int)cbSotrudnik.SelectedValue;

                // перед сохранением проверяется отсутствие дубликата в этой же группе
                bool dublikat = bd.Состав_группы.Any(sg => sg.Съемочная_группа == idGruppy &&
                                                           sg.Сотрудник == idSotrudnika &&
                                                           sg.Код_состава_группы != tekushiySostav.Код_состава_группы);
                if (dublikat)
                {
                    MessageBox.Show("Этот сотрудник уже добавлен в выбранную группу", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // выбранные внешние ключи переносятся в объект модели
                tekushiySostav.Съемочная_группа = idGruppy;
                tekushiySostav.Сотрудник = idSotrudnika;

                if (!etoRedaktirovanie)
                {
                    // новый первичный ключ рассчитывается при создании
                    int noviyKod = 1;
                    if (bd.Состав_группы.Any())
                    {
                        noviyKod = bd.Состав_группы.Max(sg => sg.Код_состава_группы) + 1;
                    }
                    tekushiySostav.Код_состава_группы = noviyKod;

                    bd.Состав_группы.Add(tekushiySostav);
                }

                // коммит транзакции в бд
                bd.SaveChanges();
                MessageBox.Show("Состав группы успешно сохранен", "Успех",
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
