﻿using FluentNetease.Classes;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace FluentNetease.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public static SearchPage INSTANCE;

        private ObservableCollection<Song> ContentCollection;
        private SearchRequest CurrentSearchRequest;

        public SearchPage()
        {
            this.InitializeComponent();
            ContentCollection = new ObservableCollection<Song>();
            INSTANCE = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Search((SearchRequest)e.Parameter);
        }

        private void MusicNameButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.PLAYER_INSTANCE.Play(new Music { ID = (string)((FrameworkElement)sender).DataContext });
        }

        private void ArtistNameButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AlbumNameButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.NAV_FRAME.Navigate(typeof(AlbumPage), ((FrameworkElement)sender).DataContext);
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            Search(CurrentSearchRequest.PrevPage());
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            Search(CurrentSearchRequest.NextPage());
        }

        public async void Search(SearchRequest request)
        {
            CurrentSearchRequest = request;
            var Result = await Network.SearchAsync(request);
            if (Result.SearchResults != null)
            {
                ContentCollection.Clear();
                foreach (var Item in Result.SearchResults)
                {
                    ContentCollection.Add(Item);
                };
                PageText.Text = request.Page.ToString() + " / " + Result.CurrentPage.ToString();
                PreviousPageButton.IsEnabled = 1 < request.Page;
                NextPageButton.IsEnabled = request.Page < Result.CurrentPage;
            }
        }
    }
}
