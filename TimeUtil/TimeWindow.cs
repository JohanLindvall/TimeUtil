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

    public class TimeWindow
    {
        public DateTimeOffset From { get; set; }

        public DateTimeOffset To { get; set; }

        public TimeWindow Extend(TimeSpan extend)
        {
            return Validate(new TimeWindow
            {
                From = this.From == DateTimeOffset.MinValue ? DateTimeOffset.MinValue : this.From - extend,
                To = this.To == DateTimeOffset.MaxValue ? DateTimeOffset.MaxValue : this.To + extend
            });
        }

        public TimeWindow Intersect(TimeWindow other)
        {
            return Validate(new TimeWindow
            {
                From = this.From > other.From ? this.From : other.From,
                To = this.To < other.To ? this.To : other.To
            });
        }

        private static TimeWindow Validate(TimeWindow input)
        {
            if (input.From < input.To)
            {
                return input;
            }
            else
            {
                return null;
            }
        }
    }
}
