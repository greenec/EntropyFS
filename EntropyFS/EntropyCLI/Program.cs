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

            Console.Write("Would you like to run benchmarks for your system? (y/n): ");

            string response = Console.ReadLine().Trim().ToLower();
            if (response == "y")
            {
                Console.WriteLine("Running benchmarks to determine which block size is ideal for you...\n");
                Benchmark(MD5.Create(), "MD5", 1);
                Benchmark(MD5.Create(), "MD5", 2);
                Benchmark(MD5.Create(), "MD5", 3);
                Benchmark(MD5.Create(), "MD5", 4);
                Benchmark(MD5.Create(), "MD5", 5);
                Benchmark(MD5.Create(), "MD5", 6);
                Benchmark(MD5.Create(), "MD5", 7);
                Benchmark(MD5.Create(), "MD5", 8);
            }

            Console.WriteLine();
            
            // determine the block size
            Console.Write("What block size would you like to use? (enter a number in bytes) ");
            var sBlockSize = Console.ReadLine();
            byte.TryParse(sBlockSize, out byte blockSize);

            Console.WriteLine();

            // run a benchmark for the chosen system
            Benchmark(MD5.Create(), "MD5", blockSize);

            Console.WriteLine();

            var inputFile = "c:\\users\\connor\\documents\\github\\entropyfs\\entropyfs\\entropycli\\input.txt";
            var outputFile = "c:\\users\\connor\\documents\\github\\entropyfs\\entropyfs\\entropycli\\output.efs";

            EntropyFS.Compressor.CompressFile(MD5.Create(), blockSize, inputFile, outputFile);

            var result = EntropyFS.Decompressor.DecompressFile(MD5.Create(), outputFile);

            Console.WriteLine(result);
        }


        private static void Benchmark(HashAlgorithm algo, string sAlgorithm, byte blockSize)
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

            double sizeChange = algo.HashSize / 8.0 / (blockSize + 1.0) * 100.0;

            var sRatio = (sizeChange > 1 ? "+" : "-") + Math.Round(sizeChange, 2) + "%";

            try
            {
                var timePerBlock = TimeSpan.FromSeconds((double)avgSecondsPerBlock);
                Console.WriteLine($"{sAlgorithm} - {blockSize} byte blocks. {hashesPerSecond.ToString("N0")} hashes per second. Avg block compute time: {timePerBlock} Size Change: {sRatio}");
            }
            catch (OverflowException)
            {
                // block size is too large for this system
                return;
            }
        }
    }
}
