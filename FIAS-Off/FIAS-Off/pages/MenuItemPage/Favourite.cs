using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Data;
using FIAS_Off;
using SQLSpace;
using Xamarin.Essentials;
using Xamarin.Forms;
using static SQLSpace.f_SQL_page;

namespace FavouriteGUI
{
	public class Favourite : ContentPage
	{

        public ScrollView scrol_item = new ScrollView();
        public List<FavouriteSQL> favourite_items { get; set; }

        public Favourite()
		{
            this.Title = "Избранное";

           
            //var result = await App.DB.DB.Table<FavouriteSQL>().ToListAsync();
            this.favourite_items = App.DB.DB.Table<FavouriteSQL>().ToList();
            if (favourite_items != null)
            {
                CreateGUI(favourite_items);
            }

            

            RefreshView refreshView = new RefreshView();
            ICommand refreshCommand = new Command(() =>
            {

                Device.InvokeOnMainThreadAsync(async () =>
                {
                    this.favourite_items = App.DB.DB.Table<FavouriteSQL>().ToList();
                    CreateGUI(favourite_items);
                });
                refreshView.IsRefreshing = false;
            });
            refreshView.Command = refreshCommand;

            this.scrol_item.BackgroundColor = Color.LightSkyBlue;

            refreshView.Content = this.scrol_item;
            this.Content = refreshView;

        }

        private async Task CopyBFAsync(object sender, EventArgs e, FavouriteSQL item)
        {
            string result = "Адресный объект: " + item.name + " " + item.sorc +
                "Код КЛАДР: " + item.code + "\n" +
                "Код ОКАТО: " + item.octd + "\n" +
                "Код ИФНС (ГНИ): " + item.gnimb + "\n" +
                "Путь к объекту: " + item.name_subject + " => " + item.name_district + " => " + item.name_citygpt + "\n";

            if (item.mail_index != null)
            {
                result += "Почтовый индекс: " + item.mail_index;
            }
            await Clipboard.SetTextAsync(result);

            await DisplayAlert("Уведомление", "Информации скопирована в буфер обмена", "ОK");
        }

        private void CreateGUI(List<Data.FavouriteSQL> favourite_items)
        {


            StackLayout main_stack = new StackLayout();

            foreach (var item in favourite_items)
            {

                Frame frame_item = new Frame();



                frame_item.Margin = new Thickness(15, 8, 15, 2);
                frame_item.CornerRadius = 10;
                frame_item.BorderColor = Color.White;
                StackLayout stack_item = new StackLayout();


                Label id_label = new Label();
                id_label.Text = item.Id.ToString();
                id_label.TextColor = Color.Blue;
                id_label.FontFamily = "Verdana";
                id_label.FontSize = 24;
                id_label.HorizontalOptions = LayoutOptions.Center;
                stack_item.Children.Add(id_label);

                Label label_name = new Label();
                label_name.Text = "Адресный объект: " + item.name;
                stack_item.Children.Add(label_name);

                Label label_sorc = new Label();
                label_sorc.Text = "Тип адресного объектаы: " + item.sorc;
                stack_item.Children.Add(label_sorc);

                Label label_code = new Label();
                label_code.Text = "Код КЛАДР: " + item.code;
                stack_item.Children.Add(label_code);

                Label label_octd = new Label();
                label_octd.Text = "Код ОКАТО: " + item.octd;
                stack_item.Children.Add(label_octd);

                Label label_gnimb = new Label();
                label_gnimb.Text = "Код ИФНС (ГНИ): " + item.gnimb;
                stack_item.Children.Add(label_gnimb);

                if (item.mail_index != null)
                {
                    Label label_mail = new Label();
                    label_mail.Text = "Почтовый индекс: " + item.mail_index.ToString();
                    stack_item.Children.Add(label_mail);
                }

                Label label_name_subject = new Label();
                label_name_subject.Text = "Субъект РФ: " + item.name_subject;
                stack_item.Children.Add(label_name_subject);

                Label label_name_district = new Label();
                label_name_district.Text = "Административный район: " + item.name_district;
                stack_item.Children.Add(label_name_district);

                Label label_name_citygpt = new Label();
                label_name_citygpt.Text = "Населённый пункт: " + item.name_citygpt;
                stack_item.Children.Add(label_name_citygpt);

                Button copy_button = new Button();
                copy_button.Text = "Скопировать в буфер обмена";
                copy_button.Clicked += delegate (object sender, EventArgs e) {

                    CopyBFAsync(sender, e, item);

                };;

                stack_item.Children.Add(copy_button);


                frame_item.Content = stack_item;
                main_stack.Children.Add(frame_item);
            }

            scrol_item.Content = main_stack;


        }


    }
}


