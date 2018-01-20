using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MControl.Caching.Demo
{
    public class DelayWorker
    {
        public static void Run()
        {
            var w = new DelayWorker();
            w.alive = true;
            w.DequeueWorker();
            //w.Start();
            Console.ReadLine();
        }

        #region Timer Sync

        int synchronized;
        Thread tt;
        private volatile bool alive;

        public bool Initialized
        {
            get { return alive; }
            //private set;
        }

        public void Start()
        {

            if (!this.Initialized)
            {
                alive = true;
                tt = new Thread(new ThreadStart(DequeueWorker));
                tt.IsBackground = true;
                tt.Start();
            }
        }

        public void Stop(int timeout = 0)
        {
            if (this.Initialized)
            {
                //this.Initialized = false;

                try
                {
                    alive = false;
                    // stop the thread
                    tt.Interrupt();

                    // Optionally block the caller
                    // by wait until the thread exits.
                    // If they leave the default timeout, 
                    // then they will not wait at all
                    tt.Join(timeout);
                }
                catch (ThreadInterruptedException tiex)
                {
                    /* Clean up. */
                    //OnError("Stop ThreadInterruptedException error ", tiex);
                }
                catch (Exception ex)
                {
                    //OnError("Stop error ", ex);
                }
            }
        }


        //DelayPerformance delayProcess=new DelayPerformance(10,10000,0.5f);
        //delayProcess.Delay(true); or delayProcess.Delay(false);
        //class DelayPerformance
        //{

        //    object dlocker = new object();
        //    int[] delays = new int[] { 10, 100, 500, 1000, 2000, 3000, 4000, 5000, 7000, 10000 };
        //    long current = 0;
        //    long counter = 0;
        //    long collector = 0;

        //    const int delaysLength = 10;
        //    const int maxSycle = 10;

        //    public void Delay(bool isActive)
        //    {
        //        long curDelay = 0;
        //        lock (dlocker)
        //        {
        //            curDelay = delays[current];
        //        }

        //        long sumDelay = curDelay * maxSycle;
        //        float midDelay = (sumDelay / 2);

        //        long icounter = Interlocked.Read(ref counter);
        //        long icollector = Interlocked.Read(ref collector);
        //        long icurrent = Interlocked.Read(ref current);


        //        if (icounter >= maxSycle)
        //        {
        //            Console.WriteLine("sum-current:{0}, counter:{1}, collector:{2}, delay:{3}", icurrent, icounter, icollector, curDelay);

        //            if (icollector >= sumDelay)//(delays[current]) * maxSycle)
        //            {
        //                if ((icurrent) > 0)
        //                    Interlocked.Decrement(ref current);//  current--;
        //            }
        //            if (icollector < sumDelay)//(delays[current]) * maxSycle)
        //            {
        //                if (icollector > midDelay)//(((delays[current]) * maxSycle) / 2))
        //                {
        //                    //stay current
        //                }
        //                else if ((icurrent + 1) < delaysLength)
        //                    Interlocked.Increment(ref current); //current++;
        //            }
        //            Interlocked.Exchange(ref counter, 0);
        //            Interlocked.Exchange(ref collector, 0);
        //        }
        //        else if (isActive)
        //        {
        //            Interlocked.Add(ref collector, curDelay);// collector += delays[current];
        //            Interlocked.Increment(ref counter);
        //        }
        //        else
        //        {
        //            Interlocked.Increment(ref counter);
        //        }

        //        lock (dlocker)
        //        {
        //            curDelay = delays[current];
        //        }
        //        //Thread.Sleep(curDelay);


        //        Console.WriteLine("end-current:{0}, counter:{1}, collector:{2}, delay:{3}", icurrent, icounter, icollector, curDelay);

        //    }
        //}

        
        static void GoOn()
        {
            string entry = Console.ReadLine();
            if (entry == "q")
            {
                Environment.Exit(1);
            }
        }
        private void DequeueWorker()
        {
            Console.WriteLine("Debuger-AsyncTasker.DequeueWorker...");
            bool iActive = true;
            int i = 0;
            while (alive)
            {
                object item = null;
                try
                {
                    if (0 == Interlocked.Exchange(ref synchronized, 1))
                    {
                        if (iActive)
                            delayProcess.Delay(true);
                        else
                            delayProcess.Delay(false);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref synchronized, 0);
                }

                var ch = Console.ReadKey();
                iActive = ch.KeyChar == 't';

                //Console.WriteLine("current:{0}, counter:{1}, collector:{2}, delay:{3}", current, counter , collector, delays[current]);

                //i++;
                //if(endPart)
                //{
                //    endPart = false;
                //    i = 0;
                //    //GoOn();
                //    Console.WriteLine("End part, entr t or f for Active");
                //    var ch= Console.ReadKey();
                //    iActive = ch.KeyChar == 't';
                //    //Thread.Sleep(1000);
                //}
            }

        }
        #endregion

        #region Delay Performance

        DelayPerformance delayProcess = new DelayPerformance(10, 10000, 0.5f);
        //delayProcess.Delay(true); or delayProcess.Delay(false);
        class DelayPerformance
        {
            private PerformanceCounter CPUCounter;
            public DelayPerformance(int minDelay, int maxDelay, float maxCpu)
            {
                if(minDelay<10)
                {
                    throw new ArgumentOutOfRangeException("minDelay should be more them 10 ms");
                }
                if (maxDelay > 60000)
                {
                    throw new ArgumentOutOfRangeException("maxDelay should be maximum 60000 ms");
                }
                if (maxDelay <= minDelay)
                {
                    throw new ArgumentException("maxDelay should be greater then minDelay");
                }

                this.maxCpu = maxCpu;
                this.minDelay = minDelay;
                this.maxDelay = maxDelay;
                this.current = maxDelay;

                this.highStPower = minDelay;
                this.midStPower = Math.Min(minDelay * 10 , 100);
                this.lowStPower = Math.Max(midStPower * 2, Math.Min(maxDelay/10, 1000));

                this.CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                Console.WriteLine("DelayPerformance start - Thread:{0}, minDelay:{1}, maxDelay:{2}, highStPower:{3}, midStPower:{4},lowStPower:{5}", Thread.CurrentThread.ManagedThreadId, minDelay, maxDelay, highStPower, midStPower, lowStPower);

            }

            const int maxSycle = 10;
            const int cpuStep = 100;

            int highStPower = 10;
            int midStPower = 100;
            int lowStPower = 1000;
            
            int maxDelay = 60000;
            int minDelay = 10;
            float maxCpu = 50.0f;

            const float lowFactor = 0.2f;
            const float midFactor = 0.5f;
            const float highFactor = 0.9f;

            long counter = 0;
            long activeCounter = 0;
            long collector = 0;
            long current = 0;
            long cpuCollector;

            public void Delay(bool isActive)
            {

                long icounter = Interlocked.Read(ref counter);
                float cpuCounter = CPUCounter.NextValue();
                long icurrent = Interlocked.Read(ref current);

                if (icounter >= maxSycle)
                {
                    long iactiveCounter = Interlocked.Read(ref activeCounter);
                    long icpuCollector = Interlocked.Read(ref cpuCollector);
                    long icollector = Interlocked.Read(ref collector);
                    long sumDelay = icurrent * maxSycle;
                    //float midDelay = (sumDelay / 2);
                    //int curDelay = (int)icurrent;

                    int avgDelay = iactiveCounter > 0 ? ((int) (icollector / iactiveCounter)): 0;

                    float fcpuCollector = (float)(icpuCollector / (float)100);
                    float cpuAvg = icounter > 0 ? fcpuCollector / icounter : 0;

                    long midHighFactor = (maxDelay - minDelay) / 2;
                    long midLowFactor = (maxDelay - minDelay) / 3;

                    int step = lowStPower;

                    if (icurrent > ((maxDelay - minDelay) / 2))
                        step = lowStPower;
                    else if(icurrent <= midStPower*2)
                        step = highStPower;
                    else
                        step = midStPower;

                    Console.WriteLine("sum- Thread:{0}, current:{1}, counter:{2}, collector:{3}, cpuCounter:{4},cpuAvg:{5}, step:{6}, avgDelay:{7} ", Thread.CurrentThread.ManagedThreadId, icurrent, icounter, icollector, cpuCounter, cpuAvg, step, avgDelay);

                    if (icollector >= sumDelay)//need more power
                    {
                        if (avgDelay > minDelay)//allow get more power
                        {
                            if (avgDelay <= cpuStep)//fast step will take more power
                            {
                                if (cpuAvg < maxCpu && icurrent - step >= minDelay)//if current avg cpu is allow more power
                                {
                                    Interlocked.Add(ref current, step * -1);// add power
                                    Console.WriteLine("add power cpuStep");
                                }
                            }
                            else if (icurrent - step >= minDelay)
                            {
                                Interlocked.Add(ref current, step * -1); // add power
                                Console.WriteLine("add power no cpuStep");
                            }
                        }
                    }
                    if (icollector < sumDelay)//release power or stay same using
                    {
                        if ((icurrent + step) > maxDelay)//can release power
                        {
                            Interlocked.Exchange(ref current, maxDelay); //release max power
                            Console.WriteLine("release max power");
                        }
                        else if (icollector > (sumDelay * highFactor))//if using more then mid power
                        {
                            if (avgDelay <= cpuStep)//fast step will take more power
                            {
                                if (cpuAvg < maxCpu && icurrent - step >= minDelay)//if current avg cpu is allow more power
                                {
                                    Interlocked.Add(ref current, step * -1);// add power
                                    Console.WriteLine("add power cpuStep highFactor");
                                }
                            }
                            else if (icurrent - step >= minDelay)
                            {
                                Interlocked.Add(ref current, step * -1); // add power
                                Console.WriteLine("add power no cpuStep highFactor");
                            }
                        }
                        else if (icollector > (sumDelay * midFactor))//if using more then mid power
                        {
                            //stay current
                            Console.WriteLine("stay current");
                        }
                        else if ((icurrent + step) < maxDelay)//can release power
                        {
                            Interlocked.Add(ref current, step); //release power
                            Console.WriteLine("release power");
                        }
                    }

                    Interlocked.Exchange(ref activeCounter, 0);
                    Interlocked.Exchange(ref counter, 0);
                    Interlocked.Exchange(ref collector, 0);
                    Interlocked.Exchange(ref cpuCollector, 0);
                }
                else if (isActive)
                {
                    Interlocked.Add(ref collector, icurrent);
                    Interlocked.Increment(ref counter);
                    Interlocked.Increment(ref activeCounter);
                    Interlocked.Add(ref cpuCollector, (long)cpuCounter * 100);
                }
                else
                {
                    Interlocked.Increment(ref counter);
                    Interlocked.Add(ref cpuCollector, (long)cpuCounter * 100);
                }

                int curDelay = (int)Interlocked.Read(ref current);

                //Thread.Sleep(curDelay);

                Console.WriteLine("current:{0}, counter:{1}, delay:{2}, cpuCounter:{3}", icurrent, icounter, curDelay, cpuCounter);

            }

            //public void Delay(bool isActive)
            //{
            //    int curDelay = 0;
            //    lock (dlocker)
            //    {
            //        curDelay = delays[current];
            //    }

            //    long sumDelay = curDelay * maxSycle;
            //    float midDelay = (sumDelay / 2);

            //    long icounter = Interlocked.Read(ref counter);
            //    long icollector = Interlocked.Read(ref collector);
            //    long icurrent = Interlocked.Read(ref current);


            //    if (icounter >= maxSycle)
            //    {
            //        Console.WriteLine("sum- Thread:{0}, current:{1}, counter:{2}, collector:{3}, delay:{4}", Thread.CurrentThread.ManagedThreadId ,icurrent, icounter, icollector, curDelay);

            //        if (icollector >= sumDelay)//(delays[current]) * maxSycle)
            //        {
            //            if ((icurrent) > 0)
            //                Interlocked.Decrement(ref current);//  current--;
            //        }
            //        if (icollector < sumDelay)//(delays[current]) * maxSycle)
            //        {
            //            if (icollector > midDelay)//(((delays[current]) * maxSycle) / 2))
            //            {
            //                //stay current
            //            }
            //            else if ((icurrent + 1) < delaysLength)
            //                Interlocked.Increment(ref current); //current++;
            //        }
            //        Interlocked.Exchange(ref counter, 0);
            //        Interlocked.Exchange(ref collector, 0);
            //    }
            //    else if (isActive)
            //    {
            //        Interlocked.Add(ref collector, curDelay);// collector += delays[current];
            //        Interlocked.Increment(ref counter);
            //    }
            //    else
            //    {
            //        Interlocked.Increment(ref counter);
            //    }

            //    lock (dlocker)
            //    {
            //        curDelay = delays[current];
            //    }
            //    Thread.Sleep(curDelay);

            //    //Console.WriteLine("end-current:{0}, counter:{1}, collector:{2}, delay:{3}", icurrent, icounter, icollector, curDelay);

            //}
        }

        #endregion

    }
}
