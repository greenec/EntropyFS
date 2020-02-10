using HashDepot;
using System;
using System.Diagnostics;
using System.Numerics;

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
                Benchmark(1);
                Benchmark(2);
                Benchmark(3);
                Benchmark(4);
                Benchmark(5);
                Benchmark(6);
                Benchmark(7);
                Benchmark(8);
            }

            Console.WriteLine();
            
            // determine the block size
            Console.Write("What block size would you like to use? (enter a number in bytes) ");
            var sBlockSize = Console.ReadLine();
            byte.TryParse(sBlockSize, out byte blockSize);

            Console.WriteLine();

            // run a benchmark for the chosen system
            Benchmark(blockSize);

            Console.WriteLine();

            var inputFile = "c:\\users\\connor\\documents\\github\\entropyfs\\entropyfs\\entropycli\\input.txt";
            var outputFile = "c:\\users\\connor\\documents\\github\\entropyfs\\entropyfs\\entropycli\\output.efs";

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            EntropyFS.Compressor.CompressFile(blockSize, inputFile, outputFile);
            stopwatch.Stop();

            Console.WriteLine($"Compression complete in {stopwatch.Elapsed}");

            stopwatch.Restart();
            var result = EntropyFS.Decompressor.DecompressFile(outputFile);
            stopwatch.Stop();

            Console.WriteLine($"Decompression complete in {stopwatch.Elapsed}");

            Console.WriteLine(result);
        }


        private static void Benchmark(byte blockSize)
        {
            byte[] block = new byte[blockSize];

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            int testCycles = 1000000;
            for (int i = 0; i < testCycles; i++)
            {
                XXHash.Hash64(block);
            }
            stopwatch.Stop();

            long hashesPerSecond = testCycles * 1000 / stopwatch.ElapsedMilliseconds;

            // compute the storage costs
            var maxCyclesPerBlock = BigInteger.Pow(2, blockSize * 8);
            var avgCyclesPerBlock = BigInteger.Divide(maxCyclesPerBlock, 2);

            var avgSecondsPerBlock = BigInteger.Divide(avgCyclesPerBlock, hashesPerSecond);

            // each block requires an additional 8 bytes to store the collision index
            double hashSize = sizeof(ulong);
            double sizeChange = ((hashSize / blockSize) + sizeof(ulong)) * 100.0;

            var sRatio = (sizeChange > 1 ? "+" : "-") + Math.Round(sizeChange, 2) + "%";

            try
            {
                var timePerBlock = TimeSpan.FromSeconds((double)avgSecondsPerBlock);
                Console.WriteLine($"xxHash - {blockSize} byte blocks. {hashesPerSecond.ToString("N0")} hashes per second. Avg block compute time: {timePerBlock} Size Change: {sRatio}");
            }
            catch (OverflowException)
            {
                // block size is too large for this system
                return;
            }
        }
    }
}
