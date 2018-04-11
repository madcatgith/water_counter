using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace voda
{
    public partial class Form2 : Form
    {
        IniFile INI = new IniFile("config.ini");
        string path = AppDomain.CurrentDomain.BaseDirectory;
        Form1 main_form;

        bool DataSaved = false;

        public Form2()
        {
            InitializeComponent();
            read_settings();
            uncheck_all();
            DataSaved = true;
        }

        public Form2(Form1 main)
        {
            InitializeComponent();
            main_form = main;
            read_settings();
            uncheck_all();
            DataSaved = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            save_settings();
            main_form.Text = windowName.Text.ToString();
            main_form.InitializeProperties();
        }

        private void save_settings() {
            //Настройки фтп
            INI.Write("FTP","Host",host_textBox.Text.ToString());
            INI.Write("FTP","Login",ftplogin_textBox.Text.ToString());
            INI.Write("FTP","Password",ftppassword_textBox.Text.ToString());
            INI.Write("FTP","FolderPath",folder_textBox.Text.ToString());

            //Настройки водоканала
            INI.Write("KVK","Login",vlogin_textBox.Text.ToString());
            INI.Write("KVK","Password",vpassword_textBox.Text.ToString());
            INI.Write("KVK","Interval",time_interval.Text.ToString());

            //Опции
            INI.Write("Setup", "IdPath", idlist_path.Text.ToString());
            INI.Write("Setup", "LogPath", logPath.Text.ToString());
            INI.Write("Setup", "WindowName",windowName.Text.ToString());

            //Mysql
            INI.Write("Mysql","Server",mysqlServer.Text.ToString());
            INI.Write("Mysql", "Login", mysqlLogin.Text.ToString());
            INI.Write("Mysql", "Password", mysqlPassword.Text.ToString());
            INI.Write("Mysql", "DBName", mysqlDBName.Text.ToString());
            INI.Write("Mysql", "Table", mysqlTable.Text.ToString());

            DataSaved = true;
        }

        private void read_settings() {
            //Загрузка данных фтп
            if (INI.KeyExists("Host","FTP")) {
                host_textBox.Text = INI.ReadINI("FTP","Host");              
            }
            if (INI.KeyExists("Login", "FTP"))
            {
                ftplogin_textBox.Text = INI.ReadINI("FTP", "Login");
            }
            if (INI.KeyExists("Password", "FTP"))
            {
                ftppassword_textBox.Text = INI.ReadINI("FTP", "Password");
            }
            if (INI.KeyExists("FolderPath", "FTP"))
            {
                folder_textBox.Text = INI.ReadINI("FTP", "FolderPath");
            }

            //Загрузка настроек водоканала
            if (INI.KeyExists("Login", "KVK"))
            {
                vlogin_textBox.Text = INI.ReadINI("KVK", "Login");
            }
            if (INI.KeyExists("Password", "KVK"))
            {
                vpassword_textBox.Text = INI.ReadINI("KVK", "Password");
            }
            if (INI.KeyExists("Interval", "KVK"))
            {
                time_interval.Text = INI.ReadINI("KVK", "Interval");
            }

            //Другие опции
            if (INI.KeyExists("IdPath", "Setup"))
            {
                idlist_path.Text = INI.ReadINI("Setup", "IdPath");
            }
            if (INI.KeyExists("LogPath", "Setup"))
            {
                logPath.Text = INI.ReadINI("Setup", "LogPath");
            }
            if (INI.KeyExists("WindowName","Setup")) {
                windowName.Text = INI.ReadINI("Setup","WindowName");
            }

            //Mysql
            if (INI.KeyExists("Server","Mysql")) {
                mysqlServer.Text = INI.ReadINI("Mysql","Server");
            }
            if (INI.KeyExists("Login","Mysql")) {
                mysqlLogin.Text = INI.ReadINI("Mysql","Login");
            }
            if (INI.KeyExists("Password","Mysql")) {
                mysqlPassword.Text = INI.ReadINI("Mysql","Password");
            }
            if (INI.KeyExists("DBName", "Mysql"))
            {
                mysqlDBName.Text = INI.ReadINI("Mysql", "DBName");
            }
            if (INI.KeyExists("Table","Mysql")) {
                mysqlTable.Text = INI.ReadINI("Mysql","Table");
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            if (!DataSaved)
            {
                var dlgResult = MessageBox.Show("Сохранить настройки перед выходом?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (dlgResult == System.Windows.Forms.DialogResult.No)
                {
                    main_form.InitializeProperties();
                }
                if (dlgResult == DialogResult.Yes)
                {
                    save_settings();
                    main_form.InitializeProperties();
                }
                if (dlgResult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void show_main_form() {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            uncheck_all();
            ftp.ftp_options connection_options = new ftp.ftp_options();
            connection_options.login = ftplogin_textBox.Text;
            connection_options.password = ftppassword_textBox.Text;
            connection_options.host = host_textBox.Text;
            connection_options.path = folder_textBox.Text;
            ftp connection = new ftp();

            var connection_status=connection.check_connection(connection_options);
            if (connection_status.no_error)
            {
                pictureBox1.ImageLocation = path + "\\img\\check.png";
                pictureBox2.ImageLocation = path + "\\img\\check.png";
                pictureBox3.ImageLocation = path + "\\img\\check.png";
            }
            else {
                switch (connection_status.error_status) {
                    case 2: pictureBox1.ImageLocation = path + "\\img\\check.png"; break;
                    case 3: pictureBox1.ImageLocation = path + "\\img\\check.png"; pictureBox2.ImageLocation = path + "\\img\\check.png"; break;
                }
            }

            if (connection_status.no_error) {
                var list=connection.file_list(connection_options);
                if (list.Count > 0) {
                    files_listBox.DataSource = list;
                    pictureBox4.ImageLocation = path + "\\img\\check.png";
                }
            }

        }

        private void uncheck_all() {
            pictureBox1.ImageLocation = path + "\\img\\cross.png";
            pictureBox2.ImageLocation = path + "\\img\\cross.png";
            pictureBox3.ImageLocation = path + "\\img\\cross.png";
            pictureBox4.ImageLocation = path + "\\img\\cross.png";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = path;
            openFileDialog1.Filter = "txt files (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                idlist_path.Text = openFileDialog1.FileName;
            }
        }

        private void host_textBox_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void ftplogin_textBox_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void ftppassword_textBox_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void folder_textBox_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void vlogin_textBox_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void vpassword_textBox_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void time_interval_ValueChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void idlist_path_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void windowName_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void mysqlServer_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void mysqlLogin_path_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void mysqlPassword_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void mysqlDBName_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void mysqlTable_TextChanged(object sender, EventArgs e)
        {
            DataSaved = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                logPath.Text = folderDialog.SelectedPath;
            }
        }
    }
}
