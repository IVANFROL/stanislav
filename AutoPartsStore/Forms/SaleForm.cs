using System.Linq;
using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class SaleForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Sale? _sale;
        private ComboBox? _cmbCustomer;
        private ComboBox? _cmbOrder;
        private DateTimePicker? _dtpSaleDate;
        private ComboBox? _cmbPaymentMethod;
        private TextBox? _txtNotes;
        private DataGridView? _dgvItems;
        private ComboBox? _cmbProduct;
        private NumericUpDown? _numQuantity;
        private Label? _lblTotal;
        private List<SaleItem> _saleItems;

        public SaleForm(DatabaseHelper dbHelper, Sale? sale = null)
        {
            _dbHelper = dbHelper;
            _sale = sale;
            _saleItems = sale?.Items ?? new List<SaleItem>();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _sale == null ? "Создать продажу" : "Редактировать продажу";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Панель информации о продаже
            var infoPanel = new Panel { Dock = DockStyle.Top, Height = 180 };
            var infoTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(5)
            };
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Клиент
            infoTable.Controls.Add(new Label { Text = "Клиент:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _cmbCustomer = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            infoTable.Controls.Add(_cmbCustomer, 1, row++);

            // Заказ (опционально)
            infoTable.Controls.Add(new Label { Text = "Заказ (опц.):", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _cmbOrder = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            infoTable.Controls.Add(_cmbOrder, 1, row++);

            // Дата продажи
            infoTable.Controls.Add(new Label { Text = "Дата продажи:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _dtpSaleDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            infoTable.Controls.Add(_dtpSaleDate, 1, row++);

            // Способ оплаты
            infoTable.Controls.Add(new Label { Text = "Способ оплаты:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _cmbPaymentMethod = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
            _cmbPaymentMethod.Items.AddRange(new[] { "Наличные", "Банковская карта", "Безналичный расчет", "Другое" });
            infoTable.Controls.Add(_cmbPaymentMethod, 1, row++);

            // Примечания
            infoTable.Controls.Add(new Label { Text = "Примечания:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 40 };
            infoTable.Controls.Add(_txtNotes, 1, row++);

            infoPanel.Controls.Add(infoTable);

            // Панель добавления товаров
            var addItemPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(240, 240, 240) };
            var addTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(5)
            };
            addTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            addTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            addTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            addTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            _cmbProduct = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _numQuantity = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 9999, Value = 1 };
            var btnAddItem = new Button
            {
                Text = "Добавить",
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddItem.FlatAppearance.BorderSize = 0;
            btnAddItem.Click += BtnAddItem_Click;

            addTable.Controls.Add(new Label { Text = "Товар:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, 0);
            addTable.Controls.Add(_cmbProduct, 1, 0);
            addTable.Controls.Add(new Label { Text = "Кол-во:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 2, 0);
            addTable.Controls.Add(_numQuantity, 3, 0);

            var btnPanel = new Panel { Dock = DockStyle.Fill };
            btnPanel.Controls.Add(btnAddItem);
            btnAddItem.Dock = DockStyle.Left;
            btnAddItem.Width = 100;
            addTable.Controls.Add(btnPanel, 4, 0);

            addItemPanel.Controls.Add(addTable);

            // DataGridView для позиций продажи
            _dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            var btnDeleteItem = new Button
            {
                Text = "Удалить позицию",
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteItem.FlatAppearance.BorderSize = 0;
            btnDeleteItem.Click += (s, e) =>
            {
                if (_dgvItems.SelectedRows.Count > 0)
                {
                    var index = _dgvItems.SelectedRows[0].Index;
                    _saleItems.RemoveAt(index);
                    RefreshItemsGrid();
                }
            };

            // Панель итогов
            var totalPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(240, 240, 240) };
            _lblTotal = new Label
            {
                Text = "Итого: 0 руб.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(10, 0, 10, 0)
            };
            totalPanel.Controls.Add(_lblTotal);

            // Кнопки сохранения
            var btnSave = new Button
            {
                Text = "✓ Сохранить продажу",
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Dock = DockStyle.Bottom,
                Height = 40
            };

            mainPanel.Controls.Add(btnCancel);
            mainPanel.Controls.Add(btnSave);
            mainPanel.Controls.Add(totalPanel);
            mainPanel.Controls.Add(_dgvItems);
            mainPanel.Controls.Add(btnDeleteItem);
            mainPanel.Controls.Add(addItemPanel);
            mainPanel.Controls.Add(infoPanel);

            this.Controls.Add(mainPanel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadData()
        {
            // Загрузка клиентов
            var customers = _dbHelper.GetCustomers();
            _cmbCustomer?.Items.Clear();
            _cmbCustomer?.Items.Add(new { Text = "(не выбран)", Value = (int?)null });
            foreach (var customer in customers)
            {
                _cmbCustomer?.Items.Add(new { Text = customer.FullName, Value = (int?)customer.CustomerID });
            }
            if (_cmbCustomer != null)
            {
                _cmbCustomer.DisplayMember = "Text";
                _cmbCustomer.ValueMember = "Value";
            }

            // Загрузка заказов
            var orders = _dbHelper.GetOrders();
            _cmbOrder?.Items.Clear();
            _cmbOrder?.Items.Add(new { Text = "(не выбран)", Value = (int?)null });
            foreach (var order in orders)
            {
                _cmbOrder?.Items.Add(new { Text = $"{order.OrderNumber} - {order.CustomerName}", Value = (int?)order.OrderID });
            }
            if (_cmbOrder != null)
            {
                _cmbOrder.DisplayMember = "Text";
                _cmbOrder.ValueMember = "Value";
            }

            // Загрузка товаров
            var products = _dbHelper.GetProducts();
            _cmbProduct?.Items.Clear();
            foreach (var product in products.Where(p => p.StockQuantity > 0))
            {
                _cmbProduct?.Items.Add(new { Text = $"{product.ProductCode} - {product.ProductName} (Остаток: {product.StockQuantity}, Цена: {product.Price:N2} руб.)", Value = product });
            }
            if (_cmbProduct != null)
            {
                _cmbProduct.DisplayMember = "Text";
                _cmbProduct.ValueMember = "Value";
            }

            if (_sale != null)
            {
                _dtpSaleDate!.Value = _sale.SaleDate;
                _cmbPaymentMethod!.Text = _sale.PaymentMethod;
                _txtNotes!.Text = _sale.Notes;

                if (_sale.CustomerID.HasValue)
                {
                    for (int i = 0; i < _cmbCustomer!.Items.Count; i++)
                    {
                        var item = _cmbCustomer.Items[i];
                        var value = item.GetType().GetProperty("Value")?.GetValue(item);
                        if (value != null && value.Equals(_sale.CustomerID.Value))
                        {
                            _cmbCustomer.SelectedIndex = i;
                            break;
                        }
                    }
                }

                if (_sale.OrderID.HasValue)
                {
                    for (int i = 0; i < _cmbOrder!.Items.Count; i++)
                    {
                        var item = _cmbOrder.Items[i];
                        var value = item.GetType().GetProperty("Value")?.GetValue(item);
                        if (value != null && value.Equals(_sale.OrderID.Value))
                        {
                            _cmbOrder.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                _dtpSaleDate!.Value = DateTime.Now;
                _cmbPaymentMethod!.SelectedIndex = 0;
                if (_cmbCustomer!.Items.Count > 0)
                    _cmbCustomer.SelectedIndex = 0;
                if (_cmbOrder!.Items.Count > 0)
                    _cmbOrder.SelectedIndex = 0;
            }

            RefreshItemsGrid();
        }

        private void BtnAddItem_Click(object? sender, EventArgs e)
        {
            if (_cmbProduct?.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var product = _cmbProduct.SelectedItem.GetType().GetProperty("Value")?.GetValue(_cmbProduct.SelectedItem) as Product;
            if (product == null) return;

            var quantity = (int)_numQuantity!.Value;
            
            if (quantity > product.StockQuantity)
            {
                MessageBox.Show($"Недостаточно товара на складе! Доступно: {product.StockQuantity}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var unitPrice = product.Price;
            var subtotal = quantity * unitPrice;

            _saleItems.Add(new SaleItem
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal
            });

            RefreshItemsGrid();
            _cmbProduct.SelectedIndex = -1;
            _numQuantity.Value = 1;
        }

        private void RefreshItemsGrid()
        {
            _dgvItems!.DataSource = null;
            _dgvItems.DataSource = _saleItems.Select(item => new
            {
                item.ProductCode,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Subtotal
            }).ToList();

            if (_dgvItems.Columns.Count > 0)
            {
                _dgvItems.Columns["ProductCode"].HeaderText = "Код";
                _dgvItems.Columns["ProductName"].HeaderText = "Название";
                _dgvItems.Columns["Quantity"].HeaderText = "Количество";
                _dgvItems.Columns["UnitPrice"].HeaderText = "Цена";
                _dgvItems.Columns["Subtotal"].HeaderText = "Сумма";
                _dgvItems.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
                _dgvItems.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
            }

            var total = _saleItems.Sum(item => item.Subtotal);
            _lblTotal!.Text = $"Итого: {total:N2} руб.";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_saleItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в продажу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customerValue = _cmbCustomer?.SelectedItem?.GetType().GetProperty("Value")?.GetValue(_cmbCustomer.SelectedItem);
            var orderValue = _cmbOrder?.SelectedItem?.GetType().GetProperty("Value")?.GetValue(_cmbOrder.SelectedItem);

            var sale = new Sale
            {
                SaleID = _sale?.SaleID ?? 0,
                CustomerID = customerValue as int?,
                OrderID = orderValue as int?,
                SaleDate = _dtpSaleDate!.Value,
                PaymentMethod = _cmbPaymentMethod?.Text ?? "",
                Notes = _txtNotes?.Text ?? "",
                TotalAmount = _saleItems.Sum(item => item.Subtotal),
                Items = _saleItems
            };

            try
            {
                if (_sale == null)
                {
                    _dbHelper.CreateSale(sale);
                    MessageBox.Show("Продажа успешно создана!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Редактирование продаж будет добавлено", "Информация");
                    return;
                }
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении продажи: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

