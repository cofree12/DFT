using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using Core;
using ZedGraph;

namespace DFT
{
    public partial class MainForm : Form
    {
        private readonly FunctionGenerator _functionGenerator;
        private readonly Random _random = new Random();
        private readonly DftProcessor _dftProcessor;
        private readonly FftProcessor _fftProcessor;
        private double _dt = 0.7;
        private double _from;
        private double _to = 480;
        private Complex[] _fftResult;

        public MainForm()
        {
            InitializeComponent();
            _functionGenerator = new FunctionGenerator();
            _dftProcessor = new DftProcessor();
            _fftProcessor = new FftProcessor();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            DrawGraph(functionChart, _functionGenerator.Generate(_from, _to, _dt));
        }

        private void DrawGraph(ZedGraphControl control, double[] values, double[] times = null)
        {
            var pane = control.GraphPane;
            pane.XAxis.Title.Text = "Time";
            pane.YAxis.Title.Text = "Value";
            var t = _from;
            var data = values.Select((x, i) => new {item = x, index = t += _dt})
                .ToList();
            var pointPairList = new PointPairList(times ?? data.Select(x => x.index).ToArray(),
                data.Select(x => x.item).ToArray());
            pane.AddCurve("", pointPairList, GetRandomColor(), SymbolType.None);
            control.AxisChange();
            control.Invalidate();
        }

        private Color GetRandomColor()
        {
            return Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));
        }

        private void btnDFT_Click(object sender, EventArgs e)
        {
            Clear();
            DrawGraph(functionChart, _functionGenerator.Generate(_from, _to, _dt));
            var result = _dftProcessor.Transform(_functionGenerator.Generate(_from, _to, _dt), false);
            var t = result.Select((_, i) => (double) 1 / (i / _dt / result.Length)).ToArray();
            DrawGraph(fourierChartA,
                result.Select(x => Math.Sqrt(Math.Pow(x.Real, 2) + Math.Pow(x.Imaginary, 2)) / result.Length).ToArray(),
                t);

            DrawGraph(fourierChartP, result.Select(x => Math.Atan2(x.Imaginary, x.Real)).ToArray(), t);
            fourierChartA.AxisChange();
            fourierChartP.AxisChange();
            fourierChartP.Invalidate();
            fourierChartA.Invalidate();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (double.TryParse(txtFrom.Text, out value))
                _from = value;
        }

        private void txtTo_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (double.TryParse(txtTo.Text, out value))
                _to = value;
        }

        private void txtDt_TextChanged(object sender, EventArgs e)
        {
            double value;
            if (double.TryParse(txtDt.Text, out value))
                _dt = value;
        }

        private void functionChart_Load(object sender, EventArgs e)
        {
        }

        private void Clear()
        {
            functionChart.GraphPane.CurveList.Clear();
            fourierChartP.GraphPane.CurveList.Clear();
            fourierChartA.GraphPane.CurveList.Clear();
        }

        private void btnFFT_Click(object sender, EventArgs e)
        {
            try
            {
                Clear();
                var generated = _functionGenerator.Generate(_from, _to, _dt);

                generated = generated.Take((int)Math.Pow(2, (int)Math.Log(generated.Length, 2))).ToArray();

                DrawGraph(functionChart, generated);
                _fftResult = _fftProcessor.Process(generated.Select(x=>new Complex(x, 0)).ToArray(), false);
                var t = _fftResult.Select((_, i) => (double)1 / (i / _dt / _fftResult.Length)).ToArray();
                DrawGraph(fourierChartA,
                    _fftResult.Select(x => Math.Sqrt(Math.Pow(x.Real, 2) + Math.Pow(x.Imaginary, 2)) / _fftResult.Length).ToArray(),
                    t);

                DrawGraph(fourierChartP, _fftResult.Select(x => Math.Atan2(x.Imaginary, x.Real)).ToArray(), t);
                btnFFTRev.Enabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void chkReverse_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnFFTRev_Click(object sender, EventArgs e)
        {
            var reversed = _fftProcessor.Process(_fftResult, true);
            functionChart.GraphPane.CurveList.Clear();
            functionChart.Invalidate();
            DrawGraph(functionChart, reversed.Select(x=>x.Real).ToArray());

        }

        private void btnFFTwoRec_Click(object sender, EventArgs e)
        {
            try
            {
                Clear();
                var generated = _functionGenerator.Generate(_from, _to, _dt);

                generated = generated.Take((int)Math.Pow(2, (int)Math.Log(generated.Length, 2))).ToArray();

                DrawGraph(functionChart, generated);
                _fftResult = _fftProcessor.ProcessWORecursion(generated.Select(x => new Complex(x, 0)).ToArray(), false);
                var t = _fftResult.Select((_, i) => (double)1 / (i / _dt / _fftResult.Length)).ToArray();
                DrawGraph(fourierChartA,
                    _fftResult.Select(x => Math.Sqrt(Math.Pow(x.Real, 2) + Math.Pow(x.Imaginary, 2)) / _fftResult.Length).ToArray(),
                    t);

                DrawGraph(fourierChartP, _fftResult.Select(x => Math.Atan2(x.Imaginary, x.Real)).ToArray(), t);
                btnFFTRev.Enabled = true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}