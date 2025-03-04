namespace Engine.Junk.Confidence
{
    public sealed class ConfidenceRecord
    {
        public int Change { get; }

        public string Reason { get; }

        public ConfidenceRecord(int change, string reason)
        {
            Change = change;
            Reason = reason;
        }

        public ConfidenceRecord(int change)
        {
            Change = change;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConfidenceRecord casted))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return casted.Change == Change && casted.Reason == Reason;
        }

        public override int GetHashCode() => Change.GetHashCode() ^ Reason.GetHashCode();
    }
}