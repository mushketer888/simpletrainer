using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
namespace StockSim
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<dbdata> vals=new List<dbdata>();
        List<dbdata> k = new List<dbdata>();

        int ind = 0;
        int maxbars = 150;

        double balance=100000;
        double pat = 0;
        int psize;
        int type = 0;//0 no 1 buy 2 sell

        public void BlockBtn()
        {
            if (type == 0)
            {
                buybtn.Enabled = true;
                shortbtn.Enabled = true;
                closebtn.Enabled = false;
                return;
            }
           
                buybtn.Enabled = false;
                shortbtn.Enabled = false;
                closebtn.Enabled = true;
                return;
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            UpdStats();





            vals = File.ReadAllLines("eurusd.csv").Skip(1)
                                           .Select(v => dbdata.FromCsv(v))
                                           
                                           .ToList();
           
            k = vals.Skip(ind).Take(maxbars).ToList();
            LoadChart(k);
            
           
        }
        public void LoadChart(List<dbdata> k)
        {
            Series ser = new Series();
            Series price = new Series("price"); // <<== make sure to name the series "price"
            price.Color = Color.Black;
            if (chart1.Series.Count==0) 
                chart1.Series.Add(price);
            chart1.ChartAreas[0].BorderColor = Color.Gray;

            chart1.ChartAreas[0].AxisX.LabelAutoFitStyle = LabelAutoFitStyles.WordWrap;
            chart1.ChartAreas[0].AxisX.IsLabelAutoFit = true;
            chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
            // Set series chart type
            chart1.Series["price"].ChartType = SeriesChartType.Candlestick;

            // Set the style of the open-close marks
            chart1.Series["price"]["OpenCloseStyle"] = "Triangle";

            // Show both open and close marks
            chart1.Series["price"]["ShowOpenClose"] = "Both";

            // Set point width
            chart1.Series["price"]["PointWidth"] = "1.0";

            // Set colors bars
            chart1.Series["price"]["PriceUpColor"] = "Green"; // <<== use text indexer for series
            chart1.Series["price"]["PriceDownColor"] = "Red"; // <<== use text indexer for series

            chart1.Series["price"].Points.Clear();
            for (int i = 0; i < k.Count; i++)
            {
                // adding date and high
                chart1.Series["price"].Points.AddXY(DateTime.Parse(k[i].Date), k[i].High);
                // adding low
                chart1.Series["price"].Points[i].YValues[1] = k[i].Low;
                //adding open
                chart1.Series["price"].Points[i].YValues[2] = k[i].Open;
                // adding close
                chart1.Series["price"].Points[i].YValues[3] = k[i].Close;
            }
            chart1.ChartAreas[0].RecalculateAxesScale();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Console.WriteLine(1);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ind++;
            
            k = vals.Skip(ind).Take(maxbars).ToList();
            LoadChart(k);
            UpdStats();
        }

        private void buybtn_Click(object sender, EventArgs e)
        {
            type = 1;//buy
            int mane;
            int.TryParse(textBox1.Text, out mane);
            mane = (int)(mane * 0.01 * balance);
            System.Console.WriteLine(String.Format("Buy power: {0}", mane));

            psize = (int)(mane / k.Last().Close);
            System.Console.WriteLine(String.Format("Lot size: {0}", psize));

            balance = balance - (psize * k.Last().Close);
            pat = k.Last().Close;

            BlockBtn();
            UpdStats();
        }
        public double CalcProfit()
        {
            double prof=0;

            if (type == 1)
            {
                prof = (k.Last().Close / (pat) - 1)*100;//1 2
            }

            if (type == 2)
            {
                prof = ((pat /k.Last().Close ) - 1) * 100;//1 2
            }

            return prof;
        }
        private void UpdStats()
        {
            label2.Text = String.Format("{0:0}",balance);
            label4.Text = String.Format("{0:+0.##;-0.##}%", ((balance / 100000) - 1) * 100);
            if (type == 0)
            {
                poslabel.Text = "No Orders";
                label8.Text = "N/A";
            }
            else
            {
                poslabel.Text = String.Format("{0} {1} @ {2}", (type == 1 ? "BUY" : "SHORT"), psize, pat);
            }

            if (type != 0)
            {
                label8.Text = String.Format("{0:+0.##;-0.##}%", CalcProfit());
            }
        }

        private void closebtn_Click(object sender, EventArgs e)
        {

            if (type == 1)
            {
                balance = balance + (psize * k.Last().Close);
            }

            if (type == 2)
            {
                System.Console.WriteLine("pat {0}", pat);
                System.Console.WriteLine("Last {0}", k.Last().Close);
                System.Console.WriteLine("Psize {0}", psize);
                balance = balance + (psize *pat* (pat/k.Last().Close));
            }
        //            pat 1.45029
        //Last 1.41776
        //Psize 34475
            type = 0;
            BlockBtn();
            UpdStats();
        }

        private void shortbtn_Click(object sender, EventArgs e)
        {
            type = 2;//short
            int mane;
            int.TryParse(textBox1.Text, out mane);
            mane = (int)(mane * 0.01 * balance);
            System.Console.WriteLine(String.Format("Buy power: {0}", mane));

            psize = (int)(mane / k.Last().Close);
            System.Console.WriteLine(String.Format("Lot size: {0}", psize));

            balance = balance - (psize * k.Last().Close);
            pat = k.Last().Close;

            BlockBtn();
            UpdStats();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
    public class dbdata
    {
        public string Date;
        public double High;
        public double Low;
        public double Open;
        public double Close;
        public dbdata(string d, float h, float l, float o, float c) { Date = d; High = h; Low = l; Open = o; Close = c; }
        public dbdata() { }
        public void Debug()
        {
            System.Console.WriteLine(String.Format("{0},{1},{2},{3},{4}",this.Date,this.Open,this.High,this.Low,this.Close));
        }
        public static dbdata FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            dbdata dailyValues = new dbdata();
            dailyValues.Date = values[0];
            dailyValues.Open = Convert.ToDouble(values[1]);
            dailyValues.High = Convert.ToDouble(values[2]);
            dailyValues.Low = Convert.ToDouble(values[3]);
            dailyValues.Close = Convert.ToDouble(values[4]);
           
            return dailyValues;
        }
    }
}
