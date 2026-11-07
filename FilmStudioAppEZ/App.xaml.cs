using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FilmStudioAppEZ
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        // имя текущего пользователя
        public static string imyaUsera { get; set; } = "Гость";
        // роль текущего пользователя
        public static string rolUsera { get; set; } = "";
    }
}
