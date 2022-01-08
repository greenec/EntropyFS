using EntropyFS.Models;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace EntropyFS
{
    public class Compressor
    {
        public static void CompressFile(byte blockSize, string inputFilePath, string outputFilePath)
        {
            try
            {
                File.Delete(outputFilePath);
            }
            catch { }

            var outputFile = File.OpenWrite(outputFilePath);

            outputFile.WriteByte(blockSize);

            // read through the input file
            
            using (FileStream fs = new FileStream(inputFilePath, FileMode.Open))
            {
                while (fs.Position < fs.Length)
                {
                    var targetBlock = new Block(blockSize);

                    fs.Read(targetBlock.Data, 0, blockSize);

                    var targetHash = targetBlock.ComputeHash();

                    // hash it out
                    ulong collisionIdx = 0;
                    var workingBlock = new Block(blockSize);

                    var iterations = BigInteger.Pow(256, blockSize);
                    for (BigInteger i = 0; i < iterations; i++)
                    {
                        ulong hash = workingBlock.ComputeHash();
                        if (targetHash == hash)
                        {
                            if (targetBlock.Data.SequenceEqual(workingBlock.Data))
                            {
                                var aHash = BitConverter.GetBytes(hash);
                                outputFile.Write(aHash, 0, aHash.Length);

                                // write collision index to file (max 256 for the byte type, hopefully that's not a problem)
                                var aCollisionIdx = BitConverter.GetBytes(collisionIdx);
                                outputFile.Write(aCollisionIdx, 0, aCollisionIdx.Length);

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
            }

            outputFile.Close();
        }
    }
}
