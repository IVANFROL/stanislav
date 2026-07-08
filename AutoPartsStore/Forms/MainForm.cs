using System.Linq;
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

            var reportsMenu = new ToolStripMenuItem("Отчеты");
            var salesReportMenuItem = new ToolStripMenuItem("Отчет по продажам", null, (s, e) => ShowSalesReport());
            var purchasesReportMenuItem = new ToolStripMenuItem("Отчет по закупкам", null, (s, e) => ShowPurchasesReport());
            var productsInStockMenuItem = new ToolStripMenuItem("Товары в наличии", null, (s, e) => ShowProductsInStockReport());
            reportsMenu.DropDownItems.Add(salesReportMenuItem);
            reportsMenu.DropDownItems.Add(purchasesReportMenuItem);
            reportsMenu.DropDownItems.Add(productsInStockMenuItem);

            var adminMenu = new ToolStripMenuItem("Администрирование");
            var usersMenuItem = new ToolStripMenuItem("Пользователи", null, (s, e) => ShowUsersForm());
            adminMenu.DropDownItems.Add(usersMenuItem);
            adminMenu.Visible = _authService.IsAdmin; // Показываем только админу

            var accountMenu = new ToolStripMenuItem("Аккаунт");
            var logoutMenuItem = new ToolStripMenuItem("Выход", null, (s, e) => Logout());
            accountMenu.DropDownItems.Add(logoutMenuItem);

            _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, dataMenu, reportsMenu, adminMenu, accountMenu });
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

            // Вкладки только для админа
            if (_authService.IsAdmin)
            {
                var suppliersTab = new TabPage("Поставщики");
                suppliersTab.Controls.Add(CreateSuppliersPanel());
                _tabControl.TabPages.Add(suppliersTab);

                var categoriesTab = new TabPage("Категории");
                categoriesTab.Controls.Add(CreateCategoriesPanel());
                _tabControl.TabPages.Add(categoriesTab);

                var deliveriesTab = new TabPage("Поставки");
                deliveriesTab.Controls.Add(CreateDeliveriesPanel());
                _tabControl.TabPages.Add(deliveriesTab);

                var statisticsTab = new TabPage("Статистика");
                statisticsTab.Controls.Add(CreateStatisticsPanel());
                _tabControl.TabPages.Add(statisticsTab);
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
            
            // Кнопки только для админа
            if (_authService.IsAdmin)
            {
                panel.Controls.Add(btnDelete);
                panel.Controls.Add(btnAdd);
            }

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

            var btnDelete = new Button
            {
                Text = "Удалить заказ",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    try
                    {
                        if (dgv.SelectedRows[0].DataBoundItem is Order order)
                        {
                            if (MessageBox.Show($"Удалить заказ '{order.OrderNumber}'?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_dbHelper.DeleteOrder(order.OrderID))
                                {
                                    MessageBox.Show("Заказ успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка при удалении заказа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    try
                    {
                        if (dgv.Rows[e.RowIndex].DataBoundItem is Order order)
                        {
                            ShowOrderDetails(order.OrderID);
                        }
                        else
                        {
                            var orderID = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["OrderID"].Value);
                            ShowOrderDetails(orderID);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnDelete);
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

            var btnDelete = new Button
            {
                Text = "Удалить продажу",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    try
                    {
                        if (dgv.SelectedRows[0].DataBoundItem is Sale sale)
                        {
                            if (MessageBox.Show($"Удалить продажу '{sale.SaleNumber}'?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_dbHelper.DeleteSale(sale.SaleID))
                                {
                                    MessageBox.Show("Продажа успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка при удалении продажи!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    try
                    {
                        if (dgv.Rows[e.RowIndex].DataBoundItem is Sale sale)
                        {
                            ShowSaleDetails(sale.SaleID);
                        }
                        else
                        {
                            var saleID = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["SaleID"].Value);
                            ShowSaleDetails(saleID);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnDelete);
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

            var btnDelete = new Button
            {
                Text = "Удалить клиента",
                Dock = DockStyle.Top,
                Height = 40,
                Enabled = _authService.IsAdmin,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    try
                    {
                        if (dgv.SelectedRows[0].DataBoundItem is Customer customer)
                        {
                            if (MessageBox.Show($"Удалить клиента '{customer.FullName}'?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_dbHelper.DeleteCustomer(customer.CustomerID))
                                {
                                    MessageBox.Show("Клиент успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка при удалении клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

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
            panel.Controls.Add(btnDelete);
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

            var btnAdd = new Button
            {
                Text = "Добавить поставщика",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowSupplierForm(null);

            var btnDelete = new Button
            {
                Text = "Удалить поставщика",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    try
                    {
                        if (dgv.SelectedRows[0].DataBoundItem is Supplier supplier)
                        {
                            if (MessageBox.Show($"Удалить поставщика '{supplier.CompanyName}'?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_dbHelper.DeleteSupplier(supplier.SupplierID))
                                {
                                    MessageBox.Show("Поставщик успешно удален!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка при удалении поставщика. Возможно, он используется в товарах.", 
                                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            dgv.CellDoubleClick += (s, e) =>
            {
                if (_authService.IsAdmin && e.RowIndex >= 0)
                {
                    try
                    {
                        if (dgv.Rows[e.RowIndex].DataBoundItem is Supplier supplier)
                        {
                            ShowSupplierForm(supplier);
                        }
                        else
                        {
                            var supplierID = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["SupplierID"].Value);
                            var supplierFromDb = _dbHelper.GetSupplier(supplierID);
                            if (supplierFromDb != null)
                                ShowSupplierForm(supplierFromDb);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии поставщика: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnAdd);
            panel.Tag = dgv;

            return panel;
        }

        private Panel CreateCategoriesPanel()
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
                Text = "Добавить категорию",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowCategoryForm(null);

            var btnDelete = new Button
            {
                Text = "Удалить",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count > 0)
                {
                    try
                    {
                        if (dgv.SelectedRows[0].DataBoundItem is Category category)
                        {
                            if (MessageBox.Show($"Удалить категорию '{category.CategoryName}'?", "Подтверждение",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                if (_dbHelper.DeleteCategory(category.CategoryID))
                                {
                                    MessageBox.Show("Категория успешно удалена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                }
                                else
                                {
                                    MessageBox.Show("Ошибка при удалении категории. Возможно, она используется в товарах.",
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
            };

            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    try
                    {
                        if (dgv.Rows[e.RowIndex].DataBoundItem is Category category)
                        {
                            ShowCategoryForm(category);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при открытии категории: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            panel.Controls.Add(dgv);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnAdd);
            panel.Tag = dgv;

            return panel;
        }

        private Panel CreateDeliveriesPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    try
                    {
                        var delivery = dgv.Rows[e.RowIndex].DataBoundItem;
                        var deliveryID = delivery?.GetType().GetProperty("DeliveryID")?.GetValue(delivery);
                        if (deliveryID != null)
                        {
                            ShowDeliveryDetails(Convert.ToInt32(deliveryID));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            panel.Controls.Add(dgv);
            panel.Tag = dgv;

            return panel;
        }

        private Panel CreateStatisticsPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var stats = _dbHelper.GetStatistics();

            var lblTitle = new Label
            {
                Text = "Статистика магазина",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            var statsPanel = new TableLayoutPanel
            {
                Location = new Point(10, 50),
                Size = new Size(700, 500),
                ColumnCount = 2,
                RowCount = 15
            };
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350));
            statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;
            
            // Основная информация
            var lblSection1 = new Label 
            { 
                Text = "ОСНОВНАЯ ИНФОРМАЦИЯ", 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(0, 120, 215) 
            };
            statsPanel.Controls.Add(lblSection1, 0, row);
            statsPanel.SetColumnSpan(lblSection1, 2);
            row++;

            AddStatRow(statsPanel, "Всего товаров (шт.):", stats.TotalProducts.ToString(), row++);
            AddStatRow(statsPanel, "Товаров с низким остатком (шт.):", stats.LowStockProducts.ToString(), row++);
            AddStatRow(statsPanel, "Стоимость товаров на складе (руб.):", $"{stats.TotalStockValue:N2} руб.", row++);
            AddStatRow(statsPanel, "Всего клиентов (чел.):", stats.TotalCustomers.ToString(), row++);
            AddStatRow(statsPanel, "Всего поставщиков (компаний):", stats.TotalSuppliers.ToString(), row++);
            AddStatRow(statsPanel, "Всего категорий (шт.):", stats.TotalCategories.ToString(), row++);
            
            row++;
            var lblSection2 = new Label 
            { 
                Text = "ПРОДАЖИ", 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(40, 167, 69) 
            };
            statsPanel.Controls.Add(lblSection2, 0, row);
            statsPanel.SetColumnSpan(lblSection2, 2);
            row++;

            AddStatRow(statsPanel, "Продаж сегодня (транзакций):", stats.TodaySalesCount.ToString(), row++);
            AddStatRow(statsPanel, "Сумма продаж сегодня (руб.):", $"{stats.TodaySalesAmount:N2} руб.", row++);
            AddStatRow(statsPanel, "Продаж за месяц (транзакций):", stats.MonthlySalesCount.ToString(), row++);
            AddStatRow(statsPanel, "Сумма продаж за месяц (руб.):", $"{stats.MonthlySalesAmount:N2} руб.", row++);
            AddStatRow(statsPanel, "Общая сумма всех продаж (руб.):", $"{stats.TotalSalesAmount:N2} руб.", row++);
            AddStatRow(statsPanel, "Средний чек за месяц (руб.):", $"{stats.AverageCheck:N2} руб.", row++);
            
            row++;
            var lblSection3 = new Label 
            { 
                Text = "ЗАКАЗЫ", 
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), 
                ForeColor = Color.FromArgb(255, 193, 7) 
            };
            statsPanel.Controls.Add(lblSection3, 0, row);
            statsPanel.SetColumnSpan(lblSection3, 2);
            row++;

            AddStatRow(statsPanel, "Заказов за месяц (шт.):", stats.MonthlyOrdersCount.ToString(), row++);
            AddStatRow(statsPanel, "Сумма заказов за месяц (руб.):", $"{stats.MonthlyOrdersAmount:N2} руб.", row++);
            AddStatRow(statsPanel, "Общая сумма всех заказов (руб.):", $"{stats.TotalOrdersAmount:N2} руб.", row++);

            var btnRefresh = new Button
            {
                Text = "Обновить статистику",
                Location = new Point(10, 560),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) =>
            {
                var newStats = _dbHelper.GetStatistics();
                UpdateStatisticsPanel(statsPanel, newStats);
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(statsPanel);
            panel.Controls.Add(btnRefresh);

            return panel;
        }

        private void AddStatRow(TableLayoutPanel panel, string label, string value, int row)
        {
            panel.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 10F), Anchor = AnchorStyles.Left }, 0, row);
            panel.Controls.Add(new Label { Text = value, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Anchor = AnchorStyles.Left }, 1, row);
        }

        private void UpdateStatisticsPanel(TableLayoutPanel panel, Database.Statistics stats)
        {
            int row = 1; // Пропускаем заголовок
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.TotalProducts.ToString();
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.LowStockProducts.ToString();
            ((Label)panel.GetControlFromPosition(1, row++)).Text = $"{stats.TotalStockValue:N2} руб.";
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.TotalCustomers.ToString();
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.TotalSuppliers.ToString();
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.TotalCategories.ToString();
            
            row += 2; // Пропускаем заголовок продаж
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.TodaySalesCount.ToString();
            ((Label)panel.GetControlFromPosition(1, row++)).Text = $"{stats.TodaySalesAmount:N2} руб.";
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.MonthlySalesCount.ToString();
            ((Label)panel.GetControlFromPosition(1, row++)).Text = $"{stats.MonthlySalesAmount:N2} руб.";
            ((Label)panel.GetControlFromPosition(1, row++)).Text = $"{stats.TotalSalesAmount:N2} руб.";
            ((Label)panel.GetControlFromPosition(1, row++)).Text = $"{stats.AverageCheck:N2} руб.";
            
            row += 2; // Пропускаем заголовок заказов
            ((Label)panel.GetControlFromPosition(1, row++)).Text = stats.MonthlyOrdersCount.ToString();
            ((Label)panel.GetControlFromPosition(1, row++)).Text = $"{stats.MonthlyOrdersAmount:N2} руб.";
            ((Label)panel.GetControlFromPosition(1, row++)).Text = $"{stats.TotalOrdersAmount:N2} руб.";
        }

        private void ShowCategoryForm(Category? category)
        {
            using (var form = new CategoryForm(_dbHelper, category))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void LoadData()
        {
            if (_tabControl == null) return;

            foreach (TabPage tab in _tabControl.TabPages)
            {
                if (tab.Controls.Count > 0 && tab.Controls[0] is Panel panel)
                {
                    // Обработка панели статистики (не имеет DataGridView)
                    if (tab.Text == "Статистика")
                    {
                        // Статистика обновляется автоматически при открытии
                        continue;
                    }

                    // Обработка панелей с DataGridView
                    if (panel.Tag is DataGridView dgv)
                    {
                        switch (tab.Text)
                        {
                            case "Товары":
                                dgv.DataSource = _dbHelper.GetProducts();
                                SetRussianColumnNames(dgv, "Products");
                                break;
                            case "Заказы":
                                dgv.DataSource = _dbHelper.GetOrders();
                                SetRussianColumnNames(dgv, "Orders");
                                break;
                            case "Продажи":
                                dgv.DataSource = _dbHelper.GetSales();
                                SetRussianColumnNames(dgv, "Sales");
                                break;
                            case "Клиенты":
                                dgv.DataSource = _dbHelper.GetCustomers();
                                SetRussianColumnNames(dgv, "Customers");
                                break;
                            case "Поставщики":
                                dgv.DataSource = _dbHelper.GetSuppliers();
                                SetRussianColumnNames(dgv, "Suppliers");
                                break;
                            case "Категории":
                                dgv.DataSource = _dbHelper.GetCategories();
                                SetRussianColumnNames(dgv, "Categories");
                                break;
                            case "Поставки":
                                dgv.DataSource = _dbHelper.GetDeliveries();
                                SetRussianColumnNames(dgv, "Deliveries");
                                break;
                        }
                    }
                }
            }
        }

        private void SetRussianColumnNames(DataGridView dgv, string tableType)
        {
            if (dgv.Columns.Count == 0) return;

            // Скрываем ID столбцы
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Name.EndsWith("ID"))
                    col.Visible = false;
            }

            // Устанавливаем русские названия в зависимости от типа таблицы
            switch (tableType)
            {
                case "Products":
                    SetColumnHeader(dgv, "ProductCode", "Код товара");
                    SetColumnHeader(dgv, "ProductName", "Название");
                    SetColumnHeader(dgv, "CategoryName", "Категория");
                    SetColumnHeader(dgv, "SupplierName", "Поставщик");
                    SetColumnHeader(dgv, "Description", "Описание");
                    SetColumnHeader(dgv, "Price", "Цена");
                    SetColumnHeader(dgv, "CostPrice", "Себестоимость");
                    SetColumnHeader(dgv, "StockQuantity", "Остаток");
                    SetColumnHeader(dgv, "MinStockLevel", "Мин. остаток");
                    SetColumnHeader(dgv, "Unit", "Ед. изм.");
                    SetColumnHeader(dgv, "CreatedDate", "Дата создания");
                    SetColumnHeader(dgv, "LastModifiedDate", "Дата изменения");
                    SetColumnFormat(dgv, "Price", "N2");
                    SetColumnFormat(dgv, "CostPrice", "N2");
                    break;

                case "Orders":
                    SetColumnHeader(dgv, "OrderNumber", "Номер заказа");
                    SetColumnHeader(dgv, "CustomerName", "Клиент");
                    SetColumnHeader(dgv, "OrderDate", "Дата");
                    SetColumnHeader(dgv, "Status", "Статус");
                    SetColumnHeader(dgv, "TotalAmount", "Сумма");
                    SetColumnHeader(dgv, "Notes", "Примечания");
                    SetColumnFormat(dgv, "TotalAmount", "N2");
                    break;

                case "Sales":
                    SetColumnHeader(dgv, "SaleNumber", "Номер продажи");
                    SetColumnHeader(dgv, "CustomerName", "Клиент");
                    SetColumnHeader(dgv, "SaleDate", "Дата");
                    SetColumnHeader(dgv, "TotalAmount", "Сумма");
                    SetColumnHeader(dgv, "PaymentMethod", "Способ оплаты");
                    SetColumnHeader(dgv, "Notes", "Примечания");
                    SetColumnFormat(dgv, "TotalAmount", "N2");
                    break;

                case "Customers":
                    SetColumnHeader(dgv, "FirstName", "Имя");
                    SetColumnHeader(dgv, "LastName", "Фамилия");
                    SetColumnHeader(dgv, "Phone", "Телефон");
                    SetColumnHeader(dgv, "Email", "Email");
                    SetColumnHeader(dgv, "Address", "Адрес");
                    SetColumnHeader(dgv, "City", "Город");
                    SetColumnHeader(dgv, "RegistrationDate", "Дата регистрации");
                    SetColumnHeader(dgv, "IsActive", "Активен");
                    break;

                case "Suppliers":
                    SetColumnHeader(dgv, "CompanyName", "Название компании");
                    SetColumnHeader(dgv, "ContactPerson", "Контактное лицо");
                    SetColumnHeader(dgv, "Phone", "Телефон");
                    SetColumnHeader(dgv, "Email", "Email");
                    SetColumnHeader(dgv, "Address", "Адрес");
                    SetColumnHeader(dgv, "City", "Город");
                    SetColumnHeader(dgv, "Country", "Страна");
                    SetColumnHeader(dgv, "CreatedDate", "Дата создания");
                    break;

                case "Categories":
                    SetColumnHeader(dgv, "CategoryName", "Название");
                    SetColumnHeader(dgv, "Description", "Описание");
                    break;

                case "Deliveries":
                    SetColumnHeader(dgv, "DeliveryNumber", "Номер поставки");
                    SetColumnHeader(dgv, "SupplierName", "Поставщик");
                    SetColumnHeader(dgv, "DeliveryDate", "Дата");
                    SetColumnHeader(dgv, "TotalAmount", "Сумма");
                    SetColumnHeader(dgv, "Status", "Статус");
                    SetColumnHeader(dgv, "Notes", "Примечания");
                    SetColumnFormat(dgv, "TotalAmount", "N2");
                    break;
            }
        }

        private void SetColumnHeader(DataGridView dgv, string columnName, string headerText)
        {
            if (dgv.Columns.Contains(columnName))
            {
                dgv.Columns[columnName].HeaderText = headerText;
            }
        }

        private void SetColumnFormat(DataGridView dgv, string columnName, string format)
        {
            if (dgv.Columns.Contains(columnName))
            {
                dgv.Columns[columnName].DefaultCellStyle.Format = format;
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
            using (var form = new OrderForm(_dbHelper, order))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void ShowSaleForm(Sale? sale)
        {
            using (var form = new SaleForm(_dbHelper, sale))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
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

        private void ShowSupplierForm(Supplier? supplier)
        {
            using (var form = new SupplierForm(_dbHelper, supplier))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void ShowSalesReport()
        {
            using (var form = new SalesReportForm(_dbHelper))
            {
                form.ShowDialog();
            }
        }

        private void ShowPurchasesReport()
        {
            using (var form = new PurchasesReportForm(_dbHelper))
            {
                form.ShowDialog();
            }
        }

        private void ShowProductsInStockReport()
        {
            using (var form = new ProductsInStockReportForm(_dbHelper))
            {
                form.ShowDialog();
            }
        }

        private void ShowOrderDetails(int orderID)
        {
            var items = _dbHelper.GetOrderItems(orderID);
            if (items.Count == 0)
            {
                MessageBox.Show("В заказе нет позиций", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var form = new Form
            {
                Text = $"Детали заказа #{orderID}",
                Size = new Size(800, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                DataSource = items
            };

            SetColumnHeader(dgv, "ProductCode", "Код товара");
            SetColumnHeader(dgv, "ProductName", "Название");
            SetColumnHeader(dgv, "Quantity", "Количество");
            SetColumnHeader(dgv, "UnitPrice", "Цена");
            SetColumnHeader(dgv, "Subtotal", "Сумма");
            SetColumnFormat(dgv, "UnitPrice", "N2");
            SetColumnFormat(dgv, "Subtotal", "N2");

            form.Controls.Add(dgv);
            form.ShowDialog();
        }

        private void ShowSaleDetails(int saleID)
        {
            var items = _dbHelper.GetSaleItems(saleID);
            if (items.Count == 0)
            {
                MessageBox.Show("В продаже нет позиций", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var form = new Form
            {
                Text = $"Детали продажи #{saleID}",
                Size = new Size(800, 400),
                StartPosition = FormStartPosition.CenterParent
            };

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                DataSource = items
            };

            SetColumnHeader(dgv, "ProductCode", "Код товара");
            SetColumnHeader(dgv, "ProductName", "Название");
            SetColumnHeader(dgv, "Quantity", "Количество");
            SetColumnHeader(dgv, "UnitPrice", "Цена");
            SetColumnHeader(dgv, "Subtotal", "Сумма");
            SetColumnFormat(dgv, "UnitPrice", "N2");
            SetColumnFormat(dgv, "Subtotal", "N2");

            form.Controls.Add(dgv);
            form.ShowDialog();
        }

        private void ShowDeliveryDetails(int deliveryID)
        {
            var deliveries = _dbHelper.GetDeliveries();
            var delivery = deliveries.FirstOrDefault(d => d.DeliveryID == deliveryID);
            if (delivery == null) return;

            var deliveryItems = _dbHelper.GetDeliveryItems(deliveryID);

            var form = new Form
            {
                Text = $"Детали поставки {delivery.DeliveryNumber}",
                Size = new Size(900, 600),
                StartPosition = FormStartPosition.CenterParent
            };

            // Панель с кнопками
            var buttonsPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            var btnAdd = new Button
            {
                Text = "Добавить товар",
                Location = new Point(10, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Enabled = _authService.IsAdmin;
            btnAdd.Click += (s, e) =>
            {
                MessageBox.Show("Функция добавления товаров в поставку будет реализована", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var btnDelete = new Button
            {
                Text = "Удалить товар",
                Location = new Point(140, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Enabled = _authService.IsAdmin;
            btnDelete.Click += (s, e) =>
            {
                MessageBox.Show("Функция удаления товаров из поставки будет реализована", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            buttonsPanel.Controls.Add(btnAdd);
            buttonsPanel.Controls.Add(btnDelete);

            // Панель информации
            var infoPanel = new Panel { Dock = DockStyle.Top, Height = 150, Padding = new Padding(10) };
            var info = new Label
            {
                Text = $"Номер: {delivery.DeliveryNumber}\n" +
                       $"Поставщик: {delivery.SupplierName}\n" +
                       $"Дата: {delivery.DeliveryDate:dd.MM.yyyy}\n" +
                       $"Сумма: {delivery.TotalAmount:N2} руб.\n" +
                       $"Статус: {delivery.Status}",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F)
            };
            infoPanel.Controls.Add(info);

            // DataGridView для товаров
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgv.DataSource = deliveryItems.Select(item => new
            {
                item.ProductCode,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Subtotal
            }).ToList();

            if (dgv.Columns.Count > 0)
            {
                dgv.Columns["ProductCode"].HeaderText = "Код товара";
                dgv.Columns["ProductName"].HeaderText = "Название";
                dgv.Columns["Quantity"].HeaderText = "Количество";
                dgv.Columns["UnitPrice"].HeaderText = "Цена за единицу";
                dgv.Columns["Subtotal"].HeaderText = "Сумма";
                dgv.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
                dgv.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
            }

            form.Controls.Add(dgv);
            form.Controls.Add(buttonsPanel);
            form.Controls.Add(infoPanel);
            form.ShowDialog();
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
