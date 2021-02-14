using SkiaSharp;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace iDraw.Models
{
    public class PathItem
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        public int PathIndex { get; set; }
        public string Path { get; set; }
    }
}
