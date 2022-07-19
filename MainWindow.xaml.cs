using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess
{
    public partial class MainWindow : Window
    {
        ChessBoard Game = new ChessBoard();
        public double CellSize = 80;
        public Image moved = new Image();
        public bool moving = false;
        public Thickness offset;
        public int fromX;
        public int fromY;
        public int toX;
        public int toY;
        public bool promotion = false;
        public int depth = 3;
        public bool reversed = false;

        public MainWindow()
        {
            InitializeComponent();
            MyCanvas.Width = CellSize * 8;
            MyCanvas.Height = CellSize * 8;
            MyWindow.Width = MyCanvas.Width + 16;
            MyWindow.Height = MyCanvas.Height + 39;
            Display();
            //Trace.WriteLine(Game.possibleBoards(4));
        }

        public void Display()
        {
            MyCanvas.Children.Clear();
            Image dynamicImage = new Image();
            dynamicImage.Name = "board";
            Panel.SetZIndex(dynamicImage, 0);
            dynamicImage.Width = CellSize * 8;
            dynamicImage.Height = CellSize * 8;
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/board.png");
            bitmap.EndInit();
            dynamicImage.Source = bitmap;
            MyCanvas.Children.Add(dynamicImage);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (!Game.isEmpty(y, x))
                    {
                        string id = "";
                        if (Game.isWhite(y, x)) id += "w";
                        else if (Game.isBlack(y, x)) id += "b";
                        else id += " ";

                        if (Game.isPawn(y, x)) id += "p";
                        else if (Game.isRook(y, x)) id += "r";
                        else if (Game.isKnight(y, x)) id += "n";
                        else if (Game.isBishop(y, x)) id += "b";
                        else if (Game.isQueen(y, x)) id += "q";
                        else if (Game.isKing(y, x)) id += "k";
                        else id += " ";

                        dynamicImage = new Image();
                        Panel.SetZIndex(dynamicImage, 1);
                        dynamicImage.Width = CellSize;
                        dynamicImage.Height = CellSize;
                        if (reversed) dynamicImage.Margin = new Thickness((7 - x) * CellSize, (7 - y) * CellSize, 0, 0);
                        else dynamicImage.Margin = new Thickness(x * CellSize, y * CellSize, 0, 0);
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/" + id + ".png");
                        bitmap.EndInit();
                        dynamicImage.Source = bitmap;
                        MyCanvas.Children.Add(dynamicImage);
                    }
                }
            }
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (promotion)
            {
                if (e.OriginalSource is Image)
                {
                    Image img = (Image)e.OriginalSource;
                    if (img.Name == "Q")
                    {
                        Game.move(fromY, fromX, toY, toX, 16);
                        reversed = !reversed;
                    }
                    else if (img.Name == "N")
                    {
                        Game.move(fromY, fromX, toY, toX, 4);
                        reversed = !reversed;
                    }
                    else if (img.Name == "R")
                    {
                        Game.move(fromY, fromX, toY, toX, 2);
                        reversed = !reversed;
                    }
                    else if (img.Name == "B")
                    {
                        Game.move(fromY, fromX, toY, toX, 8);
                        reversed = !reversed;
                    }
                }
                Display();
                promotion = false;
                return;
            }

            if (moving || e.OriginalSource is not Image || Game.isfinished) return;
            if (((Image)e.OriginalSource).Name == "board") return;

            fromX = (int)(e.GetPosition(MyCanvas).X / CellSize);
            fromY = (int)(e.GetPosition(MyCanvas).Y / CellSize);
            if (reversed)
            {
                fromY = 7 - fromY;
                fromX = 7 - fromX;
            }
            if (Game.isWhite(fromY, fromX) != Game.isWhiteTurn) return;
            moved = (Image)e.OriginalSource;
            Panel.SetZIndex((UIElement)e.OriginalSource, 2);
            offset = new Thickness(moved.Margin.Left - e.GetPosition(MyCanvas).X, moved.Margin.Top - e.GetPosition(MyCanvas).Y, 0, 0);
            moving = true;

            foreach ((int velY, int velX, byte promotion) in Game.legalMoves(fromY, fromX))
            {
                Image dynamicImage = new Image();
                dynamicImage.Width = CellSize;
                dynamicImage.Height = CellSize;
                if (reversed) dynamicImage.Margin = new Thickness((7 - fromX - velX) * CellSize, (7 - fromY - velY) * CellSize, 0, 0);
                else dynamicImage.Margin = new Thickness((fromX + velX) * CellSize, (fromY + velY) * CellSize, 0, 0);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                if (Game.isEmpty(fromY + velY, fromX + velX)) bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/move.png");
                else bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/eating.png");
                bitmap.EndInit();
                dynamicImage.Source = bitmap;
                MyCanvas.Children.Add(dynamicImage);
            }
        }

        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (moving && e.LeftButton == MouseButtonState.Released)
            {
                moving = false;
                Display();
            }
            else if (moving) moved.Margin = new Thickness(e.GetPosition(MyCanvas).X + offset.Left, e.GetPosition(MyCanvas).Y + offset.Top, 0, 0);
        }

        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!moving) return;
            moving = false;
            Panel.SetZIndex((UIElement)e.OriginalSource, 1);
            toX = (int)((moved.Margin.Left + CellSize / 2) / CellSize);
            toY = (int)((moved.Margin.Top + CellSize / 2) / CellSize);
            if (reversed)
            {
                toY = 7 - toY;
                toX = 7 - toX;
            }
            if (0 > toX) toX = 0;
            else if (toX > 7) toX = 7;
            if (0 > toY) toY = 0;
            else if (toY > 7) toY = 7;


            if (Game.isPawn(fromY, fromX) && (toY == 0 || toY == 7) && (Game.LegalMoves.Contains((toY - fromY, toX - fromX, 16)) || Game.LegalMoves.Contains((toY - fromY, toX - fromX, 4)) || Game.LegalMoves.Contains((toY - fromY, toX - fromX, 8)) || Game.LegalMoves.Contains((toY - fromY, toX - fromX, 2))))
            {
                moved.Margin = new Thickness(toX * CellSize, toY * CellSize, 0, 0);
                promotion = true;
                Rectangle rect = new Rectangle();
                rect.Width = CellSize;
                rect.Height = 4 * CellSize;
                rect.Fill = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                double left;
                double top;
                if (toX >= 3) left = e.GetPosition(MyCanvas).X - rect.Width;
                else left = e.GetPosition(MyCanvas).X;
                if (toY >= 3) top = e.GetPosition(MyCanvas).Y - rect.Height;
                else top = e.GetPosition(MyCanvas).Y;
                rect.Margin = new Thickness(left, top, 0, 0);
                MyCanvas.Children.Add(rect);
                Panel.SetZIndex(rect, 2);

                if (Game.isWhite(fromY, fromX))
                {
                    Image dynamicImage = new Image();
                    BitmapImage bitmap = new BitmapImage();
                    dynamicImage.Name = "Q";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/wq.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);

                    dynamicImage = new Image();
                    bitmap = new BitmapImage();
                    dynamicImage.Name = "N";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top + CellSize, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/wn.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);

                    dynamicImage = new Image();
                    bitmap = new BitmapImage();
                    dynamicImage.Name = "R";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top + 2 * CellSize, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/wr.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);

                    dynamicImage = new Image();
                    bitmap = new BitmapImage();
                    dynamicImage.Name = "B";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top + 3 * CellSize, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/wb.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);
                }
                else if (Game.isBlack(fromY, fromX))
                {
                    Image dynamicImage = new Image();
                    BitmapImage bitmap = new BitmapImage();
                    dynamicImage.Name = "Q";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/bq.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);

                    dynamicImage = new Image();
                    bitmap = new BitmapImage();
                    dynamicImage.Name = "N";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top + CellSize, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/bn.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);

                    dynamicImage = new Image();
                    bitmap = new BitmapImage();
                    dynamicImage.Name = "R";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top + 2 * CellSize, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/br.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);

                    dynamicImage = new Image();
                    bitmap = new BitmapImage();
                    dynamicImage.Name = "B";
                    Panel.SetZIndex(dynamicImage, 3);
                    dynamicImage.Width = CellSize;
                    dynamicImage.Height = CellSize;
                    dynamicImage.Margin = new Thickness(left, top + 3 * CellSize, 0, 0);
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Skins/bb.png");
                    bitmap.EndInit();
                    dynamicImage.Source = bitmap;
                    MyCanvas.Children.Add(dynamicImage);
                }
            }
            else
            {
                if (Game.move(fromY, fromX, toY, toX, 0)) reversed = !reversed;
                Display();
            }
        }

        private void MyWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newsize = Math.Min(MyWindow.Width - 16, MyWindow.Height - 39) / 8;
            CellSize = newsize;
            MyCanvas.Width = newsize;
            MyCanvas.Height = newsize;
            if (MyWindow.Width >= MyWindow.Height - 23) MyCanvas.Margin = new Thickness((MyWindow.Width - MyWindow.Height + 23) / 2, 0, 0, 0);
            else MyCanvas.Margin = new Thickness(0, (MyWindow.Height - MyWindow.Width - 23) / 2, 0, 0);
            Trace.WriteLine(String.Format("resised window to {0}x{1} pixels", MyWindow.Width-16, MyWindow.Height-39));
            Display();
        }
    }
}