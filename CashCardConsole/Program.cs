using CashCard;
using System;
using System.Threading.Tasks;

namespace CashCardConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                const int pin = 123;

                var card = new Card(pin);

                var result = card.TopUp(pin, 1000);
                Console.WriteLine(string.Format("Top up amount={0}, Card balance={1}", 1000, result.Balance));

                // -100
                var w1 = Task.Run(() => Withdraw(card, pin, 100));

                // invalid pin
                var w2 = Task.Run(() => Withdraw(card, 111, 300));

                // +500
                var t1 = Task.Run(() => Topup(card, pin, 500));

                // -400
                var w3 = Task.Run(() => Withdraw(card, pin, 400));

                // invalid pin
                var t2 = Task.Run(() => Topup(card, 111, 500));

                Task.WaitAll(new[] { w1, t2, w3, t1, w2 });

                Console.WriteLine(string.Format("Card balance:Actual={0}, Expected=1000", card.GetBalance(pin).Balance));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        static void Withdraw(Card card, int pin, int amount)
        {
            var result = card.Withdraw(pin, amount);

            if (result.HasError)
                Console.WriteLine(string.Format("Withdrawl failed due to {0}", result.Error));
            else
                Console.WriteLine(string.Format("Withdrawn amount={0}, Card balance={1}", amount, result.Balance));
        }
        
        static void Topup(Card card, int pin, int amount)
        {
            var result = card.TopUp(pin, amount);

            if (result.HasError)
                Console.WriteLine(string.Format("Topup failed due to {0}", result.Error));
            else
                Console.WriteLine(string.Format("Topup amount={0}, Card balance={1}", amount, result.Balance));
        }
    }
}
