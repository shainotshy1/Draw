using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using iDraw.Models;
using iDraw.Services;

namespace iDraw.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>();

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }
        string button0 = string.Empty;
        public string Button0
        {
            get { return button0; }
            set { SetProperty(ref button0, value); }
        }
        string button1 = string.Empty;
        public string Button1
        {
            get { return button1; }
            set { SetProperty(ref button1, value); }
        }
        string button2 = string.Empty;
        public string Button2
        {
            get { return button2; }
            set { SetProperty(ref button2, value); }
        }
        string button3 = string.Empty;
        public string Button3
        {
            get { return button3; }
            set { SetProperty(ref button3, value); }
        }
        string button4 = string.Empty;
        public string Button4
        {
            get { return button4; }
            set { SetProperty(ref button4, value); }
        }
        string button5 = string.Empty;
        public string Button5
        {
            get { return button5; }
            set { SetProperty(ref button5, value); }
        }
        string button6 = string.Empty;
        public string Button6
        {
            get { return button6; }
            set { SetProperty(ref button6, value); }
        }
        string button7 = string.Empty;
        public string Button7
        {
            get { return button7; }
            set { SetProperty(ref button7, value); }
        }
        double sliderValue = 0;
        public double SliderValue
        {
            get { return sliderValue; }
            set { SetProperty(ref sliderValue, value); }
        }

        bool divider = false;
        public bool Divider
        {
            get { return divider; }
            set { SetProperty(ref divider,value); }
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
