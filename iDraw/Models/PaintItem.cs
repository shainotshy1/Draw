using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace iDraw.Models
{
    public class PaintItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Style { get; set; }
        public int Color { get; set; }
        public float StrokeWidth { get; set; }
        public int StrokeCap { get; set; }
        public int StrokeJoin { get; set; }
    }
}
