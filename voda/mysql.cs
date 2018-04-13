using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace voda
{
    public class Mysql_options
    {
        public string host { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string dbname { get; set; }
        public string table { get; set; }

        public Mysql_options()
        {
            host = String.Empty;
            login = String.Empty;
            password = String.Empty;
            dbname = String.Empty;
            table = String.Empty;
        }
    }

    class MysqlControl
    {
        private Mysql_options con_opt;
        private string Connection_string = String.Empty;
        public bool mysql_status { get; set; }

        public MysqlControl()
        {
            con_opt = new Mysql_options();
        }

        public MysqlControl(Mysql_options options)
        {
            con_opt = options;
            mysql_status = false;
            if (check_options(options))
            {
                Connection_string = "Database=" + options.dbname + ";Data Source=" + options.host + ";User Id=" + options.login + ";Password=" + options.password;
                if (check_connection())
                {
                    mysql_status = true;
                }
            }
        }

        private bool check_options(Mysql_options options)
        {
            bool status = true;
            if (options.host == String.Empty || options.host == "")
            {
                status = false;
            }
            if (options.login == String.Empty || options.login == "")
            {
                status = false;
            }
            if (options.password == String.Empty || options.password == "")
            {
                status = false;
            }
            if (options.dbname == String.Empty || options.dbname == "")
            {
                status = false;
            }
            return status;
        }

        public bool check_connection()
        {
            if (check_options(con_opt))
            {
                try
                {
                    MySqlConnection myConnection = new MySqlConnection(Connection_string);
                    myConnection.Open();
                    myConnection.Close();
                    mysql_status = true;
                    return true;

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            mysql_status = false;
            return false;
        }

        public bool tableExists(string table_name)
        {
            bool exists = false;
            if (check_options(con_opt))
            {
                try
                {
                    string Command = "SELECT EXISTS(SELECT `TABLE_NAME` FROM `information_schema`.`TABLES` WHERE (`TABLE_NAME` = '"+table_name+"') AND (`TABLE_SCHEMA` = '"+con_opt.dbname+"')) as `is- exists`;";
                    MySqlConnection myConnection = new MySqlConnection(Connection_string);
                    MySqlCommand myCommand = new MySqlCommand(Command, myConnection);
                    myConnection.Open();
                    using (MySqlDataReader MyDataReader = (MySqlDataReader)myCommand.ExecuteReader())
                    {
                        while (MyDataReader.Read())
                        {
                            if (MyDataReader["is- exists"].ToString()=="1") {
                                exists = true;
                            }
                        }
                    }
                    myConnection.Close();
                    mysql_status = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    mysql_status = false;
                }
            }
            return exists;
        }

        public void createTable(string table) {
            if (check_options(con_opt))
            {
                try
                {
                    string Command = @"CREATE TABLE "+table+ "(id INT NOT NULL, id_nak VARCHAR(20) NULL, val VARCHAR(20) NULL, date VARCHAR(40) NULL, unixtime VARCHAR(10) NULL, id_vod VARCHAR(20) NULL, nzav VARCHAR(20) NULL, errors VARCHAR(10) NULL,send VARCHAR(1) NULL, main_title VARCHAR(10) NOT NULL, full_count VARCHAR(10),  PRIMARY KEY (id), not_in_xml VARCHAR(10000) NOT NULL, duplicates_in_xml VARCHAR(10000) NOT NULL)";
                    MySqlConnection myConnection = new MySqlConnection(Connection_string);
                    MySqlCommand myCommand = new MySqlCommand(Command, myConnection);
                    myConnection.Open();
                    myCommand.ExecuteNonQuery();
                    myConnection.Close();
                    mysql_status = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        public void insertData(string id,string id_nak,string val,string date,string unixtime,string id_vod,string nzav,string errors,string send,string title) {
            //Debugger.Break();
            if (check_options(con_opt)&&(con_opt.table!=String.Empty))
            {
                try
                {
                    bool stop = false;
                    if (errors == "") { errors = "0"; }
                    //errors=errors.Replace(',', '|');
                    if (id_nak == "") { id_nak = "0"; }
                    if (val == "") { val = "0"; }
                    if (id_vod == "") { id_vod = "0"; }
                    if (nzav == "") { nzav = "0"; }
                    string Command = @"INSERT INTO `"+con_opt.table+ "`(`id`, `id_nak`, `val`, `date`, `unixtime`, `id_vod`, `nzav`, `errors`,`send`,`main_title`) VALUES (" + id+","+id_nak+","+val+",'"+date+"',"+unixtime+","+id_vod+","+nzav+",'"+errors+"',"+send+",'"+title+"')";
                    //Debugger.Break();
                    MySqlConnection myConnection = new MySqlConnection(Connection_string);
                    MySqlCommand myCommand = new MySqlCommand(Command, myConnection);
                    myConnection.Open();
                    myCommand.ExecuteNonQuery();
                    myConnection.Close();
                    mysql_status = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
        public void insertErrors(string err, string copy)
        {
            if (check_options(con_opt) && (con_opt.table != String.Empty))
            {
                try
                {
                    bool stop = false;
                    if (err == "") { err = "0"; }
                    if (copy == "") { copy = "0"; }
                    string Command = @"INSERT INTO `"+con_opt.table+ "`(`not_in_xml`, `duplicates_in_xml`) VALUES ('" + err + "','" + copy + "')";
                    //Debugger.Break();
                    MySqlConnection myConnection = new MySqlConnection(Connection_string);
                    MySqlCommand myCommand = new MySqlCommand(Command, myConnection);
                    myConnection.Open();
                    myCommand.ExecuteNonQuery();
                    myConnection.Close();
                    mysql_status = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        public void clearTable() {
            if (check_options(con_opt) && (con_opt.table != String.Empty))
            {
                try
                {
                    string Command = @"TRUNCATE "+con_opt.table+"";
                    MySqlConnection myConnection = new MySqlConnection(Connection_string);
                    MySqlCommand myCommand = new MySqlCommand(Command, myConnection);
                    myConnection.Open();
                    myCommand.ExecuteNonQuery();
                    myConnection.Close();
                    mysql_status = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

    }
}
