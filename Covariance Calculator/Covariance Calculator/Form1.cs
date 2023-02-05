using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Covariance_Calculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PointDistribution pointDistribution = new PointDistribution(trackBar2.Value, trackBar1.Value, 1000);
            Chart chart = new Chart(pictureBox1.Width, pictureBox1.Height, 1001, 1001, pictureBox1);
            chart.InsertPoints(pointDistribution.PointCollection, pointDistribution.MeanX, pointDistribution.MeanY);
            chart.InsertCenterGravity(pointDistribution.MeanX, pointDistribution.MeanY, Color.Black);

            label6.Text = pointDistribution.MeanX.ToString();
            label7.Text = pointDistribution.MeanY.ToString();
            label8.Text = pointDistribution.Covariance.ToString();            
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label10.Text = trackBar1.Value.ToString();
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            label9.Text = trackBar2.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trackBar1.Value = 50;
            trackBar2.Value = 250;
            label9.Text = trackBar2.Value.ToString();
            label10.Text = trackBar1.Value.ToString();
            button3_Click(sender, e);
        }
    }
    public static class Global
    {
        public static int RADIANT0 = 0;
        public static int RADIANT1 = 157080;
        public static int RADIANT2 = 314159;
        public static int RADIANT3 = 471239;
        public static int RADIANT4 = 628319;
        public static int MAX_RADIUS = 500;
        public static int PRECISION_RAD = 100000;

        public static Color POSITIVE_COLOR = Color.FromArgb(255, 100, 0);
        public static Color NEGATIVE_COLOR = Color.FromArgb(0, 156, 255);

        public static Random rng = new Random();
    }

    public class PointDistribution
    {
        public List<(double, double)> PointCollection { get; set; }
        public double MeanX = 0;
        public double MeanY = 0;
        public double Covariance = 0;

        //Specify number of POINT, RATIO (66 means 66% of POSITIVE and 34% NEGATIVE)
        //and the AREA 
        public PointDistribution(int n, int ratio, int area)
        {
            PointCollection = new List<(double, double)>();
            List<double> angles = new List<double>();

            //Calculate how many POSITIVE and NEGATIVE points
            int numberPlus = (int)(((double)ratio / (double)100) * (double)n);
            int numberMinus = n - numberPlus;
            //Radius of the circle considered
            double maxRadius = (double)area * (double)Math.Sqrt(2);
            
            //Add every angles inside the list
            //Generate angles for POSITIVE points 
            for (int i = 0; i < numberPlus / 2; i++) { angles.Add((double)(Global.rng.Next(Global.RADIANT0, Global.RADIANT1)) / (double)Global.PRECISION_RAD); }            
            for (int i = 0; i < numberPlus / 2; i++) { angles.Add((double)(Global.rng.Next(Global.RADIANT2, Global.RADIANT3)) / (double)Global.PRECISION_RAD); }
            //Generate angles for NEGATIVE points 
            for (int i = 0; i < numberMinus / 2; i++) { angles.Add((double)(Global.rng.Next(Global.RADIANT1, Global.RADIANT2)) / (double)Global.PRECISION_RAD); }
            for (int i = 0; i < numberMinus / 2; i++) { angles.Add((double)(Global.rng.Next(Global.RADIANT3, Global.RADIANT4)) / (double)Global.PRECISION_RAD); }

            //For every angles compute x and y
            foreach (double angle in angles)
            {                                
                double x = -1;
                double y = -1;

                //If x or y are outside the area then recompute again
                while (x < 0 || x >= area || y < 0 || y >= area)
                {
                    int radius = Global.rng.Next(1, (int)maxRadius);
                    x = ((double)radius * Math.Cos(angle)) + ((double)area / (double)2);
                    y = ((double)radius * Math.Sin(angle)) + ((double)area / (double)2);
                }  

                PointCollection.Add((x, y));
            }
            //Computing means and covariance
            (MeanX, MeanY, Covariance) = ComputeVarianceMean(PointCollection);
        }

        public (double, double, double) ComputeVarianceMean(List<(double, double)> collection)
        {
            //Initialize mean and sum of product
            double sumCrossProduct = 0;
            double meanX = 0;
            double meanY = 0;
            int counter = 0;

            //For every coordinates compute mean and covariance
            foreach ((double, double) element in collection)
            {
                counter++;
                (double x, double y) = element;
                //Old y mean is needed for the sum of product formula
                double oldMeanY = meanY;

                //Mean online formula
                meanX = ((meanX * (double)(counter - 1)) + x) / (double)counter;
                meanY = ((meanY * (double)(counter - 1)) + y) / (double)counter;
                //Sum of product online formula
                sumCrossProduct = sumCrossProduct + (x - meanX)*(y - oldMeanY);
            }

            return (meanX, meanY, sumCrossProduct/(double)counter);
        }
        }

    public class Chart
    {
        public Bitmap _Bitmap { get; private set; }
        public Graphics _Graphics { get; private set; }
        public PictureBox _PictureBox { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public (List<(int, int)>, Color) CandlesMemory;
        public int YMemory;
        public (string, (int, int)) TextMemory;

        public Chart(int width, int height, int maxX, int maxY, PictureBox pictureBox)
        {
            _Bitmap = new Bitmap(width, height);
            _Graphics = Graphics.FromImage(_Bitmap);
            _PictureBox = pictureBox;

            Width = width;
            Height = height;

            MaxX = maxX;
            MaxY = maxY;
            MinX = 0;
            MinY = 0;

            _PictureBox.Image = _Bitmap;
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Pen pen)
        {
            _Graphics.DrawLine(pen, x1, y1, x2, y2);
        }
        public void DrawLine((int, int) p1, (int, int) p2, Pen pen)
        {
            _Graphics.DrawLine(pen, p1.Item1, p1.Item2, p2.Item1, p2.Item2);
        }
        public void DrawFilledCircle((int, int) point, int radius, SolidBrush brush)
        {
            _Graphics.FillEllipse(brush, point.Item1-(radius/2), point.Item2 - (radius / 2), radius, radius);            
        }

        public (int, int) ConvertCoordinates((int, int) point)
        {
            float x = ((float)(point.Item1 - MinX) / (float)(MaxX - MinX)) * Width;
            float y = Height - (((float)(point.Item2 - MinY) / (float)(MaxY - MinY)) * Height);

            return ((int)x, (int)y);
        }
        public (int, int) ConvertCoordinates((double, double) point)
        {
            float x = ((float)(point.Item1 - MinX) / (float)(MaxX - MinX)) * Width;
            float y = Height - (((float)(point.Item2 - MinY) / (float)(MaxY - MinY)) * Height);

            return ((int)x, (int)y);
        }
        public void InsertPoints(List<(double, double)> collection, double meanX, double meanY)
        {
            SolidBrush positiveBrush = new SolidBrush(Global.POSITIVE_COLOR);
            SolidBrush negativeBrush = new SolidBrush(Global.NEGATIVE_COLOR);
            foreach ((int, int) point in collection)
            {
                //DrawFilledCircle(ConvertCoordinates(point), 7, brush);
                if (((double)point.Item1 - meanX) * ((double)point.Item2 - meanY) > 0) { DrawFilledCircle(ConvertCoordinates(point), 7, positiveBrush); }
                else { DrawFilledCircle(ConvertCoordinates(point), 7, negativeBrush); }                
            }
            _PictureBox.Image = _Bitmap;
        }
        public void InsertCenterGravity(double meanX, double meanY, Color color)
        {
            Pen pen = new Pen(color, 1);
            
            DrawLine(ConvertCoordinates((meanX, (double)0)), ConvertCoordinates((meanX, (double)MaxY)), pen);
            DrawLine(ConvertCoordinates(((double)0, meanY)), ConvertCoordinates(((double)MaxX, meanY)), pen);
            _PictureBox.Image = _Bitmap;
        }
        public void DrawX(int x)
        {
            Pen pen = new Pen(Color.Black, 1);
            DrawLine(ConvertCoordinates((x, (double)0)), ConvertCoordinates((x, (double)MaxY)), pen);
        }
    }
}