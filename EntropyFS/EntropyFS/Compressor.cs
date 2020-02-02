using EntropyFS.Models;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EntropyFS
{
    public class Compressor
    {
        private int CollisionIdx;

        public void CompressFile(HashAlgorithm algo, byte blockSize, string inputFilePath, string outputFilePath)
        {
            var inputBytes = File.ReadAllBytes(inputFilePath);

            try
            {
                File.Delete(outputFilePath);
            }
            catch { }

            var outputFile = File.OpenWrite(outputFilePath);

            outputFile.WriteByte(blockSize);

            // read through the input file
            int fileIdx = 0;
            while (fileIdx < inputBytes.Length)
            {
                var targetBlock = new Block(blockSize);

                // load the block for hashing
                for (int i = 0; i < blockSize; i++)
                {
                    targetBlock.Data[i] = inputBytes[fileIdx];

                    fileIdx++;

                    if (fileIdx >= inputBytes.Length)
                    {
                        break;
                    }
                }

                // compute block here
                CollisionIdx = 0;

                var iterations = BigInteger.Pow(256, blockSize);

                var task1 = ComputeBlock(MD5.Create(), targetBlock, blockSize, 0, iterations / 6);
                var task2 = ComputeBlock(MD5.Create(), targetBlock, blockSize, iterations / 6, iterations / 3);
                var task3 = ComputeBlock(MD5.Create(), targetBlock, blockSize, iterations / 3, iterations / 2);
                var task4 = ComputeBlock(MD5.Create(), targetBlock, blockSize, iterations / 2, iterations * 2 / 3);
                var task5 = ComputeBlock(MD5.Create(), targetBlock, blockSize, iterations * 2 / 3, iterations * 5 / 6);
                var task6 = ComputeBlock(MD5.Create(), targetBlock, blockSize, iterations * 5 / 6, iterations);

                var results = Task.WhenAll(task1, task2).Result;
                var result = results.FirstOrDefault(r => r != null);

                outputFile.Write(result.Hash, 0, result.Hash.Length);

                // write collision index to file (max 256 for the byte type, hopefully that's not a problem)
                outputFile.WriteByte((byte)CollisionIdx);
            }

            outputFile.Close();
        }

        private Task<Block> ComputeBlock(HashAlgorithm algo, Block targetBlock, byte blockSize, BigInteger startIdx, BigInteger stopIdx)
        {
            var targetHash = targetBlock.ComputeHash(algo);

            // hash it out
            var workingBlock = new Block(blockSize);

            return Task.Run(() =>
            {
                // initialize the working block for this task
                for (BigInteger i = 0; i < startIdx; i++)
                {
                    workingBlock.Increment();
                }

                for (BigInteger i = startIdx; i < stopIdx; i++)
                {
                    var hash = workingBlock.ComputeHash(algo);
                    if (targetHash.SequenceEqual(hash))
                    {
                        if (targetBlock.Data.SequenceEqual(workingBlock.Data))
                        {
                            return workingBlock;
                        }
                        else
                        {
                            System.Threading.Interlocked.Increment(ref CollisionIdx);
                        }
                    }

                    workingBlock.Increment();
                }

                return null;
            });
        }
    }
}
