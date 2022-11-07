using System;
using ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Media;
using System.Drawing.Drawing2D;
using System.Drawing;
using Point = System.Drawing.Point;

namespace RacecarDemo
{
    public class Car
    {
        const float TopSpeed = 16.5f;   // The maximum speed the car can reach
        const float AccelerationSpeed = .4f; //How fast the car can accelerate
        const float BrakeSpeed = .6f;   //How many units to slow the car when a brake command is given
        const float TurnSpeed = 2.5f;  //How many units to turn the azimuth when a turn command is given
        const float Drag = .05f; //Air and ground drag that slows the car no matter what each physics step
        public const int SensorWidth = 100;
        public const int SensorDepth = 120;


        public bool Human;
        public bool EnableSensor;

        public int CarColor;

        public float Velocity = 0f;   //Cars current velocity
        public float Azimuth = 0f;    //Cars current orientation (0: North   180: South  )
        public Vector Position = new Vector(0, 0); //Cars current position
        public bool Crashed = false;
        public bool Finished = false;


        //Car Sensor
        public List<Point> SensorZone = new List<Point>();
        public Bitmap SensorImage;
        
        //Car fitness
        public float CurrentFitness = 0f; //Score of how well the car is doing

        Pixel.SurfaceType LastSurface = Pixel.SurfaceType.FinishLine;

        public List<int> chromosome = new List<int>();
        


        public Car(bool Human,bool Sensor,Vector StartPosition)
        {
            this.Human = Human;
            this.EnableSensor = Sensor;
            Position = StartPosition;
            this.Azimuth = 90f;
            this.CircuitCommands = new List<ControlEnum>();
            this.CurrentCommand = 0;
            this.CarColor = Program.r1.Next(0, Form1.Cars.Count());
          
        }

        public List<ControlEnum> CircuitCommands;
        public int CurrentCommand;
        public enum ControlEnum {RightGas,RightNone,RightBrake,NoneGas,NoneNone,NoneBrake,LeftGas,LeftNone,LeftBrake }

        public void ResetCar(Vector Start)
        {
            Crashed = false;
            Finished = false;
            Position = Start;
            Velocity = 0;
            Azimuth = 90f;
            CurrentCommand = 0;
            CurrentFitness = 0f;//
        }

       

        public void ControlCar(ControlEnum ControlMode) 
        {
            float AdjustedTurnSpeed = TurnSpeed;
            if (Velocity == 0f) { AdjustedTurnSpeed = 0; }

            if (ControlMode == ControlEnum.RightNone|| ControlMode == ControlEnum.RightGas|| ControlMode == ControlEnum.RightBrake) { Azimuth += AdjustedTurnSpeed; }
            if (ControlMode == ControlEnum.LeftNone || ControlMode == ControlEnum.LeftGas || ControlMode == ControlEnum.LeftBrake) { Azimuth -= AdjustedTurnSpeed; }
            if (Azimuth < 0f) { Azimuth = 360f + Azimuth; }
            if (Azimuth >= 360.0f) { Azimuth -= 360f; }
            
            if (ControlMode == ControlEnum.LeftGas || ControlMode == ControlEnum.RightGas || ControlMode == ControlEnum.NoneGas) { Velocity = Math.Min(TopSpeed, Velocity + AccelerationSpeed); }
            if (ControlMode == ControlEnum.LeftBrake || ControlMode == ControlEnum.RightBrake || ControlMode == ControlEnum.NoneBrake) { Velocity = Math.Max(0, Velocity - BrakeSpeed); }
        }

        public void PhysicsStep()
        {
            Velocity = Math.Max(0, Velocity - Drag);  //Apply drag
            Vector VelocityVector = new Vector(Math.Sin((Math.PI / 180f) * Azimuth), -Math.Cos((Math.PI / 180f) * Azimuth));  //Get direction vector from compass heading
            VelocityVector.Normalize();
            Position += (VelocityVector * Velocity);   //Apply velocity in direction
        }
        public ControlEnum GetKeyBoard()   //Get keyboard input
        {
            ControlEnum NextControl = ControlEnum.NoneNone;
            if (Keyboard.IsKeyDown(Key.Right)) { NextControl = ControlEnum.RightNone; }
            else if (Keyboard.IsKeyDown(Key.Left)) { NextControl = ControlEnum.LeftNone; }
            if (Keyboard.IsKeyDown(Key.Up)) 
            { 
                if(NextControl == ControlEnum.RightNone) { NextControl = ControlEnum.RightGas; }
                if (NextControl == ControlEnum.LeftNone) { NextControl = ControlEnum.LeftGas; }
                if (NextControl == ControlEnum.NoneNone) { NextControl = ControlEnum.NoneGas; }
            }
            else if (Keyboard.IsKeyDown(Key.Down)) 
            {
                if (NextControl == ControlEnum.RightNone) { NextControl = ControlEnum.RightBrake; }
                if (NextControl == ControlEnum.LeftNone) { NextControl = ControlEnum.LeftBrake; }
                if (NextControl == ControlEnum.NoneNone) { NextControl = ControlEnum.NoneBrake; }
            }
            return NextControl;
        }

        public void CalculateReward()
        {
            CurrentFitness = CurrentFitness - .001f;
            if (!Finished)
            {
                Pixel.SurfaceType CurrentSurface = Program.Track.TrackData[Position.ToPoint().X, Position.ToPoint().Y].Type;
                if (CurrentSurface == Pixel.SurfaceType.Barrier) 
                { 
                    Crashed = true;
                    CurrentFitness -= .5f;
                }
                if (CurrentSurface != Pixel.SurfaceType.FinishLine)
                {
                    if (LastSurface != CurrentSurface && LastSurface != Pixel.SurfaceType.FinishLine)
                    {
                        if (CurrentSurface == Pixel.SurfaceType.TarmacA)
                        {
                            if (LastSurface == Pixel.SurfaceType.TarmacC) { CurrentFitness++; }
                            else { Crashed = true; }
                        }
                        if (CurrentSurface == Pixel.SurfaceType.TarmacB)
                        {
                            if (LastSurface == Pixel.SurfaceType.TarmacA) { CurrentFitness++; }
                            else { Crashed = true; }
                        }
                        if (CurrentSurface == Pixel.SurfaceType.TarmacC)
                        {
                            if (LastSurface == Pixel.SurfaceType.TarmacB) { CurrentFitness++; }
                            else { Crashed = true; }
                        }
                    }
                }
                else
                {
                    if (LastSurface == Pixel.SurfaceType.TarmacB){Finished = true;}
                }
                LastSurface = CurrentSurface;
            }
        }

        public void SenseFront()
        {
            if (SensorImage == null) { SensorImage = new Bitmap(Car.SensorWidth * 2, Car.SensorDepth); }
            Vector ForwardVector = new Vector(Math.Sin((Math.PI / 180f) * Azimuth), -Math.Cos((Math.PI / 180f) * Azimuth));
            ForwardVector.Normalize();
            Vector LeftVector = new Vector(ForwardVector.Y, -ForwardVector.X);
            Vector RightVector = -new Vector(ForwardVector.Y, -ForwardVector.X);
            Vector LeftClosePoint = Position+(LeftVector*SensorWidth);
            Vector RightClosePoint = Position+(RightVector* SensorWidth);
            Vector LeftFarPoint = LeftClosePoint + (ForwardVector * SensorDepth);
            Vector RightFarPoint= RightClosePoint + (ForwardVector * SensorDepth);
            SensorZone = new List<Point>() { LeftClosePoint.ToPoint(), RightClosePoint.ToPoint(), LeftFarPoint.ToPoint(), RightFarPoint.ToPoint() };
            Program.Track.GetTrackSection(SensorImage,Position.ToPoint(), Azimuth, SensorWidth, SensorDepth);
        }

        
        public void AI_GA_run(int ACT)
        {
            ControlEnum NextControl;
            if (ACT < chromosome.Count)
            {
                Console.WriteLine("lol");
                int PastAct = chromosome[ACT];
                NextControl = (ControlEnum)PastAct;
                ControlCar(NextControl);
                PhysicsStep();
                return;
            }
            int seed = Guid.NewGuid().GetHashCode();
            Random random = new Random(seed);
            int action = random.Next(0,9);
            chromosome.Add(action);
            NextControl = (ControlEnum)action;
            ControlCar(NextControl);
            chromosome.Add(3);
            NextControl = ControlEnum.NoneGas;
            ControlCar(NextControl);

            PhysicsStep();
        }
        

    }
}
