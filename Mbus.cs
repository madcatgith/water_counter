using System;

namespace voda
{
    public class Mbus
    {
        public Mbus() { }

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
    }
}
