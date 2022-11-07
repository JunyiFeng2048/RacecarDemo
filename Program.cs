using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ExtensionMethods
{
    public static class ExtensionFunction
    {
        public static System.Drawing.Point ToPoint(this Vector v1)
        {
            return new System.Drawing.Point(Convert.ToInt32(Math.Round(v1.X)), Convert.ToInt32(Math.Round(v1.Y)));
        }
    }
}

namespace RacecarDemo
{
    internal static class Program
    {
        public static Stopwatch GameClock = new Stopwatch();
        public static Form1 GameWindow = new Form1();
        public static Track Track = new Track(@"Content/Track.bmp");
        public static List<Car> Cars = new List<Car>();
        public static int TimeStep = 20;
        public static Random r1 = new Random();


        [STAThread]
        static void Main()
        {
            while(true)
            {
                Thread SimulationThread = new Thread(RunCarSimulation);
                SimulationThread.SetApartmentState(ApartmentState.STA);
                SimulationThread.Start();
                Application.Run(GameWindow);
            }
            
        }

        static void RunCarSimulation()
        {
            Car PlayerCar = new Car(false, false, new Vector(Track.StartX, Track.StartY));
            GA ga = new GA();
            int AINum = 9;
            /*
            foreach (string t1 in Directory.GetFiles(@"Content/PastRuns"))
            {
                Car AICar = new Car(false, false, new Vector(Track.StartX, Track.StartY));                                                           //  Creates random cars and gives them the
                AICar.CircuitCommands = File.ReadAllLines(t1).Select(x => (Car.ControlEnum)Convert.ToInt32(x)).ToList();                             //  command list from the files in PastRuns folder                                                                                               //
                Cars.Add(AICar);                                                                                                                    //
            }
            */

            for (int i = 0; i < AINum; i++)
            {
                Car AICar = new Car(false, false, new Vector(Track.StartX, Track.StartY));                                                           //  Creates random cars and gives them the
                Cars.Add(AICar);
            }

            
            GameWindow.FocusCar = PlayerCar;
            Cars.Add(PlayerCar);
            while (true)
            {
                //PlayerCar.Human = true;
                int act = 0;
                while (!PlayerCar.Finished)
                {
                    
                    ga.FitnessList.Clear();
                    GameClock.Restart();
                    foreach (Car car in Cars)   //Run simulation for each car in the list "Cars"
                    {

                        if (!car.Crashed && !car.Finished)
                        {
                            if (car.Human) // If the car is human it gets the current keyboard key and adds it to the command list 
                            {
                                car.CircuitCommands.Add(car.GetKeyBoard());
                                car.ControlCar(car.CircuitCommands[car.CurrentCommand]);              // If the car is not human it uses prestored commands.  Each time a command is called the currentcommand itterator is ++
                                car.CurrentCommand++;
                                car.PhysicsStep();
                            }


                            if (!car.Human) { car.AI_GA_run(act); }

                            if (!car.Finished) { car.CalculateReward(); }

                        }
                        ga.FitnessList.Add(car);
                    }
                    act++;
                    ga.SortFitnessList();

                    //foreach (Car f in ga.FitnessList)
                        //Console.WriteLine(f.CurrentFitness);
                    //Console.WriteLine();

                    int CrashedNum = 0;
                    foreach (Car car in Cars)
                    {
                        if (car.Crashed) { CrashedNum++; }
                        if (CrashedNum == AINum + 1) { goto next; }

                    }
                    GameWindow.DrawImage();
                    Thread.Sleep(Math.Max(0, TimeStep - (int)GameClock.ElapsedMilliseconds));  //Sleep for the rest of the timestep
                }

            next:
                /*
                PlayerCar.ResetCar(new Vector(Track.StartX, Track.StartY));
                for (int i = 0; i < PlayerCar.CircuitCommands.Count(); i++)
                {
                    GameClock.Restart();
                    PlayerCar.ControlCar(PlayerCar.CircuitCommands[i]);
                    PlayerCar.PhysicsStep();
                    GameWindow.DrawImage();
                    Thread.Sleep(Math.Max(0, TimeStep - (int)GameClock.ElapsedMilliseconds));
                }
                */
                ga.Crossover();
                ga.Mutation();
                foreach (Car car in Cars)
                {
                    car.ResetCar(new Vector(Track.StartX, Track.StartY));
                    

                }

            }
            
        }
    }
}
