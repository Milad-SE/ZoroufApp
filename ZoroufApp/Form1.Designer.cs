namespace ZoroufApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnAddNewDish = new Button();
            btnEditDish = new Button();
            btnDeleteDish = new Button();
            txtSearch = new TextBox();
            flowLayoutPanelDishes = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // btnAddNewDish
            // 
            btnAddNewDish.Location = new Point(815, 76);
            btnAddNewDish.Name = "btnAddNewDish";
            btnAddNewDish.Size = new Size(249, 56);
            btnAddNewDish.TabIndex = 1;
            btnAddNewDish.Text = "افزودن ظروف جدید";
            btnAddNewDish.UseVisualStyleBackColor = true;
            btnAddNewDish.Click += btnAddNewDish_Click_1;
            // 
            // btnEditDish
            // 
            btnEditDish.Location = new Point(815, 159);
            btnEditDish.Name = "btnEditDish";
            btnEditDish.Size = new Size(119, 42);
            btnEditDish.TabIndex = 2;
            btnEditDish.Text = "ویرایش نام";
            btnEditDish.UseVisualStyleBackColor = true;
            btnEditDish.Click += btnEditDish_Click;
            // 
            // btnDeleteDish
            // 
            btnDeleteDish.Location = new Point(945, 159);
            btnDeleteDish.Name = "btnDeleteDish";
            btnDeleteDish.Size = new Size(119, 42);
            btnDeleteDish.TabIndex = 3;
            btnDeleteDish.Text = "حذف ظرف";
            btnDeleteDish.UseVisualStyleBackColor = true;
            btnDeleteDish.Click += btnDeleteDish_Click;
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(575, 28);
            txtSearch.Multiline = true;
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "جستجوی نام ظرف...";
            txtSearch.RightToLeft = RightToLeft.Yes;
            txtSearch.Size = new Size(209, 34);
            txtSearch.TabIndex = 4;
            txtSearch.TextChanged += txtSearch_TextChanged;
            txtSearch.DoubleClick += txtSearch_TextChanged;
            // 
            // flowLayoutPanelDishes
            // 
            flowLayoutPanelDishes.AutoScroll = true;
            flowLayoutPanelDishes.Location = new Point(12, 76);
            flowLayoutPanelDishes.Name = "flowLayoutPanelDishes";
            flowLayoutPanelDishes.RightToLeft = RightToLeft.Yes;
            flowLayoutPanelDishes.Size = new Size(772, 495);
            flowLayoutPanelDishes.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1102, 583);
            Controls.Add(flowLayoutPanelDishes);
            Controls.Add(txtSearch);
            Controls.Add(btnDeleteDish);
            Controls.Add(btnEditDish);
            Controls.Add(btnAddNewDish);
            Name = "Form1";
            RightToLeft = RightToLeft.No;
            Text = "مدیریت و لیست ظروف";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnAddNewDish;
        private Button btnEditDish;
        private Button btnDeleteDish;
        private TextBox txtSearch;
        private FlowLayoutPanel flowLayoutPanelDishes;
    }
}
