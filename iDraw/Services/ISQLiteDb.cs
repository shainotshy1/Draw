﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace iDraw.Services
{
    public interface ISQLiteDb
    {
        SQLiteAsyncConnection GetConnection();
    }
}
