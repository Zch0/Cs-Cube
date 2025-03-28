using System.Runtime.InteropServices;

public class Screen
{
    public int width { get; set; }
    public int height { get; set; }
    private bool[,] buffer;

    //初始化控制台输出句柄（用于 WriteConsole）
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer, uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten, IntPtr lpReserved);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    private const int STD_OUTPUT_HANDLE = -11;
    private IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
    //初始化控制台输出句柄结束
    public Screen(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.buffer = new bool[height, width];
        Console.CursorVisible = false;
    }
    public void ClearBuffer()
    {
        this.buffer = new bool[this.height, this.width];
    }
    public void RefreshScreen()
    {
        Console.SetCursorPosition(0, 0);
    }
    public void ClearScreen()
    {
        Console.Clear();
    }
    public void SetPixel(double x, double y, bool value)
    {
        if (x < 0 || x >= this.width || y < 0 || y >= this.height)
        {
            return;
        }
        this.buffer[(int)x, (int)y] = value;
    }
    public void SetPixels(double x1, double y1, double x2, double y2, bool value)
    {
        for (int x = (int)x1; x <= x2; x++)
        {
            for (int y = (int)y1; y <= y2; y++)
            {
                this.buffer[x, y] = value;
            }
        }
    }
    public void DrawLine(double x1, double y1, double x2, double y2, bool value)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        double step = Math.Max(Math.Abs(dx), Math.Abs(dy));

        if (step == 0)
        {
            SetPixel(x1, y1, value);
            return;
        }

        double xStep = dx / step;
        double yStep = dy / step;

        for (int i = 0; i <= step; i++)
        {
            double x = x1 + xStep * i;
            double y = y1 + yStep * i;
            SetPixel((int)Math.Round(x), (int)Math.Round(y), value);
        }
    }
    //画虚线
    public void DrawDashedLine(double x1, double y1, double x2, double y2, bool value, int dashLength = 5, int gapLength = 5)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        double step = Math.Max(Math.Abs(dx), Math.Abs(dy));

        if (step == 0 || dashLength <= 0)
        {
            SetPixel(x1, y1, value);
            return;
        }

        double xStep = dx / step;
        double yStep = dy / step;
        double lineLength = Math.Sqrt(dx * dx + dy * dy);
        int cycle = dashLength + gapLength;

        for (int i = 0; i <= step; i++)
        {
            double t = i / step;
            double distance = t * lineLength;

            if (distance % cycle < dashLength)
            {
                int x = (int)Math.Round(x1 + xStep * i);
                int y = (int)Math.Round(y1 + yStep * i);
                SetPixel(x, y, value);
            }
        }
    }


    public void Display()
    {
        System.Text.StringBuilder buffer_str = new System.Text.StringBuilder();
        for (int y = 0; y < this.height; y++)
        {
            for (int x = 0; x < this.width; x++)
            {
                if (this.buffer[x, y])
                {
                    buffer_str.Append('█');
                }
                else
                {
                    buffer_str.Append(' ');
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