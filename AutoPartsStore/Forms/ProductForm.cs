using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class ProductForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Product? _product;
        private TextBox? _txtProductCode;
        private TextBox? _txtProductName;
        private ComboBox? _cmbCategory;
        private ComboBox? _cmbSupplier;
        private TextBox? _txtDescription;
        private NumericUpDown? _numPrice;
        private NumericUpDown? _numCostPrice;
        private NumericUpDown? _numStockQuantity;
        private NumericUpDown? _numMinStockLevel;
        private TextBox? _txtUnit;

        public ProductForm(DatabaseHelper dbHelper, Product? product = null)
        {
            _dbHelper = dbHelper;
            _product = product;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _product == null ? "Добавить товар" : "Редактировать товар";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 12
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Код товара
            panel.Controls.Add(new Label { Text = "Код товара:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtProductCode = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3), MaxLength = 50 };
            _txtProductCode.CharacterCasing = CharacterCasing.Upper;
            panel.Controls.Add(_txtProductCode, 1, row++);

            // Название
            panel.Controls.Add(new Label { Text = "Название:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtProductName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtProductName, 1, row++);

            // Категория
            panel.Controls.Add(new Label { Text = "Категория:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _cmbCategory = new ComboBox { Dock = DockStyle.Fill, Margin = new Padding(3), DropDownStyle = ComboBoxStyle.DropDownList };
            panel.Controls.Add(_cmbCategory, 1, row++);

            // Поставщик
            panel.Controls.Add(new Label { Text = "Поставщик:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _cmbSupplier = new ComboBox { Dock = DockStyle.Fill, Margin = new Padding(3), DropDownStyle = ComboBoxStyle.DropDownList };
            panel.Controls.Add(_cmbSupplier, 1, row++);

            // Описание
            panel.Controls.Add(new Label { Text = "Описание:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtDescription = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3), Multiline = true, Height = 60 };
            panel.Controls.Add(_txtDescription, 1, row++);

            // Цена
            panel.Controls.Add(new Label { Text = "Цена:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _numPrice = new NumericUpDown { Dock = DockStyle.Fill, Margin = new Padding(3), DecimalPlaces = 2, Minimum = 0, Maximum = 999999 };
            panel.Controls.Add(_numPrice, 1, row++);

            // Себестоимость
            panel.Controls.Add(new Label { Text = "Себестоимость:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _numCostPrice = new NumericUpDown { Dock = DockStyle.Fill, Margin = new Padding(3), DecimalPlaces = 2, Minimum = 0, Maximum = 999999 };
            panel.Controls.Add(_numCostPrice, 1, row++);

            // Количество на складе
            panel.Controls.Add(new Label { Text = "Количество:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _numStockQuantity = new NumericUpDown { Dock = DockStyle.Fill, Margin = new Padding(3), Minimum = 0, Maximum = 999999 };
            panel.Controls.Add(_numStockQuantity, 1, row++);

            // Минимальный остаток
            panel.Controls.Add(new Label { Text = "Мин. остаток:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _numMinStockLevel = new NumericUpDown { Dock = DockStyle.Fill, Margin = new Padding(3), Minimum = 0, Maximum = 999999 };
            panel.Controls.Add(_numMinStockLevel, 1, row++);

            // Единица измерения
            panel.Controls.Add(new Label { Text = "Единица:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtUnit = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3), Text = "шт" };
            panel.Controls.Add(_txtUnit, 1, row++);

            // Кнопки
            var btnPanel = new Panel { Dock = DockStyle.Fill, Height = 50 };
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right,
                Location = new Point(400, 10),
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
                Location = new Point(510, 10),
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
            // Загрузка категорий
            var categories = _dbHelper.GetCategories();
            _cmbCategory?.Items.Clear();
            foreach (var category in categories)
            {
                _cmbCategory?.Items.Add(new { Text = category.CategoryName, Value = category.CategoryID });
            }
            if (_cmbCategory != null)
            {
                _cmbCategory.DisplayMember = "Text";
                _cmbCategory.ValueMember = "Value";
            }

            // Загрузка поставщиков
            var suppliers = _dbHelper.GetSuppliers();
            _cmbSupplier?.Items.Clear();
            _cmbSupplier?.Items.Add(new { Text = "(не выбран)", Value = (int?)null });
            foreach (var supplier in suppliers)
            {
                _cmbSupplier?.Items.Add(new { Text = supplier.CompanyName, Value = (int?)supplier.SupplierID });
            }
            if (_cmbSupplier != null)
            {
                _cmbSupplier.DisplayMember = "Text";
                _cmbSupplier.ValueMember = "Value";
            }

            // Заполнение данных, если редактирование
            if (_product != null)
            {
                _txtProductCode!.Text = _product.ProductCode;
                _txtProductName!.Text = _product.ProductName;
                _txtDescription!.Text = _product.Description;
                _numPrice!.Value = _product.Price;
                _numCostPrice!.Value = _product.CostPrice;
                _numStockQuantity!.Value = _product.StockQuantity;
                _numMinStockLevel!.Value = _product.MinStockLevel;
                _txtUnit!.Text = _product.Unit;

                // Выбор категории
                for (int i = 0; i < _cmbCategory!.Items.Count; i++)
                {
                    var item = _cmbCategory.Items[i];
                    var value = item.GetType().GetProperty("Value")?.GetValue(item);
                    if (value != null && (int)value == _product.CategoryID)
                    {
                        _cmbCategory.SelectedIndex = i;
                        break;
                    }
                }

                // Выбор поставщика
                if (_product.SupplierID.HasValue)
                {
                    for (int i = 0; i < _cmbSupplier!.Items.Count; i++)
                    {
                        var item = _cmbSupplier.Items[i];
                        var value = item.GetType().GetProperty("Value")?.GetValue(item);
                        if (value != null && value.Equals(_product.SupplierID.Value))
                        {
                            _cmbSupplier.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (_cmbCategory!.Items.Count > 0)
                    _cmbCategory.SelectedIndex = 0;
                if (_cmbSupplier!.Items.Count > 0)
                    _cmbSupplier.SelectedIndex = 0;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtProductCode?.Text))
            {
                MessageBox.Show("Введите код товара!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtProductName?.Text))
            {
                MessageBox.Show("Введите название товара!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_cmbCategory?.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var categoryValue = _cmbCategory.SelectedItem.GetType().GetProperty("Value")?.GetValue(_cmbCategory.SelectedItem);
            if (categoryValue == null)
            {
                MessageBox.Show("Ошибка выбора категории!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var supplierValue = _cmbSupplier?.SelectedItem?.GetType().GetProperty("Value")?.GetValue(_cmbSupplier.SelectedItem);
            int? supplierID = supplierValue as int?;

            var product = new Product
            {
                ProductID = _product?.ProductID ?? 0,
                ProductCode = _txtProductCode!.Text,
                ProductName = _txtProductName!.Text,
                CategoryID = (int)categoryValue,
                SupplierID = supplierID,
                Description = _txtDescription?.Text ?? "",
                Price = _numPrice!.Value,
                CostPrice = _numCostPrice!.Value,
                StockQuantity = (int)_numStockQuantity!.Value,
                MinStockLevel = (int)_numMinStockLevel!.Value,
                Unit = _txtUnit?.Text ?? "шт"
            };

            bool success;
            if (_product == null)
            {
                success = _dbHelper.AddProduct(product);
            }
            else
            {
                success = _dbHelper.UpdateProduct(product);
            }

            if (success)
            {
                MessageBox.Show(_product == null ? "Товар успешно добавлен!" : "Товар успешно обновлен!", 
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении товара!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}



