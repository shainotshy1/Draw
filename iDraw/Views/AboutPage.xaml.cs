using iDraw.Models;
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
        List<SKPath> pathCache = new List<SKPath>();
        List<SKPaint> colorCache = new List<SKPaint>();

        List<int> actionCount = new List<int>();

        int cacheIndex = -1;

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

            //ItemsPage._connection2 = DependencyService.Get<ISQLiteDb>().GetConnection();
            ItemsPage._connection = DependencyService.Get<ISQLiteDb>().GetConnection();
        }
        protected override async void OnAppearing()
        {
            await ItemsPage._connection.CreateTableAsync<Item>();

            var recipes = await ItemsPage._connection.Table<Item>().ToListAsync();
            ItemsPage.Drawings = new ObservableCollection<Item>(recipes);

            /*await ItemsPage._connection2.CreateTableAsync<List<SKPath>>();

            var recipes2 = await ItemsPage._connection2.Table<List<SKPath>>().ToListAsync();
            ItemsPage.Paths = new ObservableCollection<List<SKPath>>(recipes2);*/

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

            Point returnPoint = new Point();

            if (x > canvasView.Width / 2.0)
            {
                x -= 2 * (x - canvasView.Width/2.0);
            }
            else
            {
                x += 2 * (canvasView.Width/2.0 - x);
            }

            returnPoint = new Point(x, y);

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
                            Point newPoint = MakeOppositePoint(args);

                            SKPath path2 = new SKPath();
                            path2.MoveTo(ConvertToPixel(newPoint));
                            inProgressPaths.Add(args.Id + 1, path2);
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
                            Point newPoint = MakeOppositePoint(args);

                            SKPath path2 = inProgressPaths[args.Id + 1];
                            path2.LineTo(ConvertToPixel(newPoint));
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
                            completedPaths.Add(inProgressPaths[args.Id + 1]);
                            inProgressPaths.Remove(args.Id + 1);
                            pathColors.Add(newPath);
                        }
                        
                        pathCache.Clear();
                        colorCache.Clear();

                        if (divider.IsVisible)
                        {
                            actionCount.Add(2);
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

            /*using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                saveBitmap.Encode(wstream, SKEncodedImageFormat.Png, 100);

                byte[] data = memStream.ToArray();

                if (data != null && data.Length != 0)
                {
                    string fileName = "test";
                    await DependencyService.Get<IPhotoLibrary>().
                    SavePhotoAsync(data, "SaveFileFormats", fileName + ".Jpeg");
                }
            }*/

            
            using (SKImage image = SKImage.FromBitmap(saveBitmap))
            {
                SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
                DateTime dt = DateTime.Now;
                string filename = String.Format("FingerPaint-{0:D4}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}{6:D3}.png",
                                                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);

                byte[] _data = data.ToArray();

                IPhotoLibrary photoLibrary = DependencyService.Get<IPhotoLibrary>();

                //error with Save Photo Async object references

                bool result = await photoLibrary.SavePhotoAsync(_data, "FingerPaint", filename);

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

                var recipe = new Item { Text = name, Date = Convert.ToString(DateTime.Now) };

                await ItemsPage._connection.InsertAsync(recipe);
                ItemsPage.Drawings.Add(recipe);

                /*var recipe2 = completedPaths;

                await ItemsPage._connection2.InsertAsync(recipe2);
                ItemsPage.Paths.Add(completedPaths);*/
            }
        }
    }
}