using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class SupplierForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Supplier? _supplier;
        private TextBox? _txtCompanyName;
        private TextBox? _txtContactPerson;
        private MaskedTextBox? _txtPhone;
        private TextBox? _txtEmail;
        private TextBox? _txtAddress;
        private TextBox? _txtCity;
        private TextBox? _txtCountry;

        public SupplierForm(DatabaseHelper dbHelper, Supplier? supplier = null)
        {
            _dbHelper = dbHelper;
            _supplier = supplier;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _supplier == null ? "Добавить поставщика" : "Редактировать поставщика";
            this.Size = new Size(550, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 9
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Название компании
            panel.Controls.Add(new Label { Text = "Название компании:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtCompanyName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtCompanyName, 1, row++);

            // Контактное лицо
            panel.Controls.Add(new Label { Text = "Контактное лицо:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtContactPerson = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtContactPerson, 1, row++);

            // Телефон
            panel.Controls.Add(new Label { Text = "Телефон:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtPhone = new MaskedTextBox 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(3),
                Mask = "+7 (000) 000-00-00"
            };
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

            // Страна
            panel.Controls.Add(new Label { Text = "Страна:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtCountry = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3), Text = "Россия" };
            panel.Controls.Add(_txtCountry, 1, row++);

            // Кнопки
            var btnPanel = new Panel { Dock = DockStyle.Fill, Height = 50 };
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right,
                Location = new Point(350, 10),
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
                Location = new Point(460, 10),
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
            if (_supplier != null)
            {
                _txtCompanyName!.Text = _supplier.CompanyName;
                _txtContactPerson!.Text = _supplier.ContactPerson;
                if (!string.IsNullOrEmpty(_supplier.Phone))
                {
                    var phone = _supplier.Phone;
                    if (!phone.StartsWith("+7"))
                    {
                        phone = "+7 " + phone;
                    }
                    _txtPhone!.Text = phone;
                }
                _txtEmail!.Text = _supplier.Email;
                _txtAddress!.Text = _supplier.Address;
                _txtCity!.Text = _supplier.City;
                _txtCountry!.Text = _supplier.Country;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtCompanyName?.Text))
            {
                MessageBox.Show("Введите название компании!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var supplier = new Supplier
            {
                SupplierID = _supplier?.SupplierID ?? 0,
                CompanyName = _txtCompanyName!.Text,
                ContactPerson = _txtContactPerson?.Text ?? "",
                Phone = _txtPhone?.Text?.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "") ?? "",
                Email = _txtEmail?.Text ?? "",
                Address = _txtAddress?.Text ?? "",
                City = _txtCity?.Text ?? "",
                Country = _txtCountry?.Text ?? "Россия"
            };

            bool success;
            if (_supplier == null)
            {
                success = _dbHelper.AddSupplier(supplier);
            }
            else
            {
                success = _dbHelper.UpdateSupplier(supplier);
            }

            if (success)
            {
                MessageBox.Show(_supplier == null ? "Поставщик успешно добавлен!" : "Поставщик успешно обновлен!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении поставщика!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


