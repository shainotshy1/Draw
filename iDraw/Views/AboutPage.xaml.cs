﻿using iDraw.Models;
using iDraw.Services;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace iDraw.Views
{
    public partial class AboutPage : ContentPage
    {
        Dictionary<long, SKPath> inProgressPaths = new Dictionary<long, SKPath>();
        public static List<SKPath> completedPaths = new List<SKPath>();
        public static List<SKPath> pathCache = new List<SKPath>();
        public static List<SKPaint> colorCache = new List<SKPaint>();

        public static List<int> actionCount = new List<int>();

        public static int cacheIndex = -1;

        public static List<SKPaint> pathColors = new List<SKPaint>();

        SKPaint paint = new SKPaint
        {
            Style = Constants.style,
            Color = Constants.colorConstant,
            StrokeWidth = (float)Constants.size,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        SKBitmap saveBitmap;

        public AboutPage()
        {
            InitializeComponent();
            
            ItemsPage._connection = DependencyService.Get<ISQLiteDb>().GetConnection();
            ItemsPage._connection2 = DependencyService.Get<ISQLiteDb>().GetConnection();
            ItemsPage._connection3 = DependencyService.Get<ISQLiteDb>().GetConnection();
            ItemsPage._connection4 = DependencyService.Get<ISQLiteDb>().GetConnection();
        }
        protected override async void OnAppearing()
        {        
            await ItemsPage._connection.CreateTableAsync<Item>();

            var recipes = await ItemsPage._connection.Table<Item>().ToListAsync();
            ItemsPage.Drawings = new ObservableCollection<Item>(recipes);

            await ItemsPage._connection2.CreateTableAsync<PathItem>();

            var recipes2 = await ItemsPage._connection2.Table<PathItem>().ToListAsync();
            ItemsPage.Paths = new ObservableCollection<PathItem>(recipes2);

            await ItemsPage._connection3.CreateTableAsync<PaintItem>();

            var recipes3 = await ItemsPage._connection3.Table<PaintItem>().ToListAsync();
            ItemsPage.Colors = new ObservableCollection<PaintItem>(recipes3);

            await ItemsPage._connection4.CreateTableAsync<Index>();

            var recipes4 = await ItemsPage._connection4.Table<Index>().ToListAsync();
            ItemsPage.PathIndexes = new ObservableCollection<Index>(recipes4);

            UpdateBitmap();

            base.OnAppearing();
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            ResetBitmap(args);
        }

        private void ResetBitmap(SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            // Create bitmap the size of the display surface
            if (saveBitmap == null)
            {
                saveBitmap = new SKBitmap(info.Width, info.Height);
            }
            // Or create new bitmap for a new size of display surface
            else if (saveBitmap.Width < info.Width || saveBitmap.Height < info.Height)
            {
                SKBitmap newBitmap = new SKBitmap(Math.Max(saveBitmap.Width, info.Width),
                                                  Math.Max(saveBitmap.Height, info.Height));

                using (SKCanvas newCanvas = new SKCanvas(newBitmap))
                {
                    newCanvas.Clear();
                    newCanvas.DrawBitmap(saveBitmap, 0, 0);
                }

                saveBitmap = newBitmap;
            }

            // Render the bitmap
            canvas.Clear();

            canvas.DrawBitmap(saveBitmap, 0, 0);
        }

        Point MakeOppositePoint(TouchActionEventArgs args)
        {
            double x = args.Location.X;
            double y = args.Location.Y;

            if (x > canvasView.Width / 2.0)
            {
                x -= 2 * (x - canvasView.Width/2.0);
            }
            else
            {
                x += 2 * (canvasView.Width/2.0 - x);
            }

            Point returnPoint = new Point(x, y);

            return returnPoint;
        }

        Point MakeQuadPoint(TouchActionEventArgs args,int quadrant)
        {
            double x = args.Location.X;
            double y = args.Location.Y;

            switch (quadrant)
            {
                case 1:
                    if (x > canvasView.Width / 2.0)
                    {
                        x -= 2 * (x - canvasView.Width / 2.0);
                    }
                    else
                    {
                        x += 2 * (canvasView.Width / 2.0 - x);
                    }
                    break;
                case 2:
                    if (x > canvasView.Width / 2.0)
                    {
                        x -= 2 * (x - canvasView.Width / 2.0);
                    }
                    else
                    {
                        x += 2 * (canvasView.Width / 2.0 - x);
                    }
                    if (y > canvasView.Height / 2.0)
                    {
                        y -= 2 * (y - canvasView.Height / 2.0);
                    }
                    else
                    {
                        y += 2 * (canvasView.Height / 2.0 - y);
                    }
                    break;
                case 3:
                    if (y > canvasView.Height / 2.0)
                    {
                        y -= 2 * (y - canvasView.Height / 2.0);
                    }
                    else
                    {
                        y += 2 * (canvasView.Height / 2.0 - y);
                    }
                    break;
            }

            Point returnPoint = new Point(x, y);
            
            return returnPoint;
    }

    void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (!inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(ConvertToPixel(args.Location));
                        inProgressPaths.Add(args.Id, path);

                        if (divider.IsVisible)
                        {
                            if (divider2.IsVisible)
                            {
                                for(int i = 1; i < 4; i++)
                                {
                                    Point newPoint = MakeQuadPoint(args,i);

                                    SKPath path2 = new SKPath();
                                    path2.MoveTo(ConvertToPixel(newPoint));
                                    inProgressPaths.Add(args.Id + i, path2);
                                }
                            }
                            else
                            {
                                Point newPoint = MakeOppositePoint(args);

                                SKPath path2 = new SKPath();
                                path2.MoveTo(ConvertToPixel(newPoint));
                                inProgressPaths.Add(args.Id + 1, path2);
                            } 
                        }    
                        UpdateBitmap();
                    }
                    break;

                case TouchActionType.Moved:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = inProgressPaths[args.Id];
                        path.LineTo(ConvertToPixel(args.Location));

                        if (divider.IsVisible)
                        {
                            if (divider2.IsVisible)
                            {
                                for (int i = 1; i < 4; i++)
                                {
                                    Point newPoint = MakeQuadPoint(args, i);

                                    SKPath path2 = inProgressPaths[args.Id + i];
                                    path2.LineTo(ConvertToPixel(newPoint));
                                }
                            }
                            else
                            {
                                Point newPoint = MakeOppositePoint(args);

                                SKPath path2 = inProgressPaths[args.Id + 1];
                                path2.LineTo(ConvertToPixel(newPoint));
                            }
                        }
                        
                        UpdateBitmap();
                    }
                    break;

                case TouchActionType.Released:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPaint newPath = new SKPaint
                        {
                            Style = Constants.style,
                            Color = Constants.colorConstant,
                            StrokeWidth = (float)Constants.size,
                            StrokeCap = SKStrokeCap.Round,
                            StrokeJoin = SKStrokeJoin.Round
                        };

                        completedPaths.Add(inProgressPaths[args.Id]);
                        inProgressPaths.Remove(args.Id);
                        pathColors.Add(newPath);

                        if (divider.IsVisible)
                        {
                            if (divider2.IsVisible)
                            {
                                for (int i = 1; i < 4; i++)
                                {
                                    completedPaths.Add(inProgressPaths[args.Id + i]);
                                    inProgressPaths.Remove(args.Id + i);
                                    pathColors.Add(newPath);
                                }
                            }
                            else
                            {
                                completedPaths.Add(inProgressPaths[args.Id + 1]);
                                inProgressPaths.Remove(args.Id + 1);
                                pathColors.Add(newPath);
                            }
                        }
                        
                        pathCache.Clear();
                        colorCache.Clear();

                        if (divider.IsVisible)
                        {
                            if (divider2.IsVisible)
                            {
                                actionCount.Add(4);
                            }
                            else
                            {
                                actionCount.Add(2);
                            }
                        }
                        else
                        {
                            actionCount.Add(1);
                        }
                        cacheIndex = actionCount.Count - 1;

                        UpdateBitmap();
                    }
                    break;

                case TouchActionType.Cancelled:
                    if (inProgressPaths.ContainsKey(args.Id))
                    {
                        inProgressPaths.Remove(args.Id);
                        if (divider.IsVisible)
                        {
                            if (divider2.IsVisible)
                            {
                                for(int i = 1; i < 4; i++)
                                {
                                    inProgressPaths.Remove(args.Id + i);
                                }
                            }
                            inProgressPaths.Remove(args.Id + 1);
                        }
                        UpdateBitmap();
                    }
                    break;
            }
        }

        SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X/ canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y/ canvasView.Height));
        }

        void UpdateBitmap()
        {
            using (SKCanvas saveBitmapCanvas = new SKCanvas(saveBitmap))
            {
                saveBitmapCanvas.Clear();

                if (completedPaths.Count > 0)
                {
                    for (int i = 0; i < completedPaths.Count; i++)
                    {
                        saveBitmapCanvas.DrawPath(completedPaths[i], pathColors[i]);
                    }
                }

                foreach (SKPath path in inProgressPaths.Values)
                {
                    saveBitmapCanvas.DrawPath(path, paint);
                }
            }

            canvasView.InvalidateSurface();
        }

        public void buttonChange(object sender, EventArgs args)
        {
            if(sender == button1)
            {
                Constants.colorConstant = new SKColor(79, 217, 56, 0xFF);
                Constants.size = sizeSlider.Value;
                Constants.style = SKPaintStyle.Stroke;
            }
            else if(sender == button2)
            {
                Constants.colorConstant = new SKColor(56, 217, 208, 0xFF);
                Constants.size = sizeSlider.Value;
                Constants.style = SKPaintStyle.Stroke;
            }
            else if (sender == button3)
            {
                Constants.colorConstant = new SKColor(56, 113, 217, 0xFF);
                Constants.size = sizeSlider.Value;
                Constants.style = SKPaintStyle.Stroke;
            }
            else if (sender == button4)
            {
                Constants.colorConstant = new SKColor(211, 217, 56, 0xFF);
                Constants.size = sizeSlider.Value;
                Constants.style = SKPaintStyle.Stroke;
            }
            else if (sender == button5)
            {
                Constants.colorConstant = new SKColor(217, 56, 56, 0xFF);
                Constants.size = sizeSlider.Value;
                Constants.style = SKPaintStyle.Stroke;
            }
            else if (sender == button6)
            {
                Constants.colorConstant = new SKColor(217, 101, 56, 0xFF);
                Constants.size = sizeSlider.Value;
                Constants.style = SKPaintStyle.Stroke;
            }
            else if (sender == button7)
            {
                Constants.colorConstant = new SKColor(255, 255, 255, 0xFF);
                Constants.size = sizeSlider.Value;
                Constants.style = SKPaintStyle.Stroke;
            }
            paint = new SKPaint
            {
                Style = Constants.style,
                Color = Constants.colorConstant,
                StrokeWidth = (float)Constants.size,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };
        }

        private void sizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Constants.size = sizeSlider.Value;
            paint = new SKPaint
            {
                Style = Constants.style,
                Color = Constants.colorConstant,
                StrokeWidth = (float)Constants.size,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };
        }

        private void Back(object sender, EventArgs e)
        {
            if (cacheIndex >= 0)
            {
                if (completedPaths.Count == 0)
                {
                    if (actionCount[cacheIndex] < 0)
                    {
                        for (int i = actionCount[cacheIndex]; i < 0; i++)
                        {
                            if (colorCache.Count > 0)
                            {
                                pathColors.Add(colorCache[colorCache.Count - 1]);
                                completedPaths.Add(pathCache[pathCache.Count - 1]);

                                colorCache.RemoveAt(colorCache.Count - 1);
                                pathCache.RemoveAt(pathCache.Count - 1);
                            }
                        }

                        cacheIndex -= 1;
                    }  
                }
                else
                {
                    if (actionCount[cacheIndex] == 1)
                    {
                        pathCache.Add(completedPaths[completedPaths.Count - 1]);
                        colorCache.Add(pathColors[pathColors.Count - 1]);
                        completedPaths.Remove(completedPaths[completedPaths.Count - 1]);
                        pathColors.Remove(pathColors[pathColors.Count - 1]);

                        cacheIndex -= 1;
                    }
                    else if (actionCount[cacheIndex] == 2)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (completedPaths.Count > 0)
                            {
                                pathCache.Add(completedPaths[completedPaths.Count - 1]);
                                colorCache.Add(pathColors[pathColors.Count - 1]);
                                completedPaths.Remove(completedPaths[completedPaths.Count - 1]);
                                pathColors.Remove(pathColors[pathColors.Count - 1]);
                            }
                        }

                        cacheIndex -= 1;
                    }
                    else if (actionCount[cacheIndex] == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (completedPaths.Count > 0)
                            {
                                pathCache.Add(completedPaths[completedPaths.Count - 1]);
                                colorCache.Add(pathColors[pathColors.Count - 1]);
                                completedPaths.Remove(completedPaths[completedPaths.Count - 1]);
                                pathColors.Remove(pathColors[pathColors.Count - 1]);
                            }
                        }

                        cacheIndex -= 1;
                    }
                    else if (actionCount[cacheIndex] < 0)
                    {
                        for (int i = actionCount[cacheIndex]; i < 0; i++)
                        {
                            if (colorCache.Count > 0)
                            {
                                pathColors.Add(colorCache[colorCache.Count - 1]);
                                completedPaths.Add(pathCache[pathCache.Count - 1]);

                                colorCache.Remove(colorCache[colorCache.Count - 1]);
                                pathCache.Remove(pathCache[pathCache.Count - 1]);
                            }  
                        }

                        cacheIndex -= 1;
                    }
                }
                
            }
            UpdateBitmap();        
        }

        private void Forward(object sender, EventArgs e)
        {
            if (actionCount.Count > 0) {
                if (cacheIndex < actionCount.Count - 1)
                {
                    cacheIndex++;
                    if (actionCount[cacheIndex] == 1)
                    {
                        if (pathCache.Count > 0)
                        {
                            completedPaths.Add(pathCache[pathCache.Count - 1]);
                            pathColors.Add(colorCache[colorCache.Count - 1]);

                            pathCache.RemoveAt(pathCache.Count - 1);
                            colorCache.RemoveAt(colorCache.Count - 1);
                        }
                    }
                    else if (actionCount[cacheIndex] == 2)
                    {
                        for(int i = 0; i < 2; i++)
                        {
                            if (pathCache.Count > 0)
                            {
                                completedPaths.Add(pathCache[pathCache.Count - 1]);
                                pathColors.Add(colorCache[colorCache.Count - 1]);

                                pathCache.RemoveAt(pathCache.Count - 1);
                                colorCache.RemoveAt(colorCache.Count - 1);
                            }  
                        }
                    }
                    else if (actionCount[cacheIndex] == 4)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (pathCache.Count > 0)
                            {
                                completedPaths.Add(pathCache[pathCache.Count - 1]);
                                pathColors.Add(colorCache[colorCache.Count - 1]);

                                pathCache.RemoveAt(pathCache.Count - 1);
                                colorCache.RemoveAt(colorCache.Count - 1);
                            }
                        }
                    }
                    else if (actionCount[cacheIndex] < 0)
                    {
                        Clear();
                    }
                }
            }

            UpdateBitmap();
        }

        private void ClearMethod(object sender, EventArgs e)
        {
            Clear();

            cacheIndex = actionCount.Count - 1;
        }

        private void Clear()
        {
            colorCache.Clear();
            pathCache.Clear();

            if (completedPaths.Count > 0)
            {
                int count = 0;
                foreach (SKPath path in completedPaths)
                {
                    count--;
                }

                actionCount.Add(count);

                for(int i = pathColors.Count - 1; i >= 0; i--)
                {
                    colorCache.Add(pathColors[i]);
                }

                for (int i = completedPaths.Count - 1; i >= 0; i--)
                {
                    pathCache.Add(completedPaths[i]);
                }
                pathColors.Clear();
                completedPaths.Clear();
                UpdateBitmap();
                canvasView.InvalidateSurface();
            }
        }

        private async void DownloadMethod(object sender, EventArgs e)
        {

            using (SKImage image = SKImage.FromBitmap(saveBitmap))
            {
                SKData data = image.Encode();
                DateTime dt = DateTime.Now;
                string filename = String.Format("FingerPaint-{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}{6:D3}.png",
                                                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);

                IPhotoLibrary photoLibrary = DependencyService.Get<IPhotoLibrary>();
                bool result = await photoLibrary.SavePhotoAsync(data.ToArray(), "FingerPaint", filename);

                if (!result)
                {
                    await DisplayAlert("FingerPaint", "Artwork could not be saved. Sorry!", "OK");
                }
            }
        }
        private async void SaveMethod(object sender, EventArgs e)
        {
            string name = await DisplayPromptAsync("Save", "Input name of drawing", "OK", "Cancel", "Drawing...");

            if (name != "Drawing..." && name!= null && name != ""){

                var _recipe = new Index { Count = ItemsPage.PathIndexes.Count };

                await ItemsPage._connection4.InsertAsync(_recipe);
                ItemsPage.PathIndexes.Add(_recipe);

                var recipe = new Item { Text = name, Date = Convert.ToString(DateTime.Now),Index = ItemsPage.PathIndexes[ItemsPage.PathIndexes.Count-1].Count };

                await ItemsPage._connection.InsertAsync(recipe);
                ItemsPage.Drawings.Add(recipe);

                int index = recipe.Index;

                foreach(SKPath path in completedPaths)
                {
                    var recipe2 = new PathItem { Path=path.ToSvgPathData(),PathIndex = index};

                    await ItemsPage._connection2.InsertAsync(recipe2);
                    ItemsPage.Paths.Add(recipe2);

                    int _index = completedPaths.IndexOf(path);
                    SKPaint paint = pathColors[_index];

                    var recipe3 = new PaintItem { Style = Constants.styles.IndexOf(paint.Style), Color = Constants.colors.IndexOf(paint.Color),
                                                StrokeWidth = paint.StrokeWidth,StrokeCap = Constants.strokecaps.IndexOf(paint.StrokeCap),
                                                StrokeJoin = Constants.strokejoins.IndexOf(paint.StrokeJoin)};
                    await ItemsPage._connection3.InsertAsync(recipe3);
                    ItemsPage.Colors.Add(recipe3);
                } 
                
            }
        }
    }
}