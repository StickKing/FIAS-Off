using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Threading;
using Xamarin.Essentials;
using System.Threading.Tasks;
using FIAS_Off;
using System.Linq;

using Data;
using SQLSpace;
using DBSpace;
using static SQLSpace.f_SQL_page;
using Java.Util;
using System.ComponentModel;

namespace DBWorkGUI
{
    public class DBWorker2 : ContentPage
    {

        //public SQLCreator SQL_db { get; set; }
        //Класс для работы с БД
        //Создаю экземпляр класса для работы с БД
        DataBaseWorker DBWorker = new DataBaseWorker();


        //<-----------Объекты графического окружения----------->
        //Stack в котором будут лежать все элементы
        //StackLayout button_layout = new StackLayout();
        //Stack в котором будут лежать все элементы
        StackLayout stack_items = new StackLayout();
        //Stack в котором будет лежать информация о найденом объекте
        StackLayout object_info_stack = new StackLayout();
        //Picker для выбора вида деления информации 
        Picker muniz_admin_picker = new Picker();
        //Picker для субъектов РФ
        Picker rf_subject_picker = new Picker();
        //Picker для административных районов
        Picker admin_district_picker = new Picker();
        //Picker для городов и пгп
        Picker city_gpt_picker = new Picker();
        //Picker для улиц
        Picker streets_picker = new Picker();
        //Picker для домов
        Picker houses_picker = new Picker();


        //<-----------Данные БД для графического окружения----------->
        //Список субъектов РФ
        List<SubjectSQL> subjects { get; set; }
        //Список административных районой выбранного субъекта
        List<DistrictSQL> admin_district { get; set; }
        //Список городов, сёл и посёлков городского типа
        List<CityGPTSQL> citys_gpt { get; set; }
        //Список улиц
        List<StreetSQL> street_gpt = new List<StreetSQL>();
        //Список домов
        List<HouseSQL> houses_street = new List<HouseSQL>();

        private string result_info { get; set; }


        public DBWorker2()
        {

            //this.SQL_db = new SQLCreator();

            ToolbarItem clear_info_button = new ToolbarItem();
            clear_info_button.Text = "Очистить поиск";
            clear_info_button.Clicked += ClearAllInfo;

            ToolbarItems.Add(clear_info_button);


            Frame subject_frame = new Frame();
            subject_frame.BorderColor = Color.GhostWhite;
            subject_frame.CornerRadius = 10;


            muniz_admin_picker.Title = "Выбирите вид представления данных";
            muniz_admin_picker.Items.Add("Административно-территориальное деление");


            muniz_admin_picker.SelectedIndexChanged += RFSubject;




            //<<<<<<<<<<<<<<<<<<<<< Добавляем picker >>>>>>>>>>>>>>>>>>>>>>>>>
            subject_frame.Content = muniz_admin_picker;
            this.stack_items.Children.Add(subject_frame);


            //<<<<<<<<<<<<<<<<<<<<< Добавляем picker субъект >>>>>>>>>>>>>>>>>>>>>>>>>
            //Frame для picker
            Frame rf_frame = new Frame();
            rf_frame.BorderColor = Color.Blue;
            rf_frame.CornerRadius = 10;
            //Заголовок picker 
            rf_subject_picker.Title = "Выберите субъект РФ";
            //Добавляю метод обрабатывающий выбор субъекта
            this.rf_subject_picker.SelectedIndexChanged += AdminDistrict;
            //Добавляем picker во frame
            rf_frame.Content = rf_subject_picker;
            rf_subject_picker.IsVisible = false;
            //Добавляю picker на экран пользователя
            this.stack_items.Children.Add(rf_frame);


            //<<<<<<<<<<<<<<<<<<<<< Добавляем picker районы >>>>>>>>>>>>>>>>>>>>>>>>>
            //Frame для picker
            Frame gpt_frame = new Frame();
            gpt_frame.BorderColor = Color.IndianRed;
            gpt_frame.CornerRadius = 10;
            //Заголовок picker 
            admin_district_picker.Title = "Выберите административный район";
            this.admin_district_picker.SelectedIndexChanged += CityPGT;
            //Добавляем picker во frame
            gpt_frame.Content = this.admin_district_picker;
            admin_district_picker.IsVisible = false;
            //Выводим picker на экран пользователя
            this.stack_items.Children.Add(gpt_frame);


            //<<<<<<<<<<<<<<<<<<<<< Добавляем picker ГПТ >>>>>>>>>>>>>>>>>>>>>>>>>
            this.city_gpt_picker.Title = "Выберите город или посёлок городского типаы";
            this.city_gpt_picker.SelectedIndexChanged += StreetPGT;
            this.city_gpt_picker.IsVisible = false;
            this.stack_items.Children.Add(this.city_gpt_picker);

            //<<<<<<<<<<<<<<<<<<<<< Добавляем picker улиц >>>>>>>>>>>>>>>>>>>>>>>>>
            this.streets_picker.Title = "Выберите улицу";
            this.streets_picker.SelectedIndexChanged += HouseStreet;
            this.streets_picker.IsVisible = false;
            
            this.stack_items.Children.Add(this.streets_picker);

            //<<<<<<<<<<<<<<<<<<<<< Добавляем picker домов >>>>>>>>>>>>>>>>>>>>>>>>>
            this.houses_picker.Title = "Выберите интересующие дома";
            this.houses_picker.SelectedIndexChanged += ObjectInfo;
            this.houses_picker.IsVisible = false;
            
            this.stack_items.Children.Add(this.houses_picker);


            ScrollView all_scroll = new ScrollView();
            all_scroll.Content = stack_items;

            this.Content = all_scroll;

        }

        //Метод очистки поиска
        private async void ClearAllInfo(object sender, EventArgs e)
        {

            try
            {

                //
                this.houses_picker.IsVisible = false;
                this.streets_picker.IsVisible = false;
                this.city_gpt_picker.IsVisible = false;
                this.admin_district_picker.IsVisible = false;


                this.city_gpt_picker.IsEnabled = true;
                this.admin_district_picker.IsEnabled = true;
                this.rf_subject_picker.IsEnabled = true;

                //ВНИМАНИЕ поскольку когда мы очищаем Picker происходит выбор другого элемента SelectedIndexChanged
                //то перед его очищением необходимо удалить функцию срабатывающую при выборе, иначе мы произведём поиск
                //пустых значений в базе данных что очень долго
                this.houses_picker.SelectedIndexChanged -= ObjectInfo;
                this.houses_picker.ItemsSource = null;
                this.houses_picker.SelectedIndexChanged += ObjectInfo;

                this.streets_picker.SelectedIndexChanged -= HouseStreet;
                this.streets_picker.ItemsSource = null;
                this.streets_picker.SelectedIndexChanged += HouseStreet;

                this.city_gpt_picker.SelectedIndexChanged -= StreetPGT;
                this.city_gpt_picker.ItemsSource = null;
                this.city_gpt_picker.SelectedIndexChanged += StreetPGT;

                this.admin_district_picker.SelectedIndexChanged -= CityPGT;
                this.admin_district_picker.ItemsSource = null;
                this.admin_district_picker.SelectedIndexChanged += CityPGT;

                while (this.stack_items.Children.Count != 6)
                {
                    this.stack_items.Children.Remove(stack_items.Children[stack_items.Children.Count - 1]);
                }
                

                //
                this.result_info = "";

                //Возвращаю доступ к picker с субъектами РФ
                this.rf_subject_picker.IsEnabled = true;

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.ToString(), "OK");
            }
            

        }


        //Метод загружающий субъекты РФ в графическое окружение
        private async void RFSubject(object sender, EventArgs e)
        {

            this.muniz_admin_picker.IsEnabled = false;

            this.rf_subject_picker.IsVisible = true;
            this.rf_subject_picker.IsEnabled = true;

            try
            {
                this.subjects = await App.DB.GetSubjectsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------- ERROR subject ------------");
                Console.WriteLine(ex.ToString());
            }

            rf_subject_picker.ItemsSource = this.subjects.Select(x => x.name + " " + x.sorc).ToList();

            /*foreach (var item in this.subjects)
            {
                
                rf_subject_picker.Items.Add(item.name + " " + item.sorc);
            }*/

            rf_subject_picker.IsVisible = true;


        }


        //Метод загружающий административные районы в графическое окружение
        private async void AdminDistrict(object sender, EventArgs e)
        {

            try
            {

                this.admin_district = await App.DB.GetDistrictAsync(this.subjects[this.rf_subject_picker.SelectedIndex].Id);

                admin_district_picker.ItemsSource = this.admin_district.Select(x => x.name + " " + x.sorc).ToList();


                admin_district_picker.IsVisible = true;
                rf_subject_picker.IsEnabled = false;

            }
            catch (Exception ex)
            {
                Console.WriteLine("----------- ERROR admin district ------------");
                Console.WriteLine(ex.ToString());
            }

        }


        //Метод загружающий города и пгт в графическое окружение
        private async void CityPGT(object sender, EventArgs e)
        {

            try
            {

                //if (this.subjects[this.rf_subject_picker.SelectedIndex].sorc != "Город")

                this.citys_gpt = await App.DB.GetCityGPTAsync(this.admin_district[admin_district_picker.SelectedIndex].Id);
                if (this.citys_gpt.Count > 0)
                {
                    
                    this.citys_gpt.Sort((x, y) => x.name.CompareTo(y.name));
                    city_gpt_picker.ItemsSource = this.citys_gpt.Select(x => x.name + " " + x.sorc).ToList();

                    city_gpt_picker.IsVisible = true;
                    admin_district_picker.IsEnabled = false;
                }
                else
                {
                    ObjectInfo(sender, e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------- ERROR citygpt ------------");
                Console.WriteLine(ex.ToString());
            }

        }


        //Метод загружающий улицы в графическое окружение
        private async void StreetPGT(object sender, EventArgs e)
        {

            try
            {
                this.street_gpt = await App.DB.GetStreetAsync(this.citys_gpt[city_gpt_picker.SelectedIndex].Id);

                if (this.street_gpt.Count > 0)
                {
                    Console.WriteLine($"----------- Улицы {this.street_gpt.Count} ------------");

                    this.street_gpt.Sort( (x, y) => x.name.CompareTo(y.name) );
                    streets_picker.ItemsSource = this.street_gpt.Select(x => x.name + " " + x.sorc).ToList(); ;

                    streets_picker.IsVisible = true;
                    city_gpt_picker.IsEnabled = false;
                }
                else
                {
                    ObjectInfo(sender, e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------- ERROR street ------------");
                Console.WriteLine(ex.ToString());
            }


        }

      


        //Метод загружающий дома в графическое окружение
        private async void HouseStreet(object sender, EventArgs e)
        {

            try
            {
                houses_picker.ItemsSource = null;

                while (this.stack_items.Children.Count != 6)
                {
                    this.stack_items.Children.Remove(stack_items.Children[stack_items.Children.Count - 1]);
                }

                this.houses_street = await App.DB.GetHouseAsync(this.street_gpt[streets_picker.SelectedIndex].Id);
                
                if (this.houses_street.Count > 0)
                {

                    houses_picker.ItemsSource = this.houses_street.Select(x => x.name + " " + x.sorc.ToLower()).ToList();


                    houses_picker.IsVisible = true;
                }
                else
                {
                    ObjectInfo(sender, e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("----------- ERROR house ------------");
                Console.WriteLine(ex.ToString());
            }

        }


        private async void ObjectInfo(object sender, EventArgs e)
        {

            while (this.stack_items.Children.Count != 6)
            {
                this.stack_items.Children.Remove(stack_items.Children[stack_items.Children.Count - 1]);
            }

            //Frame для picker
            Frame object_info_frame = new Frame();
            object_info_frame.BorderColor = Color.YellowGreen;

            var search_result = "";

            if (this.houses_picker.Items.Count != 0)
            {
                var user_choice = houses_street[houses_picker.SelectedIndex];
                object_info_frame.Content = new Label
                {
                    TextColor = Color.WhiteSmoke,
                    FontSize = 16,
                    FontFamily = "Verdana",
                    Text = "Результат поиска\n\n" +
                    search_result + user_choice.name + "\n" +
                    "Код региона: " + user_choice.code.Substring(0, 2) + "\n" +
                    "Код ИФНС (ГНИ): " + this.subjects[rf_subject_picker.SelectedIndex].gnimb + "\n" +
                    "Код КЛАДР: " + user_choice.code + "\n" +
                    "Код ОКАТО: " + user_choice.octd + "\n" +
                    "Код налоговой: " + subjects[rf_subject_picker.SelectedIndex].gnimb + "\n" +
                    "Почтовый индекс: " + user_choice.mail_index
                };
                search_result = "Дома: ";


                this.result_info = this.subjects[rf_subject_picker.SelectedIndex].name + " " + this.subjects[rf_subject_picker.SelectedIndex].sorc + " => " +
                        this.admin_district[admin_district_picker.SelectedIndex].name + " " + this.admin_district[admin_district_picker.SelectedIndex].sorc + " => " +
                        this.citys_gpt[city_gpt_picker.SelectedIndex].name + "\n" +
                        user_choice.sorc + ": " + user_choice.name + "\n" +
                        "Код региона: " + user_choice.code.Substring(0, 2) + "\n" +
                        "Код ИФНС (ГНИ): " + this.subjects[rf_subject_picker.SelectedIndex].gnimb + "\n" +
                        "Код КЛАДР: " + user_choice.code + "\n" +
                        "Код ОКАТО: " + user_choice.octd + "\n" +
                        "Почтовый индекс: " + user_choice.mail_index + " ";
            }
            else if (this.streets_picker.Items.Count != 0)
            {
                var user_choice = street_gpt[streets_picker.SelectedIndex];
                var user_choice_citygpt = citys_gpt[city_gpt_picker.SelectedIndex];

                try
                {
                    search_result = "У выбранной улицы нет домов!!!\n\nУлица: ";
                    object_info_frame.Content = new Label
                    {
                        TextColor = Color.WhiteSmoke,
                        FontSize = 16,
                        FontFamily = "Verdana",
                        Text = "Результат поиска\n\n" +
                        search_result + user_choice.name + "\n" +
                        "Код региона: " + user_choice.code.Substring(0, 2) + "\n" +
                        "Код ИФНС (ГНИ): " + this.subjects[rf_subject_picker.SelectedIndex].gnimb + "\n" +
                        "Код КЛАДР: " + user_choice.code + "\n" +
                        "Код ОКАТО: " + user_choice_citygpt.octd + "\n" +
                        "Код налоговой: " + subjects[rf_subject_picker.SelectedIndex].gnimb + "\n"
                    };

                    this.result_info = this.subjects[rf_subject_picker.SelectedIndex].name + " " + this.subjects[rf_subject_picker.SelectedIndex].sorc + " => " +
                        this.admin_district[admin_district_picker.SelectedIndex].name + " " + this.admin_district[admin_district_picker.SelectedIndex].sorc + " => " +
                        this.citys_gpt[city_gpt_picker.SelectedIndex].name + "\n" +
                        user_choice.sorc + ": " + user_choice.name + "\n" +
                        "Код региона: " + user_choice.code.Substring(0, 2) + "\n" +
                        "Код ИФНС (ГНИ): " + this.subjects[rf_subject_picker.SelectedIndex].gnimb + "\n" +
                        "Код КЛАДР: " + user_choice.code + "\n" +
                        "Код ОКАТО: " + user_choice_citygpt.octd + "\n";
                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------- ERROR Street ------------");
                    Console.WriteLine(ex.ToString());
                }
                
                
            }
            else if (this.city_gpt_picker.Items.Count != 0)
            {
                var user_choice = citys_gpt[city_gpt_picker.SelectedIndex];
                object_info_frame.Content = new Label
                {
                    TextColor = Color.WhiteSmoke,
                    FontSize = 15,
                    FontFamily = "Verdana",
                    Text = "Результат поиска\n\n" +
                    search_result + user_choice.name + "\n" +
                    "Код региона: " + user_choice.code.Substring(0, 2) + "\n" +
                    "Код ИФНС (ГНИ): " + this.subjects[rf_subject_picker.SelectedIndex].gnimb + "\n" +
                    "Код КЛАДР: " + user_choice.code + "\n" +
                    "Код ОКАТО: " + user_choice.octd + "\n" +
                    "Код налоговой: " + subjects[rf_subject_picker.SelectedIndex].gnimb + "\n"
                };
                search_result = "У выбранного населённого пункта (города, села и т.д.) нет улиц\n\nНаселённого пункт: ";

                this.result_info = this.subjects[rf_subject_picker.SelectedIndex].name + " " + this.subjects[rf_subject_picker.SelectedIndex].sorc + " => " +
                        this.admin_district[admin_district_picker.SelectedIndex].name + " " + this.admin_district[admin_district_picker.SelectedIndex].sorc + " => " +
                        this.citys_gpt[city_gpt_picker.SelectedIndex].name + " " + this.citys_gpt[city_gpt_picker.SelectedIndex].sorc + "\n" +
                        user_choice.sorc + ": " + user_choice.name + "\n" +
                        "Код региона: " + user_choice.code.Substring(0, 2) + "\n" +
                        "Код ИФНС (ГНИ): "+ this.subjects[rf_subject_picker.SelectedIndex].gnimb + "\n" + 
                        "Код КЛАДР: " + user_choice.code + "\n" +
                        "Код ОКАТО: " + user_choice.octd + "\n" +
                        "Код территорияального участка ИФНС: " + user_choice.uno + " ";
               
            }

            

            Button copy_button = new Button();
            copy_button.Text = "Скопировать результат поиска";
            copy_button.FontFamily = "Verdana";
            copy_button.FontSize = 16;
            //copy_button.HorizontalOptions = LayoutOptions.Fill;
            copy_button.VerticalOptions = LayoutOptions.Fill;
            //copy_button.Margin = new Thickness(15, 0, 5, 0);
            copy_button.CornerRadius = 10;
            copy_button.Clicked += CopyClipboardAsync;

            ImageButton favourite_button = new ImageButton();
            favourite_button.HorizontalOptions = LayoutOptions.Fill;
            favourite_button.VerticalOptions = LayoutOptions.Fill;
            favourite_button.BackgroundColor = new Color(0, 0, 0, 0);
            favourite_button.CornerRadius = 10;
            favourite_button.Clicked += AddFavourite;
            favourite_button.Source = "favourite.png";

            StackLayout button_layout = new StackLayout();
            button_layout.HeightRequest = 50;
            button_layout.Margin = new Thickness(15, 0, 15, 15);

            button_layout.Orientation = StackOrientation.Horizontal;
            button_layout.Children.Add(copy_button);
            button_layout.Children.Add(favourite_button);

            object_info_frame.CornerRadius = 5;
            object_info_frame.BackgroundColor = Color.Black;
            object_info_frame.Margin = new Thickness(10, 10, 10, 10);

            this.stack_items.Children.Add(object_info_frame);
            this.stack_items.Children.Add(button_layout);

        }

        private async void CopyClipboardAsync(object sender, EventArgs e)
        {

            await Clipboard.SetTextAsync(this.result_info);
            await DisplayAlert("Уведомление", "Информации скопирована в буфер обмена", "ОK");

        }

        private async void AddFavourite(object sender, EventArgs e)
        {

            FavouriteSQL favourite = new FavouriteSQL();
            if (this.houses_picker.Items.Count != 0)
            {
                var user_choice = houses_street[houses_picker.SelectedIndex];
                favourite.name = user_choice.name;
                favourite.sorc = user_choice.sorc;
                favourite.name_subject = this.subjects[rf_subject_picker.SelectedIndex].name + " " + this.subjects[rf_subject_picker.SelectedIndex].sorc;
                favourite.name_district = this.admin_district[admin_district_picker.SelectedIndex].name + " " + this.admin_district[admin_district_picker.SelectedIndex].sorc;
                favourite.name_citygpt = this.citys_gpt[city_gpt_picker.SelectedIndex].name + " " + this.citys_gpt[city_gpt_picker.SelectedIndex].sorc;
                favourite.octd = this.citys_gpt[city_gpt_picker.SelectedIndex].octd;
                favourite.gnimb = this.subjects[rf_subject_picker.SelectedIndex].gnimb;
                favourite.code = user_choice.code;
                favourite.mail_index = int.Parse(user_choice.mail_index);
            }
            else if (this.streets_picker.Items.Count != 0)
            {
                var user_choice = street_gpt[streets_picker.SelectedIndex];
                favourite.name = user_choice.name;
                favourite.sorc = user_choice.sorc;
                favourite.name_subject = this.subjects[rf_subject_picker.SelectedIndex].name + " " + this.subjects[rf_subject_picker.SelectedIndex].sorc;
                favourite.name_district = this.admin_district[admin_district_picker.SelectedIndex].name + " " + this.admin_district[admin_district_picker.SelectedIndex].sorc;
                favourite.name_citygpt = this.citys_gpt[city_gpt_picker.SelectedIndex].name + " " + this.citys_gpt[city_gpt_picker.SelectedIndex].sorc;
                favourite.octd = this.citys_gpt[city_gpt_picker.SelectedIndex].octd;
                favourite.gnimb = this.subjects[rf_subject_picker.SelectedIndex].gnimb;
                favourite.code = user_choice.code;
            }
            else if (this.city_gpt_picker.Items.Count != 0)
            {

                var user_choice = citys_gpt[city_gpt_picker.SelectedIndex];
                favourite.name = user_choice.name;
                favourite.sorc = user_choice.sorc;
                favourite.name_subject = this.subjects[rf_subject_picker.SelectedIndex].name + " " + this.subjects[rf_subject_picker.SelectedIndex].sorc;
                favourite.name_district = this.admin_district[admin_district_picker.SelectedIndex].name + " " + this.admin_district[admin_district_picker.SelectedIndex].sorc;
                favourite.name_citygpt = favourite.name + " " + favourite.sorc;
                favourite.octd = favourite.octd;
                favourite.gnimb = this.subjects[rf_subject_picker.SelectedIndex].gnimb;
                favourite.code = user_choice.code;

            }

            var result = App.DB.DB.Table<FavouriteSQL>().ToList();
            var clone_test = result.Any(item => (item.name == favourite.name) && (item.name_subject == favourite.name_subject) && (item.name_district == favourite.name_district) && (item.name_citygpt == favourite.name_citygpt));

            Console.WriteLine("-----------" + clone_test + "------------");

            if (!clone_test)
            {
                App.DB.DB.Insert(favourite);
            }

            await DisplayAlert("Уведомление", "Адресный объект добавлен в избранное", "ОK");

        }

        //Открываю диалоговое окно предпреждения и закрываю приложение
        private async void InfoDisplayAndCloseAsync(string zag, string body, string but)
        {

            //Открыть диалоговое окно с информацией
            await DisplayAlert(zag, body, "ОK");
           

        }//InfoDisplayAndCloseAsync

    }


}


