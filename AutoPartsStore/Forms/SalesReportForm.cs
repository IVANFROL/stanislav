using AutoPartsStore.Database;

namespace AutoPartsStore.Forms
{
    public partial class SalesReportForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private DataGridView? _dgvSales;
        private DateTimePicker? _dtpFrom;
        private DateTimePicker? _dtpTo;
        private Label? _lblTotal;

        public SalesReportForm(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            InitializeComponent();
            LoadReport();
        }

        private void InitializeComponent()
        {
            this.Text = "Отчет по продажам";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            var mainPanel = new Panel { Dock = DockStyle.Fill };

            // Панель фильтров
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            var lblFrom = new Label { Text = "С:", Location = new Point(10, 20), Size = new Size(30, 23) };
            _dtpFrom = new DateTimePicker
            {
                Location = new Point(45, 17),
                Size = new Size(150, 27),
                Format = DateTimePickerFormat.Short
            };
            _dtpFrom.Value = DateTime.Now.AddMonths(-1);

            var lblTo = new Label { Text = "По:", Location = new Point(210, 20), Size = new Size(30, 23) };
            _dtpTo = new DateTimePicker
            {
                Location = new Point(245, 17),
                Size = new Size(150, 27),
                Format = DateTimePickerFormat.Short
            };
            _dtpTo.Value = DateTime.Now;

            var btnFilter = new Button
            {
                Text = "Применить фильтр",
                Location = new Point(410, 15),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => LoadReport();

            filterPanel.Controls.AddRange(new Control[] { lblFrom, _dtpFrom, lblTo, _dtpTo, btnFilter });

            // DataGridView
            _dgvSales = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // Панель итогов
            var totalPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            _lblTotal = new Label
            {
                Text = "Общая сумма: 0 руб.",
                Location = new Point(10, 10),
                Size = new Size(300, 23),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            totalPanel.Controls.Add(_lblTotal);

            mainPanel.Controls.Add(_dgvSales);
            mainPanel.Controls.Add(totalPanel);
            mainPanel.Controls.Add(filterPanel);

            this.Controls.Add(mainPanel);
        }

        private void LoadReport()
        {
            var sales = _dbHelper.GetSalesReport(_dtpFrom!.Value, _dtpTo!.Value);
            
            _dgvSales!.DataSource = sales;
            
            // Настройка русских названий столбцов
            if (_dgvSales.Columns.Count > 0)
            {
                _dgvSales.Columns["SaleID"].Visible = false;
                if (_dgvSales.Columns.Contains("SaleNumber"))
                    _dgvSales.Columns["SaleNumber"].HeaderText = "Номер продажи";
                if (_dgvSales.Columns.Contains("SaleDate"))
                    _dgvSales.Columns["SaleDate"].HeaderText = "Дата";
                if (_dgvSales.Columns.Contains("CustomerName"))
                    _dgvSales.Columns["CustomerName"].HeaderText = "Клиент";
                if (_dgvSales.Columns.Contains("TotalAmount"))
                {
                    _dgvSales.Columns["TotalAmount"].HeaderText = "Сумма";
                    _dgvSales.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";
                }
                if (_dgvSales.Columns.Contains("PaymentMethod"))
                    _dgvSales.Columns["PaymentMethod"].HeaderText = "Способ оплаты";
                if (_dgvSales.Columns.Contains("ProductCount"))
                    _dgvSales.Columns["ProductCount"].HeaderText = "Кол-во товаров";
            }

            // Расчет общей суммы
            decimal total = sales.Sum(s => s.TotalAmount);
            _lblTotal!.Text = $"Общая сумма: {total:N2} руб.";
        }
    }
}

