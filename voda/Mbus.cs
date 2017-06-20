using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace voda
{
    public class Mbus
    {
        public Mbus() { }


        //Информационные переменные
        public int readerrors { get; set; }
            //Данные счетчика
            public int normval { get; set; }
            public int id { get; set; }
            public string m_date { get; set; }

        //Телеграмма
        public class mbus_telegram {
            public byte[] header { get; set;}
            public byte[] ci { get; set; }
            public data_header data_header = new data_header();
            public byte[] data_part { get; set; }
            public byte crc { get; set; }
            public byte stop { get; set; }
        }

        //Заглавие инфоблока
        public class data_header {
            public byte[] id = new byte[4];
            public byte[] man_id = new byte[2];
            public byte ver { get; set; }
            public byte devtype { get; set; }
            public byte access { get; set; }
            public byte[] signature = new byte[2];
        }

        //Обьект инфоблока
        public class data_block {
            public byte dif { get; set; }
            public byte[] dife = new byte[10];
            public byte vif { get; set; }
            public byte[] vife = new byte[10];
            public byte[] data;
        }

        public class mbus_decoded {
            public List<data_block>  data_in_hex = new List<data_block>();
        }
        

        //Перевод даты и времени
        public string convertDate(byte[] data)
        {
            int minutes = data[0] & 0x3F;
            int hours = data[1] & 0x1F;
            int day = data[2] & 0x1F;
            int month = (data[3] & 0x0F);
            int year = ((data[2] & 0xE0) >> 5) | ((data[3] & 0xF0) >> 1);
            string date = hours.ToString().PadLeft(2, '0') + ":" + minutes.ToString().PadLeft(2, '0') + " " + day.ToString().PadLeft(2, '0') + "." + month.ToString().PadLeft(2, '0') + "." + year.ToString();
            return date;
        }
        //--------------------------------------

        //Примитивная раскодировка телеграммы без разбития на части
        public bool decodeTelegram(string data)
        {
            byte[] byteData = StringToByteArray(data);
            var mbus_header = new byte[4];
            var mbus_afterhead = new byte[3];
            var mbus_id = new byte[4];
            int i;
            for (i = 0; i < 4; i++)
            {
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
                else
                {
                    if (mbus_decode_manufacturer(new byte[] { byteData[11], byteData[12] }) == "INV")
                    {
                        byte[] val = new byte[4];
                        Array.Copy(byteData, byteData.Length - 6, val, 0, 4);
                        normval = Convert.ToInt32(BitConverter.ToString(val.Reverse().ToArray()).Replace("-", String.Empty));
                        Debug.WriteLine(normval);
                        return true;
                    }
                    else
                    {
                        readerrors++;
                        Debug.WriteLine("manufacturer error");
                        Debug.WriteLine(mbus_decode_manufacturer(new byte[] { byteData[11], byteData[12] }));
                        Debug.WriteLine(id);
                        debugHex(byteData);
                        return false;
                    }
                }

            }
            else
            {
                readerrors++;
                Debug.WriteLine("header error");
                return false;
            }
        }
        //-----------------------------------------------------------------------

        //Разложение телеграммы
        public void telegram_decode(string string_telegram){
            //Перевод строки в массив байтов
            byte[] byte_telegram = StringToByteArray(string_telegram);
            //Выделение заголовка
            mbus_telegram telegram = new mbus_telegram();
            Debug.WriteLine("Длинна телеграммы " + byte_telegram.Length);
            //Проверка длинны
            if (byte_telegram.Length > 4)
            {
                //header
                telegram.header = new byte[] { byte_telegram[0], byte_telegram[1], byte_telegram[2], byte_telegram[3] };
                //ci
                telegram.ci = new byte[] { byte_telegram[6] };
                //dataheder
                //id накладки
                Array.Copy(byte_telegram,7,telegram.data_header.id,0,4);
                //id производителя
                Array.Copy(byte_telegram, 11, telegram.data_header.man_id, 0, 2);
                //Версия
                telegram.data_header.ver = byte_telegram[13];
                //Тип накладки
                telegram.data_header.devtype = byte_telegram[14];
                //Доступ
                telegram.data_header.access = byte_telegram[15];
                //Сигнатура
                Array.Copy(byte_telegram,16,telegram.data_header.signature,0,2);
                //Выделяем блок с данными
                int length = byte_telegram.Length - 19;
                telegram.data_part = new byte[length-2];
                Array.Copy(byte_telegram, 19, telegram.data_part, 0, length-2);
                //Отделяем контрольную сумму
                telegram.crc = byte_telegram[byte_telegram.Length - 2];
                //Стопбайт = 16
                telegram.stop = byte_telegram[byte_telegram.Length - 1];
                
                //Отладночная информация
                    //debugHex(telegram.data_part);
                    //Debug.WriteLine(mbus_decode_manufacturer(telegram.data_header.man_id));
                //Передача данных для расшифровки
                mbus_decode_data(telegram);
               
            }
        }

        public void mbus_decode_data(mbus_telegram telegram) {
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        //Проверка правильности заголовка
        public bool validateHeader(byte[] header)
        {
            if ((header[0] == 0x68) && (header[3] == 0x68))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //-----------------------------------
        
        //Проверка производителя
        public bool validateMan(byte[] manuf)
        {
            if ((manuf[0] == 0x01) && (manuf[1] == 0x06))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //------------------------------------



        //Перевод строки в массив байтов
        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        //-------------------------------------
        private static void debugHex(byte[] data)
        {
            Debug.WriteLine(BitConverter.ToString(data).Replace("-", " "));
        }

        //Закодировать производителя
        public void manuf_encode(byte[] m_code) {
            int m_val;
            m_val = ((((int)m_code[0] - 64) & 0x001F) << 10) +
            ((((int)m_code[1] - 64) & 0x001F) << 5) +
            ((((int)m_code[2] - 64) & 0x001F));
            Debug.WriteLine(m_val);
        }

        //Раскодировать производителя
        public string mbus_decode_manufacturer(byte[] man_id)
        {
            byte[] m_str = new byte[3];
            string MAN;

            int m_id;

            m_id = Convert.ToInt32(BitConverter.ToString(man_id.Reverse().ToArray()).Replace("-", String.Empty), 16);
      
            m_str[0] = Convert.ToByte(((m_id >> 10) & 0x001F) + 64);
            m_str[1] = Convert.ToByte(((m_id >> 5) & 0x001F) + 64);
            m_str[2] = Convert.ToByte(((m_id) & 0x001F) + 64);

            MAN = Convert.ToString(Convert.ToChar(m_str[0])) + Convert.ToString(Convert.ToChar(m_str[1]))+ Convert.ToString(Convert.ToChar(m_str[2]));
            return MAN;
        }
    }
}
