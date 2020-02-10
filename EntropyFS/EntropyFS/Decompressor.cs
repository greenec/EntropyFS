using EntropyFS.Models;
using System;
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
        public static string DecompressFile(string inputFilePath)
        {
            var blocks = new List<Block>();

            var inputBytes = File.ReadAllBytes(inputFilePath);

            // read through the input file
            int fileIdx = 0;

            byte blockSize = inputBytes.ElementAt(fileIdx++);

            while (fileIdx < inputBytes.Length)
            {
                ulong hash = BitConverter.ToUInt64(inputBytes.Skip(fileIdx).Take(sizeof(ulong)).ToArray(), 0);

                fileIdx += sizeof(ulong);

                ulong collisionIdx = BitConverter.ToUInt64(inputBytes.Skip(fileIdx).Take(sizeof(ulong)).ToArray(), 0);

                fileIdx += sizeof(ulong);

                var block = new Block(blockSize, hash, collisionIdx);

                blocks.Add(block);
            }

            // hash it out
            var workingBlock = new Block(blockSize);

            var iterations = BigInteger.Pow(256, blockSize);
            for (BigInteger i = 0; i < iterations; i++)
            {
                var hash = workingBlock.ComputeHash();

                var matches = blocks.Where(b => hash == b.Hash);
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
