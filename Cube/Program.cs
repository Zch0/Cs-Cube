class Cube
{
    public int width { get; set; }
    public int height { get; set; }
    public int length { get; set; }
    public Screen screen { get; }
    private readonly int centerX, centerY;
    private double angleX, angleY, angleZ;
    private double angleX_increment, angleY_increment, angleZ_increment;
    private double fps;
    private int[,] vertices;
    private int[,] edges;
    private int[,] faces;

    public Cube(int width, int height, int length)
    {
        this.width = width;
        this.height = height;
        this.length = length / 2;
        this.screen = new Screen(width, height);
        this.centerX = width / 2;
        this.centerY = height / 2;
        this.angleX = 0;
        this.angleY = 0;
        this.angleZ = 0;
        this.angleX_increment = 1.0;
        this.angleY_increment = 1.0;
        this.angleZ_increment = 1.0;
        this.fps = 60;
        this.vertices = new int[8, 3] {
            { -1, -1, -1 },
            { 1, -1, -1 },
            { 1, 1, -1 },
            { -1, 1, -1 },
            { -1, -1, 1 },
            { 1, -1, 1 },
            { 1, 1, 1 },
            { -1, 1, 1 }
        };
        this.vertices = FormatVertices(this.vertices, this.length);
        this.edges = new int[12, 2] {
            { 0, 1 },
            { 1, 2 },
            { 2, 3 },
            { 3, 0 },
            { 4, 5 },
            { 5, 6 },
            { 6, 7 },
            { 7, 4 },
            { 0, 4 },
            { 1, 5 },
            { 2, 6 },
            { 3, 7 }
        };
        this.faces = new int[6,4] {
            { 0, 1, 2, 3 },
            { 4, 5, 6, 7 },
            { 0, 1, 5, 4 },
            { 2, 3, 7, 6 },
            { 0, 3, 7, 4 },
            { 1, 2, 6, 5 }
        };
    }
    private int[,] FormatVertices(int[,] vertices, int lenth)
    {
        int rows = vertices.GetLength(0);
        int cols = vertices.GetLength(1);
        int[,] new_vertices = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                new_vertices[i, j] = vertices[i, j] * length;
            }
        }
        return new_vertices;
    }
    static private (double, double) RotateX(double y, double z, double angle)
    {
        double rad = angle * Math.PI / 180;
        double new_y = y * Math.Cos(rad) - z * Math.Sin(rad);
        double new_z = y * Math.Sin(rad) + z * Math.Cos(rad);
        return (new_y, new_z);
    }
    static private (double, double) RotateY(double x, double z, double angle)
    {
        double rad = angle * Math.PI / 180;
        double new_x = x * Math.Cos(rad) - z * Math.Sin(rad);
        double new_z = x * Math.Sin(rad) + z * Math.Cos(rad);
        return (new_x, new_z);
    }
    static private (double, double) RotateZ(double x, double y, double angle)
    {
        double rad = angle * Math.PI / 180;
        double new_x = x * Math.Cos(rad) - y * Math.Sin(rad);
        double new_y = x * Math.Sin(rad) + y * Math.Cos(rad);
        return (new_x, new_y);
    }
    private void AutoRotate()
    {
        this.angleX += this.angleX_increment;
        this.angleY += this.angleY_increment;
        this.angleZ += this.angleZ_increment;
        this.angleX = this.angleX % 360;
        this.angleY = this.angleY % 360;
        this.angleZ = this.angleZ % 360;
    }
    public (double,double,double,double) FaceEqPara(double x1,double y1,double z1, double x2,double y2,double z2,double x3,double y3,double z3)
    {
        double A = (y3 - y1) * (z3 - z1) - (z2 - z1) * (y3 - y1);
        double B = (x3 - x1) * (z2 - z1) - (x2 - x1) * (z3 - z1);
        double C = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
        double D = -(A * x1 + B * y1 + C * z1);
        return (A, B, C, D);
    }
    public void Draw()
    {
        //Thread.Sleep((int)(1000 / this.fps));
        this.screen.clear_buffer();
        this.screen.refresh_screen();
        this.AutoRotate();
        for (int i = 0; i < this.edges.GetLength(0); i++)
        {
            double x1 = this.vertices[this.edges[i, 0], 0];
            double y1 = this.vertices[this.edges[i, 0], 1];
            double z1 = this.vertices[this.edges[i, 0], 2];
            double x2 = this.vertices[this.edges[i, 1], 0];
            double y2 = this.vertices[this.edges[i, 1], 1];
            double z2 = this.vertices[this.edges[i, 1], 2];
            (y1, z1) = RotateX(y1, z1, this.angleX);
            (x1, z1) = RotateY(x1, z1, this.angleY);
            (x1, y1) = RotateZ(x1, y1, this.angleZ);
            (y2, z2) = RotateX(y2, z2, this.angleX);
            (x2, z2) = RotateY(x2, z2, this.angleY);
            (x2, y2) = RotateZ(x2, y2, this.angleZ);
            this.screen.draw_line(x1 + this.centerX, y1 + this.centerY, x2 + this.centerX, y2 + this.centerY, true);
        }
    }
    public void Menu()
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.W:
                    this.angleX_increment += 1.0;
                    break;
                case ConsoleKey.S:
                    this.angleX_increment -= 1.0;
                    break;
                case ConsoleKey.A:
                    this.angleY_increment -= 1.0;
                    break;
                case ConsoleKey.D:
                    this.angleY_increment += 1.0;
                    break;
                case ConsoleKey.Q:
                    this.angleZ_increment -= 1.0;
                    break;
                case ConsoleKey.E:
                    this.angleZ_increment += 1.0;
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.P:
                    this.angleX_increment = 0;
                    this.angleY_increment = 0;
                    this.angleZ_increment = 0;
                    break;
            }
        }
    }
}

class MainClass
{
    public static void Main(string[] args)
    {
        Cube cube = new Cube(512, 512, 256);
        while (true)
        {
            cube.Draw();
            cube.Menu();
            cube.screen.display();
        }
    }
}