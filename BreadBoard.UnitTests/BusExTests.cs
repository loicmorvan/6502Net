using Moq;
using System;
using Xunit;

namespace BreadBoard.UnitTests
{
    public class BusExTests
    {
        [Fact]
        public void SignalWorks()
        {
            var observerMock = new Mock<IObserver<bool>>();
            var signalBus = new Bus<bool>();
            signalBus.ValueChanged.Subscribe(observerMock.Object);
            observerMock.Verify(o => o.OnNext(false));

            signalBus.Signal();

            observerMock.Verify(o => o.OnNext(true));
            observerMock.Verify(o => o.OnNext(false));
            observerMock.VerifyNoOtherCalls();
        }
    }
}
