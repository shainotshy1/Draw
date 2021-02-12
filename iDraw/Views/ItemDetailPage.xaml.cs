using System.ComponentModel;
using Xamarin.Forms;
using iDraw.ViewModels;

namespace iDraw.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}