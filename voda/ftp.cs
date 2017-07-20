using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

namespace voda
{
    class ftp
    {
        public class ftp_options {
            public string login { get; set; }
            public string password { get; set; }
            public string host { get; set; }
            public string path { get; set; }
        }

        public class ftp_error {
            public bool no_error { get; set; }
            public int error_status { get; set; }
        }

        public List<string> file_list(ftp_options options) {
            if (check_connection(options).no_error) {
                try
                {
                    var result = new StringBuilder();

                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + options.host + "/"+options.path.Trim('/',' ')+"/");
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    request.Credentials = new NetworkCredential(options.login, options.password);
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var line = reader.ReadLine();
                        while (line != null)
                        {
                            result.Append(line);
                            result.Append("\n");
                            line = reader.ReadLine();
                        }
                        result.Remove(result.ToString().LastIndexOf('\n'), 1);
                    }

                    var list = result.ToString().Split('\n').ToList<string>();
                    List<string> xml_list = new List<string>();
                    foreach (string item in list) {
                        if (item.IndexOf(".xml") > -1) {
                            string[] xml_arr = item.Split('_');
                            if (xml_arr.Length == 2) {
                                xml_list.Add(xml_arr[0].Trim());
                            }  
                        }
                    }

                    var unique_list = xml_list.Distinct().ToList();

                    return unique_list;
                    
                }
                catch (WebException ex) {

                }
            }
            return new List<string>();
        }

        public ftp_error check_connection(ftp_options options) {
            try
            {
                var result = new StringBuilder();

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + options.host + "/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(options.login, options.password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        result.Append(line);
                        result.Append("\n");
                        line = reader.ReadLine();
                    }
                    result.Remove(result.ToString().LastIndexOf('\n'), 1);
                }

                var list = result.ToString().Split('\n');

                if ( list.Contains(options.path)) {
                    return new ftp_error() {no_error=true, error_status = 0};
                }

                return new ftp_error() { no_error = false, error_status = 3 };
            }
            catch (WebException e) {
                FtpWebResponse response = (FtpWebResponse)e.Response;
                Debug.WriteLine(response);
                if (response.StatusCode == FtpStatusCode.NotLoggedIn) {
                    return new ftp_error() {no_error=false,error_status=2 };
                }
                return new ftp_error() { no_error = false,error_status=1};
            }
            return new ftp_error() { no_error = false, error_status=9};
        }

        public List<string[]> get_files_list(ftp_options options) {
            try
            {
                var result = new StringBuilder();

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + options.host + "/"+ options.path.Trim('/', ' ') + "/");
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(options.login, options.password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        result.Append(line);
                        result.Append("\n");
                        line = reader.ReadLine();
                    }
                    result.Remove(result.ToString().LastIndexOf('\n'), 1);
                }
                
                var list = result.ToString().Split('\n').ToList<string>();

                try
                {
                    List<string[]> files = new List<string[]>();
                    foreach (string item in list)
                    {
                        string[] temp = item.Split(' ');
                        if (temp.Length > 5)
                        {
                            files.Add(new string[] { temp[temp.Length - 1], temp[temp.Length - 2], temp[temp.Length - 3] + " " + temp[temp.Length - 4]});
                        }
                    }
                    //files = files.OrderByDescending(x => x[0].Remove(0,x[0].IndexOf('_')+1).Replace(".xml", "").Replace(".rdy", "")).ToList();
                    //Debugger.Break();
                    return files;
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex);
                }
                
            }
            catch (WebException e)
            {
                Debug.WriteLine(e);
            }
            return new List<string[]>();
        }

        public List<string> get_last_files(List<string> files,List<string[]> list)
        {
            List<string[]> xml = new List<string[]>();
            List<string[]> rdy = new List<string[]>();
            DateTime start = DateTime.ParseExact("00:00 01 Dec 2016", "HH:mm dd MMM yyyy",CultureInfo.InvariantCulture);

            if (list.Count > 0 && files.Count>0) {
                string[] latest_files = new string[files.Count]; 
                DateTime[] latest = new DateTime[files.Count];

                for (int i = 0; i < files.Count; i++) {
                    latest[i] = start;
                }

                foreach (string[] item in list) {
                    /*if (item[0].IndexOf(".rdy")>-1) {
                        rdy.Add(item);
                    }*/
                    if (item[0].IndexOf(".xml")>-1) {
                        //xml.Add(item);
                        int counter = 0;
                        if (list.FindIndex(x=>x[0]==item[0].Replace(".xml",".rdy"))>-1) {
                            foreach (string filename in files)
                            {
                                if (item[0].IndexOf(filename) > -1)
                                {
                                    var datetime = item[1] + " " + item[2] + " 2017";
                                    DateTime myDate = DateTime.ParseExact(datetime, "HH:mm dd MMM yyyy", CultureInfo.InvariantCulture);
                                    if (myDate > latest[counter])
                                    {
                                        latest[counter] = myDate;
                                        latest_files[counter] = item[0];
                                    }
                                }
                                counter++;
                            }
                        }
                    }
                }
                Debugger.Break();
            }
            return new List<string>();
        }
    }
}
