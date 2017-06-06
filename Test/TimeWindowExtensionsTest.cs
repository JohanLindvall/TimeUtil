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

namespace Test
{
    using System;
    using System.Linq;
    using TimeUtil;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TimeWindowExtensionsTest
    {
        TimeWindow[] Tw => new[]
        {
            new TimeWindow
            {
                From = new DateTimeOffset(2017, 1, 1, 0, 0, 0, TimeSpan.Zero),
                To = new DateTimeOffset(2017, 1, 2, 0, 0, 0, TimeSpan.Zero),

            },
            new TimeWindow
            {
                From = new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.Zero),
                To = new DateTimeOffset(2018, 1, 2, 0, 0, 0, TimeSpan.Zero),

            },
            new TimeWindow
            {
                From = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero),
                To = new DateTimeOffset(2019, 1, 2, 0, 0, 0, TimeSpan.Zero),
            }
        };


        [TestMethod]
        public void TestNegate()
        {
            var tw = Tw;

            var neg = tw.Negate().ToList();

            Assert.AreEqual(4, neg.Count);
            Assert.AreEqual(DateTimeOffset.MinValue, neg[0].From);
            Assert.AreEqual(tw[0].From, neg[0].To);
            Assert.AreEqual(tw[0].To, neg[1].From);
            Assert.AreEqual(tw[1].From, neg[1].To);
            Assert.AreEqual(tw[1].To, neg[2].From);
            Assert.AreEqual(tw[2].From, neg[2].To);
            Assert.AreEqual(tw[2].To, neg[3].From);
            Assert.AreEqual(DateTimeOffset.MaxValue, neg[3].To);
        }

        [TestMethod]
        public void TestNegateTwice()
        {
            var tw = Tw;

            var negneg = tw.Negate().Negate().ToList();

            Assert.AreEqual(3, negneg.Count);
            for (var i = 0; i < negneg.Count; ++i)
            {
                Assert.AreEqual(tw[i].From, negneg[i].From);
                Assert.AreEqual(tw[i].To, negneg[i].To);
            }
        }

        [TestMethod]
        public void TestExtend()
        {
            var tw = Tw;
            var result = tw.Extend(TimeSpan.FromDays(1)).ToList();
            Assert.AreEqual(3, result.Count);
            for (var i = 0; i < result.Count; ++i)
            {
                Assert.AreEqual(tw[i].From.Subtract(TimeSpan.FromDays(1)), result[i].From);
                Assert.AreEqual(tw[i].To.Add(TimeSpan.FromDays(1)), result[i].To);
            }
        }

        [TestMethod]
        public void TestExtendOverlap()
        {
            var tw = Tw;
            var result = tw.Extend(TimeSpan.FromDays(200)).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(new DateTimeOffset(2016, 6, 15, 0, 0, 0, TimeSpan.Zero), result[0].From);
            Assert.AreEqual(new DateTimeOffset(2019, 7, 21, 0, 0, 0, TimeSpan.Zero), result[0].To);
        }

        [TestMethod]
        public void TestExtendNegative()
        {
            var tw = Tw.ToList();
            tw.Add(new TimeWindow
            {
                From = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                To = new DateTimeOffset(2020, 1, 1, 1, 0, 0, TimeSpan.Zero)
            });

            var result = tw.Extend(TimeSpan.FromMinutes(-30)).ToList();
            Assert.AreEqual(3, result.Count);
            for (var i = 0; i < result.Count; ++i)
            {
                Assert.AreEqual(tw[i].From.Add(TimeSpan.FromMinutes(30)), result[i].From);
                Assert.AreEqual(tw[i].To.Subtract(TimeSpan.FromMinutes(30)), result[i].To);
            }
        }
    }
}
