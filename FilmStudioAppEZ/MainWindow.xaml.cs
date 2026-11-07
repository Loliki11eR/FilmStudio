using System;
using System.Linq;
using System.Windows;

namespace FilmStudioAppEZ
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            proveritPrevyshenieBudzheta();

            // подтягиваем данные глобальной сессии в шапку
            txtImya.Text = App.imyaUsera;
            txtRol.Text = App.rolUsera;

            // кнопку затрат видим только администраторам
            if (App.rolUsera != "Администратор")
            {
                btnZatraty.Visibility = Visibility.Collapsed;
            }
        }

        // проверка проектов на превышение фактических затрат над бюджетом
        private void proveritPrevyshenieBudzheta()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    // из базы данных извлекаются проекты с их расходами
                    var proekty = bd.Проект.Include("Затрата").AsNoTracking().ToList();
                    string spisokPreDuprozhdeniy = "";

                    foreach (var proekt in proekty)
                    {
                        // рассчитывается общая сумма всех затрат по текущему проекту
                        decimal obshchieZatraty = proekt.Затрата.Sum(z => z.Сумма);
                        decimal limitBudzheta = proekt.Бюджет ?? 0;

                        // проверяется факт превышения лимита расходов
                        if (limitBudzheta > 0 && obshchieZatraty > limitBudzheta)
                        {
                            decimal prevyshenie = obshchieZatraty - limitBudzheta;
                            spisokPreDuprozhdeniy += $"• Проект \"{proekt.Название}\":\n" +
                                $"  Бюджет: {limitBudzheta:N2} руб.\n" +
                                $"  Затраты: {obshchieZatraty:N2} руб.\n" +
                                $"  Превышение на: {prevyshenie:N2} руб.\n\n";
                        }
                    }

                    // если найдены проблемные проекты, выводится системное окно
                    if (!string.IsNullOrEmpty(spisokPreDuprozhdeniy))
                    {
                        MessageBox.Show("Внимание! Обнаружено превышение бюджета по проектам:\n\n" + spisokPreDuprozhdeniy,
                            "Финансовый контроль студии", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка финансовой проверки: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // переход к модулю сотрудников
        private void btnSotrudniki_Click(object sender, RoutedEventArgs e)
        {
            new SotrudnikiWindow().ShowDialog();
        }

        // переход к модулю проектов
        private void btnProekty_Click(object sender, RoutedEventArgs e)
        {
            new ProektyWindow().ShowDialog();
        }

        // переход к модулю площадок
        private void btnPloshchadki_Click(object sender, RoutedEventArgs e)
        {
            new PloshchadkiWindow().ShowDialog();
        }

        // переход к модулю съёмочной группы
        private void btnSemka_Click(object sender, RoutedEventArgs e)
        {
            new SemkaWindow().ShowDialog();
        }

        // переход к модулю графика съёмок
        private void btnGrafik_Click(object sender, RoutedEventArgs e)
        {
            new GrafikWindow().ShowDialog();
        }

        // переход к модулю затрат
        private void btnZatraty_Click(object sender, RoutedEventArgs e)
        {
            new ZatratyWindow().ShowDialog();
        }

        // переход к модулю проката
        private void btnProkat_Click(object sender, RoutedEventArgs e)
        {
            new ProkatWindow().ShowDialog();
        }

        // выход из приложения с возвратом на окно авторизации
        private void btnVyhod_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти из системы?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // очищаем сессию и открываем окно входа заново
                App.imyaUsera = "";
                App.rolUsera = "";

                var autorization = new AutorizationWindow();
                autorization.Show();
                this.Close();
            }
        }
    }
}