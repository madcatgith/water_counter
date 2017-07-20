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

        public Form2()
        {
            InitializeComponent();
            read_settings();
            uncheck_all();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            save_settings();
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
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
            var dlgResult = MessageBox.Show("Сохранить настройки перед выходом?", "", MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
            
            if (dlgResult == System.Windows.Forms.DialogResult.No)
            {
                //this.Close();
            }
            if (dlgResult == DialogResult.Yes)
            {
                save_settings();
                //this.Close();
            }
            if (dlgResult == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            
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
    }
}
