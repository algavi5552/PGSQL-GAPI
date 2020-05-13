using System;
using System.Collections.Generic;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Google.Apis.Sheets.v4.Data;
using System.Threading;

namespace ConsoleApp9
{
    public class Program
    {
        public static string[] configFromFile = File.ReadAllLines("server_conf.ini");//массив строк из файла конфигурации
        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string ApplicationName = "ConsoleApp9";
        static readonly string SpreadSheetId = configFromFile[1];
        static readonly string sheet = configFromFile[2];
        static readonly string host = configFromFile[3];
        public static int Delay = Convert.ToInt32(configFromFile[5]) * 1000;//Период в милисекундах, с которым будет выполняться обновление списка БД.
        static int calculateCount = 1;//Счетчик выполнений функции обновления списка БД
        static bool isAlive = false;

        static SheetsService service;
        static Postgres program;
        static void Main(string[] args)
        {
            GoogleCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            program = new Postgres();

            //Запустим поток
            isAlive = true;
            Thread thr = new Thread(new ThreadStart(Calculate));
            thr.Start();
            //------------------------------
            Console.WriteLine("Для выхода из программы нажмите Enter");
            Console.WriteLine();
            Console.ReadLine();
            //После нажатия Enter обнулим флаг потока
            isAlive = false;            
        }
        /// <summary>
        /// Функция обновления списка БД, будет выполняться из отдельного потока
        /// </summary>
        static void Calculate()
        {
            while (isAlive)//Пока поток жив
            {
                Console.WriteLine("Выполняем обновление № " + calculateCount + " списка БД. ");
                calculateCount++;//Увеличим счетчик выполнений
                //Делаем запрос к БД
                var result = program.MeetPSQL3("select datname, pg_size_pretty(pg_database_size(datname)) from pg_database;", host);
                //Объявляем счетчик суммарного объема занимаемого базами пространства
                double dbSumSize = 0.0;
                //В каждую полученную строчку добавим поле "обновление" со значением текущей даты
                foreach (var item in result)
                {
                    dbSumSize += Convert.ToDouble(((string)item[2]).Substring(0, ((string)item[2]).Length - 3));//Добавим к счетчику пространства размер текущей базы
                    item.Add(DateTime.Now.ToShortDateString());
                }
                //Рассчитаем свободное место на диске
                double emptySpace = Math.Round(Convert.ToDouble(configFromFile[4]) - dbSumSize / (1024 * 1024), 2);
                List<object> lastRow = new List<object>();
                lastRow.Add(host);
                lastRow.Add("Свободно");
                lastRow.Add(emptySpace.ToString() + " GB");
                lastRow.Add(DateTime.Now.ToShortDateString());

                result.Add(lastRow);
                //----------------------------------
                
                Console.WriteLine(" Обновление закончено. Запись в Google Sheet... ");
                UpdateGoogleSheet(result);//Запишем результат в Гугл Таблицу
                Console.WriteLine("Выполнено. Ожидаем " + Delay / 1000 + " секунд до следующего обновления");
                Thread.Sleep(Delay);
            }
        }
       
        /// <summary>
        /// Обновляет таблицу в Google Sheets
        /// </summary>
        /// <param name="objectList"></param>
        static void UpdateGoogleSheet(IList<IList<object>> objectList)
        {
            var range = $"{sheet}!A2:D";
            var valueRange = new ValueRange();
            valueRange.Values = objectList;
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadSheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse = updateRequest.Execute();
        }
    }
}
