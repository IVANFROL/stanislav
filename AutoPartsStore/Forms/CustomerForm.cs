using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class CustomerForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Customer? _customer;
        private TextBox? _txtFirstName;
        private TextBox? _txtLastName;
        private TextBox? _txtPhone;
        private TextBox? _txtEmail;
        private TextBox? _txtAddress;
        private TextBox? _txtCity;

        public CustomerForm(DatabaseHelper dbHelper, Customer? customer = null)
        {
            _dbHelper = dbHelper;
            _customer = customer;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _customer == null ? "Добавить клиента" : "Редактировать клиента";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 8
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Имя
            panel.Controls.Add(new Label { Text = "Имя:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtFirstName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtFirstName, 1, row++);

            // Фамилия
            panel.Controls.Add(new Label { Text = "Фамилия:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtLastName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtLastName, 1, row++);

            // Телефон
            panel.Controls.Add(new Label { Text = "Телефон:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtPhone = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtPhone, 1, row++);

            // Email
            panel.Controls.Add(new Label { Text = "Email:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtEmail = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtEmail, 1, row++);

            // Адрес
            panel.Controls.Add(new Label { Text = "Адрес:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtAddress = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtAddress, 1, row++);

            // Город
            panel.Controls.Add(new Label { Text = "Город:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtCity = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtCity, 1, row++);

            // Кнопки
            var btnPanel = new Panel { Dock = DockStyle.Fill, Height = 50 };
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right,
                Location = new Point(300, 10),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Right,
                Location = new Point(410, 10),
                Size = new Size(100, 35)
            };

            btnSave.Click += BtnSave_Click;
            btnPanel.Controls.Add(btnSave);
            btnPanel.Controls.Add(btnCancel);

            panel.Controls.Add(btnPanel, 0, row);
            panel.SetColumnSpan(btnPanel, 2);

            this.Controls.Add(panel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            if (_customer != null)
            {
                _txtFirstName!.Text = _customer.FirstName;
                _txtLastName!.Text = _customer.LastName;
                _txtPhone!.Text = _customer.Phone;
                _txtEmail!.Text = _customer.Email;
                _txtAddress!.Text = _customer.Address;
                _txtCity!.Text = _customer.City;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtFirstName?.Text))
            {
                MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtLastName?.Text))
            {
                MessageBox.Show("Введите фамилию!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customer = new Customer
            {
                CustomerID = _customer?.CustomerID ?? 0,
                FirstName = _txtFirstName!.Text,
                LastName = _txtLastName!.Text,
                Phone = _txtPhone?.Text ?? "",
                Email = _txtEmail?.Text ?? "",
                Address = _txtAddress?.Text ?? "",
                City = _txtCity?.Text ?? ""
            };

            bool success;
            if (_customer == null)
            {
                success = _dbHelper.AddCustomer(customer);
            }
            else
            {
                success = _dbHelper.UpdateCustomer(customer);
            }

            if (success)
            {
                MessageBox.Show(_customer == null ? "Клиент успешно добавлен!" : "Клиент успешно обновлен!", 
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
