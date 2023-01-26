using System;

using Xamarin.Forms;

namespace AboutGUI
{
    public class About : ContentPage
    {
        public About()
        {
            Title = "О приложении";


            StackLayout stack_lay = new StackLayout();
            stack_lay.WidthRequest = 250;
            stack_lay.BackgroundColor = Color.Black;
            
            Image icon = new Image()
            {
                Source = "fias.png",
                WidthRequest = 150,
            };


            Label info = new Label()
            {
                Text = "Данное приложение разработано для работы с федеральной информационной адресной системой Российской Федерации. " +
                "База данный используемая в приложении скачивается в формате КЛАДР, источником адресной базы служит " +
                "официальный сайт налоговой fias.nalog.ru. \n\n Версия приложения: 1.0",
                FontFamily = "Verdana",
                FontSize = 20,
                TextColor = Color.WhiteSmoke,
                //HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Margin = new Thickness(20),
            };

            stack_lay.Children.Add(icon);
            stack_lay.Children.Add(info);

            this.Content = stack_lay;


        }
    }
}


