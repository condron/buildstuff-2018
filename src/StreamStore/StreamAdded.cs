namespace StreamStore {
    public class AccountStreamAdded {
        public readonly string AccountNumber;

        public AccountStreamAdded(string accountNumber) {
            AccountNumber = accountNumber;
        }
    }
}
