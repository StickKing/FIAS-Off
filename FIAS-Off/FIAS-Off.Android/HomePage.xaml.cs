using System.Collections.Generic;
using Xamarin.Forms;
using System;
using FIAS_off.Droid.FIAS_func;

namespace FIASoff
{
    public class HomePage : ContentPage
    {
        public HomePage()
        {

            DataBaseCreator DBCreator = new DataBaseCreator();

            DBCreator.DBPath = ApplicationContext.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments).AbsolutePath;

            AlertButton_Clicked( "Уведомление", DBCreator.DBPath);

            //Создаю элемент позволяющий двигать веритикально контент
            CarouselView home_page_info = new CarouselView();

            //Создаю шаблон для отоброжаемого контента в карусели
            var info_template = new DataTemplate(() =>
            {
                //Создаю элемент в котором будет хратиться вся информация
                StackLayout stklay = new StackLayout();

                //Устанавливаю позиционирование элементов в стэклэйаут по вертикали и горизонтали
                //stklay.HorizontalOptions = LayoutOptions.Center;
                //stklay.VerticalOptions = LayoutOptions.End;

                //Создаю элементы в которых будет храниться информация, задаю им нужные свойтсва
                Label info_txt = new Label { FontAttributes = FontAttributes.None, FontSize = 20, TextColor = Color.WhiteSmoke };
                Image info_img = new Image { };
                ActivityIndicator info_load = new ActivityIndicator { Color = Color.Orange, IsRunning = true };

                info_txt.HorizontalOptions = LayoutOptions.End;
                info_img.HorizontalOptions = LayoutOptions.Center;
                info_load.HorizontalOptions = LayoutOptions.Center;
                info_load.VerticalOptions = LayoutOptions.End;
                info_load.HorizontalOptions = LayoutOptions.Center;
                

                //Привязываю данные к шаблону 
                info_txt.SetBinding(Label.TextProperty, "info_text");
                info_img.SetBinding(Image.SourceProperty, "info_imag_path");
                info_load.SetBinding(ActivityIndicator.IsRunningProperty, "info_loading");

                //Добавляю элементы на стэклэйаут
                stklay.Children.Add(info_img);
                stklay.Children.Add(info_txt);
                stklay.Children.Add(info_load);
                

                //Возвращаю полученную конструкцию
                return stklay;
            });

            //Привязываю созданный ранее шаблон с каруселью
            home_page_info.ItemTemplate = info_template;

            //Создаю список элементов с нужной информацией которая будет крутиться к карусели
            home_page_info.ItemsSource = new List<HomeInfo>
            {

                new HomeInfo { info_text = "Добро пожаловать в приложение FIAS-off", info_imag_path = "hello.gif", info_loading = false },
                new HomeInfo { info_text = "Данное прилодение предназначено для работы с базой данных ФИАС в формате КЛАДР." +
                "Данный формат был выбран в силу своей легковесности.", info_imag_path = "fias.gif", info_loading = false },
                new HomeInfo { info_text = "Добро пожаловать в приложение FIAS-off", info_imag_path = "", info_loading = true }

            };


            //Создаю индикатор прокрутки карусели
            IndicatorView home_indicator = new IndicatorView();
            //Указываю цвет заднего фона
            home_indicator.IndicatorColor = Color.AntiqueWhite;
            //Указываю цвет открытой в данный момент страницы
            home_indicator.SelectedIndicatorColor = Color.DodgerBlue;
            //Указываю размер индикатора страницы
            home_indicator.IndicatorSize = 7;

            home_page_info.IndicatorView = home_indicator;
            home_page_info.Loop = false;

           

            //Добавляю все элементы на страницу
            StackLayout global_stack = new StackLayout();
            global_stack.VerticalOptions = LayoutOptions.End;
            global_stack.Children.Add(home_page_info);
            global_stack.Children.Add(home_indicator);

            this.Content = global_stack;
            this.BackgroundColor = Color.LightSkyBlue;
            this.Padding = new Thickness(10, 20, 10, 20);

        }//HomePage


        private void InfoDisplay(string zag, string body)
        {
            DisplayAlert(zag, body, "ОK");
        }

    }


    //Класс информации для карусели
    public class HomeInfo
    {

        public string info_text { get; set; }
        public string info_imag_path { get; set; }
        public bool info_loading { get; set; }

    }//HomeInfo
}

