using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace EntropyCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to EntropyCLI!\n");

            /*
            Console.WriteLine("Running benchmarks to determine which block size is ideal for you...\n");
            Benchmark(MD5.Create(), "MD5", 1, "+ 1600%");
            Benchmark(MD5.Create(), "MD5", 2, "+ 800%");
            Benchmark(MD5.Create(), "MD5", 3, "+ 533%");
            Benchmark(MD5.Create(), "MD5", 4, "+ 400%");
            Benchmark(MD5.Create(), "MD5", 5, "+ 320%");
            Benchmark(MD5.Create(), "MD5", 6, "+ 266%");
            Benchmark(MD5.Create(), "MD5", 7, "+ 228%");
            Benchmark(MD5.Create(), "MD5", 8, "+ 200%");
            */

            var inputFile = "c:\\users\\connor\\documents\\github\\entropyfs\\entropyfs\\entropycli\\input.txt";
            var outputFile = "c:\\users\\connor\\documents\\github\\entropyfs\\entropyfs\\entropycli\\output.efs";

            EntropyFS.Compressor.CompressFile(MD5.Create(), 3, inputFile, outputFile);
        }


        private static void Benchmark(HashAlgorithm algo, string sAlgorithm, int blockSize, string sRatio)
        {
            byte[] block = new byte[blockSize];

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            int testCycles = 1000000;

            for (int i = 0; i < testCycles; i++)
            {
                algo.ComputeHash(block);
            }
            stopwatch.Stop();

            long hashesPerSecond = testCycles * 1000 / stopwatch.ElapsedMilliseconds;

            // compute the storage costs
            var maxCyclesPerBlock = BigInteger.Pow(2, blockSize * 8);
            var avgCyclesPerBlock = BigInteger.Divide(maxCyclesPerBlock, 2);

            var avgSecondsPerBlock = BigInteger.Divide(avgCyclesPerBlock, hashesPerSecond);

            try
            {
                var timePerBlock = TimeSpan.FromSeconds((double)avgSecondsPerBlock);
                Console.WriteLine($"{sAlgorithm} - {blockSize} byte blocks. {hashesPerSecond.ToString("N0")} hashes per second. Avg block compute time: {timePerBlock}. Size Change: {sRatio}");
            }
            catch (OverflowException)
            {
                // block size is too large for this system
                return;
            }
        }
    }
}
