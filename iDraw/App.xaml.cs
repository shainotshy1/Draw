﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using iDraw.Services;
using iDraw.Views;

namespace iDraw
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
