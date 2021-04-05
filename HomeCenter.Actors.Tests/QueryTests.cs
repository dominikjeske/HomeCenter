using HomeCenter.Abstractions;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Xunit;

namespace HomeCenter.Actors.Tests
{
    public class QueryTests
    {
        [Fact]
        public void Test()
        {
            var query = new MotionQuery();
            var sub = query.Rerister(new Dictionary<int, string> {
                { 18500, Detectors.toilet },
                { 20500, Detectors.toilet },
                { 21500, Detectors.hallwayToilet }
            });

            query.MoveTime(30000);



        }
    }

    internal class TestRoomDictionary : IRoomDictionary
    {
        public Room this[string uid] => throw new NotImplementedException();

        public void CheckRooms(DateTimeOffset motionTime)
        {
            throw new NotImplementedException();
        }

        public MotionVector? GetLastLeaveVector(MotionVector motionVector)
        {
            throw new NotImplementedException();
        }

        public void HandleVectors(IList<MotionVector> motionVectors)
        {
            throw new NotImplementedException();
        }

        public bool IsProperVector(MotionPoint start, MotionPoint potencialEnd)
        {
            if(potencialEnd.TimeStamp - start.TimeStamp > TimeSpan.FromMilliseconds(500) && !string.Equals(potencialEnd.Uid, start.Uid))
            {
                return true;
            }

            return false;
        }

        public void MarkLeave(MotionVector motionVector)
        {
            throw new NotImplementedException();
        }

        public void MarkMotion(MotionWindow point)
        {
            throw new NotImplementedException();
        }

        public bool MoveInNeighborhood(string roomid, string roomToExclude, DateTimeOffset referenceTime)
        {
            throw new NotImplementedException();
        }

        public int NumberOfPersons()
        {
            throw new NotImplementedException();
        }
    }

    public class MotionQuery
    {
        private TestScheduler _scheduler = new();
        private IConcurrencyProvider _concurrencyProvider;
        private IRoomDictionary _roomDictionary = new TestRoomDictionary();

        public MotionQuery()
        {
            _concurrencyProvider = new TestConcurrencyProvider(_scheduler);
        }

        public IDisposable Rerister(Dictionary<int, string> moves)
        {
            List<Recorded<Notification<MotionEnvelope>>> _motionEvents = new();
            foreach (var move in moves)
            {
                _motionEvents.Add(new(Time.Tics(move.Key), Notification.CreateOnNext(new MotionEnvelope(move.Value))));
            }

            var events = _scheduler.CreateColdObservable(_motionEvents.ToArray());

            var motionWindows = events.Timestamp(_concurrencyProvider.Scheduler)
                                      .Select(move => new MotionWindow(move.Value.Message.MessageSource, move.Timestamp, _roomDictionary));

            var sub =
            motionWindows.Window(events, _ => Observable.Timer(TimeSpan.FromMilliseconds(3000), _concurrencyProvider.Scheduler))
                         .SelectMany(x => x.Scan((vectors, currentPoint) => vectors.AccumulateVector(currentPoint.Start))
                         .SelectMany(window => window.ToVectors()))
                         .GroupBy(room => room.EndPoint)
                         //  .Subscribe(HandleVectors);
                         .SelectMany(r => r.Buffer(TimeSpan.FromMilliseconds(100), _concurrencyProvider.Scheduler)
                                          .Where(b => b.Count > 0)
                                    )
                        .Subscribe(HandleVectors, HandleError);

            return sub;
        }

        public void MoveTime(int miliseconds)
        {
            _scheduler.AdvanceTo(Time.Tics(miliseconds));
        }

        private void HandleVectors(IList<MotionVector> vectors)
        {
        }

        private void HandleVectors(MotionVector vectors)
        {
        }

        private void HandleVectors(IGroupedObservable<string, MotionVector> vectors)
        {
           
        }

        private void HandleError(Exception ex)
        {
        }
    }
}