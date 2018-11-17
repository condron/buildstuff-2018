using System.Collections.Generic;
using System.Data.SqlClient;
using Messages;

namespace Adapter {
    public class AccountAdapter {
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
