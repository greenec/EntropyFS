using EntropyFS.Models;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace EntropyFS
{
    public class Compressor
    {
        public static void CompressFile(HashAlgorithm algo, byte blockSize, string inputFilePath, string outputFilePath)
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

                var targetHash = targetBlock.ComputeHash(algo);

                // hash it out
                byte collisionIdx = 0;
                var workingBlock = new Block(blockSize);

                var iterations = BigInteger.Pow(256, blockSize);
                for (BigInteger i = 0; i < iterations; i++)
                {
                    var hash = workingBlock.ComputeHash(algo);
                    if (targetHash.SequenceEqual(hash))
                    {
                        if (targetBlock.Data.SequenceEqual(workingBlock.Data))
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

                    workingBlock.Increment();
                }
            }

            outputFile.Close();
        }
    }
}
