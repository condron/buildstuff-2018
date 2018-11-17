using System;
using System.Threading;
using StreamStore;

namespace Adapter {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            var adapter = new AccountAdapter();
            var repo = new Repository();
            var account = "100";
            while (true) {
                try {
                    adapter.UpdateDb(account, repo.ReadStreamToEnd(account));
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }
                Thread.Sleep(500);
            }
        }
    }
}
