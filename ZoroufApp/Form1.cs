using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ZoroufApp
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=dishes.db;Mode=ReadWriteCreate;";
        private int selectedDishId = -1;
        private Button selectedButton = null;

        private DateTime lastClickTime = DateTime.MinValue;
        private Button lastClickedButton = null;

        private const bool isDarkMode = true;

        private Color colorFormBgDark = Color.FromArgb(18, 18, 18);
        private Color colorPanelBgDark = Color.FromArgb(30, 30, 30);
        private Color colorCardBgDark = Color.FromArgb(43, 43, 43);
        private Color colorCardBorderDark = Color.FromArgb(60, 60, 60);
        private Color colorCardHoverDark = Color.FromArgb(55, 55, 55);
        private Color colorTextDark = Color.FromArgb(240, 240, 240);

        private Color colorCardSelected = Color.FromArgb(41, 121, 255);

        private Color colorBtnAdd = Color.FromArgb(46, 204, 113);
        private Color colorBtnEdit = Color.FromArgb(52, 152, 219);
        private Color colorBtnDelete = Color.FromArgb(231, 76, 60);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public Form1()
        {
            InitializeComponent();
            LoadProgramIcon();

            // توجه: کنترل TextBox استاندارد رویداد Paint را پشتیبانی نمی‌کند.
            // کادر دور تکست‌باکس را در رویداد Paint خود فرم یا کانتینر آن رسم می‌کنیم.
            this.Paint += Form1_Paint; 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            InitializeDatabase();
            LoadDishes();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = colorFormBgDark;
            this.ForeColor = colorTextDark;

            SetFormTitleBarTheme(this.Handle, true);

            txtSearch.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            txtSearch.BackColor = Color.FromArgb(45, 45, 45);
            txtSearch.ForeColor = Color.White;
            txtSearch.BorderStyle = BorderStyle.FixedSingle; // تغییر به حالت استاندارد برای مچ شدن با تم

            flowLayoutPanelDishes.RightToLeft = RightToLeft.Yes;
            flowLayoutPanelDishes.BackColor = colorPanelBgDark;
            flowLayoutPanelDishes.Refresh();

            StyleActionControlButtons();

            if (flowLayoutPanelDishes.Controls.Count > 0)
            {
                LoadDishes(txtSearch.Text);
            }

            SetRoundedRegion(txtSearch, 8);
            if (btnAddNewDish != null) SetRoundedRegion(btnAddNewDish, 12);
            if (btnEditDish != null) SetRoundedRegion(btnEditDish, 10);
            if (btnDeleteDish != null) SetRoundedRegion(btnDeleteDish, 10);

            this.Invalidate(); // بازنشانی رسم کادر دور تکست‌باکس روی فرم
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // رسم یک کادر زیباتر دور تکست‌باکس روی سطح فرم (جایگزین تگ Paint تکست باکس)
            if (txtSearch != null && txtSearch.Visible)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color borderColor = Color.FromArgb(80, 80, 80);
                
                // ایجاد محدوده کادر کمی بزرگتر از خود تکست باکس جهت نمایش بهتر
                Rectangle rect = new Rectangle(txtSearch.Left - 1, txtSearch.Top - 1, txtSearch.Width + 1, txtSearch.Height + 1);
                using (Pen p = new Pen(borderColor, 1.5f))
                using (GraphicsPath path = GetRoundedPath(rect, 8))
                {
                    e.Graphics.DrawPath(p, path);
                }
            }
        }

        private void SetFormTitleBarTheme(IntPtr handle, bool useDark)
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                if (Environment.OSVersion.Version.Build < 19041)
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                }

                int useImmersiveDarkMode = useDark ? 1 : 0;
                DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int));

                this.Size = new Size(this.Width + 1, this.Height);
                this.Size = new Size(this.Width - 1, this.Height);
            }
        }

        private void StyleActionControlButtons()
        {
            if (btnAddNewDish != null)
            {
                btnAddNewDish.FlatStyle = FlatStyle.Flat;
                btnAddNewDish.BackColor = colorBtnAdd;
                btnAddNewDish.ForeColor = Color.White;
                btnAddNewDish.Font = new Font("Segoe UI", 13, FontStyle.Bold);
                btnAddNewDish.FlatAppearance.BorderSize = 0;
                btnAddNewDish.Cursor = Cursors.Hand;
                btnAddNewDish.Height = 50;
                btnAddNewDish.TextAlign = ContentAlignment.MiddleCenter;
                btnAddNewDish.Padding = new Padding(0);
            }

            StyleSingleButton(btnEditDish, colorBtnEdit);
            StyleSingleButton(btnDeleteDish, colorBtnDelete);
        }

        private void StyleSingleButton(Button btn, Color themeColor)
        {
            if (btn == null) return;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = themeColor;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Height = 40;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            btn.Padding = new Padding(0);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            
            // جلوگیری از خطای ابعاد منفی در کنترل‌های کوچک
            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = diameter = rect.Height;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseAllFigures();
            return path;
        }

        private void SetRoundedRegion(Control control, int radius)
        {
            if (control == null || control.ClientRectangle.Width == 0) return;

            // آزاد کردن ریجن قبلی برای جلوگیری از نشت حافظه (Memory Leak)
            if (control.Region != null)
            {
                control.Region.Dispose();
            }

            using (GraphicsPath path = GetRoundedPath(control.ClientRectangle, radius))
            {
                control.Region = new Region(path);
            }
        }

        private void LoadProgramIcon()
        {
            try
            {
                if (System.IO.File.Exists("icon.ico"))
                {
                    this.Icon = new Icon("icon.ico");
                }
            }
            catch { }
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createDishesTable = @"
                CREATE TABLE IF NOT EXISTS Dishes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT
                );";
                using (var command = new SqliteCommand(createDishesTable, connection)) { command.ExecuteNonQuery(); }

                string createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DishId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Unit TEXT,
                    Price REAL DEFAULT 0,
                    Quantity INTEGER DEFAULT 0,
                    FOREIGN KEY (DishId) REFERENCES Dishes(Id) ON DELETE CASCADE
                );";
                using (var command = new SqliteCommand(createProductsTable, connection)) { command.ExecuteNonQuery(); }
            }
        }

        private void LoadDishes(string searchTerm = "")
        {
            // ابتدا برای جلوگیری از نشت حافظه دکمه‌های قبلی را Dispose می‌کنیم
            foreach (Control ctrl in flowLayoutPanelDishes.Controls)
            {
                ctrl.Dispose();
            }
            flowLayoutPanelDishes.Controls.Clear();
            
            selectedDishId = -1;
            selectedButton = null;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Id, Name FROM Dishes";

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query += " WHERE Name LIKE @Search";
                }

                using (var command = new SqliteCommand(query, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        command.Parameters.AddWithValue("@Search", "%" + searchTerm.Trim() + "%");
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);

                            Button btnDish = new Button();
                            btnDish.Text = "  " + name;
                            btnDish.Tag = id;
                            btnDish.Size = new Size(245, 52);
                            btnDish.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                            btnDish.TextAlign = ContentAlignment.MiddleCenter;

                            btnDish.BackColor = colorCardBgDark;
                            btnDish.ForeColor = colorTextDark;
                            btnDish.FlatStyle = FlatStyle.Flat;
                            btnDish.FlatAppearance.BorderSize = 0;
                            btnDish.Cursor = Cursors.Hand;
                            btnDish.Margin = new Padding(6);

                            btnDish.MouseEnter += (s, e) => {
                                Button b = (Button)s;
                                if (b != selectedButton) b.BackColor = colorCardHoverDark;
                            };

                            btnDish.MouseLeave += (s, e) => {
                                Button b = (Button)s;
                                if (b != selectedButton) b.BackColor = colorCardBgDark;
                            };

                            btnDish.Click += BtnDish_Click;

                            SetRoundedRegion(btnDish, 10);
                            flowLayoutPanelDishes.Controls.Add(btnDish);
                        }
                    }
                }
            }
        }

        private void BtnDish_Click(object sender, EventArgs e)
        {
            Button currentButton = (Button)sender;
            TimeSpan clickDelta = DateTime.Now - lastClickTime;

            if (currentButton == lastClickedButton && clickDelta.TotalMilliseconds < 500)
            {
                OpenProductsForm(currentButton);
                lastClickTime = DateTime.MinValue;
                lastClickedButton = null;
                return;
            }

            lastClickTime = DateTime.Now;
            lastClickedButton = currentButton;

            if (selectedButton != null)
            {
                selectedButton.BackColor = colorCardBgDark;
                selectedButton.FlatAppearance.BorderSize = 0;
                selectedButton.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                selectedButton.ForeColor = colorTextDark;
            }

            selectedButton = currentButton;
            selectedButton.BackColor = colorCardSelected;
            selectedButton.FlatAppearance.BorderSize = 0;
            selectedButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            selectedButton.ForeColor = Color.White;

            selectedDishId = (int)selectedButton.Tag;
        }

        private void OpenProductsForm(Button btn)
        {
            if (btn.Tag != null)
            {
                int dishId = (int)btn.Tag;
                string dishName = btn.Text.Trim();

                // فراخوانی فرم دوم به صورت مودال
                using (Form2 productsForm = new Form2(dishId, dishName))
                {
                    productsForm.ShowDialog(this);
                }
            }
        }

        private void btnAddNewDish_Click_1(object sender, EventArgs e)
        {
            string newDishName = ShowCustomInputBox("لطفاً نام ظرف جدید را وارد کنید:", "ثبت ظرف جدید", "");

            if (string.IsNullOrWhiteSpace(newDishName)) return;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Dishes (Name, Description) VALUES (@Name, @Description)";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", newDishName.Trim());
                    command.Parameters.AddWithValue("@Description", "توضیحاتی برای این ظرف ثبت نشده است.");
                    command.ExecuteNonQuery();
                }
            }

            LoadDishes();
            ShowCustomMessage($"ظرف «{newDishName}» با موفقیت به لیست اضافه شد.", "موفقیت");
        }

        private void btnEditDish_Click(object sender, EventArgs e)
        {
            if (selectedDishId == -1 || selectedButton == null)
            {
                ShowCustomMessage("لطفاً ابتدا یک ظرف را از لیست انتخاب کنید.", "هشدار");
                return;
            }

            string updatedName = ShowCustomInputBox("نام جدید ظرف را وارد کنید:", "ویرایش نام ظرف", selectedButton.Text.Trim());

            if (string.IsNullOrWhiteSpace(updatedName)) return;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Dishes SET Name = @Name WHERE Id = @Id";

                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", updatedName.Trim());
                    command.Parameters.AddWithValue("@Id", selectedDishId);
                    command.ExecuteNonQuery();
                }
            }

            LoadDishes();
            ShowCustomMessage("نام ظرف با موفقیت ویرایش شد.", "موفقیت");
        }

        private void btnDeleteDish_Click(object sender, EventArgs e)
        {
            if (selectedDishId == -1 || selectedButton == null)
            {
                ShowCustomMessage("لطفاً ابتدا یک ظرف را برای حذف انتخاب کنید.", "هشدار");
                return;
            }

            bool confirmDelete = ShowCustomConfirm($"آیا از حذف ظرف «{selectedButton.Text.Trim()}» مطمئن هستید؟\nبا حذف این ظرف، تمام محصولات مرتبط با آن نیز حذف خواهند شد.", "تایید حذف عملیات");

            if (confirmDelete)
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM Dishes WHERE Id = @Id";

                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", selectedDishId);
                        command.ExecuteNonQuery();
                    }
                }

                LoadDishes();
                ShowCustomMessage("ظرف مورد نظر با موفقیت از لیست حذف شد.", "موفقیت");
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadDishes(txtSearch.Text);
        }

        private void flowLayoutPanelDishes_Paint(object sender, PaintEventArgs e)
        {
            Color borderColor = Color.FromArgb(55, 55, 55);
            using (Pen p = new Pen(borderColor, 1))
            {
                e.Graphics.DrawRectangle(p, 0, 0, flowLayoutPanelDishes.Width - 1, flowLayoutPanelDishes.Height - 1);
            }
        }

        private string ShowCustomInputBox(string promptText, string title, string defaultText)
        {
            string inputResult = null;

            Form inputForm = new Form();
            inputForm.FormBorderStyle = FormBorderStyle.None;
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.Size = new Size(400, 190);
            inputForm.BackColor = Color.FromArgb(40, 40, 40);

            inputForm.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color borderCol = Color.FromArgb(75, 75, 75);
                using (Pen p = new Pen(borderCol, 2f))
                using (GraphicsPath path = GetRoundedPath(inputForm.ClientRectangle, 14))
                {
                    ev.Graphics.DrawPath(p, path);
                }
            };

            Label lblPrompt = new Label();
            lblPrompt.Text = promptText;
            lblPrompt.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblPrompt.ForeColor = Color.FromArgb(240, 240, 240);
            lblPrompt.Location = new Point(20, 20);
            lblPrompt.Size = new Size(360, 30);
            lblPrompt.TextAlign = ContentAlignment.MiddleRight;
            lblPrompt.RightToLeft = RightToLeft.Yes;

            TextBox txtInput = new TextBox();
            txtInput.Text = defaultText;
            txtInput.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            txtInput.Location = new Point(25, 65);
            txtInput.Size = new Size(350, 30);
            txtInput.RightToLeft = RightToLeft.Yes;
            txtInput.BackColor = Color.FromArgb(55, 55, 55);
            txtInput.ForeColor = Color.White;
            txtInput.BorderStyle = BorderStyle.FixedSingle;

            Button btnSubmit = new Button();
            btnSubmit.Text = "تایید";
            btnSubmit.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnSubmit.Size = new Size(95, 34);
            btnSubmit.Location = new Point(210, 125);
            btnSubmit.FlatStyle = FlatStyle.Flat;
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.BackColor = colorBtnEdit;
            btnSubmit.ForeColor = Color.White;
            btnSubmit.Cursor = Cursors.Hand;
            btnSubmit.Click += (s, ev) => { inputResult = txtInput.Text; inputForm.Close(); };

            Button btnCancel = new Button();
            btnCancel.Text = "انصراف";
            btnCancel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnCancel.Size = new Size(95, 34);
            btnCancel.Location = new Point(95, 125);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.BackColor = Color.FromArgb(65, 65, 65);
            btnCancel.ForeColor = Color.White;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, ev) => { inputForm.Close(); };

            inputForm.Controls.Add(lblPrompt);
            inputForm.Controls.Add(txtInput);
            inputForm.Controls.Add(btnSubmit);
            inputForm.Controls.Add(btnCancel);

            SetRoundedRegion(inputForm, 14);
            SetRoundedRegion(btnSubmit, 8);
            SetRoundedRegion(btnCancel, 8);

            inputForm.Load += (s, ev) => {
                txtInput.Focus();
                txtInput.SelectAll();
            };

            inputForm.ShowDialog(this);
            return inputResult;
        }

        private void ShowCustomMessage(string message, string title)
        {
            Form msgForm = new Form();
            msgForm.FormBorderStyle = FormBorderStyle.None;
            msgForm.StartPosition = FormStartPosition.CenterParent;
            msgForm.Size = new Size(380, 160);
            msgForm.BackColor = Color.FromArgb(40, 40, 40);

            msgForm.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color borderCol = Color.FromArgb(75, 75, 75);
                using (Pen p = new Pen(borderCol, 2f))
                using (GraphicsPath path = GetRoundedPath(msgForm.ClientRectangle, 12))
                {
                    ev.Graphics.DrawPath(p, path);
                }
            };

            Label lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblMessage.ForeColor = Color.FromArgb(240, 240, 240);
            lblMessage.Location = new Point(20, 25);
            lblMessage.Size = new Size(340, 65);
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.RightToLeft = RightToLeft.Yes;

            Button btnOk = new Button();
            btnOk.Text = "تایید";
            btnOk.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnOk.Size = new Size(95, 34);
            btnOk.Location = new Point(142, 105);
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.BackColor = colorBtnAdd;
            btnOk.ForeColor = Color.White;
            btnOk.Cursor = Cursors.Hand;
            btnOk.Click += (s, ev) => { msgForm.Close(); };

            msgForm.Controls.Add(lblMessage);
            msgForm.Controls.Add(btnOk);

            SetRoundedRegion(msgForm, 12);
            SetRoundedRegion(btnOk, 8);

            msgForm.ShowDialog(this);
        }

        private bool ShowCustomConfirm(string message, string title)
        {
            bool result = false;

            Form confirmForm = new Form();
            confirmForm.FormBorderStyle = FormBorderStyle.None;
            confirmForm.StartPosition = FormStartPosition.CenterParent;
            confirmForm.Size = new Size(380, 160);
            confirmForm.BackColor = Color.FromArgb(40, 40, 40);

            confirmForm.Paint += (s, ev) => {
                ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color borderCol = Color.FromArgb(75, 75, 75);
                using (Pen p = new Pen(borderCol, 2f))
                using (GraphicsPath path = GetRoundedPath(confirmForm.ClientRectangle, 12))
                {
                    ev.Graphics.DrawPath(p, path);
                }
            };

            Label lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblMessage.ForeColor = Color.FromArgb(240, 240, 240);
            lblMessage.Location = new Point(20, 25);
            lblMessage.Size = new Size(340, 65);
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.RightToLeft = RightToLeft.Yes;

            Button btnYes = new Button();
            btnYes.Text = "بله";
            btnYes.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnYes.Size = new Size(90, 34);
            btnYes.Location = new Point(195, 105);
            btnYes.FlatStyle = FlatStyle.Flat;
            btnYes.FlatAppearance.BorderSize = 0;
            btnYes.BackColor = colorBtnDelete;
            btnYes.ForeColor = Color.White;
            btnYes.Cursor = Cursors.Hand;
            btnYes.Click += (s, ev) => { result = true; confirmForm.Close(); };

            Button btnNo = new Button();
            btnNo.Text = "خیر";
            btnNo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnNo.Size = new Size(90, 34);
            btnNo.Location = new Point(95, 105);
            btnNo.FlatStyle = FlatStyle.Flat;
            btnNo.FlatAppearance.BorderSize = 0;
            btnNo.BackColor = Color.FromArgb(65, 65, 65);
            btnNo.ForeColor = Color.White;
            btnNo.Cursor = Cursors.Hand;
            btnNo.Click += (s, ev) => { result = false; confirmForm.Close(); };

            confirmForm.Controls.Add(lblMessage);
            confirmForm.Controls.Add(btnYes);
            confirmForm.Controls.Add(btnNo);

            SetRoundedRegion(confirmForm, 12);
            SetRoundedRegion(btnYes, 8);
            SetRoundedRegion(btnNo, 8);

            confirmForm.ShowDialog(this);
            return result;
        }
    }
}