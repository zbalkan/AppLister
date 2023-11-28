namespace InventoryEngine.Shared
{
    internal static partial class Sift4
    {
        private class OffsetPair
        {
            internal int C1 { get; set; }
            internal int C2 { get; set; }
            internal bool IsTransposition { get; set; }

            internal OffsetPair(int c1, int c2)
            {
                C1 = c1;
                C2 = c2;
                IsTransposition = false;
            }
        }
    }
}