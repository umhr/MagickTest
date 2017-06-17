using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MagickTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            label.Content = "";
            setIchimatsu();

        }

        private void setIchimatsu()
        {
            // 市松模様をXAMLだけで描く方法 – Rain or Shine
            // https://www.rainorshine.asia/2013/06/11/post2437.html
            GeometryDrawing geometryDrawingBg = new GeometryDrawing();
            geometryDrawingBg.Brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 204, 204, 204));
            geometryDrawingBg.Geometry = new RectangleGeometry(new Rect(0, 0, 16, 16));

            GeometryGroup geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 8, 8)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(8, 8, 8, 8)));

            GeometryDrawing geometryDrawing = new GeometryDrawing();
            geometryDrawing.Brush = System.Windows.Media.Brushes.White;
            geometryDrawing.Geometry = geometryGroup;

            DrawingGroup drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(geometryDrawingBg);
            drawingGroup.Children.Add(geometryDrawing);

            DrawingBrush drawingBrush = new DrawingBrush();
            //drawingBrush.Stretch = Stretch.None;
            drawingBrush.TileMode = TileMode.Tile;
            drawingBrush.ViewportUnits = BrushMappingMode.Absolute;
            drawingBrush.Viewport = new Rect(0, 0, 16, 16);
            drawingBrush.Drawing = drawingGroup;

            bgRectangle.Fill = drawingBrush;
        }


        // System.Drawing.BitmapをWPF用に変換
        // http://axion.sakura.ne.jp/blog/index.php?UID=1333166737
        private BitmapFrame bitmapFrameByBitmap(ref Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

        // 「ファイルを開く」ダイアログボックスを表示する: .NET Tips: C#, VB.NET
        // http://dobon.net/vb/dotnet/form/openfiledialog.html
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();


            ofd.Filter = "すべてのファイル(*.*)|*.*|Image Files(*.jpg;*.png;*.bmp;*.tif;*.tiff;*.gif;*.ico;*.wmp)|*.jpg;*.png;*.bmp;*.tif;*.tiff;*.gif;*.ico;*.wmp|ImageMagick対応 File(*.psd;*.svg)|*.psd;*.svg|Ghostscript対応 File(*.pdf;*.eps)|*.pdf;*.eps";
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            //ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == true)
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                string fileName = ofd.FileName;
                string extension = System.IO.Path.GetExtension(fileName.ToLower());

                label.Content = fileName;
                if (Regex.IsMatch(extension, "\\.(?:jpg|jpeg|png|bmp|tiff|tif|gif|ico|wmp)$"))
                {
                    targetImage.Source = new BitmapImage(new Uri(fileName));
                    fitImage();
                }
                else
                {
                    OpenByImageMagick(fileName, extension);
                }

            }
        }

        private void OpenByImageMagick(string fileName, string extension)
        {
            // Magick.NETを使って、.NETがサポートしていない形式の画像を読み込む - DoboWiki
            // https://wiki.dobon.net/index.php?.NET%A5%D7%A5%ED%A5%B0%A5%E9%A5%DF%A5%F3%A5%B0%B8%A6%B5%E6%2F112
            // Formats @ ImageMagick
            // http://www.imagemagick.org/script/formats.php
            ImageMagick.MagickReadSettings settings = new ImageMagick.MagickReadSettings();
            if (extension == ".pdf")
            {
                //settings.Density = new ImageMagick.Density(300, 300);
            }
            try
            {
                ImageMagick.MagickImage img = new ImageMagick.MagickImage(fileName, settings);
                Bitmap bitmap = img.ToBitmap();
                targetImage.Source = bitmapFrameByBitmap(ref bitmap);
                fitImage();
                label.Content += " Open by ImageMagick";
                if (extension == ".pdf" || extension == ".eps")
                {
                    label.Content += " with Ghostscript";
                }

            }
            catch
            {
                label.Content += " は読めませんでした。";
            }

        }

        private void fitImage()
        {
            double scale = Math.Min(((Panel)this.Content).RenderSize.Width / targetImage.Source.Width, ((Panel)this.Content).RenderSize.Height / targetImage.Source.Height);
            scale = Math.Min(1, scale);
            targetImage.Width = (int)(targetImage.Source.Width * scale);
            targetImage.Height = (int)(targetImage.Source.Height * scale);
            // 書式を指定して数値を文字列に変換する: .NET Tips: C#, VB.NET
            // https://dobon.net/vb/dotnet/string/inttostring.html
            label.Content += " " + (int)targetImage.Source.Width + "x" + (int)targetImage.Source.Height + " (" + scale.ToString("p") + ")";

            bgRectangle.Width = targetImage.Width;
            bgRectangle.Height = targetImage.Height;
        }

    }
}
