using HashDepot;

namespace EntropyFS.Models
{
    public class Block
    {
        public byte[] Data;

        public ulong? Hash { get; set; }

        public ulong CollisionIndex { get; set; }

        public ulong CollisionsDetected { get; set; } = 0;

        public Block(int blockSize, ulong? hash = null, ulong collisionIdx = 0)
        {
            Data = new byte[blockSize];
            Hash = hash;
            CollisionIndex = collisionIdx;
        }

        public ulong ComputeHash()
        {
            return XXHash.Hash64(Data);
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
