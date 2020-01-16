using System;
using System.Reactive.Subjects;

namespace BreadBoard
{
    public class Bus<TData>
    {
        private readonly BehaviorSubject<TData> _subject = new BehaviorSubject<TData>(default);

        public TData Value
        {
            get => _subject.Value;
            set => _subject.OnNext(value);
        }

        public IObservable<TData> ValueChanged => _subject;
    }
}