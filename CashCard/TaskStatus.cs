using System;

namespace CashCard
{
    public class TaskResult : Tuple<bool, string, double?>
    {
        public TaskResult(bool status, string error, double? balance = null) : base(status, error, balance) { }

        public bool HasError { get { return Item1; } }
        public string Error { get { return Item2; } }
        public double? Balance { get { return Item3; } }
    }
}
