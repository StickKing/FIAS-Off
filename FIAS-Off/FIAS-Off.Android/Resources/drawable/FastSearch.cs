using System;

using Xamarin.Forms;
using static FIASOff.page_func.f_SQL_page;

namespace FIASOff.pages.MenuItemPage
{
    public class FastSearch : ContentPage
    {

        private SearchBar search_bar = new SearchBar();
        private SQLCreator sql_worcker { get; set; }

        public FastSearch(string DBPath)
        {

            this.sql_worcker = new SQLCreator(DBPath);

            Label info_about = new Label();
            info_about.Text = "Напишите через запятую субъект РФ, город или населённый пункт, улица, дом";
            info_about.FontSize = 18;
            info_about.FontFamily = "Arial";
            

            
            this.search_bar.Placeholder = "Пример Москва, Сухаревская, 22 ";

            Button search_button = new Button();
            search_button.CornerRadius = 10;
            
            search_button.Text = "Поиск";
            search_button.FontFamily = "Arial";
            search_button.FontSize = 14;
            search_button.Clicked += SearchClick;

            StackLayout stack = new StackLayout();
            stack.Children.Add(info_about);
            stack.Children.Add(search_bar);
            stack.Children.Add(search_button);
            Content = stack;
            
        }

        private async void SearchClick(object sender, EventArgs e)
        {
            string[] search_text = search_bar.Text.Replace(" ", "").Split(',');
            if (this.search_bar.Text == "" || search_text.Length > 1 )
            {
                var test = await this.sql_worcker.SearchAdress(search_text);
                await DisplayAlert("Ошибка", test.ToString(), "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Введены не корректные данные", "OK");
            }
        }


    }
}


