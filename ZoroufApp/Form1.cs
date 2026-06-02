using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace ZoroufApp
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=dishes.db;Mode=ReadWriteCreate;";

        // ذخیره شناسه ظرف انتخاب شده به جای استفاده از SelectedItems در لیست‌ویو
        private int selectedDishId = -1;
        private Button selectedButton = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            // تنظیم جهت چیدمان دکمه‌ها در پنل از راست به چپ
            flowLayoutPanelDishes.RightToLeft = RightToLeft.Yes;
            // اضافه کردن بوردِر استاندارد ویندوزی دور پنل ظروف
            
            flowLayoutPanelDishes.Paint += flowLayoutPanelDishes_Paint;
            InitializeDatabase();
            LoadDishes();
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
        }
        private void flowLayoutPanelDishes_Paint(object sender, PaintEventArgs e)
        {
            // رسم یک بوردِر خاکستری ملایم (۱ پیکسلی) دقیقاً دور تا دور پنل
            using (Pen p = new Pen(Color.LightGray, 1))
            {
                // ابعاد دقیق پنل را می‌گیرد و یک پیکسل جمع‌ترش می‌کند تا خط لبه بیرون نزند
                e.Graphics.DrawRectangle(p, 0, 0, flowLayoutPanelDishes.Width - 1, flowLayoutPanelDishes.Height - 1);
            }
        }
        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Dishes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT Null,
                        Description TEXT
                    );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                string checkQuery = "SELECT COUNT(*) FROM Dishes";
                using (var checkCmd = new SqliteCommand(checkQuery, connection))
                {
                    long count = (long)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string insertQuery = @"
                            INSERT INTO Dishes (Name, Description) VALUES 
                            (N'قابلمه چدنی سایز ۲۸', N'توضیحات: مناسب برای پخت انواع خورش و پلو با کیفیت بالا.'),
                            (N'بشقاب ملامین طرح‌دار', N'توضیحات: نشکن، سبک و مناسب برای مصارف روزمره و پیکنیک.'),
                            (N'لیوان کریستال تراش‌خورده', N'توضیحات: جنس بلور باکیفیت، بسیار شفاف و مخصوص پذیرایی.');";

                        using (var insertCmd = new SqliteCommand(insertQuery, connection))
                        {
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        // متد بارگذاری ظروف به صورت دکمه‌های شیک و منظم در دو ستون
        private void LoadDishes(string searchTerm = "")
        {
            // پاک کردن دکمه‌های قبلی از پنل
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

                            // ساخت یک دکمه سفارشی و شیک برای هر ظرف
                            Button btnDish = new Button();
                            btnDish.Text = name;
                            btnDish.Tag = id; // ذخیره شناسه ظرف در تگ دکمه

                            // تنظیم اندازه دکمه‌ها (متناسب با چیدمان دو ستونه در پنل)
                            btnDish.Size = new Size(245, 50);
                            btnDish.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                            btnDish.BackColor = Color.White;
                            btnDish.ForeColor = Color.Black;
                            btnDish.FlatStyle = FlatStyle.Flat;
                            btnDish.FlatAppearance.BorderColor = Color.LightGray;
                            btnDish.FlatAppearance.BorderSize = 1;
                            btnDish.Cursor = Cursors.Hand;
                            btnDish.Margin = new Padding(5);

                            // متصل کردن رویدادهای کلیک و دبل کلیک
                            btnDish.Click += BtnDish_Click;
                            btnDish.DoubleClick += BtnDish_DoubleClick;

                            // اضافه کردن دکمه به پنل
                            flowLayoutPanelDishes.Controls.Add(btnDish);
                        }
                    }
                }
            }
        }

        // رویداد کلیک برای انتخاب ظرف (شبیه‌سازی حالت انتخاب سلول)
        private void BtnDish_Click(object sender, EventArgs e)
        {
            // بازگرداندن رنگ دکمه قبلی به حالت عادی
            if (selectedButton != null)
            {
                selectedButton.BackColor = Color.White;
                selectedButton.FlatAppearance.BorderColor = Color.LightGray;
            }

            // انتخاب دکمه جدید و تغییر رنگ آن به آبی ملایم و شیک
            selectedButton = (Button)sender;
            selectedButton.BackColor = Color.LightBlue;
            selectedButton.FlatAppearance.BorderColor = Color.DodgerBlue;

            // ذخیره شناسه ظرف انتخاب شده
            selectedDishId = (int)selectedButton.Tag;
        }

        // رویداد دبل کلیک روی دکمه ظرف
        private void BtnDish_DoubleClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int dishId = (int)btn.Tag;
            MessageBox.Show($"شما روی ظرف با کد {dishId} و نام «{btn.Text}» دبل کلیک کردید.\nدر مرحله بعد، این کلیک فرم دوم را باز می‌کند.");
        }

        // رویداد افزودن ظرف جدید
        private void btnAddNewDish_Click_1(object sender, EventArgs e)
        {
            string newDishName = Microsoft.VisualBasic.Interaction.InputBox("لطفاً نام ظرف جدید را وارد کنید:", "ثبت ظرف جدید", "");

            if (string.IsNullOrWhiteSpace(newDishName))
            {
                return;
            }

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

            LoadDishes(); // بروزرسانی دکمه‌ها
            MessageBox.Show($"ظرف «{newDishName}» با موفقیت به لیست اضافه شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // رویداد ویرایش نام ظرف انتخاب شده
        private void btnEditDish_Click(object sender, EventArgs e)
        {
            if (selectedDishId == -1 || selectedButton == null)
            {
                MessageBox.Show("لطفاً ابتدا یک ظرف را از لیست انتخاب کنید.", "هشدار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string updatedName = Microsoft.VisualBasic.Interaction.InputBox("نام جدید ظرف را وارد کنید:", "ویرایش نام ظرف", selectedButton.Text);

            if (string.IsNullOrWhiteSpace(updatedName))
            {
                return;
            }

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
            MessageBox.Show("نام ظرف با موفقیت ویرایش شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // رویداد حذف ظرف انتخاب شده
        private void btnDeleteDish_Click(object sender, EventArgs e)
        {
            if (selectedDishId == -1 || selectedButton == null)
            {
                MessageBox.Show("لطفاً ابتدا یک ظرف را برای حذف انتخاب کنید.", "هشدار", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dialogResult = MessageBox.Show($"آیا از حذف ظرف «{selectedButton.Text}» مطمئن هستید؟", "تایید حذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
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
                MessageBox.Show("ظرف مورد نظر با موفقیت از لیست حذف شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // رویداد جستجوی لحظه‌ای با تغییر متن تکست‌باکس
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadDishes(txtSearch.Text);
        }
    }
}