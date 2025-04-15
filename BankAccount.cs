using System;

namespace BankingSystem
{
    public class BankAccount
    {
        private string accountNumber;
        private string accountHolder;
        private decimal balance;
        private readonly decimal minimumBalance = 100.00m;

        public BankAccount(string accountNumber, string accountHolder, decimal initialDeposit)
        {
            if (initialDeposit < minimumBalance)
                throw new ArgumentException($"Initial deposit must be at least {minimumBalance:C}");

            this.accountNumber = accountNumber;
            this.accountHolder = accountHolder;
            this.balance = initialDeposit;
        }

        public decimal Balance => balance;

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            balance += amount;
            LogTransaction("Deposit", amount);
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (balance - amount < minimumBalance)
            {
                Console.WriteLine("Insufficient funds!");
                return false;
            }

            balance -= amount;
            LogTransaction("Withdrawal", amount);
            return true;
        }

        private void LogTransaction(string type, decimal amount)
        {
            Console.WriteLine($"Transaction: {type}");
            Console.WriteLine($"Amount: {amount:C}");
            Console.WriteLine($"New Balance: {balance:C}");
            Console.WriteLine($"Date: {DateTime.Now}");
            Console.WriteLine("------------------------");
        }

        public override string ToString()
        {
            return $"Account: {accountNumber}\n" +
                   $"Holder: {accountHolder}\n" +
                   $"Balance: {balance:C}";
        }
    }
}