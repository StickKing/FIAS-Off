using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using FIAS_Off;
using Data;

using DBSpace;
using System.IO;

namespace SQLSpace
{
    public struct f_SQL_page
    {

        public class SQLCreator
        {

            public SQLiteConnection DB { get; set; }

            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< Конструктор >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            public SQLCreator()
            {
                this.DB = new SQLiteConnection(App.DBpath + "/KLADR.db");
            }

            public async Task CreateInfoTableAsync(DateTime db_date)
            {
                //Таблица информации о БД
                this.DB.CreateTable<DBInfoSQL>();
                //this.DB.Execute("CREATE TABLE DBInfo(" +
                //    "id INTEGER PRIMARY KEY AUTOINCREMENT not null," +
                //    "db_version date)");

                //Заполнение таблица DBInfo
                this.DB.Insert(new DBInfoSQL()
                {
                    db_version = db_date.Date,
                    //db_version = new DateTime(2022, 11, 11),
                });


                //var test = await this.DB.Table<DBInfoSQL>().ToList<DBInfoSQL>();

                //Console.WriteLine($"___________________________________________________ {test[0].db_version} _____________________________________________________________");

            }


            public async void CreateAllTable()
            {

                DataBaseWorker DB_worker = new DataBaseWorker();


                //<<<<<<<<<<<<<<<< Создание таблиц базы данных >>>>>>>>>>>>>>>
                //Таблица избранного
                this.DB.CreateTable<FavouriteSQL>();
                //Таблица субъектов РФ
                this.DB.CreateTable<SubjectSQL>();
                //Таблица наименований типов адресных объектов
                this.DB.CreateTable<SocrBaseSQL>();
                //Таблица административных районов
                this.DB.Execute("CREATE TABLE District(" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT not null," +
                    "id_subject INTEGER not null," +
                    "name varchar," +
                    "sorc varchar," +
                    "code varchar," +
                    "octd varchar," +
                    "FOREIGN KEY (id_subject) REFERENCES Subject (id) ON DELETE CASCADE)");
                //Таблица городов и населённых пунктов
                this.DB.Execute("CREATE TABLE CityGPT(" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT not null," +
                    "id_district INTEGER not null," +
                    "name varchar," +
                    "sorc varchar," +
                    "code varchar," +
                    "octd varchar," +
                    "gnimb varchar," +
                    "uno varchar," +
                    "FOREIGN KEY (id_district) REFERENCES District (id) ON DELETE CASCADE)");
                //Таблица улиц
                this.DB.Execute("CREATE TABLE Street(" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT not null," +
                    "id_citygpt INTEGER not null," +
                    "name varchar," +
                    "sorc varchar," +
                    "code varchar," +
                    "FOREIGN KEY (id_citygpt) REFERENCES CityGPT (id) ON DELETE CASCADE)");
                //Таблица домов
                this.DB.Execute("CREATE TABLE House(" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT not null," +
                    "id_street INTEGER not null," +
                    "name varchar," +
                    "sorc varchar," +
                    "code varchar," +
                    "octd varchar," +
                    "mail_index varchar," +
                    "FOREIGN KEY (id_street) REFERENCES Street (id) ON DELETE CASCADE)");


                //<<<<<<<<<<<<<<<< Заполнение таблиц базы данных >>>>>>>>>>>>>>>

                //Заполнение таблицы SorcName
                await DB_worker.GetSOCR();

                var socr_names = DB.Table<SocrBaseSQL>().ToList();

                //Console.WriteLine("- - - - - - - - - - - - - insert SOCR _ _ _ _ _ _ _ _ _ _ _ _");


                //var socr = await this.DB.Table<SocrBaseSQL>().Where(x => x.level == 1).ToListAsync();
                var socr = this.DB.Table<SocrBaseSQL>().ToList();
                try
                {
                    //Заполнение таблицы Subject
                    await DB_worker.GetKLADR();
                    var items = from i in DB_worker.KLADR
                                where (i.sorc == "Респ"
                                || (i.sorc == "АО"
                                || i.sorc == "край"
                                || i.sorc == "обл"
                                || i.sorc == "Аобл"
                                || i.sorc == "Чувашия")
                                || ((i.name == "Москва") && (i.sorc == "г"))
                                || ((i.name == "Санкт-Петербург") && (i.sorc == "г"))
                                || ((i.name == "Севастополь") && (i.sorc == "г")))
                                && i.code.Substring(2, 11) == "00000000000"
                                select new SubjectSQL()
                                {
                                    name = i.name,
                                    sorc = socr.Find(x => x.name == i.sorc).sorc_name,
                                    octd = i.octd,
                                    code = i.code,
                                    gnimb = i.gninmb,
                                };
                    this.DB.InsertAll(items);


                    Console.WriteLine("- - - - - - - - - - - - - insert Subject _ _ _ _ _ _ _ _ _ _ _ _");

                    items = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("-------------Hello error ____________");
                    Console.WriteLine(ex.ToString());
                    //return false;
                }


                //Заполняем таблицу District
                var district_list = from i in DB_worker.KLADR
                                    join j in this.DB.Table<SubjectSQL>().ToList() on i.code.Substring(0, 2) equals j.code.Substring(0, 2)
                                    where i.sorc == "р-н" && i.code.Substring(i.code.Length - 2, 2) == "00"
                                    select new DistrictSQL()
                                    {
                                        name = i.name,
                                        code = i.code,
                                        octd = i.octd,
                                        sorc = socr.Find(x => x.name == i.sorc).sorc_name,
                                        id_subject = j.Id,
                                    };
                DB.InsertAll(district_list);
                Console.WriteLine("- - - - - - - - - - - - - insert District _ _ _ _ _ _ _ _ _ _ _ _");
                district_list = null;
                GC.Collect();

                //Добавление городов федерального значения в административные районы
                var city_federal = from i in this.DB.Table<SubjectSQL>().ToList()
                                   where i.sorc == "Город"
                                   select new DistrictSQL()
                                   {
                                       name = i.name,
                                       code = i.code,
                                       octd = i.octd,
                                       sorc = "Город федерального значения",
                                       id_subject = i.Id,
                                   };

                DB.InsertAll(city_federal);



                //Заполняем таблицу CityGPT
                var citygpt_list = from i in DB_worker.KLADR
                                   join j in this.DB.Table<DistrictSQL>().ToList() on i.code.Substring(0, 5) equals j.code.Substring(0, 5)
                                   where i.code.Substring(i.code.Length - 2, 2) == "00"
                                   select new CityGPTSQL()
                                   {
                                       name = i.name,
                                       code = i.code,
                                       octd = i.octd,
                                       sorc = socr.Find(x => x.name == i.sorc).sorc_name,
                                       uno = i.uno,
                                       gnimb = i.gninmb,
                                       id_district = j.Id,
                                   };
                DB.InsertAll(citygpt_list);
                Console.WriteLine("- - - - - - - - - - - - - insert CityGPT _ _ _ _ _ _ _ _ _ _ _ _");
                citygpt_list = null;
                GC.Collect();

                //Очистка данных из памяти
                DB_worker.KLADR.Clear();


                //Заполняем таблицу Street 
                try
                {
                    await DB_worker.GetSTREETSQL();
                    var street_list = from i in DB_worker.STREET
                                      join j in this.DB.Table<CityGPTSQL>().ToList() on i.code.Substring(0, 11) equals j.code.Substring(0, 11)
                                      where i.code.Substring(i.code.Length - 2, 2) == "00"
                                      select new StreetSQL()
                                      {
                                          name = i.name,
                                          code = i.code,
                                          sorc = i.sorc,
                                          id_citygpt = j.Id,
                                      };

                    //Console.WriteLine("<<<<<<<<<     " + street_list.ToString() + "     >>>>>>>>");
                    DB.InsertAll(street_list);
                    Console.WriteLine("- - - - - - - - - - - - - insert Street _ _ _ _ _ _ _ _ _ _ _ _");
                    //Очистка данных
                    DB_worker.STREET.Clear();
                    street_list = null;
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("<<<<<<< Set Street Error >>>>>>>>");
                    Console.WriteLine(ex.ToString());
                    //return false;
                }


                //Заполняем таблицу House
                await DB_worker.GetDOMASQL();
                var house_list = from i in DB_worker.DOMA
                                 join j in this.DB.Table<StreetSQL>().ToList() on i.code.Substring(0, 17) equals j.code.Substring(0, 17)
                                 select new HouseSQL()
                                 {
                                     name = i.name,
                                     code = i.code,
                                     octd = i.octd,
                                     sorc = i.sorc,
                                     mail_index = i.mail_index,
                                     id_street = j.Id,
                                 };
                try
                {
                    DB.InsertAll(house_list);
                    Console.WriteLine("- - - - - - - - - - - - - insert House _ _ _ _ _ _ _ _ _ _ _ _");
                    DB_worker.DOMA.Clear();
                    house_list = null;
          
                }
                catch (Exception ex)
                {
                    Console.WriteLine("<<<<<<< Set House Error >>>>>>>>");
                    Console.WriteLine(ex.ToString());
                    //return false;
                }

                Console.WriteLine("---------------- Yeeeeeeeeeeeess -------------------------------");
                //return true;

            }

            public async Task<List<SubjectSQL>> GetSubjectsAsync()
            {
                try
                {
                    return DB.Query<SubjectSQL>("select * from Subject");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------- ERROR ------------");
                    Console.WriteLine(ex.ToString());
                    return null;
                }

            }

            public async Task<List<DistrictSQL>> GetDistrictAsync(int id)
            {
                try
                {
                    return DB.Query<DistrictSQL>("select * from District where id_subject = ?", id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------- ERROR ------------");
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }

            public async Task<List<CityGPTSQL>> GetCityGPTAsync(int id)
            {
                try
                {
                    return DB.Query<CityGPTSQL>("select * from CityGPT where id_district = ?", id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------- ERROR ------------");
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }

            public async Task<List<StreetSQL>> GetStreetAsync(int id)
            {
                try
                {
                    return DB.Query<StreetSQL>("select * from Street where id_citygpt = ?", id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------- ERROR ------------");
                    Console.WriteLine(ex.ToString());
                    return null;
                }

            }

            public async Task<List<HouseSQL>> GetHouseAsync(int id)
            {
                try
                {
                    return DB.Query<HouseSQL>("select * from House where id_street = ?", id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------- ERROR ------------");
                    Console.WriteLine(ex.ToString());
                    return null;
                }

            }

            public async Task<DateTime> GetDBVersionAsync()
            {

                var version_db = DB.Table<DBInfoSQL>().ToList();
                return version_db[0].db_version.Date;
            }

            public async Task<string> SearchAdress(string[] search_text)
            {


                try
                {

                    var subject = DB.Query<SubjectSQL>("select * from Subject where name = ?", search_text[0]);
                    if (subject != null)
                    {
                        var districts = await GetDistrictAsync(subject[0].Id);
                        if (districts != null)
                        {
                            var city_gpt = from i in districts
                                           join j in this.DB.Table<CityGPTSQL>().ToList() on i.code.Substring(0, 5) equals j.code.Substring(0, 5)
                                           where j.name == search_text[1] && j.code.Substring(j.code.Length - 2, 2) == "00"
                                           select new CityGPTSQL()
                                           {
                                               name = j.name,
                                               code = j.code,
                                               octd = j.octd,
                                               sorc = j.sorc,
                                               uno = j.uno,
                                               id_district = i.Id,
                                           };
                            

                            if (city_gpt != null)
                            {

                                var street = from i in city_gpt
                                             join j in this.DB.Table<StreetSQL>().ToList() on i.code.Substring(0, 11) equals j.code.Substring(0, 11)
                                             where j.name == search_text[2] && j.code.Substring(j.code.Length - 2, 2) == "00"
                                             select new StreetSQL()
                                             {
                                                 name = j.name,
                                                 code = j.code,
                                                 sorc = j.sorc,
                                                 id_citygpt = i.Id,
                                             };

                                if (street != null)
                                {
                                    var house = from i in street
                                                join j in this.DB.Table<HouseSQL>().ToList() on i.code.Substring(0, 17) equals j.code.Substring(0, 17)
                                                select new House()
                                                {
                                                    name = j.name.Split(',').ToList<string>(),
                                                    code = j.code,
                                                    octd = j.octd,
                                                    sorc = j.sorc,
                                                    mail_index = j.mail_index,
                                                };

                                    house = from i in house
                                            where i.name.Find(x => x == search_text[3]) != null
                                            select i;

                                    if (house != null)
                                    {
                                        return "Субъект: " + subject[0].name + "\nКод субъекта: " + subject[0].code.Substring(0, 2)
                                            + "\nНаселённый пункт: "
                                            + "\nДом: " + house.ToString();
                                    }
                                    else
                                    {
                                        return "Дом не найден";
                                    }

                                }
                                else
                                {
                                    return "Улица не найдена";
                                }

                            }
                            else
                            {
                                return "Город, населённый пункт не найден";
                            }

                        }
                        else
                        {
                            return "Некорректно введены данные";
                        }
                    }
                    else
                    {
                        return "Некорректно введены данные";
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------- Search Error ------------");
                    Console.WriteLine(ex.ToString());
                    return "Error";
                }
            }

            /*public List<string> FastStreet(int city_id, string street)
            {

                //Console.WriteLine(city_id + " " + street);
                List<string> test1 = DB.Table<StreetSQL>().Where(x => x.id_citygpt == city_id && x.name.StartsWith(street)).Select(y => y.id_citygpt + " " + y.name).ToList();
                return test1;
            }*/

            public Task<IEnumerable<string>> FastCityAsync(object? search_item)
            {
                
                //List<string> test1 = DB.Table<StreetSQL>().Where(x => x.id_citygpt == city_id && x.name.StartsWith(street)).Select(y => y.id_citygpt + " " + y.name).ToList();

                var result = DB.Query<CityGPTSQL>($"select name from CityGPT where name like '{search_item}%'").Select(x => x.name);
                return (Task<IEnumerable<string>>)result;
            }

        }
        
    }
}

