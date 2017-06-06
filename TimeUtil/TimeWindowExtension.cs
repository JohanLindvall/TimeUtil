// Copyright(c) 2017 Johan Lindvall
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace TimeUtil
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class TimeWindowExtension
    {
        public static IEnumerable<TimeWindow> Negate(this IEnumerable<TimeWindow> input)
        {
            DateTimeOffset previous = DateTimeOffset.MinValue;

            foreach (var tw in input)
            {
                if (tw.From != previous)
                {
                    yield return new TimeWindow
                    {
                        From = previous,
                        To = tw.From
                    };
                }

                previous = tw.To;
            }

            if (previous != DateTimeOffset.MaxValue)
            {
                yield return new TimeWindow
                {
                    From = previous,
                    To = DateTimeOffset.MaxValue
                };
            }
        }

        public static IEnumerable<TimeWindow> Extend(this IEnumerable<TimeWindow> input, TimeSpan expand)
        {
            TimeWindow previous = null;

            foreach (var tw in input)
            {
                var extended = tw.Extend(expand);

                if (previous == null)
                {
                    previous = extended;
                }
                else if (extended != null && extended.From <= previous.To)
                {
                    previous.To = extended.To;
                }
                else
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (previous != null)
                    {
                        yield return previous;
                    }

                    previous = extended;
                }
            }

            if (previous != null)
            {
                yield return previous;
            }
        }

        public static IEnumerable<TimeWindow> Offset(this IEnumerable<TimeWindow> input, TimeSpan offset)
        {
            foreach (var tw in input)
            {
                yield return new TimeWindow
                {
                    From = tw.From + offset,
                    To = tw.To + offset
                };
            }
        }

        public static IEnumerable<TimeWindow> Intersect(this IEnumerable<IEnumerable<TimeWindow>> input)
        {
            var iterators = input.Select(i => i.GetEnumerator()).ToList();

            foreach (var iter in iterators)
            {
                if (!iter.MoveNext())
                {
                    yield break;
                }
            }

            while (true)
            {
                IEnumerator<TimeWindow> minimumFrom = null;
                IEnumerator<TimeWindow> minimumTo = null;
                TimeWindow intersect = new TimeWindow
                {
                    From = DateTimeOffset.MinValue,
                    To = DateTimeOffset.MaxValue
                };

                foreach (var iter in iterators)
                {
                    var value = iter.Current;

                    if (minimumFrom == null || value.From < minimumFrom.Current.From)
                    {
                        minimumFrom = iter;
                    }

                    if (minimumTo == null || value.To < minimumTo.Current.To)
                    {
                        minimumTo = iter;
                    }

                    if (intersect != null)
                    {
                        intersect = iter.Current.Intersect(intersect);
                    }
                }

                if (intersect == null)
                {
                    // Advance minimum from
                    if (!minimumFrom.MoveNext())
                    {
                        yield break;
                    }
                }
                else
                {
                    // Advance to minimum to
                    yield return intersect;

                    // ReSharper disable once PossibleNullReferenceException
                    if (!minimumTo.MoveNext())
                    {
                        yield break;
                    }
                }
            }
        }

        public static IEnumerable<TimeWindow> Union(this IEnumerable<IEnumerable<TimeWindow>> input)
        {
            return Negate(Intersect(input.Select(Negate)));
        }
    }
}
