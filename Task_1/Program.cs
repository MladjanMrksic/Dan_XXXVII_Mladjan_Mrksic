﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Task_1
{
    class Program
    {
        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static CancellationToken ct = cts.Token;
        static readonly object l = new object();
        static List<int> routes = new List<int>();
        static Queue<int> bestRoutes;
        static readonly Random rnd = new Random();
        static string path = @".../.../Routes.txt";
        static StreamReader sr;
        static StreamWriter sw;
        static SemaphoreSlim semaphore = new SemaphoreSlim(2,2);
        static List<Truck> trucks = new List<Truck>();
        static void Main(string[] args)
        {
            Thread t1 = new Thread(ManagersJob);           
            t1.Start();
            Thread t2 = new Thread(RNG);
            Thread.Sleep(100);
            t2.Start();
            t1.Join();
            t2.Join();
            for (int i = 1; i < 11; i++)
            {
                Truck truck = new Truck(i);
                Thread t = new Thread(TruckLoading);
                t.Name = "Thread " + i;
                truck.T = t;                
                truck.T.Start(truck);
            }
            Console.ReadLine();            
        }
        public static void ManagersJob()
        {
            lock (l)
            {
                Console.WriteLine("Manager is waiting for routes.");
                cts.CancelAfter(3000);
                Monitor.Wait(l,3000);
                Console.WriteLine("Manager is chosing the best routes available.");
                LowestDigitsDivisibleByThree();
                foreach (var item in bestRoutes)
                    Console.WriteLine(item);
                Console.WriteLine("Routes are chosen, start loading the trucks.");
            }
        }
        public static void RNG()
        {
            lock (l)
            {
                sw = new StreamWriter(path);
                using (sw)
                {
                    Console.WriteLine("Generating routes.");
                    for (int i = 0; i < 1000; i++)
                    {
                        if (ct.IsCancellationRequested != true)
                        {
                            int temp = rnd.Next(1, 5000);
                            routes.Add(temp);
                            sw.WriteLine(temp);
                        }
                        else
                        {                            
                            break;
                        }
                    }
                }
                Monitor.Pulse(l);
            }
        }
        public static void LowestDigitsDivisibleByThree()
        {
            List<int> temp = new List<int>();
            sr = new StreamReader(path);
            using (sr)
            {                            
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (Convert.ToInt32(line)%3==0)
                        temp.Add(Convert.ToInt32(line));
                }
            } 
            temp.Sort();
            IEnumerable<int> distinctRoutes = temp.Distinct().Take(10);
            bestRoutes = new Queue<int>(distinctRoutes);            
        }
        public static void TruckLoading(object obj)
        {
            Truck truck = (Truck)obj;
            semaphore.Wait();          
            truck.TruckLoadTime = rnd.Next(500, 5000);
            Console.WriteLine("Truck " + truck.ID + " is loading. Estimated load time is " + truck.TruckLoadTime +".");
            Thread.Sleep(truck.TruckLoadTime);
            Console.WriteLine("Truck " + truck.ID + " loaded.");
            trucks.Add(truck);
            if (trucks.Count % 2 == 0)
                semaphore.Release(2);
            else
                Thread.Sleep(1);
            RouteAcquisition(truck);
        }
        public static void RouteAcquisition(Truck truck)
        {
            while (trucks.Count < 10)
                Thread.Sleep(1);
            truck.TruckRoute = bestRoutes.Dequeue();
            Console.WriteLine("Truck " + truck.ID + " acquired route " + truck.TruckRoute + ".");
            truck.TruckETA = rnd.Next(500, 5000);
            Thread.Sleep(15);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Truck " + truck.ID + " started the delivery. ETA " + truck.TruckETA + ".");
            Console.ForegroundColor = ConsoleColor.Gray;
            DestinationDelivery(truck);
        }
        public static void DestinationDelivery(Truck truck)
        {
            if (truck.TruckETA > 3000)
            {
                Thread.Sleep(3000);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Truck " + truck.ID + " failed the delivery. Returning to starting point.");
                Console.ForegroundColor = ConsoleColor.Gray;
                Thread.Sleep(3000);
                Console.WriteLine("Truck " + truck.ID + " returned to the starting point.");
            }
            else
            {
                Thread.Sleep(truck.TruckETA);
                int unloadTime = Convert.ToInt32(truck.TruckLoadTime / 1.5);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Truck " + truck.ID + " successfuly delivered the load. Unload time estimated at " + unloadTime + ".");
                Console.ForegroundColor = ConsoleColor.Gray;
                Thread.Sleep(unloadTime);
                Console.WriteLine("Truck " + truck.ID + " unloaded.");
            }
        }
    }
    public class Truck
    {
        public Thread T;
        public int ID;
        public int TruckRoute;
        public int TruckLoadTime;
        public int TruckETA;

        public Truck(int id)
        {            
            ID = id;
        }
    }
}
