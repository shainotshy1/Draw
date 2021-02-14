using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using iDraw.Models;
using iDraw.Views;
using SQLite;
using iDraw.Services;

namespace iDraw.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {

        public ItemsViewModel()
        {
            Title = "Drawings";

            DeleteCommand = new Command(() => Delete());
        }

        public void Delete()
        {
            Title = "Done";
        }

        public Command LoadCommand { get; }
        public Command DeleteCommand { get; }
    }
}