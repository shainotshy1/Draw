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
        public static SQLiteAsyncConnection _connection3;
        public static SQLiteAsyncConnection _connection4;
        public static ObservableCollection<Item> Drawings;
        public static ObservableCollection<PathItem> Paths;
        public static ObservableCollection<PaintItem> Colors;
        public static ObservableCollection<Index> PathIndexes;

        public ItemsPage()
        {
            InitializeComponent();

            _connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            _connection2 = DependencyService.Get<ISQLiteDb>().GetConnection();
            _connection3 = DependencyService.Get<ISQLiteDb>().GetConnection();

        }

        protected override async void OnAppearing()
        {
            await _connection.CreateTableAsync<Item>();

            var recipes = await _connection.Table<Item>().ToListAsync();
            Drawings = new ObservableCollection<Item>(recipes);

            await _connection2.CreateTableAsync<PathItem>();

            var recipes2 = await _connection2.Table<PathItem>().ToListAsync();
            Paths = new ObservableCollection<PathItem>(recipes2);

            await _connection3.CreateTableAsync<PaintItem>();

            var recipes3 = await _connection3.Table<PaintItem>().ToListAsync();
            Colors = new ObservableCollection<PaintItem>(recipes3);

            await _connection4.CreateTableAsync<Index>();

            var recipes4 = await _connection4.Table<Index>().ToListAsync();
            PathIndexes = new ObservableCollection<Index>(recipes4);

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

            int indexOfDrawing = Drawings[index].Index;

            await DisplayAlert("Loading", "Loaded " + Drawings[index].Text + " on drawing pad", "OK");

            AboutPage.completedPaths.Clear();
            AboutPage.pathColors.Clear();

            bool atIndex = false;

            foreach(PathItem path in Paths)
            {
                if(path.PathIndex == indexOfDrawing)
                {
                    atIndex = true;

                    int _index = Paths.IndexOf(path);

                    SKPath newPath = SKPath.ParseSvgPathData(path.Path);
                    AboutPage.completedPaths.Add(newPath);

                    PaintItem item = Colors[_index];

                    SKPaint paint = new SKPaint
                    {
                        Style = Constants.styles[item.Style],
                        Color = Constants.colors[item.Color],
                        StrokeWidth = item.StrokeWidth,
                        StrokeCap = Constants.strokecaps[item.StrokeCap],
                        StrokeJoin = Constants.strokejoins[item.StrokeJoin]
                    };
                    AboutPage.pathColors.Add(paint);
                }
                else if(atIndex)
                {
                    break;
                }
            }

            AboutPage.cacheIndex = -1;
            AboutPage.actionCount.Clear();
            AboutPage.colorCache.Clear();
            AboutPage.pathCache.Clear();
        }

        private async void Delete(object sender, EventArgs e)
        {
            Button button = sender as Button;

            int index = Drawings.IndexOf(button.BindingContext);

            int indexOfDrawing = Drawings[index].Index;

            string delete = await DisplayActionSheet("Are you sure you want to delete "+Drawings[index].Text+"?", null, null, "Cancel", "OK");

            if (delete == "OK")
            {
                int count = 0;
                int start = -1;
                bool atIndex = false;

                foreach (PathItem path in Paths)
                {
                    if (!atIndex)
                    {
                        start++;
                    }
                    if (path.PathIndex == indexOfDrawing)
                    {
                        count++;
                        atIndex = true;
                    }
                    else if (path.PathIndex != indexOfDrawing && atIndex)
                    {
                        break;
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    var recipe2 = Paths[start];

                    await _connection2.DeleteAsync(recipe2);
                    Paths.Remove(recipe2);

                    var recipe3 = Colors[start];

                    await _connection3.DeleteAsync(recipe3);
                    Colors.Remove(recipe3);
                }

                var recipe = Drawings[index];
                await _connection.DeleteAsync(recipe);
                Drawings.Remove(recipe);
            }
        }
    }
}