using AutoPartsStore.Database;
using AutoPartsStore.Services;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class MainForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly AuthService _authService;
        private MenuStrip? _menuStrip;
        private StatusStrip? _statusStrip;
        private TabControl? _tabControl;

        public MainForm(DatabaseHelper dbHelper, AuthService authService)
        {
            _dbHelper = dbHelper;
            _authService = authService;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Настройка формы
            this.Text = "Магазин автозапчастей";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Создание меню
            _menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("Файл");
            var exitMenuItem = new ToolStripMenuItem("Выход", null, (s, e) => Application.Exit());
            fileMenu.DropDownItems.Add(exitMenuItem);

            var dataMenu = new ToolStripMenuItem("Данные");
            var refreshMenuItem = new ToolStripMenuItem("Обновить", null, (s, e) => LoadData());
            dataMenu.DropDownItems.Add(refreshMenuItem);

            var adminMenu = new ToolStripMenuItem("Администрирование");
            var usersMenuItem = new ToolStripMenuItem("Пользователи", null, (s, e) => ShowUsersForm());
            adminMenu.DropDownItems.Add(usersMenuItem);
            adminMenu.Visible = _authService.IsAdmin; // Показываем только админу

            var accountMenu = new ToolStripMenuItem("Аккаунт");
            var logoutMenuItem = new ToolStripMenuItem("Выход", null, (s, e) => Logout());
            accountMenu.DropDownItems.Add(logoutMenuItem);

            _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, dataMenu, adminMenu, accountMenu });
            this.MainMenuStrip = _menuStrip;

            // Создание статусной строки
            _statusStrip = new StatusStrip();
            var userLabel = new ToolStripStatusLabel($"Пользователь: {_authService.CurrentUser?.FullName} ({_authService.CurrentUser?.Role})");
            var roleLabel = new ToolStripStatusLabel($"Роль: {(_authService.IsAdmin ? "Администратор" : "Пользователь")}");
            roleLabel.Alignment = ToolStripItemAlignment.Right;
            _statusStrip.Items.AddRange(new ToolStripItem[] { userLabel, roleLabel });

            // Создание вкладок
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };

            // Вкладка "Товары"
            var productsTab = new TabPage("Товары");
            productsTab.Controls.Add(CreateProductsPanel());
            _tabControl.TabPages.Add(productsTab);

            // Вкладка "Заказы"
            var ordersTab = new TabPage("Заказы");
            ordersTab.Controls.Add(CreateOrdersPanel());
            _tabControl.TabPages.Add(ordersTab);

            // Вкладка "Продажи"
            var salesTab = new TabPage("Продажи");
            salesTab.Controls.Add(CreateSalesPanel());
            _tabControl.TabPages.Add(salesTab);

            // Вкладка "Клиенты"
            var customersTab = new TabPage("Клиенты");
            customersTab.Controls.Add(CreateCustomersPanel());
            _tabControl.TabPages.Add(customersTab);

            // Вкладка "Поставщики" (только для админа)
            if (_authService.IsAdmin)
            {
                var suppliersTab = new TabPage("Поставщики");
                suppliersTab.Controls.Add(CreateSuppliersPanel());
                _tabControl.TabPages.Add(suppliersTab);
            }

            // Добавление элементов на форму
            this.Controls.Add(_tabControl);
            this.Controls.Add(_statusStrip);
            this.Controls.Add(_menuStrip);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Panel CreateProductsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = !_authService.IsAdmin,
                AllowUserToAddRows = false
            };

            var btnAdd = new Button
            {
                Text = "Добавить",
                Dock = DockStyle.Top,
                Height = 40,
                Enabled = _authService.IsAdmin,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;

            var btnDelete = new Button
            {
                Text = "Удалить",
                Dock = DockStyle.Top,
                Height = 40,
                Enabled = _authService.IsAdmin,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;

            btnAdd.Click += (s, e) => ShowProductForm(null);
            btnDelete.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    try
                    {
                        var selectedRow = dgv.SelectedRows[0];
                        if (selectedRow.DataBoundItem is Product product)
                        {
                            if (MessageBox.Show($"Удалить товар '{product.ProductName}'?", "Подтверждение", 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_dbHelper.DeleteProduct(product.ProductID))
                                {
                                    MessageBox.Show("Товар успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка при удалении товара. Возможно, товар используется в заказах или продажах.", 
                                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            // Альтернативный способ получения ID
                            var productID = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);
                            if (MessageBox.Show("Удалить товар?", "Подтверждение", 
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_dbHelper.DeleteProduct(productID))
                                {
                                    MessageBox.Show("Товар успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка при удалении товара. Возможно, товар используется в заказах или продажах.", 
                                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите товар для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            dgv.CellDoubleClick += (s, e) =>
            {
                if (_authService.IsAdmin && e.RowIndex >= 0)
                {
                    try
                    {
                        if (dgv.Rows[e.RowIndex].DataBoundItem is Product product)
                        {
                            ShowProductForm(product);
                        }
                        else
                        {
                            var productID = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ProductID"].Value);
                            var productFromDb = _dbHelper.GetProduct(productID);
                            if (productFromDb != null)
                                ShowProductForm(productFromDb);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии товара: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnAdd);

            // Сохраняем ссылку на DataGridView для загрузки данных
            panel.Tag = dgv;

            return panel;
        }

        private Panel CreateOrdersPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            var btnAdd = new Button
            {
                Text = "Создать заказ",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowOrderForm(null);

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnAdd);
            panel.Tag = dgv;

            return panel;
        }

        private Panel CreateSalesPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            var btnAdd = new Button
            {
                Text = "Создать продажу",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowSaleForm(null);

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnAdd);
            panel.Tag = dgv;

            return panel;
        }

        private Panel CreateCustomersPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            var btnAdd = new Button
            {
                Text = "Добавить клиента",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowCustomerForm(null);

            dgv.CellDoubleClick += (s, e) =>
            {
                if (_authService.IsAdmin && e.RowIndex >= 0)
                {
                    try
                    {
                        if (dgv.Rows[e.RowIndex].DataBoundItem is Customer customer)
                        {
                            ShowCustomerForm(customer);
                        }
                        else
                        {
                            var customerID = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["CustomerID"].Value);
                            var customerFromDb = _dbHelper.GetCustomer(customerID);
                            if (customerFromDb != null)
                                ShowCustomerForm(customerFromDb);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии клиента: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnAdd);
            panel.Tag = dgv;

            return panel;
        }

        private Panel CreateSuppliersPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            panel.Controls.Add(dgv);
            panel.Tag = dgv;

            return panel;
        }

        private void LoadData()
        {
            if (_tabControl == null) return;

            foreach (TabPage tab in _tabControl.TabPages)
            {
                if (tab.Controls[0] is Panel panel && panel.Tag is DataGridView dgv)
                {
                    switch (tab.Text)
                    {
                        case "Товары":
                            dgv.DataSource = _dbHelper.GetProducts();
                            break;
                        case "Заказы":
                            dgv.DataSource = _dbHelper.GetOrders();
                            break;
                        case "Продажи":
                            dgv.DataSource = _dbHelper.GetSales();
                            break;
                        case "Клиенты":
                            dgv.DataSource = _dbHelper.GetCustomers();
                            break;
                        case "Поставщики":
                            dgv.DataSource = _dbHelper.GetSuppliers();
                            break;
                    }
                }
            }
        }

        private void ShowProductForm(Product? product)
        {
            using (var form = new ProductForm(_dbHelper, product))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void ShowOrderForm(Order? order)
        {
            // Здесь будет форма создания заказа
            MessageBox.Show("Форма создания заказа будет реализована", "Информация");
        }

        private void ShowSaleForm(Sale? sale)
        {
            // Здесь будет форма создания продажи
            MessageBox.Show("Форма создания продажи будет реализована", "Информация");
        }

        private void ShowCustomerForm(Customer? customer)
        {
            using (var form = new CustomerForm(_dbHelper, customer))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void ShowUsersForm()
        {
            var users = _dbHelper.GetUsers();
            var form = new Form
            {
                Text = "Управление пользователями",
                Size = new Size(600, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                DataSource = users
            };

            form.Controls.Add(dgv);
            form.ShowDialog();
        }

        private void Logout()
        {
            if (MessageBox.Show("Выйти из системы?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _authService.Logout();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
