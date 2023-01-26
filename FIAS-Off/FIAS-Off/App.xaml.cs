using Xamarin.Forms;
using MenuGUI;
using Android;
using static SQLSpace.f_SQL_page;
using System;
using System.IO;

namespace FIAS_Off
{
    public partial class App : Application
    {
        //Получаем путь до папки где будет храниться БД (получить доступ можно будет из любой чатси программы)
        public static string DBpath = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;
        //Подключаем БД здесь чтобы она тоже была доступна из любой части приложения
        public static SQLCreator DB = new SQLCreator();
        

        public App ()
        {

            InitializeComponent();

            MainPage = new MenuPage();

        }

        protected override void OnStart ()
        {
        }

        protected override void OnSleep ()
        {
        }

        protected override void OnResume ()
        {
        }

    }
}

