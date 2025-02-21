﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InventoryEngine.Junk.Confidence
{
    public sealed class ConfidenceCollection : IEnumerable<ConfidenceRecord>
    {
        internal IEnumerable<ConfidenceRecord> ConfidenceParts => _items;
        public bool IsEmpty => _items.Count == 0;
        private readonly List<ConfidenceRecord> _items = new List<ConfidenceRecord>();

        internal ConfidenceCollection()
        {
        }

        internal ConfidenceLevel GetConfidence()
        {
            if (_items.Count < 1)
            {
                return ConfidenceLevel.Unknown;
            }

            var result = GetRawConfidence();

            if (result < 0)
            {
                return ConfidenceLevel.Bad;
            }

            if (result < 2)
            {
                return ConfidenceLevel.Questionable;
            }

            if (result < 5)
            {
                return ConfidenceLevel.Good;
            }

            return ConfidenceLevel.VeryGood;
        }

        public IEnumerator<ConfidenceRecord> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

        // Returns a number representing the confidence. 0 is a mid-point.
        public int GetRawConfidence()
        {
            var result = 0;
            _items.ForEach(x => result += x.Change);
            return result;
        }

        internal void Add(int value) => _items.Add(new ConfidenceRecord(value));

        internal void Add(ConfidenceRecord value) => _items.Add(value);

        internal void AddRange(IEnumerable<ConfidenceRecord> values) => _items.AddRange(values.Where(x => !_items.Contains(x)));
    }
}