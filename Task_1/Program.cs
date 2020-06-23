﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task_1
{
    class Program
    {        
        static readonly object l = new object();
        static List<int> routes = new List<int>();
        static Queue<int> bestRoutes;
        static readonly Random rnd = new Random();
        static string path = @".../.../Routes.txt";
        static StreamWriter sw = new StreamWriter(path,append:true);
        static SemaphoreSlim semaphore = new SemaphoreSlim(2,2);
        static List<Truck> trucks = new List<Truck>();
        static void Main(string[] args)
        {
            Thread t1 = new Thread(ManagersJob);
            Thread t2 = new Thread(RNG);
            t1.Start();
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
        public static void RNG()
        {
            lock (l)
            {
                using (sw)
                {
                    Console.WriteLine("Generating routes.");
                    for (int i = 0; i < 1000; i++)
                    {
                        int temp = rnd.Next(1,5000);
                        routes.Add(temp);
                        sw.WriteLine(temp);
                    }
                }
                Monitor.Pulse(l);
            }
        }
        public static void ManagersJob()
        {
            lock (l)
            {
                Console.WriteLine("Manager is waiting for routes.");
                Monitor.Wait(l, 3000);
                Console.WriteLine("Manager is chosing the best routes available.");
                LowestDigitsDivisibleByThree();
                foreach (var item in bestRoutes)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("Routes are chosen, start loading the trucks.");
            }
            
        }
        public static void LowestDigitsDivisibleByThree()
        {
            List<int> temp = new List<int>();
            foreach (var num in routes)
            {
                if (num%3==0)
                    temp.Add(num);
            }
            temp.Sort();
            IEnumerable<int> distinctRoutes = temp.Distinct().Take(10);
            bestRoutes = new Queue<int>(distinctRoutes);            
        }
        public static void TruckLoading(object obj)
        {
            Truck truck = (Truck)obj;
            semaphore.Wait();
            Console.WriteLine("Truck " + truck.ID + " ready for loading");            
            truck.TruckLoadTime = rnd.Next(500, 5000);
            Thread.Sleep(truck.TruckLoadTime);
            Console.WriteLine("Truck " + truck.ID + " loaded");
            trucks.Add(truck);
            if (trucks.Count % 2 == 0)
            {
                semaphore.Release(2);
            }
            else
            {
                Thread.Sleep(1);
            }
            Delivery(truck);
        }
        public static void Delivery(Truck truck)
        {
            while (trucks.Count < 10)
            {
                Thread.Sleep(1);
            }
            truck.TruckRoute = bestRoutes.Dequeue();
            Console.WriteLine("Truck " + truck.ID + " acquired route " + truck.TruckRoute);
            Thread.Sleep(15);
            truck.TruckETA = rnd.Next(500, 5000);
            Console.WriteLine("Truck " + truck.ID + " started the delivery.");
            if (truck.TruckETA > 3000)
            {
                Thread.Sleep(3000);
                Console.WriteLine("Truck " + truck.ID + " failed the delivery. Returning to depo.");
                Thread.Sleep(3000);
                Console.WriteLine("Truck " + truck.ID + " returned to the depo");                
            }
            else
            {
                Thread.Sleep(truck.TruckETA);
                Console.WriteLine("Truck " + truck.ID + " successfuly delivered the load.");
                double unloadTime = truck.TruckLoadTime / 1.5;
                Thread.Sleep(Convert.ToInt32(unloadTime));
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
