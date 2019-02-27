using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace Calculating_Technical_Indicator
{
    public partial class CTI : Form
    {
        public CTI()
        {
            InitializeComponent();
            Init_table();
            GetData();
        }

        // Set datatable for datagridview
        DataTable table = new DataTable();
        void Init_table()
        {
            table.Columns.Add(new DataColumn("Indicator Name", typeof(string)));
            table.Columns.Add(new DataColumn("Material", typeof(string)));
            table.Columns.Add(new DataColumn("Type", typeof(string)));
            table.Columns.Add(new DataColumn("Add", typeof(string)));
            dgv1.DataSource = table;
            dgv1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            // MovingAverage
            DataRow row = table.NewRow();
            row["Indicator Name"] = "MovingAverage";
            row["Material"] = "Price";
            row["Type"] = "Trend";
            table.Rows.Add(row);
            // MACD
            row = table.NewRow();
            row["Indicator Name"] = "MACD";
            row["Material"] = "Price";
            row["Type"] = "Trend";
            table.Rows.Add(row);
            // Stochastic 
            row = table.NewRow();
            row["Indicator Name"] = "Stochastic";
            row["Material"] = "Price";
            row["Type"] = "Trend";
            table.Rows.Add(row);
        }

        // Set Dataset
        List<data> mydata = new List<data>();
        class data
        {
            public string date { get; set; }
            public double open { get; set; }
            public double high { get; set; }
            public double low { get; set; }
            public double close { get; set; }
            public double volume { get; set; }
        }
        void GetData()
        {
            string fname = Environment.CurrentDirectory + "\\005930.csv";
            int cc = 0;
            using (var reader = new StreamReader(fname))
            {                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (cc > 0)
                    {
                        mydata.Add(new data()
                        {
                            date = values[0],
                            open = Convert.ToDouble(values[1]),
                            high = Convert.ToDouble(values[2]),
                            low = Convert.ToDouble(values[3]),
                            close = Convert.ToDouble(values[4]),
                            volume = Convert.ToDouble(values[5])
                        });
                    }
                    cc++;
                }
            }
            Draw_Graph();
        }

        // Draw 
        void Draw_Graph()
        {
            GraphPane myPane = zedGraphControl1.GraphPane;

            // Set Graph Title
            myPane.Title.Text = "Stock Price";
            myPane.XAxis.Title.Text = "Time";
            myPane.YAxis.Title.Text = "Price";
            myPane.Y2Axis.Title.Text = "Volume";
            // Input Data
            PointPairList pp = new PointPairList();
            StockPointList pl = new StockPointList();

            for (int i = 0; i < mydata.Count; i++)
            {
                pl.Add(i, mydata[i].high, mydata[i].low, mydata[i].open, mydata[i].close, mydata[i].volume);
                pp.Add(i, mydata[i].volume);
            }
            //     LineItem pCurve_ = myPane.AddCurve("Price", pp, Color.Black, SymbolType.None); pCurve_.Line.Width = 2;

            BarItem vBar = myPane.AddBar("Volume", pp, Color.Black);
            vBar.IsY2Axis = true;
            myPane.Y2Axis.IsVisible = true;
            JapaneseCandleStickItem myCandle = myPane.AddJapaneseCandleStick("Price", pl);
            myCandle.Stick.RisingFill.Color = Color.LightGreen;
            myCandle.Stick.FallingFill.Color = Color.PaleVioletRed;
   
            zedGraphControl1.AxisChange();
            zedGraphControl1.ZoomEvent += MyZoomEvent;
            myPane.XAxis.Scale.Min = 0;
            myPane.X2Axis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 500;
            myPane.X2Axis.Scale.Max = 500;            
            zedGraphControl1.Refresh();
            ZoomState zoomState = new ZoomState(zedGraphControl1.GraphPane, ZoomState.StateType.Zoom);
            MyZoomEvent(zedGraphControl1, zoomState, zoomState);
        }

        // Set Auto Zoom
        private void MyZoomEvent(ZedGraphControl control, ZoomState oldstate, ZoomState newstate)
        {
            int min = Convert.ToInt32(control.GraphPane.XAxis.Scale.Min);
            int max = Convert.ToInt32(control.GraphPane.XAxis.Scale.Max);
            List<double> value = new List<double>();
            List<double> value2 = new List<double>();
            for (int i = min;  i < max; i++)
            {
                if ( i < mydata.Count) 
                {
                    value.Add(mydata[i].close);
                    value2.Add(mydata[i].volume);
                }
            }
  
            control.GraphPane.YAxis.Scale.Min = value.Min(x => x) * 0.8;
            control.GraphPane.YAxis.Scale.Max = value.Max(x => x) * 1.05;
            control.GraphPane.Y2Axis.Scale.Min = value2.Min(x => x) * 1;
            control.GraphPane.Y2Axis.Scale.Max = value2.Max(x => x) * 4;
        }
        
        // Add Technical Indicator to Graph
        private void dgv1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string ti = dgv1.Rows[e.RowIndex].Cells[0].Value.ToString();
            if (dgv1.Rows[e.RowIndex].Cells[3].Value.ToString() == "")
            {
                DialogResult result = MessageBox.Show("Do you want to add this Technical Indicator? : " + ti, "Yes or No", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    string check = "ν";
                    string TI = dgv1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    dgv1.Rows[e.RowIndex].Cells[3].Value = check;
                    switch (TI)
                    {
                        case "MovingAverage":
                            Show_MovingAverage();
                            break;
                    }
                }
            }
            else
            {
                DialogResult result = MessageBox.Show("Do you want to remove this Technical Indicator? : " + ti, "Yes or No", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    string TI = dgv1.Rows[e.RowIndex].Cells[0].Value.ToString();
                    dgv1.Rows[e.RowIndex].Cells[3].Value = "";
                    switch (TI)
                    {
                        case "MovingAverage":
                            Remove_MovingAverage();
                            break;
                    }
                }
            }
        }

        // Form Moving 
        public Point downPoint = Point.Empty;
        private void CTI_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            downPoint = new Point(e.X, e.Y);
        }

        private void CTI_MouseMove(object sender, MouseEventArgs e)
        {
            if (downPoint == Point.Empty)
            {
                return;
            }
            Point location = new Point(
                this.Left + e.X - downPoint.X,
                this.Top + e.Y - downPoint.Y);
            this.Location = location;
        }

        private void CTI_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            downPoint = Point.Empty;
        }

        // Show
        private void Show_MovingAverage()
        {
            GraphPane myPane = zedGraphControl1.GraphPane;

            PointPairList ma5 = new PointPairList();
            PointPairList ma20 = new PointPairList();
            PointPairList ma60 = new PointPairList();
            PointPairList ma120 = new PointPairList();

            for (int i = 0; i < mydata.Count; i++)
            {
                ma5.Add(i, Calculate_Moving_Average(5, i));
                ma20.Add(i, Calculate_Moving_Average(20, i));
                ma60.Add(i, Calculate_Moving_Average(60, i));
                ma120.Add(i, Calculate_Moving_Average(120, i));
            }
            
            LineItem mCurve_5 = myPane.AddCurve("MA5", ma5, Color.Red, SymbolType.None);
            LineItem mCurve_20 = myPane.AddCurve("MA20", ma20, Color.Blue, SymbolType.None);
            LineItem mCurve_60 = myPane.AddCurve("MA60", ma60, Color.Purple, SymbolType.None);
            LineItem mCurve_120 = myPane.AddCurve("MA120", ma120, Color.Green, SymbolType.None);

            zedGraphControl1.AxisChange();
            zedGraphControl1.Refresh();
        }

        // Remove
        private void Remove_MovingAverage()
        {
            GraphPane myPane = zedGraphControl1.GraphPane;

            for (int i =0; i < myPane.CurveList.Count; i++)
            {
                if (myPane.CurveList[i].Label.Text.Contains("MA"))
                {
                    myPane.CurveList.RemoveAt(i);
                    i = 0;
                }         
            }
            zedGraphControl1.AxisChange();
            zedGraphControl1.Refresh();
        }

        // Function 
        double Calculate_Moving_Average(int period, int ci)
        {
            double value = 0;
            if (ci > period)
            {
                for (int i = ci - period + 1; i <= ci; i ++)
                {
                    value += mydata[i].close;
                }
            }        
            return value / period;

        }

        double Calculate_MACD()
        {
            double value;
            value = 1;
            return value;
        }


    }
}
