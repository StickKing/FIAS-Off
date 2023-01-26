using System;
using System.Threading;
using System.Threading.Tasks;
using FIAS_Off;
using Xamarin.Forms;
using Data;
using static SQLSpace.f_SQL_page;
using System.Linq;
using System.Collections.Generic;
using Java.Sql;
using Javax.Security.Auth;
using Xamarin.Forms.Shapes;
using static Android.Content.ClipData;
using Android.Graphics.Drawables;
using Xamarin.Essentials;

namespace FastSearchGUI
{
    public class FastSearch : ContentPage
    {
        StackLayout stack = new StackLayout();
        private SearchBar search_bar = new SearchBar();
        private ListView search_results = new ListView();

        List<SubDistCity> city_db { get; set; }
        IEnumerable<string> city_list { get; set; }
        List<StreetSQL> street_db { get; set; }

        int id_city = -1;
        int id_street { get; set; }

        int count_text = 0;
        int street_count = 1;


        public FastSearch()
        {

            ToolbarItem clear_info_button = new ToolbarItem();
            clear_info_button.Text = "Очистить поиск";
            clear_info_button.Clicked += ClearAllInfo;

            ToolbarItems.Add(clear_info_button);

            Label info_about = new Label();
            info_about.Text = "Введите наименование населённого пункта, выбирите в выпадающем списке нужный адресный объект, после начните вводить наименование улицы и так же выберите искомый пункт из списка";
            info_about.FontSize = 14;
            info_about.Margin= new Thickness(10, 10, 10, 10);
            info_about.FontFamily = "Arial";
            

            
            this.search_bar.Placeholder = "Москва, Домодедовская";
            this.search_bar.TextChanged += OnTextChangedAsync;


            this.search_results.ItemSelected += OnSelectItem;

            
            stack.Children.Add(info_about);
            stack.Children.Add(search_bar);
            stack.Children.Add(search_results);

            ScrollView scroll = new ScrollView();
            scroll.Content = stack;
            Content = scroll;
            
        }

        private void ClearAllInfo(object sender, EventArgs e)
        {
            while (this.stack.Children.Count != 3)
            {
                this.stack.Children.Remove(stack.Children[stack.Children.Count - 1]);
            }
            search_results.IsVisible = true;
            search_bar.IsEnabled = true;
            search_bar.Text = "";
            GC.Collect();

            id_city = -1;
        }

        async void OnTextChangedAsync(object sender, EventArgs e)
        {
            SearchBar searchBar = (SearchBar)sender;
            if (search_bar.Text.Length != 0)
            {

                GC.Collect();

                    Thread search = new Thread(async () =>
                    {
                        await Device.InvokeOnMainThreadAsync(() =>
                        {

                            if ((search_bar.Text.Split(", ").Count() == 1) && (search_bar.Text.Count() > count_text))
                            {

                                
                                count_text = search_bar.Text.Count();
                                string search_item = "";
                                try
                                {
                                    search_item = search_bar.Text.ToUpper()[0] + search_bar.Text.ToLower().Substring(1);
                                    search_results.ItemsSource = null;
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine($"!!!!!! Error: {ex.ToString()}");
                                }

                                /*city_db = App.DB.DB.Query<SubDistCity>($"select Subject.id as id_sub, Subject.name as name_sub, District.id as id_dis, District.name as name_dis, CityGPT.id as id_cit, CityGPT.name as name_cit, CityGPT.sorc as sorc_cit " +
                                    $"from CityGPT " +
                                    $"inner join District ON CityGPT.id_district = District.id " +
                                    $"inner join Subject ON Subject.id = District.id_subject " +
                                    $"where CityGPT.name like '{search_item}%'");*/

                                city_db = App.DB.DB.Query<SubDistCity>($"select Subject.name as name_sub, Subject.sorc as sorc_sub, " +
                                    $"District.name as name_dis, District.sorc as sorc_dis," +
                                    $"CityGPT.id as id_cit, CityGPT.name as name_cit, CityGPT.sorc as sorc_cit, CityGPT.uno as uno_cit, CityGPT.gnimb as gnimb_cit, CityGPT.octd as octd_cit, CityGPT.code as code_cit " +
                                    $"from CityGPT " +
                                    $"inner join District ON CityGPT.id_district = District.id " +
                                    $"inner join Subject ON Subject.id = District.id_subject " +
                                    $"where CityGPT.name like '{search_item}%'");

                                this.search_results.ItemsSource = city_db.Select((x, index) => $"{index}) {x.name_cit} {x.sorc_cit} ({x.name_sub} => {x.name_dis}) ");
                            }
                            else if ((search_bar.Text.Count() > count_text) && (id_city != -1))
                            {
                                count_text = search_bar.Text.Count();
                                string search_item = "";
                                    try
                                    {

                                        search_item = search_bar.Text.Split(", ")[1].ToUpper()[0] + search_bar.Text.Split(", ")[1].ToLower().Substring(1);
                                        

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"!!!!!! Error: {ex.ToString()}");
                                    }

                                street_db = App.DB.DB.Query<StreetSQL>($"select * from Street where id_citygpt = {city_db[id_city].id_cit} and name like '{search_item}%'");
                                //Console.WriteLine($"!!!!!! Error: {street_db.Count}");
                                search_results.ItemsSource = null;

                                this.search_results.ItemsSource = street_db.Select((x, index) => $"{index}) {x.name} {x.sorc}");
                                
                            }
                            else
                            {
                                count_text = search_bar.Text.Count();
                            }
                           
                        });
                    });
                    search.Priority = System.Threading.ThreadPriority.Highest;
                    search.Start();
            }
        }

        void OnSelectItem(object sender, EventArgs e)
        {

            GC.Collect();

            if ((search_bar.Text.Split(", ").Count() == 1) && (search_bar.Text.Count() > 0))
            {
                id_city = (int)Char.GetNumericValue(this.search_results.SelectedItem.ToString()[0]);
                //Console.WriteLine($"!!!!!! Warning: {id_city}");
                //Console.WriteLine($"/ / / / / / / {id_city} / / / / / / / ");
                search_results.ItemsSource = null;

                this.search_bar.TextChanged -= OnTextChangedAsync;
                this.search_bar.Text = this.search_results.SelectedItem.ToString().Split(" ")[1] + ", ";
                this.search_bar.TextChanged += OnTextChangedAsync;

                if (App.DB.DB.Query<StreetSQL>($"select * from Street where id_citygpt = {city_db[id_city].id_cit}").Count == 0)
                {
                    //Console.WriteLine($"/ / / / / / / без улиц и домов  / / / / / / / ");
                    street_count = 0;
                    ViewSearchItem();
                }
            }
            else if (search_bar.Text.Count() > 0)
            {

                id_street = (int)Char.GetNumericValue(this.search_results.SelectedItem.ToString()[0]);
                //Console.WriteLine($"/ / / / / / / {id_street} / / / / / / / ");
                search_results.ItemsSource = null;

                this.search_bar.TextChanged -= OnTextChangedAsync;
                this.search_bar.Text = search_bar.Text.Split(", ")[0] + ", " + this.search_results.SelectedItem.ToString().Substring(2);
                this.search_bar.TextChanged += OnTextChangedAsync;

                
                ViewSearchItem();
                search_results.ItemsSource = null;

            }
            
            //this.search_bar.Text = this.search_results.se;
        }

        void ViewSearchItem()
        {
            
            search_results.IsVisible = false;
            search_bar.IsEnabled = false;

            Thread search = new Thread(async () =>
            {
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    if (street_count == 0)
                    {
                        Console.WriteLine($"/ / / / / / /  город без домов / / / / / / / ");
                        Frame item_frame = new Frame();
                        item_frame.BorderColor = Color.YellowGreen;
                        item_frame.CornerRadius = 5;
                        //item_frame.BackgroundColor = Color.Black;
                        item_frame.Margin = new Thickness(10, 10, 10, 10);

                        Label item_label = new Label();
                        item_label.FontFamily = "Verdana";
                        item_label.FontSize = 15;

                        Button copy_button = new Button();
                        copy_button.Text = "Скопировать результат поиска";
                        copy_button.FontFamily = "Verdana";
                        copy_button.FontSize = 14;
                        copy_button.VerticalOptions = LayoutOptions.Fill;
                        copy_button.CornerRadius = 10;
                        

                        ImageButton favourite_button = new ImageButton();
                        favourite_button.HorizontalOptions = LayoutOptions.Fill;
                        favourite_button.VerticalOptions = LayoutOptions.Fill;
                        favourite_button.BackgroundColor = new Color(0, 0, 0, 0);
                        favourite_button.CornerRadius = 10;
                        favourite_button.Clicked += async delegate (object sender, EventArgs e) {

                            await AddFavouriteAsync(sender, e, 0, city_db[id_city]);

                        }; ; ;

                        favourite_button.Source = "favourite.png";

                        StackLayout button_layout = new StackLayout();
                        button_layout.HeightRequest = 40;
                        button_layout.Margin = new Thickness(10, 0, 10, 0);

                        button_layout.Orientation = StackOrientation.Horizontal;
                        button_layout.Children.Add(copy_button);
                        button_layout.Children.Add(favourite_button);

                        StackLayout all_item = new StackLayout();
                        all_item.Children.Add(item_label);
                        all_item.Children.Add(button_layout);

                        //Console.WriteLine($"/ / / / / / / дом / / / / / / / ");
                        item_label.Text = $"{city_db[id_city].sorc_cit}: {city_db[id_city].name_cit} \n" +
                        $"Код КЛАДР: {city_db[id_city].code_cit} \n" +
                        $"Код ОКАТО: {city_db[id_city].octd_cit} \n" +
                        $"Код ИФНС: {city_db[id_city].gnimb_cit}";


                        copy_button.Clicked += async delegate (object sender, EventArgs e) {

                            await CopyClipboardAsync(sender, e, item_label.Text);

                        };;

                        item_frame.Content = all_item;

                        this.stack.Children.Add(item_frame);
                        street_count = 1;
                    }
                    else
                    {
                        var houses = App.DB.DB.Query<HouseSQL>($"select * from House where id_street = {street_db[id_street].Id}");

                        if (houses.Count > 0)
                        {
                            Console.WriteLine($"/ / / / / / /  город с улице и домом / / / / / / / ");
                            foreach (var item in houses)
                            {

                                Frame item_frame = new Frame();
                                item_frame.BorderColor = Color.YellowGreen;
                                item_frame.CornerRadius = 5;
                                //item_frame.BackgroundColor = Color.Black;
                                item_frame.Margin = new Thickness(10, 10, 10, 10);

                                Label item_label = new Label();
                                item_label.FontFamily = "Verdana";
                                item_label.FontSize = 15;

                                Button copy_button = new Button();
                                copy_button.Text = "Скопировать результат поиска";
                                copy_button.FontFamily = "Verdana";
                                copy_button.FontSize = 14;
                                copy_button.VerticalOptions = LayoutOptions.Fill;
                                copy_button.CornerRadius = 10;

                                ImageButton favourite_button = new ImageButton();
                                favourite_button.HorizontalOptions = LayoutOptions.Fill;
                                favourite_button.VerticalOptions = LayoutOptions.Fill;
                                favourite_button.BackgroundColor = new Color(0, 0, 0, 0);
                                favourite_button.CornerRadius = 10;
                                favourite_button.Clicked += async delegate (object sender, EventArgs e) {

                                    await AddFavouriteAsync(sender, e, 2, city: city_db[id_city], house: item);

                                }; ;
                                favourite_button.Source = "favourite.png";

                                StackLayout button_layout = new StackLayout();
                                button_layout.HeightRequest = 40;
                                button_layout.Margin = new Thickness(10, 0, 10, 0);

                                button_layout.Orientation = StackOrientation.Horizontal;
                                button_layout.Children.Add(copy_button);
                                button_layout.Children.Add(favourite_button);

                                StackLayout all_item = new StackLayout();
                                all_item.Children.Add(item_label);
                                all_item.Children.Add(button_layout);

                                item_label.Text = $"Дома: {item.name.Replace(",", ", ")} \n" +
                                $"Код КЛАДР: {item.code} \n" +
                                $"Почтовый индекс: {item.mail_index} \n" +
                                $"Код ОКАТО: {item.octd} \n" +
                                $"Код ИФНС: {city_db[id_city].gnimb_cit};";

                                copy_button.Clicked += async delegate (object sender, EventArgs e) {

                                    await CopyClipboardAsync(sender, e, item_label.Text);

                                }; ;

                                item_frame.Content = all_item;

                                this.stack.Children.Add(item_frame);

                            }

                        }
                        else
                        {
                            Frame item_frame = new Frame();
                            item_frame.BorderColor = Color.YellowGreen;
                            item_frame.CornerRadius = 5;
                            //item_frame.BackgroundColor = Color.Black;
                            item_frame.Margin = new Thickness(10, 10, 10, 10);

                            Label item_label = new Label();
                            item_label.FontFamily = "Verdana";
                            item_label.FontSize = 15;

                            Button copy_button = new Button();
                            copy_button.Text = "Скопировать результат поиска";
                            copy_button.FontFamily = "Verdana";
                            copy_button.FontSize = 14;
                            copy_button.VerticalOptions = LayoutOptions.Fill;
                            copy_button.CornerRadius = 10;

                            ImageButton favourite_button = new ImageButton();
                            favourite_button.HorizontalOptions = LayoutOptions.Fill;
                            favourite_button.VerticalOptions = LayoutOptions.Fill;
                            favourite_button.BackgroundColor = new Color(0, 0, 0, 0);
                            favourite_button.CornerRadius = 10;
                            favourite_button.Clicked += async delegate (object sender, EventArgs e) {

                                await AddFavouriteAsync(sender, e, 1, city: city_db[id_city], street: street_db[id_street]);

                            };;
                            favourite_button.Source = "favourite.png";

                            StackLayout button_layout = new StackLayout();
                            button_layout.HeightRequest = 40;
                            button_layout.Margin = new Thickness(10, 0, 10, 0);

                            button_layout.Orientation = StackOrientation.Horizontal;
                            button_layout.Children.Add(copy_button);
                            button_layout.Children.Add(favourite_button);

                            StackLayout all_item = new StackLayout();
                            all_item.Children.Add(item_label);
                            all_item.Children.Add(button_layout);

                            item_label.Text = $"{street_db[id_street].sorc}: {street_db[id_street].name} \n" +
                            $"Код КЛАДР: {street_db[id_street].code} \n" +
                            $"Код ОКАТО: {city_db[id_city].octd_cit} \n" +
                            $"Код ИФНС: {city_db[id_city].gnimb_cit};";

                            copy_button.Clicked += async delegate (object sender, EventArgs e) {

                                await CopyClipboardAsync(sender, e, item_label.Text);

                            };;

                            item_frame.Content = all_item;

                            this.stack.Children.Add(item_frame);

                        }
                    }
                });
            });
            search.Priority = System.Threading.ThreadPriority.Highest;
            search.Start();

        }

        private async Task AddFavouriteAsync(object sender, EventArgs e, int type_item, SubDistCity city, StreetSQL street = null, HouseSQL house = null)
        {
            FavouriteSQL favourite = new FavouriteSQL();
            if (type_item == 2)
            {
                favourite.name = house.name;
                favourite.sorc = house.sorc;
                favourite.name_subject = city.name_sub + " " + city.sorc_sub;
                favourite.name_district = city.name_dis + " " + city.sorc_dis;
                favourite.name_citygpt = city.name_cit;
                favourite.octd = city.octd_cit;
                favourite.gnimb = city.gnimb_cit;
                favourite.code = house.code;
                favourite.mail_index = int.Parse(house.mail_index);
            }
            else if (type_item == 1)
            {
                favourite.name = street.name;
                favourite.sorc = street.sorc;
                favourite.name_subject = city.name_sub + " " + city.sorc_sub;
                favourite.name_district = city.name_dis + " " + city.sorc_dis;
                favourite.name_citygpt = city.name_cit;
                favourite.octd = city.octd_cit;
                favourite.gnimb = city.gnimb_cit;
                favourite.code = street.code;
            }
            else if (type_item == 0)
            {

                favourite.name = city.name_cit;
                favourite.sorc = city.sorc_cit;
                favourite.name_subject = city.name_sub + " " + city.sorc_sub;
                favourite.name_district = city.name_dis + " " + city.sorc_dis;
                favourite.name_citygpt = favourite.name + " " + favourite.sorc;
                favourite.octd = city.octd_cit;
                favourite.gnimb = city.gnimb_cit;
                favourite.code = city.code_cit;

            }

            var result = App.DB.DB.Table<FavouriteSQL>();
            var clone_test = result.Any(item => (item.name == favourite.name) && (item.name_subject == favourite.name_subject) && (item.name_district == favourite.name_district) && (item.name_citygpt == favourite.name_citygpt));

            if (!clone_test)
            {
                App.DB.DB.Insert(favourite);
            }

            await DisplayAlert("Уведомление", "Адресный объект добавлен в избранное", "ОK");
        }

        private async Task CopyClipboardAsync(object sender, EventArgs e, string item)
        {
            await Clipboard.SetTextAsync(item);
            await DisplayAlert("Уведомление", "Информации скопирована в буфер обмена", "ОK");
        }
    }
}


