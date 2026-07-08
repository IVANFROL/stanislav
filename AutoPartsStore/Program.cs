using AutoPartsStore.Database;
using AutoPartsStore.Forms;
using AutoPartsStore.Services;

namespace AutoPartsStore
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Настройка приложения
            ApplicationConfiguration.Initialize();

            // Строка подключения к базе данных
            // Измените на вашу строку подключения
            var connectionString = "Server=localhost;Database=AutoPartsStore;Integrated Security=True;TrustServerCertificate=True;";
            
            // Если используется SQL Server Authentication:
            // var connectionString = "Server=localhost;Database=AutoPartsStore;User Id=sa;Password=your_password;TrustServerCertificate=True;";

            // Создание экземпляров сервисов
            var dbHelper = new DatabaseHelper(connectionString);
            var authService = new AuthService(dbHelper);

            // Показываем форму входа
            while (true)
            {
                using (var loginForm = new LoginForm(authService))
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        // Пользователь отменил вход
                        return;
                    }
                }

                // Если авторизация успешна, показываем главную форму
                if (authService.IsAuthenticated)
                {
                    using (var mainForm = new MainForm(dbHelper, authService))
                    {
                        if (mainForm.ShowDialog() != DialogResult.OK)
                        {
                            // Пользователь вышел из системы
                            authService.Logout();
                            continue; // Возвращаемся к форме входа
                        }
                    }
                }
            }
        }
    }
}










