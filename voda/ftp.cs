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

        public class ftp_download {
            public string filename { get; set; }
            public bool transfer_succ { get; set; }
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

        /*функция получения даты последнего изменения файла 13.04.18*/
            public string get_file_date(ftp_options options, string file_name)
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + options.host + "/" + options.path.Trim('/', ' ') + "/" + file_name);
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                request.Credentials = new NetworkCredential(options.login, options.password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                string date_last_mod = Convert.ToString(response.LastModified);
                return date_last_mod;
            }
        /*функция получения даты последнего изменения файла конец*/
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
                    //Debugger.Break();
                    while (line != null)
                    {
                        result.Append(line);
                        result.Append("\n");
                        line = reader.ReadLine();
                    }
                    result.Remove(result.ToString().LastIndexOf('\n'), 1);
                }
                
                var list = result.ToString().Split('\n').ToList<string>();
                //Debugger.Break();
                try
                {
                    List<string[]> files = new List<string[]>();
                    foreach (string item in list)
                    {
                        string[] temp = item.Split(' ');
                        //Debugger.Break();
                        if ((temp.Length > 5)&&(temp[temp.Length - 1].IndexOf(".archive")<0))
                        {
                            //Debugger.Break();
                            if (temp[temp.Length - 3].Trim().Length == 1) {
                                files.Add(new string[] { temp[temp.Length - 1], temp[temp.Length - 2], "0"+temp[temp.Length - 3] + " " + temp[temp.Length - 5] });
                            }
                            else
                            {
                                files.Add(new string[] { temp[temp.Length - 1], temp[temp.Length - 2], temp[temp.Length - 3] + " " + temp[temp.Length - 4] });
                            }
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

        public List<string> get_last_files(List<string> files,List<string[]> list, ftp_options options)
        {
            //Debugger.Break();
            DateTime start = DateTime.ParseExact("01.01.2016 00:00:01", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
           
            if (list.Count > 0 && files.Count>0) {
                string[] latest_files = new string[files.Count]; 
                DateTime[] latest = new DateTime[files.Count];

                for (int i = 0; i < files.Count; i++) {
                    latest[i] = start;
                }

                foreach (string[] item in list) {
                    //Debugger.Break();
                    if (item[0].IndexOf(".xml")>-1) {
                        int counter = 0;
                        if (list.FindIndex(x=>x[0]==item[0].Replace(".xml",".rdy"))>-1) {
                            foreach (string filename in files)
                            {
                                //Debugger.Break();
                                if (item[0].IndexOf(filename) > -1)
                                {
                                    //var datetime = item[1] + " " + item[2] + " 2018";
                                    var datetime = get_file_date(options, item[0]);
                                    // Debugger.Break();
                                    DateTime myDate = DateTime.ParseExact(datetime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);                                    
                                    //Debugger.Break();
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
                //Debugger.Break();
                return latest_files.ToList();
            }
            return new List<string>();
        }

        public List<ftp_download> download_files(List<string> files, ftp_options options, string save_path="archive/")
        {
            List<ftp_download> status = new List<ftp_download>();
            foreach (string file in files)
            {
                ftp_download file_satus = new ftp_download();
                file_satus.filename = file;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://"+options.host+"/"+options.path.Trim('/',' ')+"/"+ file);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                string username = options.login;
                string password = options.password;
                request.Credentials = new NetworkCredential(username.Normalize(), password.Normalize());

                // устанавливаем метод на загрузку файлов
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // создаем поток для загрузки файла
                if (File.Exists(save_path+file))
                {
                    // Note that no lock is put on the
                    // file and the possibility exists
                    // that another process could do
                    // something with it between
                    // the calls to Exists and Delete.
                    File.Delete(save_path+file);
                }

                if (!Directory.Exists(save_path)) {
                    DirectoryInfo di = Directory.CreateDirectory(save_path);
                }

                try
                {

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Stream ftpStream = response.GetResponseStream();

                    FileStream localFileStream = new FileStream(save_path + file, FileMode.Create);
                    int bufferSize = 2048;
                    byte[] byteBuffer = new byte[bufferSize];
                    int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
                    }

                    localFileStream.Close();
                    ftpStream.Close();
                    response.Close();
                    request = null;

                    // получаем ответ от сервера в виде объекта FtpWebResponse
                    //FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    //ToConsole("Загрузка файлов завершена. Статус: " + response.StatusDescription);
                    Debug.WriteLine(response.StatusDescription);
                    if (response.StatusDescription.IndexOf("226") > -1)
                    {
                        file_satus.transfer_succ = true;
                    }
                    else
                    {
                        file_satus.transfer_succ = false;
                    }
                    status.Add(file_satus);
                }

                catch (Exception e) {
                    Debug.WriteLine(e);
                    file_satus.transfer_succ = false;
                }
            }
            return status;
        }

        public void delete_processed(List<string> files,string save_path="archive/") {
            try {
                foreach (string file in files) {
                    if (File.Exists(save_path + file))
                    {
                        File.Delete(save_path + file);
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
            }
        }

        public void archivate_parsed(List<string> old_files, ftp_options options) {
            List<string[]> all_files = get_files_list(options);
            foreach (string[] file in all_files) {
                try
                {
                    if (file[0].IndexOf(".archive") < 0)
                    {
                        if (old_files.FindIndex(x => x.Replace(".xml", "") == file[0].Replace(".xml", "").Replace(".rdy","")) < 0)
                        {
                            var client = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + options.host + "/" + options.path.Trim('/', ' ') + "/" + file[0]));
                            client.Credentials = new NetworkCredential(options.login, options.password);

                            client.Method = WebRequestMethods.Ftp.Rename;
                            client.RenameTo = "/" + options.path.Trim('/', ' ') + "/" + file[0] + ".archive";
                            //Debugger.Break();

                            FtpWebResponse response = (FtpWebResponse)client.GetResponse(); // вот тут ошибку выдает

                            Stream ftpStream = response.GetResponseStream();
                            response.Close();
                        }
                    }
                }
                catch (WebException ex) {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}
