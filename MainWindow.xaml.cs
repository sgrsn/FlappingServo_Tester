using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO.Ports;
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;

using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using static COMPortSelector;
using static SerialPortControl;
using static RegisterMap;
using static MotorSimlator;
using static DumpDataToExcel;

delegate void SerialReceivedHandle();

namespace Serial_to_Arduino
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Form1 _form1;

        private double motor_com = 0.0000000000000001;
        private double motor_slider_value = 0.0;
        private bool slider_control = false;
        private bool sim_control = false;

        private double p_slider_value = 0.0;
        private double i_slider_value = 0.0;
        private double d_slider_value = 0.0;

        private static System.Timers.Timer _timer;
        private const int tick_send_command = 100;


        public MainWindow()
        {
            InitializeComponent();
            _form1 = new Form1();
            SerialPortInit();
            SerialReceivedHandle data_received_handler = this.UpdateChartHandler;
            SerialPortControl.SetDatareceivedHandle(data_received_handler);
            this.Closing += new CancelEventHandler(CloseSerialPort);
            this.Closing += new CancelEventHandler(StopTimer);
        }

        private void UpdateChartHandler()
        {
            if (sim_control | slider_control)
            {
                _form1.UpdateDataToChart1((double)SerialPortControl.Register[CURRENT_COMMAND]);
                _form1.UpdateDataToChart3((double)SerialPortControl.Register[MOTOR_DEGREE_INC], SerialPortControl.Register[MOTOR_DEGREE_2]);

                if( (Math.Abs(SerialPortControl.Register[MOTOR_SPEED_INC])<10000 ) 
                        && ( Math.Abs(SerialPortControl.Register[MOTOR_SPEED_ABS]) < 10000) )
                _form1.UpdateDataToChart4((double)SerialPortControl.Register[MOTOR_SPEED_INC], SerialPortControl.Register[MOTOR_SPEED_ABS]);

                _form1.AddDataToChart2(Register[CURRENT_COMMAND], Register[MOTOR_SPEED_INC]);
                //_form1.AddDataToChart2(myPort.Register[CURRENT_COMMAND], (double)myPort.Register[MOTOR_SPEED]);

                DumpDataToSave(SerialPortControl.Register[CURRENT_COMMAND], SerialPortControl.Register[MOTOR_SPEED_INC]);
            }
        }

        private void StopTimer(object sender, CancelEventArgs e)
        {
            if (sim_control | slider_control)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }

        private void SetTimer(int ms, ElapsedEventHandler handler)
        {
            // Create a timer with a two second interval.
            _timer = new System.Timers.Timer(ms);
            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += handler;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void CloseSerialPort(object sender, CancelEventArgs e)
        {
            SerialPortControl.ClosePort();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            COMPortSelector.PushConnectButton();
            _form1.Show();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void MotorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (String.IsNullOrEmpty(e.NewValue.ToString())) return;
            else
            {
                motor_slider_value = Convert.ToDouble( e.NewValue.ToString() );
                //Console.WriteLine(motor_slider_value);
                SliderTextBlock.Text = ((int)motor_slider_value).ToString();
            }
        }

        private void LEDButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = (sender as System.Windows.Controls.Button);
            if (!IsConnected()) return;
            if(b.Content.ToString() == "OFF")
            {
                SerialPortControl._WriteData(0, COMMAND_LED);
                b.Content = "ON";
            }
            else
            {
                SerialPortControl._WriteData(1, COMMAND_LED);
                b.Content = "OFF";
            }
        }


        private void Pslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (String.IsNullOrEmpty(e.NewValue.ToString())) return;
            else
            {
                p_slider_value = Convert.ToDouble(e.NewValue.ToString());
                P_SliderTextBlock.Text = (p_slider_value/1000).ToString();
                SerialPortControl._WriteData((int)p_slider_value, PARAMETER_P);
            }
        }

        private void Islider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (String.IsNullOrEmpty(e.NewValue.ToString())) return;
            else
            {
                i_slider_value = Convert.ToDouble(e.NewValue.ToString());
                I_SliderTextBlock.Text = (i_slider_value / 1000).ToString();
                SerialPortControl._WriteData((int)i_slider_value, PARAMETER_I);
            }
        }

        private void Dslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (String.IsNullOrEmpty(e.NewValue.ToString())) return;
            else
            {
                d_slider_value = Convert.ToDouble(e.NewValue.ToString());
                D_SliderTextBlock.Text = (d_slider_value / 1000).ToString();
                SerialPortControl._WriteData((int)d_slider_value, PARAMETER_D);
            }
        }

        private void ParameterApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SerialPortControl._WriteData((int)p_slider_value, PARAMETER_P);
            SerialPortControl._WriteData((int)i_slider_value, PARAMETER_I);
            SerialPortControl._WriteData((int)d_slider_value, PARAMETER_D);
        }
        private void SliderControl(Object source, ElapsedEventArgs e)
        {
            motor_com = (int)motor_slider_value;
            SerialPortControl._WriteData((int)(motor_slider_value), COMMAND_MOTOR);
            SerialPortControl._WriteData((int)(motor_slider_value), COMMAND_MOTOR_CONFIRM);
        }

        private void SendStart(Object source, ElapsedEventArgs e)
        {
            if (IsConnected())
            {
                if (SerialPortControl.Register[START_REPLY] == 0)
                {
                    Console.WriteLine("wait");
                    SerialPortControl._WriteData(123, COMMAND_START);
                }
                else
                {
                    SerialPortControl.Register[START_REPLY] = 0;
                    _timer.Stop();
                    _timer.Dispose();
                    SetTimer(tick_send_command, SliderControl);
                }
            }
        }

        private void CommandStart()
        {
            if (sim_control)
            {
                _timer.Stop();
                _timer.Dispose();
                sim_control = false;
            }
            if (!IsConnected()) return;
            slider_control = true;
            SetTimer(tick_send_command, SendStart);
        }
        private void StopRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sim_control | slider_control)
            {
                _timer.Stop();
                _timer.Dispose();
                sim_control = false;
                slider_control = false;
            }
            if (!IsConnected()) return;
            MotorSimlator.StopSimulation();
            SerialPortControl._WriteData(1, COMMAND_STOP);
        }

        private void CommandRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsConnected()) return;
            MotorSimlator.StopSimulation();
            CommandStart();
            SerialPortControl._WriteData(0, COMMAND_MODE);
        }

        private void PositionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsConnected()) return;
            MotorSimlator.StopSimulation();
            CommandStart();
            SerialPortControl._WriteData(1, COMMAND_MODE);
        }

        private void SpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsConnected()) return;
            MotorSimlator.StopSimulation();
            CommandStart();
            SerialPortControl._WriteData(2, COMMAND_MODE);
        }

        private void SimRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (slider_control)
            {
                _timer.Stop();
                _timer.Dispose();
                slider_control = false;
            }
            MotorSimlator.StartSimulation();
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToExcelFile();
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            StartLog();
        }
    }
}
