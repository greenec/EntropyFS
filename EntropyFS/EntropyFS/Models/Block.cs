using System.Security.Cryptography;

namespace EntropyFS.Models
{
    public class Block
    {
        public byte[] Data;

        public byte[] Hash { get; set; }

        public int CollisionIndex { get; set; }

        public int CollisionsDetected { get; set; } = 0;

        public Block(int blockSize, byte[] hash = null, int collisionIdx = 0)
        {
            Data = new byte[blockSize];
            Hash = hash;
            CollisionIndex = collisionIdx;
        }

        public byte[] ComputeHash(HashAlgorithm algo)
        {
            Hash = algo.ComputeHash(Data);
            return Hash;
        }

        public void Increment(int idx = 0)
        {
            if (idx >= Data.Length)
            {
                return;
            }

            Data[idx]++;

            // recursively perform the carryover
            if (Data[idx] == 0)
            {
                Increment(idx + 1);
            }
        }
    }
}
