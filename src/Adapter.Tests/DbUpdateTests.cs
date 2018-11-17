using Messages;
using StreamStore;
using System.Data.SqlClient;
using System.IO;
using Xunit;

namespace Adapter.Tests {
    public class DbUpdateTests {
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
            var adapter = new AccountAdapter();
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
       
    }
}
