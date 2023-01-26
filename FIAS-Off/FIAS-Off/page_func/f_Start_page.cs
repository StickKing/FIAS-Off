using System;
using System.Linq;
using SharpCompress.Common;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives;
using Plugin.DownloadManager.Abstractions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using Xamarin.Essentials;
using Android.OS;
using System.Threading;
using Java.Net;
using Org.Apache.Http.Client;
using Newtonsoft.Json.Linq;

using Data;
using SQLSpace;
using static SQLSpace.f_SQL_page;
using System.IO;
using Java.Security;
using System.Globalization;
using FIAS_Off;

namespace StartSpace
{
    public class DataBaseCreator
    {


        //--------------------------------------------------------------------------------------------------------------------------------------------------
        //Метод проверки существования Базы Данных (дожна проверять существование архива базы и её файлов и возвращать bool в качестве ответа)
        public bool DataBaseExists()
        {
            Console.WriteLine("/ / / / / / / / / / / / / / / " + App.DBpath + " / / / / / / / / / / / / / / /");

            bool result = false;

            if (File.Exists(App.DBpath + "/KLADR.db"))
            {
                var db_size = new FileInfo(App.DBpath + "/KLADR.db").Length / 1024;
                result = db_size > 200;
            }
            else
            {
                result = false;
            }
            

            //Console.WriteLine("/ / / / / / / / / / / / / / / " + db_size + " / / / / / / / / / / / / / / /");

            return result;

        }//DataBaseExists


        //--------------------------------------------------------------------------------------------------------------------------------------------------
        //Метод проверяющий есть ли свободное простаранство во внутренней памяти пользователя
        public bool FreeSpaceExists()
        {
            //Указываем путь который относится к проверяемому хранилищу
            var path = new StatFs(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
            long blockSize = path.BlockSizeLong;
            long avaliableBlocks = path.AvailableBlocksLong;
            double freeSpace = blockSize * avaliableBlocks;
            //Формируем информацию о свободном месте в байтах
            freeSpace = Math.Round(freeSpace / (1024 * 1024), 1);

            //Если на диске 1 Гб или больше возвращаем true если нет false
            return (freeSpace >= 1000);

        }//FreeSpaceExists

        //--------------------------------------------------------------------------------------------------------------------------------------------------
        //Метод проверяющий соединение с интернетом
        public bool CheckInternetConnection()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        //--------------------------------------------------------------------------------------------------------------------------------------------------
        //Метод проверки загрузки БД
        bool IsDownloading(IDownloadFile file)
        {
            if (file == null) return false;

            switch (file.Status)
            {
                case DownloadFileStatus.INITIALIZED:
                case DownloadFileStatus.PAUSED:
                case DownloadFileStatus.PENDING:
                case DownloadFileStatus.RUNNING:
                    return true;

                case DownloadFileStatus.COMPLETED:
                case DownloadFileStatus.CANCELED:
                case DownloadFileStatus.FAILED:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------
        //Метод загрузки Базы Данных в указанный путь 
        public async Task DownloadAndUnzipDataBaseAsync()
        {
 
            // Подправляем настйки скачивания
            Plugin.DownloadManager.CrossDownloadManager.Current.PathNameForDownloadedFile = new Func<Plugin.DownloadManager.Abstractions.IDownloadFile, string>(f =>
            {

                //Задаём имя скачиваемого файла
                string fileName = Android.Net.Uri.Parse(f.Url).Path.Split('/').Last();

                //Сохраняем всё по пути /Android/data/com.company/files/Documents
                Console.WriteLine($"__________________________________  {App.DBpath}  ___________________________________________");
                return System.IO.Path.Combine(App.DBpath + "/", fileName);

            });

            Console.WriteLine($"__________________________________  Download ___________________________________________");

            //Создаём класс загрузки файла базы данных
            var downloadManager = Plugin.DownloadManager.CrossDownloadManager.Current;

            //Получаем рабочую ссылку на БД
            URLDate url_date = await GetFIASdbURL();
            Console.WriteLine($"______________________________________ {url_date.url} _____________________________________________");

            //Создаём класс загружаемого файла
            var file = downloadManager.CreateDownloadFile(url_date.url);

            //Начинаем загрузку файла
            downloadManager.Start(file);

            Console.WriteLine($"______________________________________ Старт загрузки _____________________________________________");

            //Жду пока файл загрузится
            bool isDownloading = true;
            while (isDownloading)
            {
                await Task.Delay(10 * 1000);
                isDownloading = IsDownloading(file);
            }



            Thread unzip = new Thread(UnzipDB);
            unzip.Priority = System.Threading.ThreadPriority.Highest;
            unzip.Start();

            while (unzip.ThreadState == ThreadState.Running)
            {
                await Task.Delay(10 * 3000);
            }


            await App.DB.CreateInfoTableAsync(url_date.db_date);
            //await App.DB.CreateAllTable();
            Thread create_sql = new Thread(App.DB.CreateAllTable);
            create_sql.Priority = System.Threading.ThreadPriority.Highest;
            create_sql.Start();



            while (create_sql.ThreadState == ThreadState.Running)
            {
                await Task.Delay(10 * 1000);
                Console.WriteLine($"- - - - - - - - - {create_sql.ThreadState} - - - - - - - - -");
            }

            //Удаляю лишие файлы
            DeleteDBFFile();

            GC.Collect();

        }//DownloadDataBase


        //Метод очищения от лишних файлов
        public void DeleteDBFFile()
        {

            var all_work_files = Directory.GetFiles(App.DBpath).Select(f => Path.GetFileName(f));
            //Удаляем все файлы 
            foreach (var item in all_work_files)
            {

                if (item.Split('.')[1] == "7z" || item.Split('.')[1] == "DBF")
                {

                    Console.WriteLine($"____________________________ Deleted {item} __________________________________");
                    //Удаление
                    File.Delete(Path.Combine(App.DBpath, item));

                }
            }

        }//DeleteDBFFile


        //Метод определения актуальной БД и получения даты и ссылки на неё
        public async Task<URLDate> GetFIASdbURL()
        {

            Console.WriteLine("- - - - - - - - - - - Download - - - - - - - - - - - - - - - -");
            //Адрес службы обновлений FIAS
            string url = "http://fias.nalog.ru/WebServices/Public/GetLastDownloadFileInfo/";

            HttpClient http_client = new HttpClient();
            Console.WriteLine("- - - - - - - - - - - Download 2 - - - - - - - - - - - - - - - -");
            //GET-запрос к службе обновлений
            var response = await http_client.GetAsync(url);

            //Получение Json-а для дальнейшей работы с ним
            JObject info = JObject.Parse(await response.Content.ReadAsStringAsync());

            //Разбить дату на день, месяц, год
            string[] str_date = info.SelectToken(@"$.Date").ToString().Split('.');
            string download_url = info.SelectToken(@"$.Kladr47ZUrl").ToString();

            Console.WriteLine("- - - - - - - - - - - Download 3 - - - - - - - - - - - - - - - -");

            string date_formate = str_date[1] + "/" + str_date[0] + "/" + str_date[2];
            
            Console.WriteLine("- - - - - - - - - - - Download " + date_formate + " - - - - - - - - - - - - - - - -");

            //Преобразование строки актуальной версии БД в дату
            //DateTime actual_db_date = DateTime.Parse(date_formate);

            string[] validformats = new[] { "MM/dd/yyyy", "yyyy/MM/dd", "MM/dd/yyyy HH:mm:ss",
                                        "MM/dd/yyyy hh:mm tt", "yyyy-MM-dd HH:mm:ss, fff" };

            CultureInfo provider = new CultureInfo("en-US");
            DateTime actual_db_date = DateTime.ParseExact(date_formate, validformats, provider); ;

            Console.WriteLine("- - - - - - - - - - - Download 4 - - - - - - - - - - - - - - - -");

            //Возвращение актуальных данных
            return new URLDate()
            {
                db_date = actual_db_date,
                url = info.SelectToken(@"$.Kladr47ZUrl").ToString(),
            };

        }


        public static bool FileExists(string url)
        {
            bool result = false;

            var request = WebRequest.Create(url);
            request.Timeout = 1200;
            request.Method = "HEAD";// Загружаем только заголовки, файл нам не нужен.

            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                result = true;
            }
            catch (WebException webException)
            {
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }


            return result;
        }


        //--------------------------------------------------------------------------------------------------------------------------------------------------
        //Метод разорхивации БД в папку( в качестве аргумента принимает путь до расположения БД )  
        public async void UnzipDB()
        {
            Console.WriteLine("- - - - - - - - - - - Разорхивация - - - - - - - - - - - - - - - -");
            //Указываем путь к архиву и его имя для работы с ним
            using (var archive = SevenZipArchive.Open(App.DBpath + "/base.7z"))
            {
                //Выгружаем все файлы из архива
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(App.DBpath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }

            Console.WriteLine("- - - - - - - - - - - Конец разорхивация - - - - - - - - - - - - - - - -");

            //await App.DB.CreateAllTable();

        }//UnzipDB




    }//class DataBaseCreator


}

