using Moq;
using Processor;
using System;
using Xunit;

namespace BreadBoard.UnitTests
{
    public class BusTests
    {
        [Fact]
        public void BusWorks()
        {
            var observerMock = new Mock<IObserver<int>>();
            var sut = new Bus<int>();
            sut.ValueChanged.Subscribe(observerMock.Object);

            sut.Value = 2;

            Assert.Equal(2, sut.Value);

            observerMock.Verify(o => o.OnNext(0));
            observerMock.Verify(o => o.OnNext(2));
            observerMock.VerifyNoOtherCalls();
        }
    }
}
