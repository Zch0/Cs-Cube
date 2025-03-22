using System.Diagnostics;
using System.Numerics;

class Cube
{
    public int width { get; set; }
    public int height { get; set; }
    public int length { get; set; }
    public long avg_runtime_us { get; set; }
    public Queue<long> runtime_us_queue { get; set; } = new Queue<long>();
    public Screen screen { get; }
    private readonly int centerX, centerY;
    private double angleX, angleY, angleZ;
    private double angleX_increment, angleY_increment, angleZ_increment;
    private double angleX_per_us, angleY_per_us, angleZ_per_us;
    private double[,] vertices;
    private (int, int)[] edges;
    private int[][] faces;
    //摄像机向量
    private Vector3 camera = new Vector3(0, 0, 1);

    public Cube(int width, int height, int length)
    {
        this.width = width;
        this.height = height;
        this.length = length / 2;
        this.screen = new Screen(width, height);
        this.centerX = width / 2;
        this.centerY = height / 2;
        this.angleX = 0.0;
        this.angleY = 0.0;
        this.angleZ = 0.0;
        this.angleX_increment = 1.0;
        this.angleY_increment = 1.0;
        this.angleZ_increment = 1.0;
        int angle_per_s = 60;
        this.angleX_per_us = (angle_per_s * angleX_increment) / 1_000_000.0; //每微秒绕X旋转的角度
        this.angleY_per_us = (angle_per_s * angleY_increment) / 1_000_000.0; //每微秒绕Y旋转的角度
        this.angleZ_per_us = (angle_per_s * angleZ_increment) / 1_000_000.0; //微秒绕Z旋转的角度

        this.vertices = new double[8, 3] {
            { -1.0, -1.0, -1.0 },
            { 1.0, -1.0, -1.0 },
            { 1.0, 1.0, -1.0 },
            { -1.0, 1.0, -1.0 },
            { -1.0, -1.0, 1.0 },
            { 1.0, -1.0, 1.0 },
            { 1.0, 1.0, 1.0 },
            { -1.0, 1.0, 1.0 }
        };
        this.vertices = FormatVertices(this.vertices, this.length);
        this.edges = new (int, int)[] {
            (0, 1),
            (1, 2),
            (2, 3),
            (3, 0),
            (4, 5),
            (5, 6),
            (6, 7),
            (7, 4),
            (0, 4),
            (1, 5),
            (2, 6),
            (3, 7)
        };
        this.faces = new int[][] {
            new int[] { 0, 1, 2, 3 },
            new int[] { 5, 4, 7, 6 },
            new int[] { 0, 4, 5, 1 },
            new int[] { 7, 3, 2, 6 },
            new int[] { 0, 3, 7, 4 },
            new int[] { 5, 6 ,2, 1 }
        };
    }
    private double[,] FormatVertices(double[,] vertices, int lenth)
    {
        int rows = vertices.GetLength(0);
        int cols = vertices.GetLength(1);
        double[,] new_vertices = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                new_vertices[i, j] = vertices[i, j] * (double)length;
            }
        }
        return new_vertices;
    }
    public long CalcAvgRuntimeUs()
    {
        if (this.runtime_us_queue.Count >1000)
        {
            this.runtime_us_queue.Dequeue();
        }
        long sum = 0;
        foreach (var runtime_us in this.runtime_us_queue)
        {
            sum += runtime_us;
        }
        return sum / this.runtime_us_queue.Count;
    }
    public Vector3 CalcFaceNormalVector(int[] face, double[,] transformed_vertices)
    {
        //顶点1
        double x1 = transformed_vertices[face[0], 0];
        double y1 = transformed_vertices[face[0], 1];
        double z1 = transformed_vertices[face[0], 2];
        //顶点2
        double x2 = transformed_vertices[face[1], 0];
        double y2 = transformed_vertices[face[1], 1];
        double z2 = transformed_vertices[face[1], 2];
        //顶点3
        double x3 = transformed_vertices[face[2], 0];
        double y3 = transformed_vertices[face[2], 1];
        double z3 = transformed_vertices[face[2], 2];
        //向量12
        Vector3 vector_12 = new Vector3((float)(x2 - x1), (float)(y2 - y1), (float)(z2 - z1));
        //向量13
        Vector3 vector_13 = new Vector3((float)(x3 - x1), (float)(y3 - y1), (float)(z3 - z1));
        //叉乘
        Vector3 normal_vector = Vector3.Cross(vector_12, vector_13);
        normal_vector = Vector3.Normalize(normal_vector);
        normal_vector = Vector3.Divide(normal_vector, -1);
        return normal_vector;
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
        this.angleX += this.angleX_per_us*this.avg_runtime_us;
        this.angleY += this.angleY_per_us*this.avg_runtime_us;
        this.angleZ += this.angleZ_per_us*this.avg_runtime_us;
        this.angleX = this.angleX % 360;
        this.angleY = this.angleY % 360;
        this.angleZ = this.angleZ % 360;
    }

    public void Draw()
    {
        //Thread.Sleep((int)(1000 / this.fps));
        this.screen.ClearBuffer();
        this.screen.RefreshScreen();
        this.AutoRotate();
        double[,] transformed_vertices = new double[8, 3];
        for (int i = 0; i < this.vertices.GetLength(0); i++)
        {
            double x = this.vertices[i, 0];
            double y = this.vertices[i, 1];
            double z = this.vertices[i, 2];
            (y, z) = RotateX(y, z, this.angleX);
            (x, z) = RotateY(x, z, this.angleY);
            (x, y) = RotateZ(x, y, this.angleZ);
            transformed_vertices[i, 0] = x;
            transformed_vertices[i, 1] = y;
            transformed_vertices[i, 2] = z;
        }
        //根据面的遮挡关系，生成可见与不可见的面和边列表
        List<int[]> visibal_faces = [];
        List<int[]> invisible_faces = [];
        List<(int, int)> visibal_edges = [];
        List<(int, int)> invisibal_edges = [];
        //生成可见与不可见的面列表
        for (int i = 0; i < this.faces.GetLength(0); i++)
        {
            int[] face = this.faces[i];
            Vector3 normal_vector = CalcFaceNormalVector(face, transformed_vertices);
            if (Vector3.Dot(normal_vector, this.camera) > 0)
            {
                visibal_faces.Add(face);
            }
            else
            {
                invisible_faces.Add(face);
            }
        }
        //通过面列表生成可见与不可见的边列表
        for (int i = 0; i < this.edges.GetLength(0); i++)
        {
            bool is_invisibal = true;
            for (int j = 0; j < visibal_faces.Count; j++)
            {
                if (visibal_faces[j].Contains(this.edges[i].Item1) && visibal_faces[j].Contains(this.edges[i].Item2))
                {
                    visibal_edges.Add(this.edges[i]);
                    is_invisibal = false;
                    break;

                }
            }
            if (is_invisibal)
            {
                invisibal_edges.Add(this.edges[i]);
            }
        }
        //绘制可见的边
        for (int i = 0; i < visibal_edges.Count; i++)
        {
            double x1 = transformed_vertices[visibal_edges[i].Item1, 0];
            double y1 = transformed_vertices[visibal_edges[i].Item1, 1];
            double z1 = transformed_vertices[visibal_edges[i].Item1, 2];
            double x2 = transformed_vertices[visibal_edges[i].Item2, 0];
            double y2 = transformed_vertices[visibal_edges[i].Item2, 1];
            double z2 = transformed_vertices[visibal_edges[i].Item2, 2];
            this.screen.DrawLine(x1 + this.centerX, y1 + this.centerY, x2 + this.centerX, y2 + this.centerY, true);
        }//绘制不可见的边
        for (int i = 0; i < invisibal_edges.Count; i++)
        {
            double x1 = transformed_vertices[invisibal_edges[i].Item1, 0];
            double y1 = transformed_vertices[invisibal_edges[i].Item1, 1];
            double z1 = transformed_vertices[invisibal_edges[i].Item1, 2];
            double x2 = transformed_vertices[invisibal_edges[i].Item2, 0];
            double y2 = transformed_vertices[invisibal_edges[i].Item2, 1];
            double z2 = transformed_vertices[invisibal_edges[i].Item2, 2];
            this.screen.DrawDashedLine(x1 + this.centerX, y1 + this.centerY, x2 + this.centerX, y2 + this.centerY, true);
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
        Cube cube = new Cube(256, 256, 128);
        Stopwatch stopwatch = new Stopwatch();

        while (true)
        {
            stopwatch.Restart();
            stopwatch.Start();
            cube.Draw();
            cube.Menu();
            cube.screen.Display();
            stopwatch.Stop();
            long current_runtime_us = stopwatch.ElapsedTicks / (Stopwatch.Frequency / 1_000_000);
            cube.runtime_us_queue.Enqueue(current_runtime_us);
            cube.avg_runtime_us = cube.CalcAvgRuntimeUs();
            System.Diagnostics.Debug.WriteLine($"currentFPS:{1_000_000.0/current_runtime_us} avgFPS:{1_000_000.0/cube.avg_runtime_us}");
        }

        //var vector = cube.CalcFaceNormalVector(new int[] { 0, 3, 7, 4 }, cube.vertices);
        //Console.WriteLine(vector.ToString());
    }
}