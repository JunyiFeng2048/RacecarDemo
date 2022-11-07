using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace RacecarDemo
{
    public class Pixel
    {
        public enum SurfaceType { Barrier, TarmacA, TarmacB, TarmacC, FinishLine }

        public SurfaceType Type;

        public Pixel(float R, float G, float B)
        {
            if (R == 255 && G == 201 && B == 14) { Type = SurfaceType.FinishLine; }
            else if (R == 60 && G == 60 && B == 60) { Type = SurfaceType.TarmacA; }
            else if (R == 100 && G == 100 && B == 100) { Type = SurfaceType.TarmacB; }
            else if (R == 255 && G == 0 && B == 0) { Type = SurfaceType.TarmacC; }
            else { Type = SurfaceType.Barrier; }
        }

    }
    public class Track
    {
        public const float Max8BitValue = 255f;
        public Bitmap TrackImage;
        public Bitmap TrackImageMarkers;
        public Bitmap TrackSensor;
        public Point Dimensions;
        public Pixel[,] TrackData;
        public int StartX = -1;
        public int StartY = -1;

        public Track(string path)
        {
            TrackImage = (Bitmap)Image.FromFile(path);
            TrackImageMarkers = (Bitmap)Image.FromFile(path.Replace(".bmp", "Fitness.bmp"));
            TrackSensor = (Bitmap)Image.FromFile(path.Replace(".bmp", "Sensor.bmp"));
            Dimensions.X = TrackImage.Width;
            Dimensions.Y = TrackImage.Height;
            TrackData = new Pixel[Dimensions.X, Dimensions.Y];
            List<int> StartYList = new List<int>();
            List<int> StartXList = new List<int>();
            for (int y = 0; y < Dimensions.Y; y++)
            {
                for (int x = 0; x < Dimensions.X; x++)
                {
                    Color c1 = TrackImageMarkers.GetPixel(x, y);
                    TrackData[x, y] = new Pixel(c1.R, c1.G, c1.B);
                    if (TrackData[x, y].Type == Pixel.SurfaceType.FinishLine)
                    {
                        if (!StartXList.Contains(x)) { StartXList.Add(x); }
                        if (!StartYList.Contains(y)) { StartYList.Add(y); }
                    }
                }
            }
            StartX = StartXList[StartXList.Count() / 2];
            StartY = StartYList[StartYList.Count() / 2];
        }

        Bitmap Locale = new Bitmap(300,300);
        public void GetTrackSection(Bitmap SensorImage, Point Position, float angle, int width, int height)
        {
            using (Graphics g = Graphics.FromImage(Locale))
            {
                Matrix Transform = new Matrix();
                Transform.Translate(-(Position.X - (Locale.Width/2)), -(Position.Y - (Locale.Height/2)));
                g.Transform = Transform;
                g.DrawImage(Program.Track.TrackSensor, 0, 0, Program.Track.Dimensions.X, Program.Track.Dimensions.Y);
            }
            using (Graphics g = Graphics.FromImage(SensorImage))
            {
                Matrix Transform = new Matrix();
                Transform.Translate(-((Locale.Width / 2) - width), -((Locale.Height / 2) - height));
                Transform.RotateAt(-angle, new Point((Locale.Width / 2), (Locale.Height / 2)));
                g.Transform = Transform;
                g.DrawImage(Locale, 0, 0,Locale.Width,Locale.Height);
            }
        }
    }
}
