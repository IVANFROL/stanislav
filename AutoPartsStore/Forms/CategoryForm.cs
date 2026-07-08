using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Forms
{
    public partial class CategoryForm : Form
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly Category? _category;
        private TextBox? _txtCategoryName;
        private TextBox? _txtDescription;

        public CategoryForm(DatabaseHelper dbHelper, Category? category = null)
        {
            _dbHelper = dbHelper;
            _category = category;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _category == null ? "Добавить категорию" : "Редактировать категорию";
            this.Size = new Size(500, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 2,
                RowCount = 4
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            // Название категории
            panel.Controls.Add(new Label { Text = "Название:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtCategoryName = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3) };
            panel.Controls.Add(_txtCategoryName, 1, row++);

            // Описание
            panel.Controls.Add(new Label { Text = "Описание:", Anchor = AnchorStyles.Left | AnchorStyles.Right }, 0, row);
            _txtDescription = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3), Multiline = true, Height = 60 };
            panel.Controls.Add(_txtDescription, 1, row++);

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
            if (_category != null)
            {
                _txtCategoryName!.Text = _category.CategoryName;
                _txtDescription!.Text = _category.Description;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtCategoryName?.Text))
            {
                MessageBox.Show("Введите название категории!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var category = new Category
            {
                CategoryID = _category?.CategoryID ?? 0,
                CategoryName = _txtCategoryName!.Text,
                Description = _txtDescription?.Text ?? ""
            };

            bool success;
            if (_category == null)
            {
                success = _dbHelper.AddCategory(category);
            }
            else
            {
                success = _dbHelper.UpdateCategory(category);
            }

            if (success)
            {
                MessageBox.Show(_category == null ? "Категория успешно добавлена!" : "Категория успешно обновлена!",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении категории!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

