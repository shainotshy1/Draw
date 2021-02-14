using SkiaSharp;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace iDraw.Models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Text { get; set; }
        public string Date { get; set; }
    }
}