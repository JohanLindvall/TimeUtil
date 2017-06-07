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

    /// <summary>
    /// Defines extension methods for time windows.
    /// </summary>
    public static class TimeWindowExtension
    {
        /// <summary>
        /// Negates the ipnput time windows.
        /// </summary>
        /// <param name="input">The time windows to negate.</param>
        /// <returns>A negated copy of the input.</returns>
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

        /// <summary>
        /// Extends the input at both ends with the given parameter.
        /// </summary>
        /// <param name="input">The input to extend.</param>
        /// <param name="expand">A copy of the extended input.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Offsets the input with the given parameter.
        /// </summary>
        /// <param name="input">The input to offset..</param>
        /// <param name="offset">The offset to apply to the input.</param>
        /// <returns>An offset copy of the input.</returns>
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

        /// <summary>
        /// Returns the minimum value of the input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The minimum value.</returns>
        public static DateTimeOffset Min(this IEnumerable<DateTimeOffset> input)
        {
            DateTimeOffset min = DateTimeOffset.MaxValue;

            foreach (var value in input)
            {
                if (value < min)
                {
                    min = value;
                }
            }

            return min;
        }

        /// <summary>
        /// Returns an intersection of the input.
        /// </summary>
        /// <param name="input">The input to intersect.</param>
        /// <returns>An interseciton of the input.</returns>
        public static IEnumerable<TimeWindow> Intersect(this IEnumerable<IEnumerable<TimeWindow>> input)
        {
            var iterators = input.Select(i => i.GetEnumerator()).ToList();

            if (iterators.Count == 0)
            {
                yield break;
            }

            foreach (var iter in iterators)
            {
                if (!iter.MoveNext())
                {
                    yield break;
                }
            }

            while (true)
            {
                var intersect = iterators[0].Current;

                foreach (var iter in iterators.Skip(1))
                {
                    intersect = intersect?.Intersect(iter.Current);
                }

                if (intersect != null)
                {
                    yield return intersect;
                }

                var reference = Min(iterators.Select(i => i.Current.To));

                foreach (var iter in iterators)
                {
                    while (reference > iter.Current.From && !iter.Current.Contains(reference))
                    {
                        if (!iter.MoveNext())
                        {
                            yield break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a union of the input.
        /// </summary>
        /// <param name="input">The input to form a union of.</param>
        /// <returns>A union of the input.</returns>
        public static IEnumerable<TimeWindow> Union(this IEnumerable<IEnumerable<TimeWindow>> input)
        {
            return Negate(Intersect(input.Select(Negate)));
        }
    }
}
