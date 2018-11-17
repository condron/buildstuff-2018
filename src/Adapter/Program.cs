using System;
using System.Threading;
using StreamStore;

namespace Adapter {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var adapter = new AccountAdapter();
            var repo = new Repository();
            
            while (true) {
                try {
                    var accounts = repo.ReadStreamToEnd("accounts");
                    foreach (var recordedEvent in accounts) {
                        var accountAdded = recordedEvent.Event as AccountStreamAdded;
                        adapter.UpdateDb(accountAdded.AccountNumber, repo.ReadStreamToEnd(accountAdded.AccountNumber));
                    }
                  
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
                Thread.Sleep(500);
            }
        }
    }
}
