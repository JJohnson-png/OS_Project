//NAME: JONATHAN JOHNSON
//CLASS: OPERATING SYSTEMS
//SECTION: 01
//DATE: 2/27/2025


using System;
using System.Threading;

class BankAccount
{
    private int balance = 1000;     //EACH ACCOUNT STARTS WITH 1000 DOLLARS
    private Mutex mutex = new Mutex();      //MUTEX FOR EACH ACCOUNT OBJECT

    public int GetBalance() => balance;     //GETTER FOR BALANCE

    public void Deposit(int amount)     //DEPOSIT METHOD
    {
        mutex.WaitOne();        //LOCKS THE ACCOUNT
        try
        {
            balance += amount;
            Console.WriteLine($"Deposited {amount}, New Balance: {balance}");       //ADDS TO AMOUNT TO ACCOUNT AND PRINTS
        }
        finally
        {
            mutex.ReleaseMutex();       //RELEASES ACCOUNT 
        }
    }

    public void Withdraw(int amount)        //METHOD FOR WITHDRAW
    {
        mutex.WaitOne();        //LOCKS ACCOUNT OBJECT
        try
        {
            if (balance >= amount)      //PREVENTS BALANCE FROM BEING NEGATIVE
            {
                balance -= amount;      //SUBTRACTS AMOUNT FROM BALANCE
                Console.WriteLine($"Withdrew {amount}, New Balance: {balance}");        //PRINTS NEW BALANCE
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
            }
        }
        finally
        {
            mutex.ReleaseMutex();       //RELEASES ACCOUNT OBJECTS
        }
    }

    //***FOR DEMO ONLY***//
    public void TransferDeadlock(BankAccount target, int amount)        //METHOD FOR SHOWCASING DEADLOCK SCENARIO
    {
        mutex.WaitOne();        //LOCKS ACCOUNT OBJECT
        Console.WriteLine("Waiting...");
        try
        {
            Thread.Sleep(3000);     //PAUSE FOR GUARANTEED DEADLOCK
            target.mutex.WaitOne();     //WAITS FOR OTHER ACCOUNT TO BE UNLOCKED
            try
            {
                if (balance >= amount)      //REST OF THE METHOD IS REDUNDANT
                {
                    balance -= amount;
                    target.balance += amount;
                    Console.WriteLine($"Transferred {amount} successfully.");
                }
                else
                {
                    Console.WriteLine("Insufficient funds!");
                }
            }
            finally
            {
                target.mutex.ReleaseMutex();
            }
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    //***FOR DEMO ONLY***//
    public void TransferDeadlockDetection(BankAccount target, int amount)   //SHOWCASES ISOLATED DEADLOCK DETECTION     
{
    if (!mutex.WaitOne(1000))       //WAITS ONLY ONE SECOND FOR FREE ACCOUNT
    {
        Console.WriteLine("Potential deadlock detected.");
        return;     //ESCAPES IF DEADLOCK
    }

    try
    {
        Thread.Sleep(3000);     //PAUSE FOR GURANTEED DEADLOCK

        if (!target.mutex.WaitOne(1000))        //WAITS A SECOND FOR FREE ACCOUNT
        {
            Console.WriteLine($"Potential deadlock detected. Voiding transfer of {amount}");
            return;     //ESCAPES IF DEADLOCK ... REST OF METHOD REDUNDANT
        }

        try     
        {
            if (balance >= amount)
            {
                balance -= amount;
                target.balance += amount;
                Console.WriteLine($"Transferred {amount} successfully.");
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
            }
        }
        finally
        {
            target.mutex.ReleaseMutex();
        }
    }
    finally
    {
        mutex.ReleaseMutex();
    }
}


    public void TransferWithOrderingAndDetection(BankAccount target, int amount)
{
        BankAccount first = this;       //ORDERS ACCOUNT OBJECTS
        BankAccount second = target;


    if (!first.mutex.WaitOne(1000))     //WAITS ONE SECOND FOR ACCOUNT OBJECCT
    {
        Console.WriteLine("Failed to acquire first lock. Potential deadlock detected.");
        return;     //ESCAPES IF DEADLOCK
    }

    try
    {
        if (!second.mutex.WaitOne(1000))        //WAITS ONE SECOND FOR ACCOUNT OBJECT
        {
            Console.WriteLine("Failed to acquire second lock. Potential deadlock detected.");
            return;         //ESCAPES IF DEADLOCK
        }

        try
        {
            if (balance >= amount)      //PREVENTS NEGATIVE ACCOUNT BALANCE
            {
                balance -= amount;
                target.balance += amount;       //ADDS AND SUBTRACTS AMOUNT FROM BALANCES AND PRINTS
                Console.WriteLine($"Transferred {amount} successfully.");
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
            }
        }
        finally
        {
            second.mutex.ReleaseMutex();        //RELEASES ACCOUNT
        }
    }
    finally
    {
        first.mutex.ReleaseMutex();     //RELEASES ACCOUNT
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Welcome to the bank of Operating Systems");

        BankAccount account1 = new BankAccount();       //CREATES ACCOUNT OBJECTS
        BankAccount account2 = new BankAccount();

        /*
            SECTION FOR CALING METHODS WITH THREADS
        */
        Thread t1 = new Thread(() => account1.Deposit(500));        
        Thread t2 = new Thread(() => account1.Withdraw(200));
        Thread t3 = new Thread(() => account1.TransferWithOrderingAndDetection(account2, 200));
        Thread t4 = new Thread(() => account1.Deposit(100));
        Thread t5 = new Thread(() => account1.Withdraw(150));
        Thread t6 = new Thread(() => account1.TransferWithOrderingAndDetection(account2, 200));
        Thread t7 = new Thread(() => account2.Deposit(400));
        Thread t8 = new Thread(() => account2.Withdraw(100));
        Thread t9 = new Thread(() => account1.TransferWithOrderingAndDetection(account2, 200));
        Thread t10 = new Thread(() => account1.TransferWithOrderingAndDetection(account2, 200));

        t1.Start();
        t2.Start();
        t3.Start();
        t4.Start();
        t5.Start();
        t6.Start();
        t7.Start();
        t8.Start();
        t9.Start();
        t10.Start();

        t1.Join();
        t2.Join();
        t3.Join();
        t4.Join();
        t5.Join();
        t6.Join();
        t7.Join();
        t8.Join();
        t9.Join();
        t10.Join();


        /*
            THIS SECTION IS TO DISPLAY TRANSFER DEADLOCK... DEMO ONLY
        */

        /*
        Thread t11 = new Thread(() => account1.TransferDeadlock(account2, 100));
        Thread t12 = new Thread(() => account2.TransferDeadlock(account1, 100));
        t11.Start();
        t12.Start();
        t11.Join();
        t12.Join();
        */


        /*
            THIS SECTION IS TO DISPLAY TRANSFER DEADLOCK DETECTION... DEMO ONLY
        */

        /*
        Thread t13 = new Thread(() => account1.TransferDeadlockDetection(account2, 100));
        Thread t14 = new Thread(() => account2.TransferDeadlockDetection(account1, 500));
        t13.Start();
        t14.Start();
        t13.Join();
        t14.Join();
        */


        Console.WriteLine($"Final Account 1 Balance: {account1.GetBalance()}");
        Console.WriteLine($"Final Account 2 Balance: {account2.GetBalance()}");
    }
}

}

