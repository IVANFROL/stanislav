using System.Linq;
using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class ProductsInStockReportForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private DataGridView? _dgvProducts;
        private ComboBox? _cmbProduct;
        private Button? _btnFilter;
        private Button? _btnClear;

        public ProductsInStockReportForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет: Товары в наличии";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Панель фильтров
            var filterPanel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(240, 240, 240) };
            var filterTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(5)
            };
            filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            filterTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

            var lblProduct = new Label
            {
                Text = "Товар:",
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _cmbProduct = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };

            _btnFilter = new Button
            {
                Text = "Применить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnFilter.FlatAppearance.BorderSize = 0;
            _btnFilter.Click += BtnFilter_Click;

            _btnClear = new Button
            {
                Text = "Очистить",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnClear.FlatAppearance.BorderSize = 0;
            _btnClear.Click += BtnClear_Click;

            filterTable.Controls.Add(lblProduct, 0, 0);
            filterTable.Controls.Add(_cmbProduct, 1, 0);
            filterTable.Controls.Add(_btnFilter, 2, 0);
            filterTable.Controls.Add(_btnClear, 3, 0);

            filterPanel.Controls.Add(filterTable);

            // DataGridView для товаров
            _dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            mainPanel.Controls.Add(_dgvProducts);
            mainPanel.Controls.Add(filterPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            var products = _dbHelper.GetProducts();
            
            // Загрузка товаров в ComboBox
            _cmbProduct?.Items.Clear();
            _cmbProduct?.Items.Add("(Все товары)");
            foreach (var product in products.OrderBy(p => p.ProductName))
            {
                _cmbProduct?.Items.Add($"{product.ProductCode} - {product.ProductName}");
            }
            if (_cmbProduct != null)
            {
                _cmbProduct.SelectedIndex = 0;
            }

            RefreshGrid(products);
        }

        private void RefreshGrid(List<Product> products)
        {
            _dgvProducts!.DataSource = null;
            _dgvProducts.DataSource = products.Where(p => p.StockQuantity > 0).Select(p => new
            {
                p.ProductCode,
                p.ProductName,
                p.CategoryName,
                p.SupplierName,
                p.StockQuantity,
                p.Unit,
                p.Price,
                p.CostPrice,
                StockValue = p.StockQuantity * p.CostPrice
            }).ToList();

            if (_dgvProducts.Columns.Count > 0)
            {
                _dgvProducts.Columns["ProductCode"].HeaderText = "Код товара";
                _dgvProducts.Columns["ProductName"].HeaderText = "Название";
                _dgvProducts.Columns["CategoryName"].HeaderText = "Категория";
                _dgvProducts.Columns["SupplierName"].HeaderText = "Поставщик";
                _dgvProducts.Columns["StockQuantity"].HeaderText = "Количество на складе";
                _dgvProducts.Columns["Unit"].HeaderText = "Единица измерения";
                _dgvProducts.Columns["Price"].HeaderText = "Цена продажи";
                _dgvProducts.Columns["CostPrice"].HeaderText = "Себестоимость";
                _dgvProducts.Columns["StockValue"].HeaderText = "Стоимость на складе";

                _dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N2";
                _dgvProducts.Columns["CostPrice"].DefaultCellStyle.Format = "N2";
                _dgvProducts.Columns["StockValue"].DefaultCellStyle.Format = "N2";
            }
        }

        private void BtnFilter_Click(object? sender, EventArgs e)
        {
            var allProducts = _dbHelper.GetProducts();
            var filteredProducts = allProducts.Where(p => p.StockQuantity > 0).ToList();

            if (_cmbProduct?.SelectedItem != null && _cmbProduct.SelectedIndex > 0)
            {
                var selectedText = _cmbProduct.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedText))
                {
                    filteredProducts = filteredProducts.Where(p =>
                        $"{p.ProductCode} - {p.ProductName}".IndexOf(selectedText, StringComparison.OrdinalIgnoreCase) >= 0
                    ).ToList();
                }
            }

            RefreshGrid(filteredProducts);
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (_cmbProduct != null)
            {
                _cmbProduct.SelectedIndex = 0;
            }
            var allProducts = _dbHelper.GetProducts();
            RefreshGrid(allProducts);
        }
    }
}

