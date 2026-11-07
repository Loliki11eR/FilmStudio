using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для AutorizationWindow.xaml
    /// </summary>
    public partial class AutorizationWindow : Window
    {
        public AutorizationWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            // проверка на пустые поля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hash = "";
            // создание хэша пароля
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bajty = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                hash = BitConverter.ToString(bajty).Replace("-", "").ToLower();
            }

            try
            {
                // подключение к контексту
                using (var bd = new FilmStudioEZEntities())
                {
                    // поиск записи пользователя и его роли в бд
                    var chelovek = bd.Пользователь.Include("Роль").FirstOrDefault(u => u.Логин == login && u.ПарольХеш == hash);

                    if (chelovek != null)
                    {
                        // сохранение данных вошедшего пользователя
                        App.imyaUsera = chelovek.Имя;
                        App.rolUsera = chelovek.Роль.Наименование;

                        // переход на главное окно
                        MainWindow glavnoeOkno = new MainWindow();
                        glavnoeOkno.Show();

                        // закрытие формы авторизации
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception oshibka)
            {
                MessageBox.Show($"Ошибка базы: {oshibka.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            App.imyaUsera = "Тестовый Админ";
            App.rolUsera = "Администратор";

            MainWindow glavnoeOkno = new MainWindow();
            glavnoeOkno.Show();

            this.Close();
        }

        // окно регистрации нового пользователя открывается на экране
        private void lnkRegistraciya_Click(object sender, RoutedEventArgs e)
        {
            // создается экземпляр окна регистрации
            var formaRegistracii = new RegWindow();

            // текущее окно авторизации устанавливается в качестве владельца
            formaRegistracii.Owner = this;

            // форма открывается модально на экране
            formaRegistracii.ShowDialog();
        }

    }
}