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

        public static int normval;
        public static int id;
        public static string m_date;
        public static int row = 0;
        public static int readerrors = 0;

        public static List<int[]> idList = new List<int[]>();
        public static List<CounterData> ToSend = new List<CounterData>();
        public static List<int> duplicateList = new List<int>();

        private void button1_Click(object sender, EventArgs e)
        {

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
                            //Debug.WriteLine(openFileDialog1.FileName);
                            string fileName = openFileDialog1.SafeFileName;
                            XmlDocument xDoc = new XmlDocument();
                            //Debug.WriteLine(fullPath);
                            xDoc.Load(fullPath);
                            // получим корневой элемент
                            XmlElement xRoot = xDoc.DocumentElement;
                            XmlNode mem = xRoot.SelectSingleNode("MEM");
                            readerrors = 0;
                            int duplicates = 0;
                            duplicateList.Clear();
                            foreach (XmlNode childnode in mem.ChildNodes) {
                                //richTextBox1.AppendText(childnode.Name+Environment.NewLine);


                                foreach (XmlNode child in childnode.ChildNodes) {
                                    if (child.Name == "MBTIME")
                                    {
                                        byte[] timestamp = StringToByteArray(child.InnerText);
                                        //richTextBox1.AppendText(convertDate(timestamp) + Environment.NewLine);
                                        m_date = convertDate(timestamp);

                                    }
                                    if (child.Name == "MBTEL") {
                                        if (decodeTelegram(child.InnerText))
                                        {
                                            //
                                            if (checkDuplicates(id))
                                            {
                                                var dt = DateTime.ParseExact(m_date, "HH:mm dd.MM.yy", CultureInfo.InvariantCulture);
                                                dataGridView1.Rows.Add();
                                                //richTextBox1.AppendText(id + ":" + normval + " l" + Environment.NewLine);
                                                dataGridView1[0, row].Value = row + 1;
                                                dataGridView1[1, row].Value = id;
                                                dataGridView1[2, row].Value = normval;
                                                dataGridView1[3, row].Value = m_date;
                                                dataGridView1[4, row].Value = ConvertToUnixTime(dt);
                                                int[] idrow = FindIds(id);
                                                if (idrow[0] != 0)
                                                {
                                                    dataGridView1[5, row].Value = idrow[2];
                                                    dataGridView1[6, row].Value = idrow[1];

                                                    ToSend.Add(new CounterData()
                                                    {
                                                        ID = idrow[2],
                                                        UNIXTIME = ConvertToUnixTime(dt),
                                                        POKAZ = normval,
                                                        NZAV = Convert.ToString(idrow[1])
                                                    });
                                                }
                                                row++;
                                            }
                                            else {
                                                duplicates++;
                                                duplicateList.Add(id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //richTextBox1.AppendText(child.InnerText + Environment.NewLine);
                                    }
                                }
                            }
                            ToConsole("Ошибок обработки: " + readerrors);
                            ToConsole("Дубликатов в файле найдено:" + duplicates);
                            if (duplicates > 0)
                            {
                                ToConsole("Список дубликатов:");
                                showDuplicates();
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
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

        private string convertDate(byte[] data) {
            //byte[] data = { 0x03,0x2C,0x28,0x26 };
            int minutes = data[0] & 0x3F;
            int hours = data[1] & 0x1F;
            int day = data[2] & 0x1F;
            int month = (data[3] & 0x0F);
            int year = ((data[2] & 0xE0) >> 5)|((data[3] & 0xF0) >> 1);
            //bool isdst = (data[1] & 0x80) ? 1 : 0;
            string date = hours.ToString().PadLeft(2,'0') + ":" + minutes.ToString().PadLeft(2, '0') + " " + day.ToString().PadLeft(2, '0') + "." + month.ToString().PadLeft(2, '0') + "." + year.ToString();
            return date;
        }

        private bool decodeTelegram(string data) {
            byte[] byteData=StringToByteArray(data);
            var mbus_header =  new byte[4];
            var mbus_afterhead = new byte[3];
            var mbus_id = new byte[4];
            int i;
            for (i=0; i < 4; i++) {
                mbus_header[i] = byteData[i];
            }
            if (validateHeader(mbus_header))
            {
                for (i = 4; i < 12; i++)
                {
                    if (i < 7)
                    {
                        mbus_afterhead[i - 4] = byteData[i];
                    }
                    else
                    {
                        if (i < 11)
                        {
                            mbus_id[i - 7] = byteData[i];
                        }
                    }

                }
                //debugHex(mbus_id.Reverse().ToArray());
                id = Convert.ToInt32(BitConverter.ToString(mbus_id.Reverse().ToArray()).Replace("-", String.Empty));
                //Debug.WriteLine(id);
                if (validateMan(new byte[] { byteData[11], byteData[12] }))
                {
                    byte[] val = new byte[] { byteData[33], byteData[34], byteData[35], byteData[36] };
                    //debugHex(val.Reverse().ToArray());
                    normval = Convert.ToInt32(BitConverter.ToString(val.Reverse().ToArray()).Replace("-", String.Empty), 16);
                    //Debug.WriteLine(normval);
                    return true;
                }
                else {
                    readerrors++;
                    return false;
                }
                
            }
            else {
                readerrors++;
                return false;
            }
           

        }

        private static void debugHex(byte[] data) {
                Debug.WriteLine(BitConverter.ToString(data).Replace("-"," "));
        }


        private static bool validateHeader(byte[] header) {
            if ((header[0] == 0x68) && (header[3] == 0x68))
            {
                return true;
            }
            else {
                return false;
            }
        }

        private static bool validateMan(byte[] manuf) {
            if ((manuf[0] == 0x01)&& (manuf[1] == 0x06)){
                return true;
            }
            else{
                return false;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*byte[] data = {0x24, 0x40};
            ReadIdFile("idlist.txt");
            ShowIdList();*/
            ShowJsonList();
        }

        public static int ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (int)(datetime - sTime).TotalSeconds;
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
            foreach (CounterData val in ToSend) {
                richTextBox1.AppendText("Данные для передачи"+Environment.NewLine);
                //Debug.WriteLine(val.NZAV);
                richTextBox1.AppendText("<!---Начало пакета-->"+Environment.NewLine);
                richTextBox1.AppendText(val.ID.ToString() + Environment.NewLine);
                richTextBox1.AppendText(val.NZAV.ToString() + Environment.NewLine);
                richTextBox1.AppendText(val.POKAZ.ToString() + Environment.NewLine);
                richTextBox1.AppendText(val.UNIXTIME.ToString() + Environment.NewLine);
                richTextBox1.AppendText("<!---Конец пакета-->"+Environment.NewLine);

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
            readerrors = 0;
            dataGridView1.Rows.Clear();
            //dataGridView1.Rows.Remove
            row = 0;
        }
    }
}
