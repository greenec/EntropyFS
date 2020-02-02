using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace EntropyFS
{
    public class Compressor
    {
        public static void CompressFile(HashAlgorithm algo, int blockSize, string inputFilePath, string outputFilePath)
        {
            var inputBytes = File.ReadAllBytes(inputFilePath);

            try
            {
                File.Delete(outputFilePath);
            }
            catch { }

            var outputFile = File.OpenWrite(outputFilePath);

            // read through the input file
            int idx = 0;
            while (idx < inputBytes.Length)
            {
                var block = new byte[blockSize];

                // load the block for hashing
                for (int i = 0; i < blockSize; i++)
                {
                    block[i] = inputBytes[idx++];

                    if (idx >= inputBytes.Length)
                    {
                        break;
                    }
                }

                var targetHash = algo.ComputeHash(block);

                // hash it out
                byte collisionIdx = 0;
                var workingBlock = new byte[blockSize];

                var iterations = BigInteger.Pow(256, blockSize);
                for (BigInteger i = 0; i < iterations; i++)
                {
                    var hash = algo.ComputeHash(workingBlock);
                    if (targetHash.SequenceEqual(hash))
                    {
                        if (block.SequenceEqual(workingBlock))
                        {
                            outputFile.Write(hash, 0, hash.Length);

                            // write collision index to file (max 256 for the byte type, hopefully that's not a problem)
                            outputFile.WriteByte(collisionIdx);

                            break;
                        }
                        else
                        {
                            collisionIdx++;
                        }
                    }

                    IncrementBlock(ref workingBlock);
                }
            }

            outputFile.Close();
        }

        private static void IncrementBlock(ref byte[] block, int idx = 0)
        {
            if (idx >= block.Length)
            {
                return;
            }

            block[idx]++;

            // recursively perform the carryover
            if (block[idx] == 0)
            {
                IncrementBlock(ref block, idx + 1);
            }
        }
    }
}
