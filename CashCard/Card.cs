using System.Threading;

namespace CashCard
{
    public class Card
    {
        private readonly int _pin;
        private double _balance;        
        private const int MonitorTimeout = 1000;
        public const string InvalidPinError = "Invalid PIN";

        public Card(int pin)
        {
            _pin = pin;
            SyncRoot = new object();
        }

        // to aid in testing (see the DeadlockSafety tests in CardTests.cs)
        protected internal object SyncRoot { get; private set; }

        public TaskResult Withdraw(int pin, double amount)
        {
            if (!IsValid(pin)) return new TaskResult(true, InvalidPinError);

            if (amount <= 0) return new TaskResult(true, string.Format("Invalid amount=[{0}]. Please enter valid amount.", amount));

            if (!Monitor.TryEnter(SyncRoot, MonitorTimeout))
                return new TaskResult(true, "Unable to withdraw. Please try later.");

            try
            {
                if (amount > _balance)
                    return new TaskResult(true, string.Format("Amount to be withdrawn=[{0}] is greater than the available balance=[{1}]. Please enter valid amount.", amount, _balance));

                _balance -= amount;

                return new TaskResult(false, null, _balance);
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        public TaskResult TopUp(int pin, double amount)
        {
            if (!IsValid(pin)) return new TaskResult(true, InvalidPinError);

            if (amount <= 0) return new TaskResult(true, string.Format("Invalid amount=[{0}]. Please enter valid amount.", amount));

            if (!Monitor.TryEnter(SyncRoot, MonitorTimeout))
                return new TaskResult(true, "Unable to topup. Please try later.");

            try
            {
                _balance += amount;

                return new TaskResult(false, null, _balance);
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        public TaskResult GetBalance(int pin)
        {
            if (!IsValid(pin)) return new TaskResult(true, InvalidPinError);

            if (!Monitor.TryEnter(SyncRoot, MonitorTimeout))
                return new TaskResult(true, "Unable to fetch balance. Please try later.");

            try
            {
                return new TaskResult(false, null, _balance);
            }
            finally
            {
                Monitor.Exit(SyncRoot);
            }
        }

        private bool IsValid(int pin)
        {
            return pin == _pin;
        }
    }
}
