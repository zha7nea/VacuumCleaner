using System;
using System.Threading;

namespace RobotCleaner
{
    public class Map
    {
        private enum CellType { Empty, Dirt, Obstacle, Cleaned };
        private CellType[,] _grid;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            _grid = new CellType[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _grid[x, y] = CellType.Empty;
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsDirt(int x, int y) => IsInBounds(x, y) && _grid[x, y] == CellType.Dirt;
        public bool IsObstacle(int x, int y) => IsInBounds(x, y) && _grid[x, y] == CellType.Obstacle;
        public void AddObstacle(int x, int y) => _grid[x, y] = CellType.Obstacle;
        public void AddDirt(int x, int y) => _grid[x, y] = CellType.Dirt;
        public void Clean(int x, int y)
        {
            if (IsInBounds(x, y))
                _grid[x, y] = CellType.Cleaned;
        }

        public void Display(int robotX, int robotY)
        {
            Console.Clear();
            Console.WriteLine("Vacuum Cleaner Robot Simulation");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Legend: #=Obstacle | D=Dirt | .=Empty | R=Robot | C=Cleaned\n");

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x == robotX && y == robotY)
                        Console.Write("R ");
                    else
                    {
                        switch (_grid[x, y])
                        {
                            case CellType.Empty: Console.Write(". "); break;
                            case CellType.Dirt: Console.Write("D "); break;
                            case CellType.Obstacle: Console.Write("# "); break;
                            case CellType.Cleaned: Console.Write("C "); break;
                        }
                    }
                }
                Console.WriteLine();
            }
            Thread.Sleep(300);
        }

        public bool HasDirt()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (_grid[x, y] == CellType.Dirt)
                        return true;
            return false;
        }
    }

    public interface IStrategy
    {
        void Clean(Robot robot);
    }

    public class Robot
    {
        private readonly Map _map;
        private readonly IStrategy _strategy;
        public int X { get; set; }
        public int Y { get; set; }
        public Map Map => _map;

        public Robot(Map map, IStrategy strategy)
        {
            _map = map;
            _strategy = strategy;
            X = 0;
            Y = 0;
        }

        public bool Move(int newX, int newY)
        {
            if (_map.IsInBounds(newX, newY) && !_map.IsObstacle(newX, newY))
            {
                X = newX;
                Y = newY;
                _map.Display(X, Y);
                return true;
            }
            return false;
        }

        public bool MoveStep(int dx, int dy)
        {
            int newX = X + dx;
            int newY = Y + dy;

            if (!_map.IsInBounds(newX, newY) || _map.IsObstacle(newX, newY))
                return false;

            X = newX;
            Y = newY;
            _map.Display(X, Y);
            return true;
        }

        public void CleanCurrentSpot()
        {
            if (_map.IsDirt(X, Y))
            {
                _map.Clean(X, Y);
                _map.Display(X, Y);
            }
        }

        public void StartCleaning() => _strategy.Clean(this);
    }

    public class PerimeterHuggerStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            robot.Move(0, 0);
            robot.CleanCurrentSpot();

            while (robot.Move(robot.X + 1, robot.Y))
                robot.CleanCurrentSpot();

            while (robot.Move(robot.X, robot.Y + 1))
                robot.CleanCurrentSpot();

            while (robot.Move(robot.X - 1, robot.Y))
                robot.CleanCurrentSpot();

            while (robot.Move(robot.X, robot.Y - 1))
                robot.CleanCurrentSpot();
        }
    }

    public class SpiralCleaningStrategy : IStrategy
    {
        public void Clean(Robot robot)
        {
            int left = 0;
            int right = robot.Map.Width - 1;
            int top = 0;
            int bottom = robot.Map.Height - 1;

            while (left <= right && top <= bottom)
            {
                for (int x = left; x <= right; x++)
                {
                    if (!robot.Map.HasDirt()) return;
                    if (robot.Move(x, top))
                        robot.CleanCurrentSpot();
                    Thread.Sleep(200);
                }
                top++;

                for (int y = top; y <= bottom; y++)
                {
                    if (!robot.Map.HasDirt()) return;
                    if (robot.Move(right, y))
                        robot.CleanCurrentSpot();
                    Thread.Sleep(200);
                }
                right--;

                for (int x = right; x >= left; x--)
                {
                    if (!robot.Map.HasDirt()) return;
                    if (robot.Move(x, bottom))
                        robot.CleanCurrentSpot();
                    Thread.Sleep(200);
                }
                bottom--;

                for (int y = bottom; y >= top; y--)
                {
                    if (!robot.Map.HasDirt()) return;
                    if (robot.Move(left, y))
                        robot.CleanCurrentSpot();
                    Thread.Sleep(200);
                }
                left++;
            }

            Console.WriteLine("All dirt is cleaned!");
        }
    }

    
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Choose cleaning strategy:");
            Console.WriteLine("1 - Perimeter Hugger");
            Console.WriteLine("2 - Spiral Cleaning");
            Console.Write("Enter choice: ");
            string choice = Console.ReadLine();

            IStrategy strategy = choice == "2"
                ? new SpiralCleaningStrategy()
                : new PerimeterHuggerStrategy();

            Map map = new Map(10, 6);
            map.AddDirt(5, 3);
            map.AddDirt(8, 2);
            map.AddObstacle(2, 4);
            map.AddObstacle(7, 1);

            Robot robot = new Robot(map, strategy);
            robot.StartCleaning();

            Console.WriteLine("Done cleaning!");
        }
    }
}
