using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using iDraw.Models;
using iDraw.Views;
using iDraw.ViewModels;
using iDraw.Services;
using System.Collections.ObjectModel;
using SQLite;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace iDraw.Views
{
    public partial class ItemsPage : ContentPage
    {
        

        public static SQLiteAsyncConnection _connection;
        public static SQLiteAsyncConnection _connection2;
        public static ObservableCollection<Item> Drawings;
        public static ObservableCollection<List<SKPath>> Paths;

        public ItemsPage()
        {
            InitializeComponent();

            //_connection2 = DependencyService.Get<ISQLiteDb>().GetConnection();
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

        }

        protected override async void OnAppearing()
        {
            await _connection.CreateTableAsync<Item>();

            var recipes = await _connection.Table<Item>().ToListAsync();
            Drawings = new ObservableCollection<Item>(recipes);

            /*await _connection2.CreateTableAsync<List<SKPath>>();

            var recipes2 = await _connection2.Table<List<SKPath>>().ToListAsync();
            Paths = new ObservableCollection<List<SKPath>>(recipes2);*/
            
            ItemsListView.ItemsSource = Drawings;

            base.OnAppearing();
        }
        private Item _selectedItem;
        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }
        void OnItemSelected(Item item)
        {

        }
        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        private void Load(object sender, EventArgs e)
        {
            //AboutPage.completedPaths = Paths[0];
        }

        private async void Delete(object sender, EventArgs e)
        {
            //string delete = await DisplayActionSheet("Are you sure you want to delete", null, null, "Cancel", "OK");
                
            string delete = "OK";

            if (delete == "OK")
            {
                var recipe = Drawings[0];
                await _connection.DeleteAsync(recipe);
                Drawings.Remove(recipe);

                /*var recipe2 = Paths[0];
                await _connection2.DeleteAsync(recipe2);
                Paths.Remove(recipe2);*/
            }
        }
    }
}