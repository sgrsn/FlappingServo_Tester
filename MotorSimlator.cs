using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.ComponentModel;
using static COMPortSelector;
using static RegisterMap;
using System.Windows.Controls;

using Serial_to_Arduino;

static class MotorSimlator
{
    private static System.Timers.Timer command_timer;
    private static System.Timers.Timer update_timer;
    private const int tick_update_motor_speed = 8000;
    private const int tick_send_command = 100;

    private static int plus_speed = 200;
    private static int max_command = 1000;
    private static int  motor_com = 0;

    private static bool button_checked = false;

    public static void Init()
    {
    }
    public static void StartSimulation()
    {
        update_timer = new System.Timers.Timer(tick_update_motor_speed);
        update_timer.Elapsed += UpdateMotorSpeed;
        update_timer.AutoReset = true;
        update_timer.Enabled = true;

        command_timer = new System.Timers.Timer(tick_send_command);
        command_timer.Elapsed += CommandMotor;
        command_timer.AutoReset = true;
        command_timer.Enabled = true;

        button_checked = true;
    }

    public static void StartFlapping()
    {
        update_timer = new System.Timers.Timer(tick_update_motor_speed);
        update_timer.Elapsed += FlappingMotor;
        update_timer.AutoReset = true;
        update_timer.Enabled = true;

        command_timer = new System.Timers.Timer(tick_send_command);
        command_timer.Elapsed += CommandMotor;
        command_timer.AutoReset = true;
        command_timer.Enabled = true;

        button_checked = true;
    }

    private static void UpdateMotorSpeed(Object source, ElapsedEventArgs e)
    {
        if (IsConnected())
        {
            motor_com += plus_speed;
            if (motor_com > max_command | motor_com < -max_command) plus_speed = -plus_speed;
        }
    }
    private static void FlappingMotor(Object source, ElapsedEventArgs e)
    {
        if (IsConnected())
        {
            if (motor_com < 0)
                motor_com = 60;
            else if (motor_com > 0)
                motor_com = -60;
            else
                motor_com = 60;
        }
    }
    private static void CommandMotor(Object source, ElapsedEventArgs e)
    {
        if (IsConnected())
        {
            SerialPortControl._WriteData((int)motor_com, COMMAND_MOTOR);
            SerialPortControl._WriteData((int)motor_com, COMMAND_MOTOR_CONFIRM);
        }
    }
    public static void StopSimulation()
    {
        if(button_checked)
        {
            command_timer.Stop();
            command_timer.Dispose();
            update_timer.Stop();
            update_timer.Dispose();
            motor_com = 0;
            button_checked = false;
        }
    }
}
