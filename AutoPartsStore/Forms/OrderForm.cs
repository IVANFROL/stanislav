using System.Linq;
using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class OrderForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Order? _order;
        private ComboBox? _cmbCustomer;
        private DateTimePicker? _dtpOrderDate;
        private ComboBox? _cmbStatus;
        private TextBox? _txtNotes;
        private DataGridView? _dgvItems;
        private ComboBox? _cmbProduct;
        private NumericUpDown? _numQuantity;
        private List<OrderItem> _orderItems;

        public OrderForm(DatabaseHelper dbHelper, Order? order = null)
        {
            _dbHelper = dbHelper;
            _order = order;
            _orderItems = order?.Items ?? new List<OrderItem>();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _order == null ? "Создать заказ" : "Редактировать заказ";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Панель информации о заказе
            var infoPanel = new Panel { Dock = DockStyle.Top, Height = 150 };
            var infoTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(5)
            };
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Клиент
            infoTable.Controls.Add(new Label { Text = "Клиент:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _cmbCustomer = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            infoTable.Controls.Add(_cmbCustomer, 1, row++);

            // Дата заказа
            infoTable.Controls.Add(new Label { Text = "Дата заказа:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _dtpOrderDate = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
            infoTable.Controls.Add(_dtpOrderDate, 1, row++);

            // Статус
            infoTable.Controls.Add(new Label { Text = "Статус:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _cmbStatus = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
            _cmbStatus.Items.AddRange(new[] { "Новый", "В обработке", "Выполнен", "Отменен" });
            infoTable.Controls.Add(_cmbStatus, 1, row++);

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
            addTable.SetColumnSpan(_cmbProduct, 1);
            addTable.SetColumnSpan(_numQuantity, 1);

            var btnPanel = new Panel { Dock = DockStyle.Fill };
            btnPanel.Controls.Add(btnAddItem);
            btnAddItem.Dock = DockStyle.Left;
            btnAddItem.Width = 100;
            addTable.Controls.Add(btnPanel, 4, 0);
            addTable.SetColumnSpan(btnPanel, 1);

            addItemPanel.Controls.Add(addTable);

            // DataGridView для позиций заказа
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
                    _orderItems.RemoveAt(index);
                    RefreshItemsGrid();
                }
            };

            // Кнопки сохранения
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
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
            foreach (var customer in customers)
            {
                _cmbCustomer?.Items.Add(new { Text = customer.FullName, Value = customer.CustomerID });
            }
            if (_cmbCustomer != null)
            {
                _cmbCustomer.DisplayMember = "Text";
                _cmbCustomer.ValueMember = "Value";
            }

            // Загрузка товаров
            var products = _dbHelper.GetProducts();
            _cmbProduct?.Items.Clear();
            foreach (var product in products)
            {
                _cmbProduct?.Items.Add(new { Text = $"{product.ProductCode} - {product.ProductName} ({product.Price:N2} руб.)", Value = product });
            }
            if (_cmbProduct != null)
            {
                _cmbProduct.DisplayMember = "Text";
                _cmbProduct.ValueMember = "Value";
            }

            if (_order != null)
            {
                _dtpOrderDate!.Value = _order.OrderDate;
                _cmbStatus!.Text = _order.Status;
                _txtNotes!.Text = _order.Notes;

                for (int i = 0; i < _cmbCustomer!.Items.Count; i++)
                {
                    var item = _cmbCustomer.Items[i];
                    var value = item.GetType().GetProperty("Value")?.GetValue(item);
                    if (value != null && value.Equals(_order.CustomerID))
                    {
                        _cmbCustomer.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                _dtpOrderDate!.Value = DateTime.Now;
                _cmbStatus!.SelectedIndex = 0;
                if (_cmbCustomer!.Items.Count > 0)
                    _cmbCustomer.SelectedIndex = 0;
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
            var unitPrice = product.Price;
            var subtotal = quantity * unitPrice;

            _orderItems.Add(new OrderItem
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
            _dgvItems.DataSource = _orderItems.Select(item => new
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
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_cmbCustomer?.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_orderItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в заказ!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var customerValue = _cmbCustomer.SelectedItem.GetType().GetProperty("Value")?.GetValue(_cmbCustomer.SelectedItem);
            if (customerValue == null) return;

            var order = new Order
            {
                OrderID = _order?.OrderID ?? 0,
                CustomerID = (int)customerValue,
                OrderDate = _dtpOrderDate!.Value,
                Status = _cmbStatus!.Text,
                Notes = _txtNotes?.Text ?? "",
                Items = _orderItems
            };

            try
            {
                if (_order == null)
                {
                    _dbHelper.CreateOrder(order);
                    MessageBox.Show("Заказ успешно создан!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Для обновления нужно добавить метод UpdateOrder
                    MessageBox.Show("Редактирование заказов будет добавлено", "Информация");
                    return;
                }
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

