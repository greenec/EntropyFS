using EntropyFS.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace EntropyFS
{
    public class Decompressor
    {
        public static string DecompressFile(HashAlgorithm algo, string inputFilePath)
        {
            var blocks = new List<Block>();

            var inputBytes = File.ReadAllBytes(inputFilePath);

            // read through the input file
            int fileIdx = 0;

            byte blockSize = inputBytes.ElementAt(fileIdx++);

            while (fileIdx < inputBytes.Length)
            {
                var hash = inputBytes.Skip(fileIdx).Take(algo.HashSize / 8).ToArray();

                fileIdx += algo.HashSize / 8;

                var collisionIdx = inputBytes.ElementAt(fileIdx);

                fileIdx++;

                var block = new Block(blockSize, hash, collisionIdx);

                blocks.Add(block);
            }

            // hash it out
            var workingBlock = new Block(blockSize);

            var iterations = BigInteger.Pow(256, blockSize);
            for (BigInteger i = 0; i < iterations; i++)
            {
                var hash = workingBlock.ComputeHash(algo);

                var matches = blocks.Where(b => hash.SequenceEqual(b.Hash));
                if (matches.Any() == false)
                {
                    workingBlock.Increment();
                    continue;
                }

                foreach (var match in matches)
                {
                    if (match.CollisionIndex == match.CollisionsDetected)
                    {
                        for (int j = 0; j < blockSize; j++)
                        {
                            match.Data[j] = workingBlock.Data[j];
                        }
                    }

                    match.CollisionsDetected++;
                }

                workingBlock.Increment();
            }

            string result;
            using (var ms = new MemoryStream())
            {
                foreach (var block in blocks)
                {
                    ms.Write(block.Data, 0, block.Data.Length);
                }

                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            return result;
        }
    }
}
