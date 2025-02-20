using System;
using System.Collections.Generic;

namespace InventoryEngine.Shared
{
    /// <summary>
    ///     Algorithm by Siderite https://siderite.blogspot.com/2014/11/super-fast-and-accurate-string-distance.html#at2217133354
    /// </summary>
    internal static partial class Sift4
    {
        /// <summary>
        ///     Static distance algorithm working on strings, computing transpositions as well as
        ///     stopping when maxDistance was reached.
        /// </summary>
        /// <param name="s1"> </param>
        /// <param name="s2"> </param>
        /// <param name="maxOffset"> </param>
        /// <param name="maxDistance"> </param>
        /// <returns> </returns>
        internal static double CommonDistance(string s1, string s2, int maxOffset, int maxDistance = 0)
        {
            var l1 = (s1?.Length) ?? 0;
            var l2 = (s2?.Length) ?? 0;

            if (l1 == 0)
            {
                return l2;
            }

            if (l2 == 0)
            {
                return l1;
            }

            var c1 = 0;  //cursor for string 1
            var c2 = 0;  //cursor for string 2
            var lcss = 0;  //largest common subsequence
            var localCs = 0; //local common substring
            var trans = 0;  //number of transpositions ('axb' vs 'xba')
            var offsetArr = new LinkedList<OffsetPair>();  //offset pair array, for computing the transpositions

            while ((c1 < l1) && (c2 < l2))
            {
                if (s1[c1] == s2[c2])
                {
                    localCs++;
                    var isTransposition = false;
                    var op = offsetArr.First;
                    while (op != null)
                    {  //see if current match is a transposition
                        var ofs = op.Value;
                        if (c1 <= ofs.C1 || c2 <= ofs.C2)
                        {
                            // when two matches cross, the one considered a transposition is the one
                            // with the largest difference in offsets
                            isTransposition = Math.Abs(c2 - c1) >= Math.Abs(ofs.C2 - ofs.C1);
                            if (isTransposition)
                            {
                                trans++;
                            }
                            else
                            {
                                if (!ofs.IsTransposition)
                                {
                                    ofs.IsTransposition = true;
                                    trans++;
                                }
                            }
                            break;
                        }
                        var nextOp = op.Next;
                        if (c1 > ofs.C2 && c2 > ofs.C1)
                        {
                            offsetArr.Remove(op);
                        }
                        op = nextOp;
                    }
                    offsetArr.AddLast(new OffsetPair(c1, c2)
                    {
                        IsTransposition = isTransposition
                    });
                }
                else
                {
                    lcss += localCs;
                    localCs = 0;
                    if (c1 != c2)
                    {
                        c1 = c2 = Math.Min(c1, c2);  //using min allows the computation of transpositions
                    }
                    //if matching tokens are found, remove 1 from both cursors (they get incremented at the end of the loop)
                    //so that we can have only one code block handling matches
                    for (var i = 0; i < maxOffset && (c1 + i < l1 || c2 + i < l2); i++)
                    {
                        if ((c1 + i < l1) && s1[c1 + i] == s2[c2])
                        {
                            c1 += i - 1;
                            c2--;
                            break;
                        }
                        if ((c2 + i < l2) && s1[c1] == s2[c2 + i])
                        {
                            c1--;
                            c2 += i - 1;
                            break;
                        }
                    }
                }
                c1++;
                c2++;
                if (maxDistance > 0)
                {
                    var temporaryDistance = Math.Max(c1, c2) - (lcss - trans);
                    if (temporaryDistance >= maxDistance)
                    {
                        return temporaryDistance;
                    }
                }
                // this covers the case where the last match is on the last token in list, so that
                // it can compute transpositions correctly
                if ((c1 >= l1) || (c2 >= l2))
                {
                    lcss += localCs;
                    localCs = 0;
                    c1 = c2 = Math.Min(c1, c2);
                }
            }
            lcss += localCs;
            return Math.Max(l1, l2) - (lcss - trans); //apply transposition cost to the final result
        }

        /// <summary>
        ///     Standard Sift algorithm, using strings and taking only maxOffset as a parameter
        /// </summary>
        /// <param name="s1"> </param>
        /// <param name="s2"> </param>
        /// <param name="maxOffset"> </param>
        /// <returns> </returns>
        internal static int SimplestDistance(string s1, string s2, int maxOffset)
        {
            var l1 = (s1?.Length) ?? 0;
            var l2 = (s2?.Length) ?? 0;

            if (l1 == 0)
            {
                return l2;
            }

            if (l2 == 0)
            {
                return l1;
            }

            var c1 = 0;  //cursor for string 1
            var c2 = 0;  //cursor for string 2
            var lcss = 0;  //largest common subsequence
            var localCs = 0; //local common substring

            while ((c1 < l1) && (c2 < l2))
            {
                if (s1[c1] == s2[c2])
                {
                    localCs++;
                }
                else
                {
                    lcss += localCs;
                    localCs = 0;
                    if (c1 != c2)
                    {
                        c1 = c2 = Math.Max(c1, c2);
                    }
                    //if matching tokens are found, remove 1 from both cursors (they get incremented at the end of the loop)
                    //so that we can have only one code block handling matches
                    for (var i = 0; i < maxOffset && (c1 + i < l1 && c2 + i < l2); i++)
                    {
                        if ((c1 + i < l1) && s1[c1 + i] == s2[c2])
                        {
                            c1 += i - 1;
                            c2--;
                            break;
                        }
                        if ((c2 + i < l2) && s1[c1] == s2[c2 + i])
                        {
                            c1--;
                            c2 += i - 1;
                            break;
                        }
                    }
                }
                c1++;
                c2++;
            }
            lcss += localCs;
            return Math.Max(l1, l2) - lcss;
        }
    }
}