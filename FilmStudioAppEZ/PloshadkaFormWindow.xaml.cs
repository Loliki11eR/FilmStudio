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
    /// Логика взаимодействия для PloshadkaFormWindow.xaml
    /// </summary>
    public partial class PloshadkaFormWindow : Window
    {
        // ссылка на контекст bd
        private FilmStudioEZEntities bd = new FilmStudioEZEntities();

        // текущий объект площадки
        private Площадка tekushayaPloshadka;

        // флаг режима изменения
        private bool etoRedaktirovanie = false;

        public PloshadkaFormWindow()
        {
            InitializeComponent();
            zagruzitTipyPloshadok();
            tekushayaPloshadka = new Площадка();
            etoRedaktirovanie = false;
        }

        // конструктор для редактирования существующей площадки
        public PloshadkaFormWindow(Площадка vybrannayaPloshadka)
        {
            InitializeComponent();
            zagruzitTipyPloshadok();

            // запись находится в текущем контексте по ключу
            tekushayaPloshadka = bd.Площадка.Find(vybrannayaPloshadka.Код_площадки);
            etoRedaktirovanie = true;

            zagruzitDannye();
        }

        // типы площадок загружаются в комбобокс из базы
        private void zagruzitTipyPloshadok()
        {
            try
            {
                cbTipPloshadki.ItemsSource = bd.Тип_площадки.OrderBy(t => t.Наименование).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки типов площадок: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // данные площадки выводятся на экран
        private void zagruzitDannye()
        {
            if (tekushayaPloshadka != null)
            {
                txtAdres.Text = tekushayaPloshadka.Адрес;
                cbTipPloshadki.SelectedValue = tekushayaPloshadka.Тип;
                txtStoimost.Text = tekushayaPloshadka.Стоимость_аренды_в_день.ToString();
                txtVmestimost.Text = tekushayaPloshadka.Вместимость.ToString();
            }
        }

        // данные проверяются на корректность заполнения
        private bool proveritDannye()
        {
            // проверяется заполнение обязательного адреса
            if (string.IsNullOrWhiteSpace(txtAdres.Text))
            {
                MessageBox.Show("Введите адрес площадки", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAdres.Focus();
                return false;
            }

            // проверяется выбор типа площадки
            if (cbTipPloshadki.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип площадки", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbTipPloshadki.Focus();
                return false;
            }

            // проверяется стоимость аренды на число больше нуля
            if (!decimal.TryParse(txtStoimost.Text, out decimal stoimost) || stoimost <= 0)
            {
                MessageBox.Show("Стоимость аренды должна быть числом больше нуля", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStoimost.Focus();
                return false;
            }

            // проверяется вместимость на целое число больше нуля
            if (!int.TryParse(txtVmestimost.Text, out int vmestimost) || vmestimost <= 0)
            {
                MessageBox.Show("Вместимость должна быть целым числом больше нуля", "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtVmestimost.Focus();
                return false;
            }

            return true;
        }

        // данные площадки сохраняются в базу
        private void btnSohranit_Click(object sender, RoutedEventArgs e)
        {
            if (!proveritDannye())
                return;

            try
            {
                // значения элементов интерфейса переносятся в объект модели
                tekushayaPloshadka.Адрес = txtAdres.Text.Trim();
                tekushayaPloshadka.Тип = (int)cbTipPloshadki.SelectedValue;
                tekushayaPloshadka.Стоимость_аренды_в_день = decimal.Parse(txtStoimost.Text);
                tekushayaPloshadka.Вместимость = int.Parse(txtVmestimost.Text);

                if (!etoRedaktirovanie)
                {
                    // новый первичный ключ рассчитывается при создании
                    int noviyKod = 1;
                    if (bd.Площадка.Any())
                    {
                        noviyKod = bd.Площадка.Max(p => p.Код_площадки) + 1;
                    }
                    tekushayaPloshadka.Код_площадки = noviyKod;

                    bd.Площадка.Add(tekushayaPloshadka);
                }

                // коммит транзакции в бд
                bd.SaveChanges();
                MessageBox.Show("Данные площадки успешно сохранены", "Успех",
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