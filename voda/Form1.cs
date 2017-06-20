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
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Threading;

namespace voda
{
    public partial class Form1 : Form
    {
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

        public Form1()
        {
            InitializeComponent();
            ReadIdFile("idlist.txt");
            //dataGridView1.ColumnCount = 3;
            /*dataGridView1[0,0].Value = "id";
            dataGridView1[1,0].Value = "val";
            dataGridView1[2,0].Value = "date";*/
        }

        public Mbus mbus = new Mbus();

        /*public static int normval;
        public static int id;
        public static string m_date;*/
        public static int row = 0;
        public static bool test_mode = true;
        //public static int readerrors = 0;

        public static List<int[]> idList = new List<int[]>();
        public static List<CounterData> ToSend = new List<CounterData>();
        public static List<int> duplicateList = new List<int>();

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

                                        ToSend.Add(new CounterData()
                                        {
                                            ID = idrow[2],
                                            UNIXTIME = ConvertToUnixTime(dt),
                                            POKAZ = mbus.normval,
                                            NZAV = Convert.ToString(idrow[1])
                                        });
                                    }
                                    row++;
                                }
                                else
                                {
                                    duplicates++;
                                    duplicateList.Add(mbus.id);
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
                    ToConsole("Список дубликатов:");
                    showDuplicates();
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
            foreach (int id in duplicateList) {
                ToConsole(id.ToString());
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

        public void ReadIdFile(string path) {
            int counter = 0;
            string line;

            StreamReader file =
            new StreamReader(path);
            while ((line = file.ReadLine()) != null)
            {
                
                string[] line_array=line.Split('=');
                int[] idrow = new int[3];
                for (int i=0;i<3;i++) {
                    idrow[i] = Int32.Parse(line_array[i].Replace("=",String.Empty));
                }
                idList.Add(idrow);
                counter++;
            }

            file.Close();
            Debug.WriteLine("List Imported");
            ToConsole("Список идентефикаторов загружен");
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

        public static int[] FindIds(int m_id)
        {
            for (int i = 0; i < idList.Count; i++)
            {
                if (idList[i][0]==m_id) {
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
            mbus.readerrors = 0;
            dataGridView1.Rows.Clear();
            row = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SendToVodokanal();
        }

        private bool SendToVodokanal() {
            string login = "qwerty";
            string password = "123456";

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
                return success;
            }
            else{
                return false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Process_Data();
            button7.Enabled = true;
            button6.Enabled = false;
            label2.Text = "0";
            timer1.Interval = 60000;
            timer1.Start();
        }

        private void Process_Data() {
            clear_all_data();
            bool filestate = false;
            string[] filenames = ftp_list_last();
            foreach (string filename in filenames)
            {
                ToConsole(filename);
                if (ftp_download(filename))
                {
                    string fullpath = System.IO.Directory.GetCurrentDirectory() + "\\" + filename;
                    xml_parse(fullpath);
                }
            }
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
                int[] lastfile = new int[2];
                string[] last = new string[2];
                foreach (string l_item in list)
                {
                    string val = l_item.Replace(" ", "");
                    string[] name = val.Split(':');

                    if ((name[1].IndexOf("0080A3B46769") > -1) && (name[1].IndexOf(".xml") > -1))
                    {
                        string[] f_name = name[1].Trim().Split('_');

                        //Debug.WriteLine();
                        if (lastfile[0] != null)
                        {
                            if (Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4)) > lastfile[0])
                            {
                                lastfile[0] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                            }
                        }
                        else
                        {
                            lastfile[0] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                        }

                    }
                    else if ((name[1].IndexOf("0080A3B82B1E") > -1) && (name[1].IndexOf(".xml") > -1))
                    {
                        string[] f_name = name[1].Trim().Split('_');
                        if (lastfile[1] != null)
                        {
                            if (Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4)) > lastfile[1])
                            {
                                lastfile[1] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                            }
                        }
                        else
                        {
                            lastfile[1] = Int32.Parse(f_name[1].Remove(f_name[1].Length - 4, 4));
                        }
                    }
                    //ToConsole(name[1]);
                }
                //ToConsole(lastfile[0].ToString());
                last[0] = "0080A3B46769_" + lastfile[0].ToString().PadLeft(3,'0') + ".xml";
                //ToConsole(lastfile[1].ToString());
                last[1] = "0080A3B82B1E_" + lastfile[1].ToString().PadLeft(3,'0') + ".xml";
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
            Process_Data();
            label2.Text = (Int32.Parse(label2.Text) + 1).ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            button7.Enabled = false;
            button6.Enabled = true;
        }
    }
}
