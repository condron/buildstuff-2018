using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Xunit;

namespace TestConsole {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
        }
    }

    public class Credit {
        public readonly uint Amount;

        public Credit(uint amount) {
            Amount = amount;
        }
    }
    public class Debit {
        public readonly uint Amount;

        public Debit(uint amount) {
            Amount = amount;
        }
    }
    public class StreamRepositoryTests {
        [Fact]
        public void can_append_events() {
            var repo = new Repository();
            var credit = new Credit(5);
            var debit = new Debit(5);
            uint accountNumber = 100;
            if (File.Exists(repo.GetStreamFile(accountNumber))) {
                File.Delete(repo.GetStreamFile(accountNumber));
            }
            var @events = new object[] { credit, debit };
            repo.Append(accountNumber, @events);

            var recordedEvents = repo.ReadAll(accountNumber);
            Assert.Equal(2, recordedEvents.Count);
            var recordedCredit = recordedEvents[0] as Credit;
            Assert.NotNull(recordedCredit);
            Assert.True(recordedCredit.Amount == 5);
            var recordedDebit = recordedEvents[1] as Debit;
            Assert.NotNull(recordedDebit);
            Assert.True(recordedDebit.Amount == 5);
        }
        [Fact]
        public void can_update_db() {
            var repo = new Repository();
            var credit = new Credit(5);
            var debit = new Debit(5);
            uint accountNumber = 301;
            if (File.Exists(repo.GetStreamFile(accountNumber))) {
                File.Delete(repo.GetStreamFile(accountNumber));
            }
            var @events = new object[] { credit, debit };
            repo.Append(accountNumber, @events);
            var adapter = new Adapter();
            adapter.UpdateDb(accountNumber, repo.ReadAll(accountNumber));

            adapter.Conn.Open();
            var cmd = new SqlCommand($"Select * from dbo.Accounts where id = {accountNumber}", adapter.Conn);
            var reader = cmd.ExecuteReader();
            Assert.True(reader.HasRows);
            reader.Read();
            Assert.True(accountNumber == reader.GetInt32(0));
            Assert.True(0 == reader.GetDecimal(1));
            Assert.True(2 == reader.GetInt64(2));
            adapter.Conn.Close();

        }
        [Fact]
        public void can_append_events_to_multiple_streams() {
            var repo = new Repository();
            var credit = new Credit(5);
            var debit = new Debit(5);

            for (uint i = 200; i < 210; i++) {
                repo.Append(i, new object[] { credit, debit });
            }

        }
    }
    public class Repository {
        private JsonSerializerSettings settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Objects
        };
        public string AccountFolder = Path.Combine(
                                            Environment.GetFolderPath(
                                                Environment.SpecialFolder.CommonApplicationData), "Workshop/Accounts");
        public string GetStreamFile(uint accountNumber) {
            return Path.Combine(AccountFolder, $"{accountNumber}.stream");
        }
        public void Append(uint accountNumber, object[] events) {

            using (var stream = File.AppendText(GetStreamFile(accountNumber))) {
                foreach (var @event in events) {
                    stream.WriteLine(JsonConvert.SerializeObject(@event, settings));
                }
            }
        }
        public List<object> ReadAll(uint accountNumber) {

            var recordedEvents = File.ReadLines(GetStreamFile(accountNumber));
            var @events = new List<object>();
            foreach (var recordedEvent in recordedEvents) {
                @events.Add(JsonConvert.DeserializeObject(recordedEvent, settings));
            }
            return @events;
        }
    }
    public class Adapter {
        public readonly SqlConnection Conn =
            new SqlConnection("Data Source=(localdb)\\ProjectsV13;Initial Catalog=WorkShop;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

        public void UpdateDb(uint account, List<object> events) {
            long balance = 0;
            foreach (var @event in events) {
                switch (@event) {
                    case Credit credit:
                        balance += credit.Amount;
                        break;
                    case Debit debit:
                        balance -= debit.Amount;
                        break;
                }
            }
            Conn.Open();
            var cmd = new SqlCommand("Select * from dbo.Accounts", Conn);
            var reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                while (reader.Read()) {
                    if (reader.GetInt32(0) != account) continue;
                    reader.Close();
                    var update = new SqlCommand(
                        $"Update dbo.Accounts set balance = {balance}, position = {events.Count} where id = {account}", Conn);
                    update.ExecuteNonQuery();
                    Conn.Close();
                    return;
                }
            }
            reader.Close();
            var insert = new SqlCommand(
                $"Insert into dbo.Accounts  values ({account}, {balance},{events.Count})", Conn);
            insert.ExecuteNonQuery();
            Conn.Close();
        }
    }
}
