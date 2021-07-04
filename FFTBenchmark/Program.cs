using System;
using System.Collections.Generic;
using System.Text;
using mkl;
using PetterPet.FFTSSharp;
using System.Diagnostics;

namespace FFTBenchmark
{
    class Program
    {
        static Random rand = new Random();
        static IntPtr descriptor = new IntPtr();
        static void initDesc(int len)
        {
            DFTI.DftiCreateDescriptor(ref descriptor, DFTI.SINGLE, DFTI.COMPLEX, 1, len);
            DFTI.DftiSetValue(descriptor, DFTI.PLACEMENT, DFTI.NOT_INPLACE); //Out of place FFT
            DFTI.DftiCommitDescriptor(descriptor); //Finalize the descriptor
        }
        public static void complexFFT(double[] input, double[] output)
        {
            DFTI.DftiComputeForward(descriptor, input, output); //Compute the Forward FFT
        }
        static double testMKL(double[] data, int iteration)
        {
            var clone = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
                clone[i] = data[i]; Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < iteration; i++)
                complexFFT(clone, clone);
            //printDoubleA(clone);
            watch.Stop();
            return watch.Elapsed.TotalMilliseconds;
        }
        static FFTS ffts;
        static double testFFTS(float[] data, int iteration)
        {
            var clone = new float[data.Length];
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < iteration; i++)
                ffts.Execute(data, clone);
            //printFloatA(clone);
            watch.Stop();
            return watch.Elapsed.TotalMilliseconds;
        }
        static void findDiff(float[] a1, double[] a2)
        {
            for (int i = 0; i < a2.Length; i++)
            {
                Console.WriteLine(a1[i] - a2[i]);
            }
        }
        public static double[] randDA(int length)
        {
            double[] output = new double[length];
            for (int i = 0; i < length; i++)
                output[i] = rand.NextDouble() * rand.Next(1, 100);
            return output;
        }
        static float[] toFloatArray(double[] array)
        {
            float[] fA = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
                fA[i] = (float)array[i];
            return fA;
        }
        static void printResult(double time, int testTimes, string name)
        {
            Console.WriteLine(string.Format("{0}: {1}ms ({2}ns)", name, time, time * 1000000));
            Console.WriteLine(string.Format("{0} Average: {1}ms ({2}ns)", name, time / testTimes, time / testTimes * 1000000));
        }
        static void printDoubleA(double[] array)
        {
            int len = array.Length;
            for (int i = 0; i < len - 1; i++)
            {
                Debug.Write(array[i] + ", ");
            }
            Debug.WriteLine(array[len - 1]);
        }
        static void printFloatA(float[] array)
        {
            int len = array.Length;
            for (int i = 0; i < len - 1; i++)
            {
                Debug.Write(array[i] + ", ");
            }
            Debug.WriteLine(array[len - 1]);
        }
        static void Countdown(int num)
        {
            for (int i = 0; i <= num; i++)
            {
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine(i);
            }
        }
        static void Main(string[] args)
        {
            FFTSManager.LoadAppropriateDll();
            int sPower = 8;
            int ePower = 15;
            int testTimes = 100000;
            for (int i = sPower; i <= ePower; i++)
            {
                int len = (int)Math.Pow(2, i);

                Console.WriteLine("---{0}---", len);

                double[] data = randDA(len);
                float[] dataF = toFloatArray(data);

                ffts = FFTS.Complex(FFTS.Forward, len / 2);
                initDesc(len);

                double FFTSTime = testFFTS(dataF, testTimes);
                double MKLTime = testMKL(data, testTimes);

                printResult(FFTSTime, testTimes, "FFTS");
                printResult(MKLTime, testTimes, "MKL");

                ffts.Dispose();
                DFTI.DftiFreeDescriptor(ref descriptor);

            }
            Console.WriteLine("Benchmarking done!");
            Console.ReadLine();
        }
    }
}
