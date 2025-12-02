using AutoPartsStore.Database;
using AutoPartsStore.Services;

namespace AutoPartsStore.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        public LoginForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы
            this.Text = "Авторизация - Магазин автозапчастей";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Метка "Логин"
            var lblUsername = new Label
            {
                Text = "Логин:",
                Location = new Point(30, 30),
                Size = new Size(100, 23),
                Font = new Font("Segoe UI", 10F)
            };

            // Поле ввода логина
            var txtUsername = new TextBox
            {
                Location = new Point(130, 27),
                Size = new Size(220, 27),
                Font = new Font("Segoe UI", 10F)
            };

            // Метка "Пароль"
            var lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(30, 70),
                Size = new Size(100, 23),
                Font = new Font("Segoe UI", 10F)
            };

            // Поле ввода пароля
            var txtPassword = new TextBox
            {
                Location = new Point(130, 67),
                Size = new Size(220, 27),
                Font = new Font("Segoe UI", 10F),
                PasswordChar = '*',
                UseSystemPasswordChar = true
            };

            // Кнопка "Войти"
            var btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(130, 120),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;

            // Кнопка "Отмена"
            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(250, 120),
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F),
                DialogResult = DialogResult.Cancel
            };

            // Метка с информацией
            var lblInfo = new Label
            {
                Text = "Админ: admin / admin123\nПользователь: user / user123",
                Location = new Point(30, 170),
                Size = new Size(320, 40),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.Gray
            };

            // Обработчик нажатия Enter в поле пароля
            txtPassword.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnLogin.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            // Обработчик кнопки "Войти"
            btnLogin.Click += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (_authService.Login(txtUsername.Text, txtPassword.Text))
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка авторизации", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            };

            // Добавление элементов на форму
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnCancel);
            this.Controls.Add(lblInfo);

            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;

            this.ResumeLayout(false);
        }
    }
}


