namespace voda
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.num = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nak_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.val = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unixtime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.vod_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nzav = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(69, 226);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "Открыть файл";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(185, 242);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(588, 198);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(69, 265);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(110, 45);
            this.button2.TabIndex = 2;
            this.button2.Text = "Показать JSON Обьект";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.num,
            this.nak_id,
            this.val,
            this.date,
            this.unixtime,
            this.vod_id,
            this.nzav});
            this.dataGridView1.Location = new System.Drawing.Point(69, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(704, 208);
            this.dataGridView1.TabIndex = 3;
            // 
            // num
            // 
            this.num.Frozen = true;
            this.num.HeaderText = "num";
            this.num.Name = "num";
            this.num.ReadOnly = true;
            // 
            // nak_id
            // 
            this.nak_id.Frozen = true;
            this.nak_id.HeaderText = "id";
            this.nak_id.Name = "nak_id";
            this.nak_id.ReadOnly = true;
            // 
            // val
            // 
            this.val.Frozen = true;
            this.val.HeaderText = "val";
            this.val.Name = "val";
            this.val.ReadOnly = true;
            // 
            // date
            // 
            this.date.Frozen = true;
            this.date.HeaderText = "date";
            this.date.Name = "date";
            this.date.ReadOnly = true;
            // 
            // unixtime
            // 
            this.unixtime.Frozen = true;
            this.unixtime.HeaderText = "unixtime";
            this.unixtime.Name = "unixtime";
            this.unixtime.ReadOnly = true;
            // 
            // vod_id
            // 
            this.vod_id.Frozen = true;
            this.vod_id.HeaderText = "vod_id";
            this.vod_id.Name = "vod_id";
            this.vod_id.ReadOnly = true;
            // 
            // nzav
            // 
            this.nzav.Frozen = true;
            this.nzav.HeaderText = "nzav";
            this.nzav.Name = "nzav";
            this.nzav.ReadOnly = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(185, 226);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Консоль:";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(69, 399);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(110, 41);
            this.button3.TabIndex = 5;
            this.button3.Text = "Очистить консоль";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(69, 356);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(110, 37);
            this.button4.TabIndex = 6;
            this.button4.Text = "Очистить таблицу";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(902, 452);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn num;
        private System.Windows.Forms.DataGridViewTextBoxColumn nak_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn val;
        private System.Windows.Forms.DataGridViewTextBoxColumn date;
        private System.Windows.Forms.DataGridViewTextBoxColumn unixtime;
        private System.Windows.Forms.DataGridViewTextBoxColumn vod_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn nzav;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
    }
}

