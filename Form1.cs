using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;

namespace RacecarDemo
{
    public partial class Form1 : Form
    {
        public Car FocusCar = null;
        public static List<Bitmap> Cars = Directory.GetFiles(@"Content/Cars").Select(x => new Bitmap(x)).ToList();
        Stopwatch Watch = new Stopwatch();
        int FrameCount = 0;
        string FPS = "";
        public Form1()
        {
            InitializeComponent();
            PictureBox1.SetBounds(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);
            Cars.ForEach(x => x.MakeTransparent());
            Watch.Start();
        }

        public void DrawImage()
        {
            PictureBox1.Invalidate();
            Invoke(new Action(() =>{Update();}));
            if (Watch.ElapsedMilliseconds >= 1000)
            {
                FPS = FrameCount.ToString();
                FrameCount = 0;
                Watch.Restart();
            }
            FrameCount++;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            PictureBox1.SetBounds(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);
        }



        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            Point ScreenAdjustment = new Point(0, 0);
            if (FocusCar != null) { ScreenAdjustment = new Point(FocusCar.Position.ToPoint().X - (this.ClientSize.Width / 2), FocusCar.Position.ToPoint().Y - (this.ClientSize.Height / 2)); }

            e.Graphics.TranslateTransform(-ScreenAdjustment.X, -ScreenAdjustment.Y);
            e.Graphics.DrawImage(Program.Track.TrackImage, 0, 0, Program.Track.TrackImage.Width, Program.Track.TrackImage.Height);
            e.Graphics.ResetTransform();

            foreach (Car c in Program.Cars)
            {
                e.Graphics.TranslateTransform(-ScreenAdjustment.X, -ScreenAdjustment.Y);
                if(FocusCar != null && c == FocusCar) { c.SensorZone.ForEach(x => e.Graphics.DrawEllipse(new Pen(Color.Blue, 5), (float)x.X - 1, (float)x.Y - 1, 1, 1)); }
                e.Graphics.TranslateTransform(c.Position.ToPoint().X, c.Position.ToPoint().Y);
                e.Graphics.RotateTransform(c.Azimuth);
                e.Graphics.DrawImage(Cars[c.CarColor], -25, -25, 50, 50); 
                e.Graphics.DrawEllipse(new Pen(Color.Red, 5), -1, -1, 1, 1);
                e.Graphics.ResetTransform();
            }

            if (FocusCar != null)
            {
                if (FocusCar.SensorImage != null)
                {
                    e.Graphics.DrawImage(FocusCar.SensorImage, 0, this.ClientRectangle.Height - Car.SensorDepth, FocusCar.SensorImage.Width, FocusCar.SensorImage.Height);
                    e.Graphics.DrawRectangle(new Pen(Color.Blue, 8), 0, this.ClientRectangle.Height - Car.SensorDepth, FocusCar.SensorImage.Width, FocusCar.SensorImage.Height);
                }
                e.Graphics.DrawString("Framerate: " + FPS, new Font("Verdana", 12), new SolidBrush(Color.Red), new System.Drawing.Point(0, 10));
                e.Graphics.DrawString("Fitness: " + FocusCar.CurrentFitness, new Font("Verdana", 12), new SolidBrush(Color.Red), new System.Drawing.Point(0, 30));
                if (FocusCar.Crashed) { e.Graphics.DrawString("CRASH!!!", new Font("Impact", 82), new SolidBrush(Color.Red), new System.Drawing.Point(0, this.ClientRectangle.Height / 2)); }
                if (FocusCar.Finished) { e.Graphics.DrawString("FINISH!!!", new Font("Impact", 82), new SolidBrush(Color.Red), new System.Drawing.Point(0, this.ClientRectangle.Height / 2)); }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
