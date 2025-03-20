using System;
using System.Runtime.InteropServices;

public class Screen
{
    public int width { get; set; }
    public int height { get; set; }

    private bool[,] buffer;
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    private const int STD_OUTPUT_HANDLE = -11;
    private IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
    public Screen(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.buffer = new bool[width, height];
        Console.CursorVisible = false;
    }
    public void clear_buffer()
    {
        this.buffer = new bool[this.width, this.height];
    }
    public void refresh_screen()
    {
        Console.SetCursorPosition(0, 0);
    }
    public void clear_screen()
    {
        Console.Clear();
    }
    public void set_pixel(double x, double y, bool value)
    {
        if (x < 0 || x >= this.width || y < 0 || y >= this.height)
        {
            return;
        }
        this.buffer[(int)x, (int)y] = value;
    }
    public void set_pixels(double x1, double y1, double x2, double y2, bool value)
    {
        for (int x = (int)x1; x <= x2; x++)
        {
            for (int y = (int)y1; y <= y2; y++)
            {
                this.buffer[x, y] = value;
            }
        }
    }
    public void draw_line(double x1, double y1, double x2, double y2, bool value)
    {
        double kY = (x1 != x2) ? (double)(y2 - y1) / (x2 - x1) : double.PositiveInfinity;
        double kX = (y1 != y2) ? (double)(x2 - x1) / (y2 - y1) : double.PositiveInfinity;

        // 定义 funcY 和 funcX 方法
        Func<int, int> funcY = (x) => (int)(kY * (x - x1) + y1);
        Func<int, int> funcX = (y) => (int)(kX * (y - y1) + x1);

        // 斜率不存在时判断
        if (double.IsPositiveInfinity(kY))
        {
            if (y1 <= y2)
            {
                for (int y = (int)y1; y <= y2; y++)
                {
                    this.set_pixel(x1, y, value);
                }
            }
            else
            {
                for (int y = (int)y2; y <= y1; y++)
                {
                    this.set_pixel(x1, y, value);
                }
            }
        }
        else if (double.IsPositiveInfinity(kX))
        {
            if (x1 <= x2)
            {
                for (int x = (int)x1; x <= x2; x++)
                {
                    this.set_pixel(x, y1, value);
                }
            }
            else
            {
                for (int x = (int)x2; x <= x1; x++)
                {
                    this.set_pixel(x, y1, value);
                }
            }
        }
        else
        {
            // 当斜率小于 1 时，以 x 为自变量遍历
            if (Math.Abs(kY) < 1)
            {
                if (x1 <= x2)
                {
                    for (int x = (int)x1; x <= x2; x++)
                    {
                        this.set_pixel(x, funcY(x), value);
                    }
                }
                else
                {
                    for (int x = (int)x2; x <= x1; x++)
                    {
                        this.set_pixel(x, funcY(x), value);
                    }
                }
            }
            // 当斜率大于等于 1 时，以 y 为自变量遍历
            else
            {
                if (y1 <= y2)
                {
                    for (int y = (int)y1; y <= y2; y++)
                    {
                        this.set_pixel(funcX(y), y, value);
                    }
                }
                else
                {
                    for (int y = (int)y2; y <= y1; y++)
                    {
                        this.set_pixel(funcX(y), y, value);
                    }
                }
            }
        }
    }
    public void display()
    {
        System.Text.StringBuilder buffer_str = new System.Text.StringBuilder();
        for (int y = 0; y < this.height; y++)
        {
            for (int x = 0; x < this.width; x++)
            {
                if (this.buffer[x, y])
                {
                    buffer_str.Append("██");
                }
                else
                {
                    buffer_str.Append("  ");
                }
            }
            buffer_str.Append('\n');
        }
        //Console.Write(buffer_str.ToString());
        string message = buffer_str.ToString();
        uint written;
        WriteConsole(handle, message, (uint)message.Length, out written, IntPtr.Zero);
    }
}