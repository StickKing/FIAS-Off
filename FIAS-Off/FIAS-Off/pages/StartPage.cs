using Xamarin.Forms;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Android.OS;
using FIAS_Off;
using Data;
using StartSpace;

namespace StartGUI
{
    public class StartPage : ContentPage
    {

        DataBaseCreator DBCreator = new DataBaseCreator();
        //private string DBPath { get; set; }


        public StartPage()
        {

            //Делаю верний бар не видимым
            Shell.SetNavBarIsVisible(this, false);
            //Отключаю возможность пользоваться боковым меню на этой старнице
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

            //Проверяю есть ли достаточное количесто свободной памяти
            if (!DBCreator.FreeSpaceExists())
            {
                //Если памяти нет, то вывожу сообщение об ошибке и закрываю приложение
                InfoDisplayAndCloseAsync("Проблема", "Не хватает памяти для работы приложения. Освободите 500 Мб.", "Ok");
            }

            if (!DBCreator.CheckInternetConnection())
            {
                InfoDisplayAndCloseAsync("Проблема", "Отсутствует подключение к интернету. Подключитесь к WiFi или включите мобильную связь и откройке приложние снова", "Ok");
            }


            //Создаю элемент позволяющий двигать веритикально контент
            CarouselView home_page_info = new CarouselView();

            //Создаю шаблон для отоброжаемого контента в карусели
            var info_template = new DataTemplate(() =>
            {
                //Создаю элемент в котором будет хратиться вся информация
                StackLayout stklay = new StackLayout();

                //Создаю элементы в которых будет храниться информация, задаю им нужные свойтсва
                Label info_txt = new Label
                {
                    FontAttributes = FontAttributes.None,
                    FontFamily = "Verdana",
                    FontSize = 25,
                    TextColor = Color.WhiteSmoke,
                };

                Label info_text_two = new Label
                {
                    FontAttributes = FontAttributes.None,
                    FontFamily = "Verdana",
                    FontSize = 20,
                    TextColor = Color.WhiteSmoke,
                };

                Image info_img = new Image { };
                ActivityIndicator info_load = new ActivityIndicator { Color = Color.Orange, IsRunning = true };

                info_txt.HorizontalOptions = LayoutOptions.FillAndExpand;
                info_txt.Margin = new Thickness(20, 20, 20, 20);

                info_load.Margin = new Thickness(20);


                //Привязываю данные к шаблону 
                info_txt.SetBinding(Label.TextProperty, "info_text");
                info_img.SetBinding(Image.SourceProperty, "info_imag_path");
                info_load.SetBinding(ActivityIndicator.IsRunningProperty, "info_loading");

                //Добавляю элементы на стэклэйаут
                stklay.Children.Add(info_img);
                stklay.Children.Add(info_txt);
                //stklay.Children.Add(info_text_two);
                stklay.Children.Add(info_load);

                //Возвращаю полученную конструкцию
                return stklay;

            });

            //Привязываю созданный ранее шаблон с каруселью
            home_page_info.ItemTemplate = info_template;

            //Создаю список элементов с нужной информацией которая будет крутиться к карусели
            home_page_info.ItemsSource = new List<HomeInfo>
            {

                new HomeInfo { info_text = "Добро пожаловать в приложение FIAS-off!", info_imag_path = "fias.png", info_loading = false },
                new HomeInfo { info_text = "Данное приложение предназначено для работы с федеральной информационной адресной системой в формате КЛАДР." +
                " Данная система распространяется свободно, и загружается с официального сайта налоговой (fias.nalog.ru)."
                , info_imag_path = "hello.png", info_loading = false },
                new HomeInfo { info_text = "На данный момент идёт подготовка адресной системы. Это займёт от 5 до 10 минут, " +
                "зависит от мощности вашего смартфона. \n\nДля корректности проходящей подготовки просьба не закрывать приложение" +
                " до тех пор пока не откроется экран поиска по адресной системе.", info_imag_path = "", info_loading = true }

            };


            //Создаю индикатор прокрутки карусели
            IndicatorView home_indicator = new IndicatorView();
            //Указываю цвет заднего фона
            home_indicator.IndicatorColor = Color.AntiqueWhite;
            //Указываю цвет открытой в данный момент страницы
            home_indicator.SelectedIndicatorColor = Color.DodgerBlue;
            //Указываю размер индикатора страницы
            home_indicator.IndicatorSize = 7;

            //Привязываю к карусели indicatorview
            home_page_info.IndicatorView = home_indicator;
            //Отключаю цикличное кручение карусели
            home_page_info.Loop = false;



            //Добавляю все элементы на страницу
            StackLayout global_stack = new StackLayout();
            global_stack.VerticalOptions = LayoutOptions.End;
            global_stack.Children.Add(home_page_info);
            global_stack.Children.Add(home_indicator);


            //Добавляю весь контент на экран и настраиваю контент страницу
            this.Content = global_stack;
            this.BackgroundColor = Color.DeepSkyBlue;
            this.Padding = new Thickness(10, 20, 10, 20);

            Console.WriteLine($"__________________________________  Begin app ___________________________________________");

            DBPreparationAndToCancel();


        }//class StartPage


        //Открываю диалоговое окно предпреждения и закрываю приложение
        private async void InfoDisplayAndCloseAsync(string zag, string body, string but)
        {

            //Открыть диалоговое окно с информацией
            await DisplayAlert(zag, body, "ОK");
            //Закрываю приложение после того как пользователь нажмёт на кнопку "ОК"
            Process.KillProcess(Process.MyPid());

        }//InfoDisplayAndCloseAsync


        //Функция подготавливающая БД и возвращающая пользователя на основную старницу приложения
        public async Task DBPreparationAndToCancel()
        {

            //Запускаем загрузку и разорхивацию
            await DBCreator.DownloadAndUnzipDataBaseAsync();

            //Делаю элемент не видимым и не нажимаемым
            Shell.Current.CurrentItem.IsVisible = false;
            Shell.Current.CurrentItem.IsEnabled = true;

            //Перехожу на страницу работы с БД
            await Shell.Current.GoToAsync("//DBWork");
            
        }//DBPreparationAndToCancel

    }
}

