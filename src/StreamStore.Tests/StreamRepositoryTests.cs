using System;
using Messages;
using System.IO;
using System.Threading;
using Xunit;

namespace StreamStore.Tests {
    public class StreamRepositoryTests {
        [Fact]
        public void can_append_events() {
            var repo = new Repository();
            var credit = new Credit(5);
            var debit = new Debit(5);
            string accountNumber = "100";
            if (File.Exists(repo.GetStreamFile(accountNumber))) {
                File.Delete(repo.GetStreamFile(accountNumber));
            }
            var @events = new object[] { credit, debit };
            repo.Append(accountNumber, @events);

            var recordedEvents = repo.ReadStreamToEnd(accountNumber);
            Assert.Equal(2, recordedEvents.Count);
            var recordedCredit = recordedEvents[0].Event as Credit;
            Assert.NotNull(recordedCredit);
            Assert.True(recordedCredit.Amount == 5);
            var recordedDebit = recordedEvents[1].Event as Debit;
            Assert.NotNull(recordedDebit);
            Assert.True(recordedDebit.Amount == 5);
        }

        [Fact]
        public void can_append_events_to_multiple_streams() {
            var repo = new Repository();
            var credit = new Credit(5);
            var debit = new Debit(5);

            for (uint i = 200; i < 210; i++) {
                repo.Append(i.ToString(), new object[] { credit, debit });
            }

        }
        [Fact]
        public void can_read_stream_from() {
            var repo = new Repository();
            var credit = new Credit(5);
            var credit2 = new Credit(7);
            var credit3 = new Credit(13);
            var debit = new Debit(5);
            string accountNumber = "100";
            if (File.Exists(repo.GetStreamFile(accountNumber))) {
                File.Delete(repo.GetStreamFile(accountNumber));
            }
            var @events = new object[] { credit, debit, credit2, credit3 };
            repo.Append(accountNumber, @events);

            var recordedEvents = repo.ReadStreamToEnd(accountNumber, 2);
            Assert.Equal(2, recordedEvents.Count);
            var recordedCredit = recordedEvents[0].Event as Credit;
            Assert.NotNull(recordedCredit);
            Assert.True(recordedCredit.Amount == 7);
            recordedCredit = recordedEvents[1].Event as Credit;
            Assert.NotNull(recordedCredit);
            Assert.True(recordedCredit.Amount == 13);
        }
        [Fact]
        public void can_read_stream_event() {
            var repo = new Repository();
            var credit = new Credit(5);
            var credit2 = new Credit(7);
            var credit3 = new Credit(13);
            var debit = new Debit(5);
            string accountNumber = "100";
            if (File.Exists(repo.GetStreamFile(accountNumber))) {
                File.Delete(repo.GetStreamFile(accountNumber));
            }
            var @events = new object[] { credit, debit, credit2, credit3 };
            repo.Append(accountNumber, @events);

            var recordedEvent = repo.ReadStreamEvent(accountNumber, 2);
            Assert.NotNull(recordedEvent);
            var recordedCredit = recordedEvent.Event as Credit;
            Assert.NotNull(recordedCredit);
            Assert.True(recordedCredit.Amount == 7);
        }
        [Fact]
        public void can_get_stream_names() {
            var repo = new Repository();
            var streams = repo.ReadStreamToEnd("accounts");
            Assert.NotEmpty(streams);
            var streamAddedEvent = streams[0].Event as AccountStreamAdded;
            Assert.NotNull(streamAddedEvent);
        }

        private long _subscriptionCount = 0;
        [Fact]
        public void can_subscribe_to_stream() {
            var repo = new Repository();
            var credit = new Credit(5);
            var credit2 = new Credit(7);
            var credit3 = new Credit(13);
            var debit = new Debit(5);
            string stream = "423";
            if (File.Exists(repo.GetStreamFile(stream))) {
                File.Delete(repo.GetStreamFile(stream));
            }
            var @events = new object[] { credit, debit, credit2, credit3 };
            repo.Append(stream, @events);
            repo.Subscribe(stream, 0, _ => Interlocked.Increment(ref _subscriptionCount));
            SpinWait.SpinUntil(() => Interlocked.Read(ref _subscriptionCount) == 4, TimeSpan.FromSeconds(3));
            Assert.Equal(4, _subscriptionCount);
            repo.Append(stream,new object[]{debit});
            SpinWait.SpinUntil(() => Interlocked.Read(ref _subscriptionCount) == 5, TimeSpan.FromSeconds(2));
            Assert.Equal(5, _subscriptionCount);

        }
    }
}
