using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using NDbfReader;
using System.Linq;
using FIAS_Off;

using Data;

namespace DBSpace
{

    
    //Класс для работы с базой данных
    public class DataBaseWorker
    {

        //Список для таблицы KLADR
        public List<DataRow> KLADR = new List<DataRow>();
        //Список для таблицы STREET
        public List<DataRow> STREET = new List<DataRow>();
        //Список для таблицы DOMA
        public List<DataRow> DOMA = new List<DataRow>();

        //Выгружаем данные из SOCR.DBF
        public async Task GetSOCR()
        {
            List<SocrBaseSQL> SOCR = new List<SocrBaseSQL>();
            using (var table = Table.Open(App.DBpath + "/SOCRBASE.DBF"))
            {

                var reader = table.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new SocrBaseSQL()
                    {

                        name = reader.GetString("SCNAME"),
                        sorc_name = reader.GetString("SOCRNAME"),
                        level = int.Parse(reader.GetString("LEVEL")),
                        kod_t_st = int.Parse(reader.GetString("KOD_T_ST")),

                    };

                    SOCR.Add(row);

                }

                App.DB.DB.InsertAll(SOCR);


            }

        }

        //Выгружаем дынне из KLADR.DBF
        public async Task GetKLADR()
        {

            using (var table = Table.Open(App.DBpath + "/KLADR.DBF"))
            {
               

                var reader = table.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new DataRow()
                    {
                        code = reader.GetString("CODE"),
                        name = reader.GetString("NAME"),
                        sorc = reader.GetString("SOCR"),
                        octd = reader.GetString("OCATD"),
                        gninmb = reader.GetString("GNINMB"),
                        uno = reader.GetString("UNO"),
                    };


                    KLADR.Add(row);

                }

                
            }

        }


        //Выгружаем из таблицу улиц из файла STREET.DBF
        public void GetSTREET()
        {
            using (var table = Table.Open(App.DBpath + "/STREET.DBF"))
            {
                var reader = table.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new DataRow()
                    {
                        code = reader.GetString("CODE"),
                        name = reader.GetString("NAME"),
                        sorc = reader.GetString("SOCR"),
                        octd = reader.GetString("OCATD"),
                        mail_index = reader.GetString("INDEX"),
                        gninmb = reader.GetString("GNINMB"),
                        uno = reader.GetString("UNO"),
                    };
                    STREET.Add(row);
                }
            }

        }

        public async Task GetSTREETSQL()
        {
            using (var table = Table.Open(App.DBpath + "/STREET.DBF"))
            {
                var reader = table.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new DataRow()
                    {
                        code = reader.GetString("CODE"),
                        name = reader.GetString("NAME"),
                        sorc = reader.GetString("SOCR"),
                        octd = reader.GetString("OCATD"),
                        mail_index = reader.GetString("INDEX"),
                    };
                    STREET.Add(row);
                }
            }

        }

        //Выгружаем из таблицу улиц из файла DOMA.DBF
        public void GetDOMA()
        {
            using (var table = Table.Open(App.DBpath + "/DOMA.DBF"))
            {
                var reader = table.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new DataRow()
                    {
                        code = reader.GetString("CODE"),
                        name = reader.GetString("NAME"),
                        sorc = reader.GetString("SOCR"),
                        octd = reader.GetString("OCATD"),
                        mail_index = reader.GetString("INDEX"),
                        gninmb = reader.GetString("GNINMB"),
                        uno = reader.GetString("UNO"),
                    };
                    DOMA.Add(row);
                }
            }
        }

        public async Task GetDOMASQL()
        {
            using (var table = Table.Open(App.DBpath + "/DOMA.DBF"))
            {
                var reader = table.OpenReader(Encoding.GetEncoding(866));
                while (reader.Read())
                {
                    var row = new DataRow()
                    {
                        code = reader.GetString("CODE"),
                        name = reader.GetString("NAME"),
                        sorc = reader.GetString("SOCR"),
                        octd = reader.GetString("OCATD"),
                        mail_index = reader.GetString("INDEX"),
                        gninmb = reader.GetString("GNINMB"),
                        uno = reader.GetString("UNO"),
                    };
                    DOMA.Add(row);
                }
            }
        }


        //Находим все субъекты РФ
        public List<SubjectSQL> GetRFSubjectSQL()
        {
            List<SubjectSQL> RF_subject = new List<SubjectSQL>();

            foreach (var item in this.KLADR)
            {

                if (((item.sorc == "Респ")
                    || (item.sorc == "АО")
                    || (item.sorc == "край")
                    || (item.sorc == "обл")
                    || (item.sorc == "Аобл")
                    || ((item.name == "Москва") && (item.sorc == "г"))
                    || ((item.name == "Санкт-Петербург") && (item.sorc == "г"))
                    || ((item.name == "Севастополь") && (item.sorc == "г")))
                    && item.code.Substring(2, 11) == "00000000000"
                    )
                {
                    var value = new SubjectSQL
                    {
                        name = item.name,
                        sorc = item.sorc,
                        code = item.code,
                        octd = item.octd,
                        gnimb = item.gninmb,
                    };
                    RF_subject.Add(value);

                }


            }

            //Сортирую список субъектов по типу субъекта (республика, область, город и тд.)


            return RF_subject;

        }

        //Находим все субъекты РФ
        public List<DataRow> GetRFSubject()
        {

            List<DataRow> RF_subject = new List<DataRow>();

            foreach (var item in this.KLADR)
            {

                if (((item.sorc == "Респ")
                    || (item.sorc == "АО")
                    || (item.sorc == "край")
                    || (item.sorc == "обл")
                    || (item.sorc == "Аобл")
                    || ((item.name == "Москва") && (item.sorc == "г"))
                    || ((item.name == "Санкт-Петербург") && (item.sorc == "г"))
                    || ((item.name == "Севастополь") && (item.sorc == "г")))
                    && item.code.Substring(2, 11) == "00000000000"
                    )
                {

                    RF_subject.Add(item);

                }


            }

            //Сортирую список субъектов по типу субъекта (республика, область, город и тд.)
            RF_subject.Sort((x, y) => (x.sorc.CompareTo(y.sorc)));

            return RF_subject;

        }



        //Находим все административные районы субъекта РФ
        public async Task<List<DataRow>> GetAdminDistrict(DataRow RF_subject)
        {

            //KLADR.Sort( (x, y) => x.octd.CompareTo(x.octd));
            List<DataRow> RF_districs = new List<DataRow>();


            foreach (var item in KLADR)
            {
                if (item.code != null)
                {

                    if (item.code.Substring(0, 2) == RF_subject.code.Substring(0, 2) && item.sorc == "р-н")
                    {

                        RF_districs.Add(item);

                    }

                }

            }

            RF_districs.Sort((x, y) => (x.sorc.CompareTo(y.sorc)));

            return RF_districs;   

        }


        //Находим все города и посёлки городского типа относящихся к определённому району
        //и населённые пункты, посёлки деревни хуторы и т.д.
        public async Task<List<DataRow>> GetCityGPT(DataRow RF_districts)
        {

            List<DataRow> districs_city = new List<DataRow>();

            try
            {
                foreach (var item in KLADR)
                {

                    if (item.code != null)
                    {

                        if (item.code.Substring(0, 5) == RF_districts.code.Substring(0, 5) && item.code.Substring(item.code.Length - 2, 2) == "00" )
                        {

                            districs_city.Add(item);

                        }

                    }

                }
            }
            catch (Exception ex)
            {

            }

            districs_city.Sort((x, y) => (x.name.CompareTo(y.name)));

            return districs_city;

        }

        //Находим все улицы и гпт относящиеся к городу, посёлку хутору и тд
        public async Task<List<DataRow>> GetGPTStreet(DataRow city_gpt)
        {

            List<DataRow> streets = new List<DataRow>();

            try
            {
                foreach (var item in this.STREET)
                {

                    if (item.code != null)
                    {

                        if (item.code.Substring(0, 11) == city_gpt.code.Substring(0, 11) && item.code.Substring(item.code.Length - 2, 2) == "00")
                        {

                            streets.Add(item);

                        }

                    }

                }
            }
            catch (Exception ex)
            {

            }

            streets.Sort((x, y) => (x.name.CompareTo(y.name)));

            return streets;

        }

        //Находим все улицы и гпт относящиеся к городу, посёлку хутору и тд
        public async Task<List<DataRow>> GetHouse(DataRow houses)
        {

            List<DataRow> houses_list = new List<DataRow>();

            try
            {
                foreach (var item in this.DOMA)
                {

                    if (item.code != null)
                    {

                        if (item.code.Substring(0, 17) == houses.code.Substring(0, 17))
                        {

                            houses_list.Add(item);

                        }

                    }

                }
            }
            catch (Exception ex)
            {

            }


            return houses_list;

        }


    }//class DataBaseWorker


   

}

