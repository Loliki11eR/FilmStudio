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
    /// Логика взаимодействия для RegWindow.xaml
    /// </summary>
    public partial class RegWindow : Window
    {
        public RegWindow()
        {
            InitializeComponent();
            zagruzitRoli();
        }

        // список доступных ролей выгружается в комбобокс
        private void zagruzitRoli()
        {
            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    cbRol.ItemsSource = bd.Роль.OrderBy(r => r.Наименование).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки ролей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // учетная запись проверяется и заносится в базу
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Password) || cbRol.SelectedValue == null)
            {
                MessageBox.Show("Заполните логин, пароль и выберите роль", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var bd = new FilmStudioEZEntities())
                {
                    string login = txtLogin.Text.Trim();

                    // проверяется уникальность логина в системе
                    if (bd.Пользователь.Any(u => u.Логин == login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string hash = "";
                    string password = txtPassword.Password;

                    // создание хэша пароля строго по вашему алгоритму
                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] bajty = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                        hash = BitConverter.ToString(bajty).Replace("-", "").ToLower();
                    }

                    var noviyPolzovatel = new Пользователь
                    {
                        Логин = login,
                        ПарольХеш = hash,
                        Код_роли = (int)cbRol.SelectedValue,
                        Имя = txtFio.Text.Trim()
                    };

                    bd.Пользователь.Add(noviyPolzovatel);
                    bd.SaveChanges();

                    MessageBox.Show("Пользователь успешно создан", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка регистрации: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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