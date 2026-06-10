using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ZoroufApp
{
    public partial class Form2 : Form
    {
        private string connectionString = "Data Source=dishes.db;Mode=ReadWriteCreate;";
        private int currentDishId;
        private string currentDishName;

        private Color colorFormBgDark = Color.FromArgb(18, 18, 18);
        private Color colorPanelBgDark = Color.FromArgb(30, 30, 30);
        private Color colorTextDark = Color.FromArgb(240, 240, 240);

        private Color colorBtnDelete = Color.FromArgb(231, 76, 60);
        private Color colorBtnPrint = Color.FromArgb(155, 89, 182);
        private Color colorBtnTotal = Color.FromArgb(230, 126, 34);
        private Color colorBtnSelAll = Color.FromArgb(39, 174, 96);
        private Color colorBtnAdd = Color.FromArgb(46, 204, 113);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private DataGridView dgvProducts;
        private TextBox txtSearchProduct;
        private Button btnDeleteProduct;
        private Button btnPrintProduct;
        private Button btnTotalPrice;
        private Button btnSelectAll;
        private Label lblTitle;
        private Label lblTotalResult;   // نمایش جمع کل

        private bool isBusy = false;
        private bool allSelected = false;

        public Form2(int dishId, string dishName)
        {
            InitializeComponent();
            this.currentDishId = dishId;
            this.currentDishName = dishName;
            SetupCustomUI();
        }

        // ══════════════════════════════════════════════════════════════════
        //  LOAD
        // ══════════════════════════════════════════════════════════════════
        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text = $"محصولات ظرف: {currentDishName}";
            if (Environment.OSVersion.Version.Major >= 10)
            {
                int dark = 1;
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
            }
            LoadProducts();
        }

        // ══════════════════════════════════════════════════════════════════
        //  UI SETUP
        // ══════════════════════════════════════════════════════════════════
        private void SetupCustomUI()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(700, 520);
            this.BackColor = colorFormBgDark;
            this.ForeColor = colorTextDark;

            // ── عنوان ────────────────────────────────────────────────────
            lblTitle = new Label();
            lblTitle.Text = $"لیست محصولات موجود در: {currentDishName}";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.Location = new Point(20, 15);
            lblTitle.Size = new Size(this.ClientSize.Width - 40, 30);
            lblTitle.TextAlign = ContentAlignment.TopRight;
            lblTitle.RightToLeft = RightToLeft.Yes;
            lblTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;

            // ── جستجو ────────────────────────────────────────────────────
            txtSearchProduct = new TextBox();
            txtSearchProduct.Size = new Size(300, 30);
            txtSearchProduct.Location = new Point(this.ClientSize.Width - 320, 55);
            txtSearchProduct.Font = new Font("Segoe UI", 11);
            txtSearchProduct.BackColor = Color.FromArgb(45, 45, 45);
            txtSearchProduct.ForeColor = Color.White;
            txtSearchProduct.BorderStyle = BorderStyle.FixedSingle;
            txtSearchProduct.RightToLeft = RightToLeft.Yes;
            txtSearchProduct.Text = "جستجو در محصولات...";
            txtSearchProduct.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearchProduct.Enter += (s, e) => { if (txtSearchProduct.Text == "جستجو در محصولات...") txtSearchProduct.Text = ""; };
            txtSearchProduct.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearchProduct.Text)) txtSearchProduct.Text = "جستجو در محصولات..."; };
            txtSearchProduct.TextChanged += (s, e) => { if (txtSearchProduct.Text != "جستجو در محصولات...") LoadProducts(txtSearchProduct.Text); };

            // ── جدول ─────────────────────────────────────────────────────
            dgvProducts = new DataGridView();
            dgvProducts.Location = new Point(20, 100);
            dgvProducts.Size = new Size(this.ClientSize.Width - 40, this.ClientSize.Height - 220);
            dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvProducts.RightToLeft = RightToLeft.Yes;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.BackgroundColor = colorPanelBgDark;
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.GridColor = Color.FromArgb(50, 50, 50);
            dgvProducts.MultiSelect = true;
            dgvProducts.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;

            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 45);
            dgvProducts.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProducts.ColumnHeadersHeight = 35;

            dgvProducts.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
            dgvProducts.DefaultCellStyle.ForeColor = Color.FromArgb(230, 230, 230);
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 120);
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProducts.DefaultCellStyle.Font = new Font("Segoe UI", 10);

            dgvProducts.CellValueChanged += DgvProducts_CellValueChanged;
            dgvProducts.DataError += (s, e) => { e.Cancel = true; };

            // ── Label نمایش جمع کل ───────────────────────────────────────
            lblTotalResult = new Label();
            lblTotalResult.Text = "";
            lblTotalResult.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTotalResult.ForeColor = Color.FromArgb(230, 126, 34);
            lblTotalResult.TextAlign = ContentAlignment.MiddleRight;
            lblTotalResult.RightToLeft = RightToLeft.Yes;
            lblTotalResult.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // ── دکمه‌های پایین ────────────────────────────────────────────
            btnSelectAll = new Button();
            btnSelectAll.Text = "✔ انتخاب همه";
            btnSelectAll.Size = new Size(140, 40);
            btnSelectAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            StyleButton(btnSelectAll, colorBtnSelAll);
            btnSelectAll.Click += BtnSelectAll_Click;

            btnPrintProduct = new Button();
            btnPrintProduct.Text = "🖨️ چاپ انتخابی";
            btnPrintProduct.Size = new Size(140, 40);
            btnPrintProduct.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            StyleButton(btnPrintProduct, colorBtnPrint);
            btnPrintProduct.Click += BtnPrintProduct_Click;

            btnTotalPrice = new Button();
            btnTotalPrice.Text = "🧮 جمع کل";
            btnTotalPrice.Size = new Size(120, 40);
            btnTotalPrice.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            StyleButton(btnTotalPrice, colorBtnTotal);
            btnTotalPrice.Click += BtnTotalPrice_Click;

            btnDeleteProduct = new Button();
            btnDeleteProduct.Text = "حذف";
            btnDeleteProduct.Size = new Size(100, 40);
            btnDeleteProduct.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            StyleButton(btnDeleteProduct, colorBtnDelete);
            btnDeleteProduct.Click += BtnDeleteProduct_Click;

            this.Controls.AddRange(new Control[]
            {
                lblTitle, txtSearchProduct, dgvProducts,
                lblTotalResult,
                btnSelectAll, btnPrintProduct,
                btnTotalPrice, btnDeleteProduct
            });

            PositionBottomControls();
            this.Resize += (s, e) => PositionBottomControls();
            this.Load += Form2_Load;
        }

        // موقعیت‌دهی ریسپانسیو به کنترل‌های پایین
        private void PositionBottomControls()
        {
            int right = this.ClientSize.Width;
            int btnRow = this.ClientSize.Height - 55;   // ردیف دکمه‌ها
            int labelRow = this.ClientSize.Height - 105;  // ردیف Label جمع کل

            // ردیف دکمه‌ها
            btnSelectAll.Location = new Point(20, btnRow);
            btnPrintProduct.Location = new Point(170, btnRow);
            btnDeleteProduct.Location = new Point(right - 120, btnRow);
            btnTotalPrice.Location = new Point(right - 250, btnRow);

            // Label جمع کل — بین دکمه‌های چپ و راست
            lblTotalResult.Location = new Point(20, labelRow);
            lblTotalResult.Size = new Size(right - 40, 35);
        }

        private void StyleButton(Button btn, Color bg)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
        }

        // ══════════════════════════════════════════════════════════════════
        //  LOAD PRODUCTS
        // ══════════════════════════════════════════════════════════════════
        private void LoadProducts(string search = "")
        {
            isBusy = true;
            dgvProducts.CellValueChanged -= DgvProducts_CellValueChanged;
            dgvProducts.Columns.Clear();
            dgvProducts.Rows.Clear();

            // چک‌باکس چاپ
            var chkCol = new DataGridViewCheckBoxColumn();
            chkCol.Name = "SelectToPrint";
            chkCol.HeaderText = "چاپ";
            chkCol.Width = 50;
            chkCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvProducts.Columns.Add(chkCol);

            // Id مخفی
            var colId = new DataGridViewTextBoxColumn();
            colId.Name = "ColId";
            colId.Visible = false;
            colId.ReadOnly = true;
            dgvProducts.Columns.Add(colId);

            // نام محصول
            var colName = new DataGridViewTextBoxColumn();
            colName.Name = "ColName";
            colName.HeaderText = "نام محصول";
            colName.ReadOnly = false;
            dgvProducts.Columns.Add(colName);

            // واحد — فقط عدد یا دسته
            var colUnit = new DataGridViewComboBoxColumn();
            colUnit.Name = "ColUnit";
            colUnit.HeaderText = "واحد";
            colUnit.Width = 80;
            colUnit.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colUnit.ReadOnly = false;
            colUnit.FlatStyle = FlatStyle.Flat;
            colUnit.Items.AddRange(new string[] { "عدد", "دسته" });
            colUnit.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            colUnit.DefaultCellStyle.ForeColor = Color.White;
            dgvProducts.Columns.Add(colUnit);

            // قیمت واحد
            var colPrice = new DataGridViewTextBoxColumn();
            colPrice.Name = "ColPrice";
            colPrice.HeaderText = "قیمت واحد (ت)";
            colPrice.ReadOnly = false;
            dgvProducts.Columns.Add(colPrice);

            // تعداد
            var colQty = new DataGridViewTextBoxColumn();
            colQty.Name = "ColQty";
            colQty.HeaderText = "تعداد";
            colQty.Width = 70;
            colQty.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colQty.ReadOnly = false;
            dgvProducts.Columns.Add(colQty);

            // قیمت کل (فقط‌خواندنی)
            var colTotal = new DataGridViewTextBoxColumn();
            colTotal.Name = "ColTotal";
            colTotal.HeaderText = "قیمت کل (ت)";
            colTotal.ReadOnly = true;
            dgvProducts.Columns.Add(colTotal);

            // بارگذاری از دیتابیس
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT Id, Name, Unit, Price, Quantity, (Price * Quantity) AS Total
                                 FROM Products WHERE DishId = @DishId";
                if (!string.IsNullOrWhiteSpace(search) && search != "جستجو در محصولات...")
                    query += " AND Name LIKE @Search";
                query += " ORDER BY Id";

                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@DishId", currentDishId);
                    if (!string.IsNullOrWhiteSpace(search) && search != "جستجو در محصولات...")
                        cmd.Parameters.AddWithValue("@Search", "%" + search.Trim() + "%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string unit = reader.GetString(2);
                            double price = reader.GetDouble(3);
                            int qty = reader.GetInt32(4);
                            double total = reader.GetDouble(5);
                            string safeUnit = (unit == "دسته") ? "دسته" : "عدد";

                            int rowIdx = dgvProducts.Rows.Add();
                            var row = dgvProducts.Rows[rowIdx];
                            row.Cells["SelectToPrint"].Value = false;
                            row.Cells["ColId"].Value = id;
                            row.Cells["ColName"].Value = name;
                            row.Cells["ColUnit"].Value = safeUnit;
                            row.Cells["ColPrice"].Value = $"{price:N0}";
                            row.Cells["ColQty"].Value = qty.ToString();
                            row.Cells["ColTotal"].Value = $"{total:N0}";
                        }
                    }
                }
            }

            // سطر خالی برای ورود جدید
            AddNewEmptyRow();

            // ریست وضعیت
            allSelected = false;
            btnSelectAll.Text = "✔ انتخاب همه";
            btnSelectAll.BackColor = colorBtnSelAll;
            lblTotalResult.Text = "";

            isBusy = false;
            dgvProducts.CellValueChanged += DgvProducts_CellValueChanged;

            this.BeginInvoke(new Action(() =>
            {
                SetRoundedRegion(btnDeleteProduct, 8);
                SetRoundedRegion(btnPrintProduct, 8);
                SetRoundedRegion(btnTotalPrice, 8);
                SetRoundedRegion(btnSelectAll, 8);
            }));
        }

        private void AddNewEmptyRow()
        {
            int idx = dgvProducts.Rows.Add();
            var row = dgvProducts.Rows[idx];
            row.Cells["SelectToPrint"].Value = false;
            row.Cells["ColId"].Value = -1;
            row.Cells["ColName"].Value = "";
            row.Cells["ColUnit"].Value = "عدد";
            row.Cells["ColPrice"].Value = "0";
            row.Cells["ColQty"].Value = "1";
            row.Cells["ColTotal"].Value = "0";
            row.DefaultCellStyle.BackColor = Color.FromArgb(25, 50, 30);
            row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 230, 150);
        }

        // ══════════════════════════════════════════════════════════════════
        //  CellValueChanged
        // ══════════════════════════════════════════════════════════════════
        private void DgvProducts_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (isBusy || e.RowIndex < 0 || e.RowIndex >= dgvProducts.Rows.Count) return;

            var row = dgvProducts.Rows[e.RowIndex];
            int rowDbId = -1;
            if (row.Cells["ColId"].Value != null)
                int.TryParse(row.Cells["ColId"].Value.ToString(), out rowDbId);

            string colName = dgvProducts.Columns[e.ColumnIndex].Name;

            // محاسبه لحظه‌ای قیمت کل
            double.TryParse(row.Cells["ColPrice"].Value?.ToString()?.Replace(",", ""), out double price);
            int.TryParse(row.Cells["ColQty"].Value?.ToString(), out int qty);
            if (price < 0) price = 0;
            if (qty < 0) qty = 0;

            isBusy = true;
            row.Cells["ColTotal"].Value = $"{price * qty:N0}";
            isBusy = false;

            // آپدیت سطر موجود
            if (rowDbId > 0 && (colName == "ColPrice" || colName == "ColQty" ||
                                colName == "ColName" || colName == "ColUnit"))
            {
                string nameVal = row.Cells["ColName"].Value?.ToString()?.Trim() ?? "";
                string unitVal = row.Cells["ColUnit"].Value?.ToString() ?? "عدد";
                if (string.IsNullOrWhiteSpace(nameVal)) return;

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Products SET Name=@N, Unit=@U, Quantity=@Qty, Price=@Price WHERE Id=@Id";
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@N", nameVal);
                        cmd.Parameters.AddWithValue("@U", unitVal);
                        cmd.Parameters.AddWithValue("@Qty", qty);
                        cmd.Parameters.AddWithValue("@Price", price);
                        cmd.Parameters.AddWithValue("@Id", rowDbId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return;
            }

            // ذخیره سطر جدید (Id = -1) به محض پر شدن نام
            if (rowDbId == -1)
            {
                string nameVal = row.Cells["ColName"].Value?.ToString()?.Trim() ?? "";
                string unitVal = row.Cells["ColUnit"].Value?.ToString() ?? "عدد";
                if (string.IsNullOrWhiteSpace(nameVal)) return;

                int newId;
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO Products (DishId, Name, Unit, Price, Quantity)
                                     VALUES (@DId, @N, @U, @P, @Q);
                                     SELECT last_insert_rowid();";
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@DId", currentDishId);
                        cmd.Parameters.AddWithValue("@N", nameVal);
                        cmd.Parameters.AddWithValue("@U", unitVal);
                        cmd.Parameters.AddWithValue("@P", price);
                        cmd.Parameters.AddWithValue("@Q", qty);
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }

                isBusy = true;
                row.Cells["ColId"].Value = newId;
                row.DefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(230, 230, 230);
                isBusy = false;

                AddNewEmptyRow();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  دکمه حذف
        // ══════════════════════════════════════════════════════════════════
        private void BtnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            var row = dgvProducts.CurrentRow;

            int rowDbId = -1;
            if (row.Cells["ColId"].Value != null)
                int.TryParse(row.Cells["ColId"].Value.ToString(), out rowDbId);
            if (rowDbId == -1) return;

            string pName = row.Cells["ColName"].Value?.ToString() ?? "";
            DialogResult dr = MessageBox.Show(
                $"آیا از حذف محصول «{pName}» مطمئن هستید؟",
                "تایید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Products WHERE Id=@Id";
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", rowDbId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadProducts();
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  دکمه انتخاب همه / لغو (Toggle)
        // ══════════════════════════════════════════════════════════════════
        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            allSelected = !allSelected;
            btnSelectAll.Text = allSelected ? "✖ لغو انتخاب همه" : "✔ انتخاب همه";
            btnSelectAll.BackColor = allSelected ? colorBtnDelete : colorBtnSelAll;

            isBusy = true;
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                int rowDbId = -1;
                if (row.Cells["ColId"].Value != null)
                    int.TryParse(row.Cells["ColId"].Value.ToString(), out rowDbId);
                if (rowDbId == -1) continue;
                row.Cells["SelectToPrint"].Value = allSelected;
            }
            isBusy = false;
            dgvProducts.Refresh();

            // جمع کل را نیز آپدیت کن
            UpdateTotalLabel();
        }

        // ══════════════════════════════════════════════════════════════════
        //  دکمه جمع کل — نتیجه در Label نمایش داده می‌شود
        // ══════════════════════════════════════════════════════════════════
        private void BtnTotalPrice_Click(object sender, EventArgs e)
        {
            dgvProducts.EndEdit();
            UpdateTotalLabel();
        }

        private void UpdateTotalLabel()
        {
            bool anyChecked = false;
            double total = 0;

            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                int rowDbId = -1;
                if (row.Cells["ColId"].Value != null)
                    int.TryParse(row.Cells["ColId"].Value.ToString(), out rowDbId);
                if (rowDbId == -1) continue;

                if (Convert.ToBoolean(row.Cells["SelectToPrint"].Value))
                    anyChecked = true;
            }

            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                int rowDbId = -1;
                if (row.Cells["ColId"].Value != null)
                    int.TryParse(row.Cells["ColId"].Value.ToString(), out rowDbId);
                if (rowDbId == -1) continue;

                bool isChecked = Convert.ToBoolean(row.Cells["SelectToPrint"].Value);
                if (!anyChecked || isChecked)
                {
                    double.TryParse(row.Cells["ColTotal"].Value?.ToString()?.Replace(",", ""), out double rowTotal);
                    total += rowTotal;
                }
            }

            string label = anyChecked ? "جمع موارد انتخابی" : "جمع کل";
            lblTotalResult.Text = $"🧮  {label}:   {total:N0}  تومان";
        }

        // ══════════════════════════════════════════════════════════════════
        //  دکمه چاپ
        // ══════════════════════════════════════════════════════════════════
        private void BtnPrintProduct_Click(object sender, EventArgs e)
        {
            dgvProducts.EndEdit();

            bool anySelected = false;
            foreach (DataGridViewRow row in dgvProducts.Rows)
                if (Convert.ToBoolean(row.Cells["SelectToPrint"].Value))
                { anySelected = true; break; }

            if (!anySelected)
            {
                ShowCustomMessage("لطفاً ابتدا حداقل یک محصول را برای چاپ انتخاب کنید.", "پیام");
                return;
            }

            PrintDocument printDoc = new PrintDocument();
            printDoc.PrintPage += (s, ev) =>
            {
                Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
                Font fontHeader = new Font("Arial", 10, FontStyle.Bold);
                Font fontBody = new Font("Arial", 10, FontStyle.Regular);
                Brush brushText = Brushes.Black;
                Pen penGrid = new Pen(Color.Black, 1);

                StringFormat sfRtl = new StringFormat();
                sfRtl.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sfRtl.Alignment = StringAlignment.Center;
                sfRtl.LineAlignment = StringAlignment.Center;

                StringFormat sfTitle = new StringFormat();
                sfTitle.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sfTitle.Alignment = StringAlignment.Near;

                StringFormat sfTotal = new StringFormat();
                sfTotal.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sfTotal.Alignment = StringAlignment.Far;
                sfTotal.LineAlignment = StringAlignment.Center;

                int pageWidth = ev.PageBounds.Width;
                int startX = pageWidth - 50;
                int startY = 60;
                int rowHeight = 35;

                ev.Graphics.DrawString($"گزارش محصولات ظرف: {currentDishName}", fontTitle, brushText, new PointF(startX, startY), sfTitle);
                startY += 50;

                int[] colWidths = { 180, 80, 130, 70, 140 };
                string[] headers = { "نام محصول", "واحد", "قیمت واحد (تومان)", "تعداد", "قیمت کل (تومان)" };

                int cx = startX;
                for (int i = 0; i < headers.Length; i++)
                {
                    cx -= colWidths[i];
                    Rectangle rc = new Rectangle(cx, startY, colWidths[i], rowHeight);
                    ev.Graphics.DrawRectangle(penGrid, rc);
                    ev.Graphics.DrawString(headers[i], fontHeader, brushText, rc, sfRtl);
                }
                startY += rowHeight;

                double totalSum = 0;
                foreach (DataGridViewRow row in dgvProducts.Rows)
                {
                    if (!Convert.ToBoolean(row.Cells["SelectToPrint"].Value)) continue;

                    cx = startX;
                    double.TryParse(row.Cells["ColPrice"].Value?.ToString()?.Replace(",", ""), out double price);
                    double.TryParse(row.Cells["ColTotal"].Value?.ToString()?.Replace(",", ""), out double rowTotal);

                    string[] rowData = {
                        row.Cells["ColName"].Value?.ToString() ?? "",
                        row.Cells["ColUnit"].Value?.ToString() ?? "",
                        $"{price:N0}",
                        row.Cells["ColQty"].Value?.ToString() ?? "0",
                        $"{rowTotal:N0}"
                    };

                    for (int i = 0; i < rowData.Length; i++)
                    {
                        cx -= colWidths[i];
                        Rectangle rc = new Rectangle(cx, startY, colWidths[i], rowHeight);
                        ev.Graphics.DrawRectangle(penGrid, rc);
                        ev.Graphics.DrawString(rowData[i], fontBody, brushText, rc, sfRtl);
                    }
                    totalSum += rowTotal;
                    startY += rowHeight;
                }

                startY += 10;
                int totalTableWidth = 0;
                foreach (int w in colWidths) totalTableWidth += w;
                Rectangle rectTotal = new Rectangle(startX - totalTableWidth, startY, totalTableWidth, rowHeight);
                ev.Graphics.DrawRectangle(penGrid, rectTotal);
                ev.Graphics.DrawString($"جمع کل نهایی اقلام انتخابی: {totalSum:N0} تومان ", fontHeader, brushText, rectTotal, sfTotal);
            };

            PrintDialog pDialog = new PrintDialog();
            pDialog.Document = printDoc;
            if (pDialog.ShowDialog() == DialogResult.OK)
                printDoc.Print();
        }

        // ══════════════════════════════════════════════════════════════════
        //  پنجره پیام سفارشی
        // ══════════════════════════════════════════════════════════════════
        private void ShowCustomMessage(string message, string title)
        {
            Form msgForm = new Form();
            msgForm.FormBorderStyle = FormBorderStyle.None;
            msgForm.StartPosition = FormStartPosition.CenterParent;
            msgForm.Size = new Size(380, 160);
            msgForm.BackColor = Color.FromArgb(40, 40, 40);

            msgForm.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(Color.FromArgb(75, 75, 75), 2f))
                using (GraphicsPath path = MakeRoundedPath(msgForm.ClientRectangle, 12))
                    ev.Graphics.DrawPath(p, path);
            };

            var lblMsg = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(240, 240, 240),
                Location = new Point(20, 20),
                Size = new Size(340, 65),
                TextAlign = ContentAlignment.MiddleCenter,
                RightToLeft = RightToLeft.Yes
            };

            var btnOk = new Button
            {
                Text = "تایید",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(95, 34),
                Location = new Point(142, 105),
                FlatStyle = FlatStyle.Flat,
                BackColor = colorBtnAdd,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Click += (s, ev) => msgForm.Close();

            msgForm.Controls.AddRange(new Control[] { lblMsg, btnOk });
            SetRoundedRegion(msgForm, 12);
            SetRoundedRegion(btnOk, 8);
            msgForm.ShowDialog(this);
        }

        // ══════════════════════════════════════════════════════════════════
        //  کمکی‌ها
        // ══════════════════════════════════════════════════════════════════
        private GraphicsPath MakeRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseAllFigures();
            return path;
        }

        private void SetRoundedRegion(Control control, int radius)
        {
            if (control == null || control.Width == 0 || control.Height == 0) return;
            if (control.Region != null) control.Region.Dispose();
            using (GraphicsPath path = MakeRoundedPath(control.ClientRectangle, radius))
                control.Region = new Region(path);
        }
    }
}