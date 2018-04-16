using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Threading;
using MySql.Data.MySqlClient;

namespace voda
{
    public partial class Form1 : Form
    {
        Mysql_options mysql_options = new Mysql_options();
        string mysql_table_name=String.Empty;

        IniFile INI = new IniFile("config.ini");
        string path = AppDomain.CurrentDomain.BaseDirectory;

        ftp ftp = new ftp();
        ftp.ftp_options options = new ftp.ftp_options();

        string idlistpath = String.Empty;
        string logpath = String.Empty;

        VodokanalAuth v_options = new VodokanalAuth();

        bool LoadedValidList = false;
        bool Autostart = false;
        string main_title = "Default Title";
        /*options.host = "176.111.58.218";
        options.login = "water_counter";
        options.password = "WATERcounter";
        options.path = "test_my";*/

        public class CounterData

        {

            /// <summary>

            /// Уникальный номер места размещения счетчика.

            /// </summary>

            public int ID;

            /// <summary>

            /// Время в формате UNIXTIME.

            /// </summary>

            public int UNIXTIME;

            /// <summary>

            /// Показания счетчика.

            /// </summary>

            public double POKAZ;

            /// <summary>

            /// //Заводской номер счетчика.

            /// </summary>

            public string NZAV;

        }

        public class ErrorData
        {
            /// <summary>
            /// Уникальный номер места размещения счетчика.
            /// </summary>
            public int ID;
            /// <summary>
            /// Цифровой номер ошибки из справочника
            /// </summary>
            public int ERR_NO;
            /// <summary>
            /// Время в формате UNIXTIME.
            /// </summary>
            public int UNIXTIME;
            /// <summary>
            /// //Заводской номер счетчика.
            /// </summary>
            public string NZAV;
        }

        public class VodokanalAuth
        {
            public string login { get; set; }
            public string password { get; set; }
            public string interval { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeProperties();
        }

        public Form1(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg == "-Auto")
                {
                    Autostart = true;
                }
            }
            InitializeComponent();
            InitializeProperties();
        }

        public Mbus mbus = new Mbus();

        /*public static int normval;
        public static int id;
        public static string m_date;*/
        public static int row = 0;
        public static bool test_mode = false;
        public static string Not_in_xml = "";
        public static string douplicates_in_xml = "";
        //public static int readerrors = 0;

        public static List<int[]> idList = new List<int[]>();
        public static List<CounterData> ToSend = new List<CounterData>();
        public static List<ErrorData> ErrorsToSend= new List<ErrorData>();
        public static List<int> duplicateList = new List<int>();

        public void InitializeProperties() {
            clear_all_data();
            read_settings();
            idList.Clear();
            richTextBox1.Clear();
            if (idlistpath != String.Empty)
            {
                if (File.Exists(idlistpath))
                {
                    LoadedValidList = ReadIdFile(idlistpath);
                    if (Autostart)
                    {
                        button6_Click(this,new EventArgs());
                    }
                }
                if (!LoadedValidList)
                    ToConsole("Список идентификаторов не загружен!!!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clear_all_data();
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "d:\\Показания счетчиков\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            string sourceFile = openFileDialog1.FileName;
                            string fullPath = openFileDialog1.FileName;
                            string fileName = openFileDialog1.SafeFileName;
                            xml_parse(fullPath);
                            FoundNotConnected();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private bool xml_parse(string filepath){
            try
            {
                List<string> errors_to_log = new List<string>();
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(filepath);
                // получим корневой элемент
                XmlElement xRoot = xDoc.DocumentElement;
                XmlNode mem = xRoot.SelectSingleNode("MEM");
                mbus.readerrors = 0;
                int duplicates = 0;
                duplicateList.Clear();
                foreach (XmlNode childnode in mem.ChildNodes)
                {
                    foreach (XmlNode child in childnode.ChildNodes)
                    {
                        if (child.Name == "MBTIME")
                        {
                            byte[] timestamp = mbus.StringToByteArray(child.InnerText);
                            mbus.m_date = mbus.convertDate(timestamp);
                            //Debug.WriteLine(mbus.m_date);

                        }
                        if (child.Name == "MBTEL")
                        {
                            if (mbus.decodeTelegram(child.InnerText))
                            {
                                //
                                if (checkDuplicates(mbus.id))
                                {
                                    var dt = DateTime.ParseExact(mbus.m_date, "HH:mm dd.MM.yy", CultureInfo.InvariantCulture);
                                    dataGridView1.Rows.Add();
                                    dataGridView1[0, row].Value = row + 1;
                                    dataGridView1[1, row].Value = mbus.id;
                                    dataGridView1[2, row].Value = mbus.normval;
                                    dataGridView1[3, row].Value = mbus.m_date;
                                    dataGridView1[4, row].Value = ConvertToUnixTime(dt);

                                    

                                    int[] idrow = FindIds(mbus.id);
                                    if (idrow[0] != 0)
                                    {
                                        dataGridView1[5, row].Value = idrow[2];
                                        dataGridView1[6, row].Value = idrow[1];
                                        dataGridView1[8, row].Value = "1";
                                        double m3val = (double)mbus.normval / 1000;
                                        //Debug.WriteLine(m3val);
                                        ToSend.Add(new CounterData()
                                        {
                                            ID = idrow[2],
                                            UNIXTIME = ConvertToUnixTime(dt),
                                            POKAZ = m3val,//mbus.normval,
                                            NZAV = Convert.ToString(idrow[1])
                                        });

                                        //Блок ошибок (вывод в таблицу)
                                        string show_errors = String.Empty;
                                        List<int> error_list = new List<int>(mbus.errors_decoded_apt);

                                        //Debug.WriteLine(mbus.errors_decoded_apt.Count);
                                        //Debug.WriteLine(error_list.Count);

                                        //Debugger.Break();
                                        //

                                        foreach (int err in error_list)
                                        {
                                            //Debug.WriteLine(err);
                                            switch (err)
                                            {
                                                case 1:
                                                    show_errors = show_errors + 1 + ",";
                                                    ErrorsToSend.Add(new ErrorData
                                                    {
                                                        ID = idrow[2],
                                                        UNIXTIME = ConvertToUnixTime(dt),
                                                        NZAV = Convert.ToString(idrow[1]),
                                                        ERR_NO = 1
                                                    });
                                                    errors_to_log.Add(idrow[0].ToString() + " - влияние магнитного поля");
                                                    break;
                                                case 4:
                                                    show_errors += 2 + ",";
                                                    ErrorsToSend.Add(new ErrorData
                                                    {
                                                        ID = idrow[2],
                                                        UNIXTIME = ConvertToUnixTime(dt),
                                                        NZAV = Convert.ToString(idrow[1]),
                                                        ERR_NO = 2
                                                    });
                                                    //Debugger.Break();
                                                    errors_to_log.Add(idrow[0].ToString() + " - влияние на датчик");
                                                    break;
                                                case 64:
                                                    show_errors += 3 + ",";
                                                    ErrorsToSend.Add(new ErrorData
                                                    {
                                                        ID = idrow[2],
                                                        UNIXTIME = ConvertToUnixTime(dt),
                                                        NZAV = Convert.ToString(idrow[1]),
                                                        ERR_NO = 3
                                                    });
                                                    errors_to_log.Add(idrow[0].ToString() + " - обратный поток");
                                                    break;
                                            }
                                        }
                                        dataGridView1[7, row].Value = show_errors;
                                        //--------------------------------------------------
                                    }
                                    else { dataGridView1[8, row].Value = "0"; }
                                    row++;
                                }
                                else
                                {
                                    duplicates++;
                                    duplicateList.Add(mbus.id);                                    
                                    //Debugger.Break();
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                }
                ToConsole("Ошибок обработки: " + mbus.readerrors);
                ToConsole("Дубликатов в файле найдено:" + duplicates);
                if (duplicates > 0)
                {
                    //Debugger.Break();
                    ToConsole("Список дубликатов:");
                    showDuplicates();
                }
                if (errors_to_log.Count > 0) {
                    errors_to_log.Insert(0,"Ошибки:");
                    write_to_log(errors_to_log);
                }
                return true;
            }
            catch (Exception ex) {
                ToConsole(ex.ToString());
                return false;
            }
        }

        private bool checkDuplicates(int id) {
            for (int i = 0; i < dataGridView1.RowCount; i++) {
                if (id == Int32.Parse(dataGridView1[1, i].Value.ToString())) {
                    return false;
                }
            }
            return true;
        }

        private void showDuplicates() {
            List<string> log = new List<string>();
            foreach (int id in duplicateList) {
               // Debugger.Break();
                douplicates_in_xml += "|" + Convert.ToString(id);
                ToConsole(id.ToString());
                log.Add(id.ToString());
            }
            if (log.Count > 0) {
                log.Insert(0,"Дубликаты");
                write_to_log(log);
            }
            
        }


        private void button2_Click(object sender, EventArgs e)
        {
            ShowJsonList();
        }

        public static int ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (int)(datetime.ToUniversalTime() - sTime).TotalSeconds;
        }

        public bool ReadIdFile(string path) {
            int counter = 0;
            string line;

            try
            {

                StreamReader file =
                new StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {

                    string[] line_array = line.Split('=');
                    int[] idrow = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        idrow[i] = Int32.Parse(line_array[i].Replace("=", String.Empty));
                    }
                    foreach (int[] id in idList)
                    {
                        if (id[0] == idrow[0]) {
                            Debug.WriteLine(idrow[0]);
                            ToConsole("Дубликат в списке идентификаторов "+idrow[0]);                           
                        }
                    }
                    idList.Add(idrow);
                    counter++;
                }
                
                //Debugger.Break();
                file.Close();
                Debug.WriteLine("List Imported");
                ToConsole("Список идентефикаторов загружен");
                return true;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public static void ShowIdList() {
            for (int i=0; i<idList.Count; i++)
            {
                Debug.WriteLine(idList[i][0] + " " + idList[i][1] + " " + idList[i][2]);
            }
        }

        public void ToConsole(string text) {
            richTextBox1.AppendText(text + Environment.NewLine);
        }

        public static int[] FindIds(int m_id,bool debug=false)
        {
            for (int i = 0; i < idList.Count; i++)
            {
                if (idList[i][0]==m_id) {
                    if (debug)
                    {
                        Debugger.Break();
                    }
                    return idList[i];
                }
            }
            return new int[3];
        }

        public static void SendData() {
            
        }

        public void ShowJsonList() {
            richTextBox1.AppendText("Данные для передачи" + Environment.NewLine);
            foreach (CounterData val in ToSend) {
                //Debug.WriteLine(val.NZAV);
                richTextBox1.AppendText("<!---Начало пакета-->"+Environment.NewLine);
                richTextBox1.AppendText(val.ID.ToString() + Environment.NewLine);
                richTextBox1.AppendText(val.NZAV.ToString() + Environment.NewLine);
                richTextBox1.AppendText(val.POKAZ.ToString() + Environment.NewLine);
                richTextBox1.AppendText(val.UNIXTIME.ToString() + Environment.NewLine);
                richTextBox1.AppendText("<!---Конец пакета-->"+Environment.NewLine);
            }

            ToConsole(ToSend.Count.ToString());
            richTextBox1.AppendText("Блок ошибок" + Environment.NewLine);
            foreach (ErrorData err in ErrorsToSend) {
                richTextBox1.AppendText("<!---Начало пакета-->" + Environment.NewLine);
                richTextBox1.AppendText(err.ID.ToString() + Environment.NewLine);
                richTextBox1.AppendText(err.NZAV.ToString() + Environment.NewLine);
                richTextBox1.AppendText(err.ERR_NO.ToString() + Environment.NewLine);
                richTextBox1.AppendText(err.UNIXTIME.ToString() + Environment.NewLine);
                richTextBox1.AppendText("<!---Конец пакета-->" + Environment.NewLine);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            duplicateList.Clear();
            ToSend.Clear();
            mbus.readerrors = 0;
            dataGridView1.Rows.Clear();
            //dataGridView1.Rows.Remove
            row = 0;
        }

        private void clear_all_data() {
            duplicateList.Clear();
            ToSend.Clear();
            ErrorsToSend.Clear();
            mbus.readerrors = 0;
            dataGridView1.Rows.Clear();
            row = 0;
            douplicates_in_xml = "";
            Not_in_xml = "";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //SendToVodokanal();
            //string data = "6856566808CF729254090001060307D27800000C7892540900046D022C3426041353000000023B0000441353000000426C21260227890003FD170E030804FF0A0201040002FF0B905803FF0C0A00BD0F00030D24FF00020201075216";
            //mbus.telegram_decode(data);
            //getVodokanalErrors();
            //ftp.get_last_test(options);
            //Debugger.Break();
        }

        private void DownloadAndParse() {
            clear_all_data();
            List<string> files = ftp.get_last_files(ftp.file_list(options), ftp.get_files_list(options), options);
            List<ftp.ftp_download> file_status = ftp.download_files(files, options);
            List<string> parsed_files = new List<string>();
            foreach (ftp.ftp_download downloaded in file_status)
            {
                if (downloaded.transfer_succ)
                {
                    ToConsole(downloaded.filename+" файл успешно загружен");
                    string fullpath = System.IO.Directory.GetCurrentDirectory() + "\\archive\\" + downloaded.filename;
                    xml_parse(fullpath);
                    parsed_files.Add(downloaded.filename);
                }
            }
            FoundNotConnected();
            ftp.delete_processed(parsed_files);
            //Debugger.Break();
            ftp.archivate_parsed(parsed_files,options);
            ToConsole("Сформировано данных по " + ToSend.Count() + " счетчикам");
            if (!test_mode)
            {
                if (ToSend.Count > 0)
                {
                    if (SendToVodokanal())
                    {
                        ToConsole("Данные успешно отправлены по " + ToSend.Count + " счетчикам");
                    }
                }
            }
        }

        private bool SendToVodokanal() {
            try
            {
                //string login = "vertical";
                //string password = "Lacitrev";

                string login = v_options.login;
                string password = v_options.password;

                // Упаковщик в формат JSON
                var serializer = new JavaScriptSerializer();

                // Клиент web-сервиса
                var webClient = new VodokanalService.WebServiceSoapClient("WebServiceSoap");
                // Выполнить проверку логина и пароля
                var loginRequstJson = webClient.LoginEx(login, password);

                // Вывести результат к консоль
                Console.WriteLine(loginRequstJson);

                // Распаковать строку с результатами аутентификации
                var loginRequest = (Dictionary<string, object>)serializer.Deserialize(loginRequstJson, typeof(Dictionary<string, object>));

                // Успешность аутентификации
                bool success = (bool)loginRequest["Success"];


                if (success)
                {
                    ToConsole("Аутентификация успешна");
                    // Временный билет
                    string ticket = (string)loginRequest["Ticket"];
                    ToConsole(ticket);

                    //Передадим данные с показаниями на сервер
                    //Для примера я взял данные по двум счетчикам с потолка

                    //Упакуем их в формат JSON
                    //Формат передачи такой: {"jsons":[{"ID":1,"UNIXTIME":234234234,"POKAZ":456.88,"NZAV":"qwr23r2r"},{"ID":2,"UNIXTIME":234254234,"POKAZ":5656.05,"NZAV":"qwr2dswfr2r"}]}
                    var dict = new Dictionary<string, object>();
                    dict.Add("jsons", ToSend);
                    string JsonResult = serializer.Serialize(dict);

                    //Передадим на сервер
                    var JsonAnswer = webClient.ExecuteEx("_ACCXB.ADDCOUNTERVALUES", JsonResult, ticket);

                    //Посмотрим на ответ сервера
                    var answer = (Dictionary<string, object>)serializer.Deserialize(JsonAnswer, typeof(Dictionary<string, object>));

                    //Успешность передачи данных
                    success = (bool)answer["SUCCESS"];

                    if (ErrorsToSend.Count > 0)
                    {
                        var err_dict = new Dictionary<string, object>();
                        err_dict.Add("jsons", ErrorsToSend);
                        string JsonErrorResult = serializer.Serialize(err_dict);
                        var JsonErrorAnswer = webClient.ExecuteEx("_ACCXB.ADDERRORVALUES", JsonErrorResult, ticket);
                        var err_answer = (Dictionary<string, object>)serializer.Deserialize(JsonErrorAnswer, typeof(Dictionary<string, object>));
                        success = (bool)err_answer["SUCCESS"];
                    }
                    return success;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                ToConsole(ex.ToString());
                write_to_log(new string[] {ex.ToString()} );
                return false;
            }
        }

        private void getVodokanalErrors()
        {
            //string login = "vertical";
            //string password = "Lacitrev";

            string login = v_options.login;
            string password = v_options.password;

            // Упаковщик в формат JSON
            var serializer = new JavaScriptSerializer();

            // Клиент web-сервиса
            var webClient = new VodokanalService.WebServiceSoapClient("WebServiceSoap");
            // Выполнить проверку логина и пароля
            var loginRequstJson = webClient.LoginEx(login, password);

            // Вывести результат к консоль
            Console.WriteLine(loginRequstJson);

            // Распаковать строку с результатами аутентификации
            var loginRequest = (Dictionary<string, object>)serializer.Deserialize(loginRequstJson, typeof(Dictionary<string, object>));

            // Успешность аутентификации
            bool success = (bool)loginRequest["Success"];


            if (success)
            {
                string ticket = (string)loginRequest["Ticket"];
                var JsonAnswer = webClient.ExecuteEx("_ACCXB.GETERRORS", null, ticket);
                //Смотрим что пришло
                var answer = (Dictionary<string, object>)serializer.Deserialize(JsonAnswer, typeof(Dictionary<string, object>));
                foreach (var item in answer) {
                    ToConsole(item.ToString());
                }
            }
       }

        private void button6_Click(object sender, EventArgs e)
        {
            //Process_Data();
            if (LoadedValidList)
            {
                DownloadAndParse();
                button7.Enabled = true;
                button6.Enabled = false;
                label2.Text = "1";
                int timeout = 3600000;
                string[] interval = v_options.interval.Split(':');
                if (interval.Length == 2)
                {
                    timeout = ((Int32.Parse(interval[0]) * 60 * 60) + (Int32.Parse(interval[1]) * 60)) * 1000;
                    Debug.WriteLine(timeout);
                }
                load_mysql_logs();
                timer1.Interval = timeout;
                timer1.Start();
            }
            else {
                ToConsole("Загрузите список идентификаторов");
            }
        }

        private void Process_Data() {
            clear_all_data();
            bool filestate = false;
            string[] filenames = ftp_list_last();
            foreach (string filename in filenames)
            {
                if (filename != "" && filename!=String.Empty && filename!=null)
                {
                    //Debug.WriteLine(filename);
                    ToConsole(filename);
                    if (ftp_download(filename))
                    {
                        string fullpath = System.IO.Directory.GetCurrentDirectory() + "\\" + filename;
                        xml_parse(fullpath);
                    }
                }
            }
            FoundNotConnected();
            ToConsole("Сформировано данных по "+ToSend.Count()+" счетчикам");
            if (!test_mode)
            {
                if (ToSend.Count > 0)
                {
                    if (SendToVodokanal())
                    {
                        ToConsole("Данные успешно отправлены по " + ToSend.Count + " счетчикам");
                    }
                }
            }
        }

        //Получить последние 2 файла
        private string[] ftp_list_last()
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://176.111.58.218/gmiri/");
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                string username = "water_counter";
                string password = "WATERcounter";
                request.Credentials = new NetworkCredential(username.Normalize(), password.Normalize());
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                string[] list = null;
                //Stream responseStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    list = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                }
                int[] lastfile = new int[3];
                string[] last = new string[3];
                foreach (string l_item in list)
                {
                    string val = l_item.Replace(" ", "");
                    string[] name = val.Split(':');
                    name[1] = name[1].Remove(0,2);
                    if ((name[1].IndexOf("0080A3B46769") > -1) && (name[1].IndexOf(".xml") > -1))
                    {
                        string[] f_name = name[1].Trim().Split('_');


                        if (lastfile[0] != null)
                        {
                            if (Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4)) > lastfile[0])
                            {

                                if (isRDY(list, name[1]))
                                {
                                    lastfile[0] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                                }
                            }
                        }
                        else
                        {
                            if (isRDY(list, name[1]))
                            {
                                lastfile[0] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                            }
                        }

                    }
                    else if ((name[1].IndexOf("0080A3B82B1E") > -1) && (name[1].IndexOf(".xml") > -1))
                    {
                        string[] f_name = name[1].Trim().Split('_');
                        if (lastfile[1] != null)
                        {
                            if (Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4)) > lastfile[1])
                            {
                                if (isRDY(list, name[1]))
                                {
                                    lastfile[1] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                                }
                            }
                        }
                        else
                        {
                            if (isRDY(list, name[1]))
                            {
                                lastfile[1] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                            }
                        }
                    }
                    /*else if ((name[1].IndexOf("0080A3B82B12") > -1) && (name[1].IndexOf(".xml") > -1))
                    {
                        string[] f_name = name[1].Trim().Split('_');
                        if (lastfile[1] != null)
                        {
                            if (Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4)) > lastfile[1])
                            {
                                if (isRDY(list, name[1]))
                                {
                                    lastfile[2] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                                }
                            }
                        }
                        else
                        {
                            if (isRDY(list, name[1]))
                            {
                                lastfile[2] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                            }
                        }
                    }*/
                    //ToConsole(name[1]);
                }
                //ToConsole(lastfile[0].ToString());
                //last[0] = "0080A3B462CA_" + lastfile[0].ToString().PadLeft(3,'0') + ".xml";
                last[0] = "0080A3B46769_" + lastfile[0].ToString().PadLeft(3, '0') + ".xml";
                //ToConsole(lastfile[1].ToString());
                //last[1] = "0080A3B46770_" + lastfile[1].ToString().PadLeft(3,'0') + ".xml";
                last[1] = "0080A3B82B1E_" + lastfile[1].ToString().PadLeft(3, '0') + ".xml";
                //ToConsole(lastfile[2].ToString());
                //last[2] = "0080A3B82B12_" + lastfile[2].ToString().PadLeft(3,'0') + ".xml";
                return last;
            }
            catch (Exception ex) {
                ToConsole(ex.ToString());
                return new string[2];
            }
        }

        //Скачать файлы с фтп
        private bool ftp_download(string filename) {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://176.111.58.218/gmiri/"+filename);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            string username = "water_counter";
            string password = "WATERcounter";
            request.Credentials = new NetworkCredential(username.Normalize(), password.Normalize());

            // устанавливаем метод на загрузку файлов
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            
            // создаем поток для загрузки файла
            if (File.Exists(filename))
            {
                // Note that no lock is put on the
                // file and the possibility exists
                // that another process could do
                // something with it between
                // the calls to Exists and Delete.
                File.Delete(filename);
            }

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream ftpStream = response.GetResponseStream();

            FileStream localFileStream = new FileStream(filename, FileMode.Create);
            int bufferSize = 2048;
            byte[] byteBuffer = new byte[bufferSize];
            int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);

            try
            {
                while (bytesRead > 0)
                {
                    localFileStream.Write(byteBuffer, 0, bytesRead);
                    bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                }
            }

            catch (Exception) { }

            localFileStream.Close();
            ftpStream.Close();
            response.Close();
            request = null;

            // получаем ответ от сервера в виде объекта FtpWebResponse
            //FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            ToConsole("Загрузка файлов завершена. Статус: "+response.StatusDescription);
            if (response.StatusDescription.IndexOf("226") > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Process_Data();
            Debug.WriteLine("tick");
            DownloadAndParse();
            label2.Text = (Int32.Parse(label2.Text) + 1).ToString();
            load_mysql_logs();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            button7.Enabled = false;
            button6.Enabled = true;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<string> notFound = new List<string>();
            for (int i = 0; i < idList.Count; i++) {
                bool found = false;
                for (int j = 0; j < dataGridView1.RowCount; j++) {
                    if (idList[i][0].ToString() == dataGridView1[1,j].Value.ToString()) {
                        found = true;
                    }
                }
                if (!found) {
                    notFound.Add(idList[i][0].ToString());
                }
            }
            foreach (string not in notFound) {
                ToConsole(not);
            }
        }

        private void FoundNotConnected() {
            List<string> notFound = new List<string>();            
            for (int i = 0; i < idList.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {
                    if (idList[i][0].ToString() == dataGridView1[1, j].Value.ToString())
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    notFound.Add(idList[i][0].ToString());
                }
               
            }
            if (notFound.Count > 0)
            {
                ToConsole("Список неподключенных счетчиков:");
                foreach (string not in notFound)
                {
                    ToConsole(not);
                    Not_in_xml += "|" + not;
                    int[] idrow = FindIds(Convert.ToInt32(not));
                    if (idrow[0] != 0)
                    {
                        ErrorsToSend.Add(new ErrorData
                        {
                            UNIXTIME = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
                            ID = idrow[2],
                            NZAV = idrow[1].ToString(),
                            ERR_NO = 4
                        });
                    }
                }               
                notFound.Insert(0, "Список неподключенных счетчиков");                
                //Debugger.Break();
                //ToConsole("Список неподключенных счетчиков");
                write_to_log(notFound);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            List<string> notFound = new List<string>();
            for (int i = 0; i < dataGridView1.RowCount; i++) {
                bool found = false;
                Debug.WriteLine(i);
                foreach (CounterData data in ToSend) {
                    /*if (data.ID.ToString() == dataGridView1[5, i].Value.ToString()) {
                        found = true;
                    }*/
                }
                /*if (!found) {
                    notFound.Add(dataGridView1[5,i].Value.ToString());
                }*/
            }
            foreach (string not in notFound) {
                ToConsole(not);
            }
        }

        private bool isRDY(string[] list, string filename) {
            string rdyname = filename.Replace("xml","rdy");
            //Debug.WriteLine(rdyname);
            foreach (string item in list) {
                if (item.IndexOf(rdyname)>-1){
                    return true;
                }
            }
            return false;
        }

        private void настройкиToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Form2 option_form = new Form2(this);
            option_form.Show();
        }

        private void read_settings()
        {
            //Загрузка данных фтп
            if (INI.KeyExists("Host", "FTP"))
            {
                //host_textBox.Text = INI.ReadINI("FTP", "Host");
                options.host = INI.ReadINI("FTP", "Host");
            }
            if (INI.KeyExists("Login", "FTP"))
            {
                //ftplogin_textBox.Text = INI.ReadINI("FTP", "Login");
                options.login = INI.ReadINI("FTP", "Login");
            }
            if (INI.KeyExists("Password", "FTP"))
            {
                //ftppassword_textBox.Text = INI.ReadINI("FTP", "Password");
                options.password = INI.ReadINI("FTP", "Password");
            }
            if (INI.KeyExists("FolderPath", "FTP"))
            {
                //folder_textBox.Text = INI.ReadINI("FTP", "FolderPath");
                options.path = INI.ReadINI("FTP", "FolderPath");
            }

            //Загрузка настроек водоканала
            if (INI.KeyExists("Login", "KVK"))
            {
                //vlogin_textBox.Text = INI.ReadINI("KVK", "Login");
                v_options.login = INI.ReadINI("KVK", "Login");
            }
            if (INI.KeyExists("Password", "KVK"))
            {
                v_options.password = INI.ReadINI("KVK", "Password");
                //vpassword_textBox.Text = INI.ReadINI("KVK", "Password");
            }
            if (INI.KeyExists("Interval", "KVK"))
            {
                v_options.interval = INI.ReadINI("KVK", "Interval");
                //time_interval.Text = INI.ReadINI("KVK", "Interval");
            }

            if (INI.KeyExists("IdPath", "Setup")) {
                idlistpath = INI.ReadINI("Setup","IdPath");
            }

            if (INI.KeyExists("LogPath", "Setup"))
            {
                logpath = INI.ReadINI("Setup", "LogPath")+"\\logs";
            }

            if (INI.KeyExists("WindowName","Setup")) {
                this.Text = INI.ReadINI("Setup","WindowName");
                main_title = INI.ReadINI("Setup", "WindowName");
            }

            
            if (INI.KeyExists("Server","Mysql")) {
                mysql_options.host = INI.ReadINI("Mysql","Server");
            }
            if (INI.KeyExists("Login", "Mysql")) {
                mysql_options.login = INI.ReadINI("Mysql","Login");
            }
            if (INI.KeyExists("Password", "Mysql"))
            {
                mysql_options.password = INI.ReadINI("Mysql", "Password");
            }
            if (INI.KeyExists("DBName", "Mysql")){
                mysql_options.dbname = INI.ReadINI("Mysql", "DBName");
            }

            if (INI.KeyExists("Table", "Mysql")) {
                check_mysql_table(INI.ReadINI("Mysql","Table"));
            }

        }

        private void check_mysql_table(string table) {
            MysqlControl mysql_control = new MysqlControl(mysql_options);
            if (mysql_control.check_connection())
            {
                Debug.WriteLine("mysql connected");
                if (mysql_control.tableExists(table))
                {
                    Debug.WriteLine("table_exists");
                    mysql_options.table = table;
                }
                else
                {
                    Debug.WriteLine("no table");
                    mysql_control.createTable(table);
                    if (mysql_control.tableExists(table)) {
                        mysql_options.table = table;
                    }
                }
                //mysql_control.insertData(1,"1","1","0","1","1","1","1");
                //mysql_control.clearTable();
            }
        }

        private void load_mysql_logs() {
            //Debugger.Break();
            try
            {
                MysqlControl mysql_control = new MysqlControl(mysql_options);
                mysql_control.clearTable();
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    string test = dataGridView1[0,i].Value.ToString();
                    //Debugger.Break();
                    for (int j = 0; j < 8; j++) {
                        if (dataGridView1[j,i].Value==null)
                            dataGridView1[j, i].Value = "";
                    }
                    mysql_control.insertData(dataGridView1[0, i].Value.ToString(), dataGridView1[1, i].Value.ToString(), dataGridView1[2, i].Value.ToString(), dataGridView1[3, i].Value.ToString(), dataGridView1[4, i].Value.ToString(), dataGridView1[5, i].Value.ToString(), dataGridView1[6, i].Value.ToString(), dataGridView1[7, i].Value.ToString(),dataGridView1[8,i].Value.ToString(),main_title);
                }
                mysql_control.insertErrors(Not_in_xml, douplicates_in_xml);                
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        private void write_to_log(string[] lines) {
            try
            {
                string timestamp = DateTime.Now.ToString();
                string filename = DateTime.Today.ToString("d") + ".log";
                if (!Directory.Exists(logpath))
                {
                    Directory.CreateDirectory(logpath);
                }
                if (!File.Exists(logpath + "\\" + filename))
                {
                    var file = File.Create(logpath + "\\" + filename);
                    file.Close();
                }

                using (StreamWriter logfile = File.AppendText(logpath + "\\" + filename))
                {
                    logfile.WriteLine(timestamp);

                    foreach (string line in lines)
                    {
                        logfile.WriteLine(line);
                    }

                    logfile.Close();
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        private void write_to_log(List<string> lines)
        {
            try
            {
                string timestamp = DateTime.Now.ToString();
                string filename = DateTime.Today.ToString("d") + ".log";
                filename=filename.Replace('\\', '.');
                if (!Directory.Exists(logpath))
                {
                    Directory.CreateDirectory(logpath);
                }
                if (!File.Exists(logpath + "\\" + filename))
                {
                    var file = File.Create(logpath + "\\" + filename);
                    file.Close();
                }

                using (StreamWriter logfile = File.AppendText(logpath + "\\" + filename))
                {
                    logfile.WriteLine(timestamp);

                    foreach (string line in lines)
                    {
                        logfile.WriteLine(line);
                    }

                    logfile.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
}
