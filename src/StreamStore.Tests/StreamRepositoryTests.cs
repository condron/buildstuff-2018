using Messages;
using System.IO;
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
    }
}
