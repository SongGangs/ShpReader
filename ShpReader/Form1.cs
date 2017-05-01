using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShpReader
{
    //SG  2016/05/01
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ArrayList polylines = new ArrayList(); //线集合
        private ArrayList polygons = new ArrayList(); //面集合
        private ArrayList points = new ArrayList(); //点集合
        private Pen pen = new Pen(Color.Black, 1); //定义画笔
        private int ShapeType; //shp文件类型
        private int count; //计数
        private double xmin, ymin, xmax, ymax;
        private double n1, n2; //x,y轴放大倍数

        private void OpenFileBtu_Click(object sender, EventArgs e)
        {
            string shpfilepath = "";
            openFileDialog1.Filter = "shapefile(*.shp)|*.shp|All files(*.*)|*.*"; //打开文件路径
            openFileDialog1.InitialDirectory = "D://";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                shpfilepath = openFileDialog1.FileName;
                BinaryReader br = new BinaryReader(openFileDialog1.OpenFile());
                //读取文件过程
                br.ReadBytes(24);
                int FileLength = br.ReadInt32(); // <0代表数据长度未知
                int FileBanben = br.ReadInt32();
                ShapeType = br.ReadInt32();
                xmin = br.ReadDouble();
                ymax = -1*br.ReadDouble();
                xmax = br.ReadDouble();
                ymin = -1*br.ReadDouble();
                double width = xmax - xmin;
                double height = ymax - ymin;
                n1 = (float) (this.Panel.Width*0.9/width); //x轴放大倍数
                n2 = (float) (this.Panel.Height*0.9/height); //y轴放大倍数
                br.ReadBytes(32);

                switch (ShapeType)
                {
                    case 1:
                        points.Clear();
                        while (br.PeekChar() != -1)
                        {
                            Point point = new Point();
                            uint RecordNum = br.ReadUInt32();
                            int DataLength = br.ReadInt32();
                            //读取第i个记录
                            br.ReadInt32();
                            point.X = br.ReadDouble();
                            point.Y = -1*br.ReadDouble();
                            points.Add(point);


                        }
                        StreamWriter sw = new StreamWriter("point.txt");
                        foreach (Point p in points)
                        {
                            sw.WriteLine("{0},{1},{2}", p.X, -1*p.Y, 0);
                        }
                        sw.Close();
                        break;
                    case 3:
                        polylines.Clear();
                        while (br.PeekChar() != -1)
                        {
                            Polyline polyline = new Polyline();
                            polyline.Box = new double[4];
                            polyline.Parts = new ArrayList();
                            polyline.Points = new ArrayList();

                            uint RecordNum = br.ReadUInt32();
                            int DataLength = br.ReadInt32();
                            //读取第i个记录
                            br.ReadInt32();
                            polyline.Box[0] = br.ReadDouble();
                            polyline.Box[1] = br.ReadDouble();
                            polyline.Box[2] = br.ReadDouble();
                            polyline.Box[3] = br.ReadDouble();
                            polyline.NumParts = br.ReadInt32();
                            polyline.NumPoints = br.ReadInt32();

                            for (int i = 0; i < polyline.NumParts; i++)
                            {
                                int parts = new int();
                                parts = br.ReadInt32();
                                polyline.Parts.Add(parts);
                            }
                            for (int i = 0; i < polyline.NumPoints; i++)
                            {
                                Point pointtemp = new Point();
                                pointtemp.X = br.ReadDouble();
                                pointtemp.Y = -1*br.ReadDouble();
                                polyline.Points.Add(pointtemp);
                            }
                            polylines.Add(polyline);
                        }
                        StreamWriter sw2 = new StreamWriter("line.txt");
                        foreach (Polyline p in polylines)
                        {
                            for (int i = 0; i < p.NumParts; i++)
                            {
                                int startpoint;
                                int endpoint;
                                if (i == p.NumParts - 1)
                                {
                                    startpoint = (int) p.Parts[i];
                                    endpoint = p.NumPoints;
                                }
                                else
                                {
                                    startpoint = (int) p.Parts[i];
                                    endpoint = (int) p.Parts[i + 1];
                                }
                                sw2.WriteLine("线" + count.ToString() + ":");
                                for (int k = 0, j = startpoint; k < endpoint; k++,j++)
                                {
                                    Point ps = (Point) p.Points[j];
                                    sw2.WriteLine("{0},{1},{2}", ps.X, -1*ps.Y, 0);
                                }
                                count++;
                            }
                        }
                        sw2.Close();
                        break;
                    case 5:
                        polygons.Clear();
                        while (br.PeekChar() != -1)
                        {
                            Polygon polygon = new Polygon();
                            polygon.Parts = new ArrayList();
                            polygon.Points = new ArrayList();

                            uint RecordNum = br.ReadUInt32();
                            int DataLength = br.ReadInt32();
                            //读取第i个记录
                            br.ReadInt32();
                            for (int i = 0; i < 4; i++)
                            {
                                polygon.Box[i] = br.ReadDouble();
                            }
                            polygon.NumParts = br.ReadInt32();
                            polygon.NumPoints = br.ReadInt32();

                            for (int i = 0; i < polygon.NumParts; i++)
                            {
                                int parts = new int();
                                parts = br.ReadInt32();
                                polygon.Parts.Add(parts);
                            }
                            for (int i = 0; i < polygon.NumPoints; i++)
                            {
                                Point pointtemp = new Point();
                                pointtemp.X = br.ReadDouble();
                                pointtemp.Y = -1*br.ReadDouble();
                                polygon.Points.Add(pointtemp);
                            }
                            polygons.Add(polygon);
                        }
                        StreamWriter sw1 = new StreamWriter("polygon.txt");
                        count = 1;
                        foreach (Polygon p in polygons)
                        {
                            for (int i = 0; i < p.NumParts; i++)
                            {
                                int startpoint;
                                int endpoint;
                                if (i == p.NumParts - 1)
                                {
                                    startpoint = (int) p.Parts[i];
                                    endpoint = p.NumPoints;
                                }
                                else
                                {
                                    startpoint = (int) p.Parts[i];
                                    endpoint = (int) p.Parts[i + 1];
                                }
                                sw1.WriteLine("多边形" + count.ToString() + ":");
                                for (int k = 0, j = startpoint; j < endpoint; k++,j++)
                                {
                                    Point ps = (Point) p.Points[j];
                                    sw1.WriteLine("{0},{1},{2}", ps.X, -1*ps.Y, 0);
                                }
                                count++;
                            }
                        }
                        sw1.Close();
                        break;
                }
            }
        }

        private void ReflashBtu_Click(object sender, EventArgs e)
        {
            double width = xmax - xmin; //图像宽
            double height = ymax - ymin; //图像高
            n1 = (float) (this.Panel.Width*0.9/width); //x轴放大倍数
            n2 = (float) (this.Panel.Height*0.9/height); //x轴放大倍数
            this.Panel.Refresh();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            PointF[] point;
            switch (ShapeType)
            {
                case 1: //点类型
                    foreach (Point p in points)
                    {
                        PointF pp = new PointF();
                        pp.X = (float) (10 + (p.X - xmin)*n1);
                        pp.Y = (float) (10 + (p.Y - ymin)*n2);
                        e.Graphics.DrawEllipse(pen, pp.X, pp.Y, 1.5f, 1.5f);

                    }
                    break;
                case 3: //线类型
                    foreach (Polyline p in polylines)
                    {
                        for (int i = 0; i < p.NumParts; i++)
                        {
                            int startpoint;
                            int endpoint;
                            point = null;
                            if (i == p.NumParts - 1)
                            {
                                startpoint = (int) p.Parts[i];
                                endpoint = p.NumPoints;
                            }
                            else
                            {
                                startpoint = (int) p.Parts[i];
                                endpoint = (int) p.Parts[i + 1];
                            }
                            point = new PointF[endpoint - startpoint];
                            for (int k = 0, j = startpoint; k < endpoint; k++,j++)
                            {
                                Point ps = (Point) p.Points[j];
                                point[k].X = (float) (10 + (ps.X - xmin)*n1);
                                point[k].Y = (float) (10 + (ps.Y - ymin)*n2);

                            }
                            e.Graphics.DrawLines(pen, point);
                        }
                    }
                    break;
                case 5: //面类型
                    foreach (Polygon p in polygons)
                    {
                        for (int i = 0; i < p.NumParts; i++)
                        {
                            int startpoint;
                            int endpoint;
                            point = null;
                            if (i == p.NumParts - 1)
                            {
                                startpoint = (int) p.Parts[i];
                                endpoint = p.NumPoints;
                            }
                            else
                            {
                                startpoint = (int) p.Parts[i];
                                endpoint = (int) p.Parts[i + 1];
                            }
                            point = new PointF[endpoint - startpoint];
                            for (int k = 0, j = startpoint; j < endpoint; k++,j++)
                            {
                                Point ps = (Point) p.Points[j];
                                point[k].X = (float) (10 + (ps.X - xmin)*n1);
                                point[k].Y = (float) (10 + (ps.Y - ymin)*n2);

                            }
                            e.Graphics.DrawPolygon(pen, point);
                        }
                    }
                    break;

            }
        }
    }
}
