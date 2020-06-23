using System;
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
        static SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        static void Main(string[] args)
        {

        }
        public static void RNG()
        {
            lock (l)
            {
                using (sw)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        int temp = rnd.Next(1 - 5000);
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
                Monitor.Wait(l, 3000);
                LowestDigitsDivisibleByThree();
                Console.WriteLine("Routes are chosen, start loading the trucks");
                Monitor.Pulse(l);
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
            bestRoutes = new Queue<int>(temp.GetRange(10,temp.Count-10));            
        }
        public static void TruckLoading(Truck truck)
        {
            Console.WriteLine("Truck " + truck.ID + " ready for loading");
            semaphore.Wait();
            int loadTime = rnd.Next(500, 5000);
            truck.TruckLoadTime = loadTime;
            Thread.Sleep(loadTime);
            Console.WriteLine("Truck " + truck.ID + " loaded");
            truck.TruckRoute = bestRoutes.Dequeue();
            Console.WriteLine("Truck " + truck.ID + " acquired route");
            semaphore.Release();
            int ETA = rnd.Next(500 - 5000);
            Console.WriteLine("Truck " + truck.ID + " started the delivery.");
            if (ETA > 300)
            {
                Thread.Sleep(3000);
                Console.WriteLine("Truck " + truck.ID + " failed the delivery. Returning to unload.");
                Thread.Sleep(3000);
                Console.WriteLine("Truck " + truck.ID + " returned to the depo and is about to unload.");
                double unloadTime = truck.TruckLoadTime / 1.5;
                Thread.Sleep(Convert.ToInt32(unloadTime));
            }
            else
            {
                Thread.Sleep(ETA);
                Console.WriteLine("Truck " + truck.ID + " successfuly delivered the load.");
            }
        }
    }
    public class Truck
    {
        public Thread T;
        public int ID;
        public int TruckRoute;
        public int TruckLoadTime;

        public Truck(Thread thread, int id)
        {
            T = thread;
            ID = id;
        }
    }
}
