using FIAS_Off;
using FastSearchGUI;
using Xamarin.Forms;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

using StartGUI;
using DBWorkGUI;
using AboutGUI;
using FavouriteGUI;

using DBSpace;
using StartSpace;
using static SQLSpace.f_SQL_page;
using Data;

namespace MenuGUI
{
    public class MenuPage : Shell
    {

        //Объявляем класс для установки и проверки БД 
        DataBaseCreator DBCreator = new DataBaseCreator();

        public MenuPage()
        {
            this.FlyoutBackgroundColor = Color.WhiteSmoke;
            
            //Указываем путь /0/Android/data/pntlv.fias-off/files/docements

            //Проверяю БД на существование если не сущетсвует то прееходим на стартовую страницу
            if (!DBCreator.DataBaseExists())
            {
                Console.WriteLine("");

                //Элемент стартовой страницы --> переход на страницу начальной настройки
                FlyoutItem start_page = new FlyoutItem();
                start_page.Title = "Страртовая страница";
                var Page = new StartPage();
                start_page.Items.Add(Page);
                this.Items.Add(start_page);

                DataTemplate header_template = new DataTemplate(() =>
                {
                    Grid header_grid = new Grid();
                    header_grid.HeightRequest = 200;
                    header_grid.BackgroundColor = Color.YellowGreen;


                    header_grid.Children.Add(new Button()
                    {
                        Text = $"Версия базы данных: актуальная",
                        TextColor = Color.White,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextTransform = TextTransform.None,
                        BackgroundColor = Color.DarkOliveGreen,

                    });

                    return header_grid;

                });

                this.FlyoutHeaderTemplate = header_template;

                FlyoutItem DB_workerTwo = new FlyoutItem();
                DB_workerTwo.Title = "Поиск в адресной базе";
                DB_workerTwo.Items.Add(new DBWorker2() { Title = "Детальный поиск" });
                //DB_workerTwo.Items.Add(new FastSearch(DBpath) { Title = "Быстрый поиск" });
                DB_workerTwo.Icon = "search.png";
                DB_workerTwo.Route = "DBWork2";

                DB_workerTwo.Items.Add(new FastSearch() { Title = "Быстрый поиск" });


                //Элемент бокового меню --> переход на страницу "О приложении"
                FlyoutItem about = new FlyoutItem();
                about.Title = "О приложении";
                about.Items.Add(new About());
                about.Icon = "app.png";
                about.Route = "About";


                //Добавление всех элементов меню в непосредственно само меню
                this.Items.Add(DB_workerTwo);
                this.Items.Add(about);

            }
            else
            {

                Color db_version_color = Color.Red;
                bool button_enabled = true;
                string db_version = "устарела \n\n НАЖМИТЕ ЧТОБЫ ОБНОВИТЬ";

                if (!DBCreator.CheckInternetConnection())
                {
                    //Если интернета нет
                    db_version_color = Color.BlanchedAlmond;
                    button_enabled = false;
                    db_version = "Нет соединения с интернетом";
                    DataTemplate header_template = new DataTemplate(() =>
                    {
                        Grid header_grid = new Grid();
                        header_grid.HeightRequest = 200;
                        

                        Button button_update_db = new Button()
                        {
                            Text = $"Версия базы данных: " + db_version,
                            TextColor = Color.White,
                            FontSize = 20,
                            FontAttributes = FontAttributes.Bold,
                            TextTransform = TextTransform.None,
                            BackgroundColor = db_version_color,
                            IsEnabled = button_enabled,


                        };

                        button_update_db.Clicked += UpdateDB;

                        header_grid.Children.Add(button_update_db);

                        return header_grid;

                    });

                    this.FlyoutHeaderTemplate = header_template;

                }
                else
                {

                    FlyoutItem DB_workerTwo = new FlyoutItem();
                    DB_workerTwo.Title = "Поиск в адресной базе";
                    DB_workerTwo.Items.Add(new DBWorker2() { Title = "Детальный поиск" });
                    //DB_workerTwo.Items.Add(new FastSearch(DBpath) { Title = "Быстрый поиск" });
                    DB_workerTwo.Icon = "search.png";
                    DB_workerTwo.Route = "DBWork2";


                    //Элемент бокового меню --> переход на страницу "О приложении"
                    FlyoutItem about = new FlyoutItem();
                    about.Title = "О приложении";
                    about.Items.Add(new About());
                    about.Icon = "app.png";
                    about.Route = "About";

                    //Элемент бокового меню --> переход на страницу "О приложении"
                    FlyoutItem favourite_item = new FlyoutItem();
                    favourite_item.Title = "Избранное";
                    favourite_item.Items.Add(new Favourite());
                    favourite_item.Icon = "favourite.png";
                    favourite_item.Route = "favourite";

                    DB_workerTwo.Items.Add(new FastSearch() { Title = "Быстрый поиск" });

                    //Добавление всех элементов меню в непосредственно само меню
                    this.Items.Add(DB_workerTwo);
                    this.Items.Add(favourite_item);
                    this.Items.Add(about);
                    Task.Run(() => CheckDBVersion()); 
                }

            }


            /*/Элемент бокового меню --> переход на страницу "Работа с БД"
            FlyoutItem DB_workerTwo = new FlyoutItem();
            DB_workerTwo.Title = "Поиск в адресной базе";
            DB_workerTwo.Items.Add(new DBWorker2(DBpath) { Title = "Детальный поиск" });
            //DB_workerTwo.Items.Add(new FastSearch(DBpath) { Title = "Быстрый поиск" });
            DB_workerTwo.Icon = "search.png";
            DB_workerTwo.Route = "DBWork2";


            //Элемент бокового меню --> переход на страницу "О приложении"
            FlyoutItem about = new FlyoutItem();
            about.Title = "О приложении";
            about.Items.Add(new About());
            about.Icon = "app.png";
            about.Route = "About";

            //Элемент бокового меню --> переход на страницу "О приложении"
            FlyoutItem favourite_item = new FlyoutItem();
            favourite_item.Title = "Избранное";
            favourite_item.Items.Add(new Favourite(db_path));
            favourite_item.Icon = "favourite.png";
            favourite_item.Route = "favourite";
            Console.WriteLine("---------------------------Favourite---------------------------------");


            //Добавление всех элементов меню в непосредственно само меню
            //this.Items.Add(DB_worker);
            this.Items.Add(DB_workerTwo);
            //this.Items.Add(favourite_item);
            this.Items.Add(about);*/


            FlyoutItem fast_search = new FlyoutItem();
            fast_search.Title = "Быстрый поиск";
            fast_search.Items.Add(new DBWorker2() { Title = "Быстрый поиск" });
            
            fast_search.Icon = "search.png";
            fast_search.Route = "DBWork2";


        }

        private async void CheckDBVersion()
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                

                Color db_version_color = Color.Red;
                bool button_enabled = true;
                string db_version = "устарела \n\n НАЖМИТЕ ЧТОБЫ ОБНОВИТЬ";

                //URLDate test = DBCreator.URLCreatorAsync();
                var actual_db_version = await DBCreator.GetFIASdbURL();

                //URLDate actual_db_version = DBCreator.URLCreatorAsync();
                //DBCreator.URLCreatorAsync();
                var now_db_version = await App.DB.GetDBVersionAsync();

                Console.WriteLine($"__________________________________/  {actual_db_version.db_date.Date} {now_db_version.Date} /___________________________________________");


                if (actual_db_version.db_date.Date == now_db_version.Date)
                {

                    db_version_color = Color.DarkOliveGreen;
                    button_enabled = false;
                    db_version = "актуальная";

                }


                DataTemplate header_template = new DataTemplate(() =>
                {
                    Grid header_grid = new Grid();
                    header_grid.HeightRequest = 150;

                    Button button_update_db = new Button()
                    {
                        Text = $"Версия базы данных: " + db_version,
                        TextColor = Color.White,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextTransform = TextTransform.None,
                        BackgroundColor = db_version_color,
                        IsEnabled = button_enabled,


                    };

                    button_update_db.Clicked += UpdateDB;

                    header_grid.Children.Add(button_update_db);

                    return header_grid;

                });

                this.FlyoutHeaderTemplate = header_template;
            });

        }

        private async void UpdateDB(object sender, EventArgs e)
        {

            bool result = await DisplayAlert("Обновление", "Хотите обновить адресную базу сейчас? (это может занять 10 минут)", "Да", "Нет");
            if (result)
            {

                //Получаем список всех рабочих файлов
                var all_work_files = Directory.GetFiles(App.DBpath).Select(f => Path.GetFileName(f));
                //Удаляем все файлы 
                foreach (var item in all_work_files)
                {
                    //Удаление
                    File.Delete(Path.Combine(App.DBpath, item));
                    Console.WriteLine($"____________________________ {item} __________________________________");

                }

                //Элемент стартовой страницы --> переход на страницу начальной настройки
                FlyoutItem start_page = new FlyoutItem();
                start_page.Title = "Страртовая страница";
                var Page = new StartPage();
                start_page.Items.Add(Page);
                start_page.Route = "Start";
                this.Items.Add(start_page);

                //Перехожу на страницу стартовую страницу
                await Shell.Current.GoToAsync("//Start");


                //Изменяю информацию о статусе БД
                DataTemplate header_template = new DataTemplate(() =>
                {
                    Grid header_grid = new Grid();
                    header_grid.HeightRequest = 200;
                    header_grid.BackgroundColor = Color.YellowGreen;


                    header_grid.Children.Add(new Button()
                    {
                        Text = $"Версия базы данных: актуальная",
                        TextColor = Color.White,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        TextTransform = TextTransform.None,
                        BackgroundColor = Color.DarkOliveGreen,

                    });

                    return header_grid;

                });

                this.FlyoutHeaderTemplate = header_template;

            }

        }

    }
}


