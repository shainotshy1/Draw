using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace iDraw.Models
{
    public class Index
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        public int Count { get; set; }
    }
}
