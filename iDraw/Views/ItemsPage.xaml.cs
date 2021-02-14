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
using Xamarin.Forms.Internals;

namespace iDraw.Views
{
    public partial class ItemsPage : ContentPage
    {
        

        public static SQLiteAsyncConnection _connection;
        public static SQLiteAsyncConnection _connection2;
        public static ObservableCollection<Item> Drawings;
        public static ObservableCollection<PathItem> Paths;

        public ItemsPage()
        {
            InitializeComponent();

            _connection2 = DependencyService.Get<ISQLiteDb>().GetConnection();
            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();

        }

        protected override async void OnAppearing()
        {
            await _connection.CreateTableAsync<Item>();

            var recipes = await _connection.Table<Item>().ToListAsync();
            Drawings = new ObservableCollection<Item>(recipes);

            await _connection2.CreateTableAsync<PathItem>();

            var recipes2 = await _connection2.Table<PathItem>().ToListAsync();
            Paths = new ObservableCollection<PathItem>(recipes2);
            
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

        private async void Load(object sender, EventArgs e)
        {
            Button button = sender as Button;

            int index = Drawings.IndexOf(button.BindingContext);

            await DisplayAlert("Loading", "Loaded " + Drawings[index].Text + " on drawing pad", "OK");

            AboutPage.completedPaths.Clear();

            bool atIndex = false;

            foreach(PathItem path in Paths)
            {
                if(path.PathIndex == index)
                {
                    atIndex = true;
                    SKPath newPath = SKPath.ParseSvgPathData(path.Path);
                    AboutPage.completedPaths.Add(newPath);
                }
                else if(atIndex)
                {
                    break;
                }
            }
        }

        private async void Delete(object sender, EventArgs e)
        {
            Button button = sender as Button;

            int index = Drawings.IndexOf(button.BindingContext);

            string delete = await DisplayActionSheet("Are you sure you want to delete "+Drawings[index].Text+"?", null, null, "Cancel", "OK");

            if (delete == "OK")
            {
                var recipe = Drawings[index];
                await _connection.DeleteAsync(recipe);
                Drawings.Remove(recipe);

                var recipe2 = Paths[0];
                await _connection2.DeleteAsync(recipe2);
                Paths.Remove(recipe2);
            }
        }
    }
}