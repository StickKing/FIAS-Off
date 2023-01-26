using System;
using System.Collections.Generic;
using SQLite;

namespace Data
{
    //Класс данных
    public class SOCRBASERow
    {
        public int level { get; set; }
        public string id { get; set; }
        public string sc_name { get; set; }
        public string sorc_names { get; set; }

    }

    //Класс данных для таблиц SORCBASE, KLADR, STREET
    public class DataRow
    {

        public string code { get; set; }
        public string name { get; set; }
        public string sorc { get; set; }
        public string octd { get; set; }
        public string mail_index { get; set; }
        public string gninmb { get; set; }
        public string uno { get; set; }

    }

    public class House
    {

        public List<string> name { get; set; }
        public string sorc { get; set; }
        public string code { get; set; }
        public string octd { get; set; }
        public string mail_index { get; set; }

    }

    public class URLDate
    {

        public string url { get; set; }
        public DateTime db_date {get; set;}

    }

    //Класс информации для карусели
    public class HomeInfo
    {

        public string info_text { get; set; }
        public string info_text_two { get; set; }
        public string info_imag_path { get; set; }
        public bool info_loading { get; set; }

    }//class HomeInfo

    [Table("Favourite")]
    public class FavouriteSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string name { get; set; }
        public string name_subject { get; set; }
        public string name_district { get; set; }
        public string name_citygpt { get; set; }
        public string sorc { get; set; }
        public string code { get; set; }
        public string octd { get; set; }
        public string gnimb { get; set; }
        public int mail_index { get; set; }

    }

    [Table("Subject")]
    public class SubjectSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string name { get; set; }
        public string sorc { get; set; }
        public string code { get; set; }
        public string octd { get; set; }
        public string gnimb { get; set; }

    }

    [Table("District")]
    public class DistrictSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int id_subject { get; set; }
        public string name { get; set; }
        public string sorc { get; set; }
        public string code { get; set; }
        public string octd { get; set; }

    }

    [Table("CityGPT")]
    public class CityGPTSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int id_district { get; set; }
        public string name { get; set; }
        public string sorc { get; set; }
        public string code { get; set; }
        public string octd { get; set; }
        public string gnimb { get; set; }
        public string uno { get; set; }
    }

    [Table("Street")]
    public class StreetSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int id_citygpt { get; set; }
        public string name { get; set; }
        public string sorc { get; set; }
        public string code { get; set; }

    }

    [Table("House")]
    public class HouseSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int id_street { get; set; }
        public string name { get; set; }
        public string sorc { get; set; }
        public string code { get; set; }
        public string octd { get; set; }
        public string mail_index { get; set; }

    }


    [Table("SocrBase")]
    public class SocrBaseSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public string sorc_name { get; set; }
        public int kod_t_st { get; set; }

    }


    [Table("DBInfo")]
    public class DBInfoSQL
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime db_version { get; set; }

    }

    public class SubDistCity
    {
        public int id_sub { get; set; }
        public string name_sub { get; set; }
        public string sorc_sub { get; set; }

        public int id_dis { get; set; }
        public string name_dis { get; set; }
        public string sorc_dis { get; set; }

        public int id_cit { get; set; }
        public string name_cit { get; set; }
        public string sorc_cit { get; set; }
        public string uno_cit { get; set; }
        public string gnimb_cit { get; set; }
        public string octd_cit { get; set; }
        public string code_cit { get; set; }
    }

}

