using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Serial_to_Arduino
{
    public partial class Form1 : Form
    {
        private double[] y_values = new double[100];
        private double[] y_values2 = new double[1];
        private double[] y_values3 = new double[100];
        private double[] y_values4 = new double[100];
        private double[] y_values5 = new double[100];
        private double[] y_values6 = new double[100];
        public Form1()
        {
            InitializeComponent();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
        private void chart2_Click(object sender, EventArgs e)
        {

        }
        private void chart3_Click(object sender, EventArgs e)
        {

        }

        public void UpdateDataToChart1(double data)
        {
            UpdateData(chart1, "Command", y_values, data);
        }
        public void UpdateDataToChart3(double data, double data2)
        {
            UpdateData(chart3, "Degree1", y_values3, data);
            UpdateData(chart3, "Degree2", y_values4, data2);
        }
        public void UpdateDataToChart4(double data, double data2)
        {
            UpdateData(chart4, "Speed1", y_values5, data);
            UpdateData(chart4, "Speed2", y_values6, data2);
        }
        public void AddDataToChart2(double x, double y)
        {
            AddData(chart2, "Sim", x, y);
        }

        private void UpdateData(Chart c, string legend, double[] values, double data)
        {
            if (c.InvokeRequired)
            {
                c.Invoke((MethodInvoker)delegate { UpdateData(c, legend, values, data); });
            }
            else
            {
                c.Series[legend].Points.AddY(data);
                Array.Copy(values, 1, values, 0, values.Length - 1);
                values[values.Length - 1] = data;
                c.Series[legend].Points.Clear();
                for (int i = 0; i < values.Length; i++)
                {
                    c.Series[legend].Points.AddY(values[i]);
                }
                /*double e = Math.Abs(values[0] - data);
                int min = (int)Math.Min(values[0], data);
                int max = (int)Math.Max(values[0], data);
                if(min < max)
                {
                    c.ChartAreas[0].AxisY.Maximum = max + e / 10;
                    c.ChartAreas[0].AxisY.Minimum = min - e / 10;
                }*/
            }
        }

        private void AddData(Chart c, string legend, double x, double y)
        {
            if (c.InvokeRequired)
            {
                c.Invoke((MethodInvoker)delegate { AddData(c, legend, x, y); });
            }
            else
            {
                DataPoint dp = new DataPoint(x, y);
                //Console.WriteLine(dp);
                c.Series[legend].Points.Add(dp);
            }
        }

        private void ResetChart(Chart c, string legend)
        {
            double[] tmp_values = new double[1];
            InitChart(c, legend, tmp_values, c.Series["Graph1"].ChartType);
        }

        private void InitChart(Chart c, string legend, double[] set_values, SeriesChartType type)
        {
            // フォームをロードするときの処理
            c.Series.Clear();  // ← 最初からSeriesが1つあるのでクリアします
            c.ChartAreas.Clear();

            // ChartにChartAreaを追加します
            string chart_area1 = "Area1";
            c.ChartAreas.Add(new ChartArea(chart_area1));

            AddSeries(c, legend, type);

            // データをシリーズにセットします
            for (int i = 0; i < set_values.Length; i++)
            {
                c.Series[legend].Points.AddY(set_values[i]);
            }
        }

        private void AddSeries(Chart c, string legend, SeriesChartType type)
        {
            // ChartにSeriesを追加します
            c.Series.Add(legend);
            // グラフの種別を指定
            c.Series[legend].ChartType = type; // 折れ線グラフを指定してみます
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitChart(chart1, "Command", y_values, SeriesChartType.Line);
            InitChart(chart2, "Sim", y_values2, SeriesChartType.Point);
            InitChart(chart3, "Degree1", y_values3, SeriesChartType.Line);
            AddSeries(chart3, "Degree2", SeriesChartType.Line);
            InitChart(chart4, "Speed1", y_values3, SeriesChartType.Line);
            AddSeries(chart4, "Speed2", SeriesChartType.Line);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetChart(chart2, "Sim");
        }

        private void chart4_Click(object sender, EventArgs e)
        {

        }
    }
}
