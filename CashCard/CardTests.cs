using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace CashCard
{
    [TestFixture]
    public class CardTests
    {
        private Card _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new Card(123);
        }       

        [Test]
        public void WithdrawlConcurrencyTest()
        {
            // Arrange
            _subject.TopUp(123, 10000);

            // Act (withdraw only)
            Parallel.For(0, 99, (counter) =>
            {
                _subject.Withdraw(123, 100);
            });
            var result = _subject.GetBalance(123);

            // Assert            
            Assert.That(result.HasError, Is.EqualTo(false));
            Assert.That(result.Error, Is.EqualTo(null));
            Assert.That(result.Balance, Is.EqualTo(100));
        }

        [Test]
        public void TopUpConcurrencyTest()
        {
            // Arrange
            _subject.TopUp(123, 100);

            // Act (topup only)
            Parallel.For(0, 99, (counter) =>
            {
                _subject.TopUp(123, 100);
            });
            var result = _subject.GetBalance(123);

            // Assert            
            Assert.That(result.HasError, Is.EqualTo(false));
            Assert.That(result.Error, Is.EqualTo(null));
            Assert.That(result.Balance, Is.EqualTo(10000));
        }

        [Test]
        public void WithdrawlAndTopUpConcurrencyTest()
        {
            // Arrange
            _subject.TopUp(123, 10000);

            // Act (withdraw + topup)
            Parallel.For(0, 99, (counter) =>
            {
                _subject.Withdraw(123, 100);
                _subject.TopUp(123, 100);
            });
            var result = _subject.GetBalance(123);

            // Assert            
            Assert.That(result.HasError, Is.EqualTo(false));
            Assert.That(result.Error, Is.EqualTo(null));
            Assert.That(result.Balance, Is.EqualTo(10000));
        }

        private sealed class TestCard : Card
        {
            public TestCard(int pin) : base(pin) { }
            public new object SyncRoot { get { return base.SyncRoot; } }
        }

        // Below DeadlockSafety tests highlight the importance of a timeout in case of a deadlock situation

        [Test]
        public void WithdrawlDeadlockSafetyTest()
        {            
            // Arrange
            var subject = new TestCard(123);
            subject.TopUp(123, 500);

            // Act
            // lock it first
            Monitor.Enter(subject.SyncRoot);
            try
            {
                // now let another thread try to get the monitor 
                var result = Task.Run(() => subject.Withdraw(123, 100)).Result;

                // Assert            
                Assert.That(result.HasError, Is.EqualTo(true));
                Assert.That(result.Error, Is.EqualTo("Unable to withdraw. Please try later."));
            }
            finally
            {
                Monitor.Exit(subject.SyncRoot);
            }
        }

        [Test]
        public void TopupDeadlockSafetyTest()
        {
            // Arrange
            var subject = new TestCard(123);
            subject.TopUp(123, 500);

            // Act
            // lock it first
            Monitor.Enter(subject.SyncRoot);
            try
            {
                // now let another thread try to get the monitor 
                var result = Task.Run(() => subject.TopUp(123, 100)).Result;

                // Assert            
                Assert.That(result.HasError, Is.EqualTo(true));
                Assert.That(result.Error, Is.EqualTo("Unable to topup. Please try later."));
            }
            finally
            {
                Monitor.Exit(subject.SyncRoot);
            }
        }

        [Test]
        public void GetBalanceDeadlockSafetyTest()
        {
            // Arrange
            var subject = new TestCard(123);
            subject.TopUp(123, 500);

            // Act
            // lock it first
            Monitor.Enter(subject.SyncRoot);
            try
            {
                // now let another thread try to get the monitor 
                var result = Task.Run(() => subject.GetBalance(123)).Result;

                // Assert            
                Assert.That(result.HasError, Is.EqualTo(true));
                Assert.That(result.Error, Is.EqualTo("Unable to fetch balance. Please try later."));
            }
            finally
            {
                Monitor.Exit(subject.SyncRoot);
            }
        }

        internal static object[] WithdrawValidationTestCases()
        {
            return new object[]
            {
                new object[] {"WhenInvalidPinIsSuppliedThenItFails", 111, 100, true, "Invalid PIN" },
                new object[] {"WhenInvalidAmountIsSuppliedThenItFails", 123, 0, true, "Invalid amount=[0]. Please enter valid amount." },
                new object[] {"WhenAmountToBeWithdrawnIsGreaterThanAvailableBalanceThenItFails", 123, 1000, true, "Amount to be withdrawn=[1000] is greater than the available balance=[500]. Please enter valid amount." }
            };
        }

        [TestCaseSource("WithdrawValidationTestCases")]
        public void WithdrawValidationTests(string testName, int pin, double amount, bool hasError, string error)
        {
            // Arrange
            _subject.TopUp(123, 500);

            // Act
            var result = _subject.Withdraw(pin, amount);

            // Assert            
            Assert.That(result.HasError, Is.EqualTo(hasError));
            Assert.That(result.Error, Is.EqualTo(error));
        }    

        internal static object[] TopUpValidationTestCases()
        {
            return new object[]
            {
                new object[] {"WhenInvalidPinIsSuppliedThenItFails", 111, 100, true, "Invalid PIN" },
                new object[] {"WhenInvalidAmountIsSuppliedThenItFails", 123, 0, true, "Invalid amount=[0]. Please enter valid amount." },
            };
        }

        [TestCaseSource("TopUpValidationTestCases")]
        public void TopUpValidationTests(string testName, int pin, double amount, bool hasError, string error)
        {
            // Arrange
            _subject.TopUp(123, 500);

            // Act
            var result = _subject.TopUp(pin, amount);

            // Assert            
            Assert.That(result.HasError, Is.EqualTo(hasError));
            Assert.That(result.Error, Is.EqualTo(error));
        }
    }
}
