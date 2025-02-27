using System;
using System.Runtime.InteropServices;
using System.Threading;

class BankAccount                   //BANK ACCOUNT CLASS
{
    private int balance = 1000;         //EACH ACCOUNT STARTS WITH 100 DOLLARS
    private Mutex mutex = new Mutex();

    public int getBalance(){            //CONSTRUCTOR
        return balance;
    }

    public void Deposit(int amount)             //METHOD FOR DEPOSITING MONEY
    {
        mutex.WaitOne();                  //LOCKS ACCOUNT      
        try
        {
            balance += amount;  
            Console.WriteLine($"Deposited {amount}, New Balance: {balance}");       //DEPOSITS IF NOT LOCKED
        }
        finally
        {
            mutex.ReleaseMutex();       //RELEASES
        }
    }

    public void Withdraw(int amount)                //METHOD TO WITHDRAW MONEY
    {
        mutex.WaitOne();                            //LOCKS ACCOUNT
        try
        {
            if (balance >= amount)          //PREVENTS NEGATIVE ACCOUNT BALANCE
            {
                balance -= amount;  
                Console.WriteLine($"Withdrew {amount}, New Balance: {balance}");        //TAKES AWAY FROM ACCOUNT
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
            }
        }
        finally
        {
            mutex.ReleaseMutex();       //RELEASES ACCOUNT
        }
    }

public void TransferDeadlock(BankAccount target, int amount)    //METHOD FOR INTENTIONAL DEADLOCK      
{
    
    mutex.WaitOne();            //LOCKS ACCOUNT
    try
    {
        Thread.Sleep(3000);         //DELAYS FOR BETER CHANCE AT DEADLOCK

        target.mutex.WaitOne();         //LOCKS OTHER ACCOUNT
        try
        {
            if (balance >= amount)              //PREVENTS NEGATIVE BALANCE
            {
                balance -= amount;
                target.balance += amount;                                   //COMPLETES TRANSACTION
                Console.WriteLine($"Transferred {amount} successfully.");
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
            }
        }
        finally
        {
            target.mutex.ReleaseMutex();            //REALESES ACCOUNT
        }
    }
    finally
    {
        mutex.ReleaseMutex();           //RELEASES ACCOUNT
    }
}

public bool TransferDeadlockDetection(BankAccount target, int amount)       //TRANSFER METHOD WITH DEADLOCK DETECTION
{
  
    if (!mutex.WaitOne(1000))       //WAITS FOR SECOND FOR THE ACCOUNT OBJECT TO RELEASE
    {
        Console.WriteLine("Failed to acquire first lock. Potential deadlock detected.");
        return false;
    }
    
    try
    {
        Thread.Sleep(300);                  //FOR INCREASED DEADLOCK ODDS
    
        if (!target.mutex.WaitOne(1000))        //WAITS FOR SECOND FOR THE ACCOUNT OBJECT TO RELEASE 
        {
            Console.WriteLine("Failed to acquire second lock. Potential deadlock detected.");
            return false;
        }
        
        try
        {
            if (balance >= amount)          //PREVENTS NEGATIVE BALANCE
            {
                balance -= amount;
                target.balance += amount;                                   //COMPLETES TRANSACTION
                Console.WriteLine($"Transferred {amount} successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
                return false;
            }
        }
        finally
        {
            target.mutex.ReleaseMutex();            //RELEASES ACCOUNT
        }
    }
    finally
    {
        mutex.ReleaseMutex();                   //RELEASES ACCOUNT
    }
}

public void TransferOrdering(BankAccount target, int amount)            //DEMONSTRATES RESOURCE ORDERING
{
    BankAccount first;          //VARS FOR FIRST AND SECOND ACCOUNTS
    BankAccount second;

    if (this.GetHashCode() < target.GetHashCode())          //DETERMINES WHICH COMES FIRST
    {
        first = this;
        second = target;
    }
    else
    {
        first = target;
        second = this;
    }

    lock (first)                //LOCKS FIRST OBJECT
    {
        lock (second)           //LOCKS SECOND OBJECT
        {
            if (balance >= amount)      //PREVENTS BALANCE FROM BEING NEGATIVE
            {
                balance -= amount;
                target.balance += amount;               //COMPLETES THE TRANSACTION
                Console.WriteLine($"Transferred {amount} successfully.");
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
            }
        }
    }
}

public bool TransferWithOrderingAndDetection(BankAccount target, int amount)        //COMBINES PREVIOUS TWO METHODS
{
    BankAccount first;
    BankAccount second;

    if (this.GetHashCode() < target.GetHashCode())
    {
        first = this;
        second = target;                                //THIS SECTION DETERMINES WHICH ACCOUNT COMES FIRST AND SECOND
    }
    else
    {
        first = target;
        second = this;
    }

    if (!first.mutex.WaitOne(1000))             //THIS SECTION USES THE ORDER TO COMPLETE TRANSACTION WITH DEADLOCK DETECTION THE SAME AS THE OTHER METHOD
    {
        Console.WriteLine("Failed to acquire first lock. Potential deadlock detected.");
        return false;
    }

    try
    {
        if (!second.mutex.WaitOne(1000))
        {
            Console.WriteLine("Failed to acquire second lock. Potential deadlock detected.");
            return false;
        }

        try
        {
            if (balance >= amount)
            {
                balance -= amount;
                target.balance += amount;
                Console.WriteLine($"Transferred {amount} successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Insufficient funds!");
                return false;
            }
        }
        finally
        {
            second.mutex.ReleaseMutex();
        }
    }
    finally
    {
        first.mutex.ReleaseMutex();
    }
}
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Welcome to the Bank of Operating Systems!");
        bool option = true;
        BankAccount account1 = new BankAccount();           //CREATES BANK ACCOUNT OBJECTS
        BankAccount account2 = new BankAccount();
        
        do{                                                         //LOOP FOR MENU
            Console.WriteLine("Please select an option...\n");
            Console.WriteLine("1. Deposit");
            Console.WriteLine("2. Withdraw");
            Console.WriteLine("3. Deadlock Demonstration");
            Console.WriteLine("4. Deadlock Transfer detection/fixes");
            Console.WriteLine("5. Resource Ordering Demonstration");
            Console.WriteLine("6. Resource Ordering + Deadlock Transfer Detection/Fixes");
            Console.WriteLine("7. Quit");

            int selection = int.Parse(Console.ReadLine());      //GETS USER CHOICE

            switch(selection){          //SWITCH COMPLETES USER REQUEST AND CALL SCORRESPONDING METHOD
                case 1: 
                Console.WriteLine("How much would you like deposit? ");
                deposit(account1, int.Parse(Console.ReadLine()));
                break;

                case 2: 
                Console.WriteLine("How much would you like withdraw? ");
                withdraw(account1, int.Parse(Console.ReadLine()));
                break;

                case 3:
                mutexLocker(account1, account2, 200, 100);  
                break;

                case 4: 
                Console.WriteLine("How much would you like to transfer from your account? ");
                int fromYou = int.Parse(Console.ReadLine());
                Console.WriteLine("How much would you like to transfer to your account? ");
                int toYou = int.Parse(Console.ReadLine());
                mutexLockerDetection(account1, account2, fromYou, toYou);     
                break;
                
                case 5: 
                Console.WriteLine("How much would you like to transfer from your account? ");
                fromYou = int.Parse(Console.ReadLine());
                Console.WriteLine("How much would you like to transfer to your account? ");
                toYou = int.Parse(Console.ReadLine());
                transferOrdering(account1, account2, fromYou, toYou);  
                break;

                case 6 : 
                Console.WriteLine("How much would you like to transfer from your account? ");
                fromYou = int.Parse(Console.ReadLine());
                Console.WriteLine("How much would you like to transfer to your account? ");
                toYou = int.Parse(Console.ReadLine());
                TransferWithOrderingAndDetection(account1, account2, fromYou, toYou);  
                break;

                default:
                option = false;
                break;
            }

        }while(option);

        Console.WriteLine("Transactions completed.");
    }

    public static void deposit(BankAccount accountA, int depositAmount){        //CALLS DEPOSIT METHOD IN BANK ACCOUNT CLASS

        Thread t1 = new Thread(() => accountA.Deposit(depositAmount));  //CREATES THREAD FOR DEPOSIT

        t1.Start();
    
        Console.WriteLine("Waiting...");

        t1.Join();

    }

    public static void withdraw(BankAccount accountA, int withdrawAmount){      //CALLS WITHDRAW METHOD IN BANK ACCOUNT CLASS

        Thread t1 = new Thread(() => accountA.Withdraw(withdrawAmount));        //CREATES THREAD FOR WITHDRAW

        t1.Start();
    
        Console.WriteLine("Waiting...");

        t1.Join();

    }

public static void mutexLocker(BankAccount accountA, BankAccount accountB, int amountA, int amountB)        //CALLS FOR MUTEX DEADLOCK METHOD IN BANK ACCOUNT CLASS
{
    Thread t1 = new Thread(() => accountA.TransferDeadlock(accountB, amountA));     //CALLS FOR TRANSFER OF ACCOUNT A AND B WITH AMOUNTS  AT THE SAME TIME FOR DEADLOCK
    Thread t2 = new Thread(() => accountB.TransferDeadlock(accountA, amountB));

    t1.Start();
    t2.Start();

    Console.WriteLine("Waiting...");

    t1.Join();
    t2.Join();
}

public static void mutexLockerDetection(BankAccount accountA, BankAccount accountB, int amountA, int amountB)   //CALLS FOR ACCOUNT DEADLOCK DETECTION METHOD IN BANK ACCOUNT CLASS
{
    Thread t1 = new Thread(() => accountA.TransferDeadlockDetection(accountB, amountA));        //STARTS THREADS SIMULTANEOUSLY
    Thread t2 = new Thread(() => accountB.TransferDeadlockDetection(accountA, amountB));

    t1.Start();
    t2.Start();

    Console.WriteLine("Waiting...");

    t1.Join();
    t2.Join();

    Console.WriteLine("Account A Balance " + accountA.getBalance());        //PRINTS BALANCES
    Console.WriteLine("Account B Balance " + accountB.getBalance());

}

public static void transferOrdering(BankAccount accountA, BankAccount accountB, int amountA, int amountB)   //CALLS TRANSFER ORDERING METHOD IN BANK ACCOUNT CLASS
{
    Thread t1 = new Thread(() => accountA.TransferOrdering(accountB, amountA));     //STARTS THREADS SIMULTANEOUSLY
    Thread t2 = new Thread(() => accountB.TransferOrdering(accountA, amountB));

    t1.Start();
    t2.Start();

    Console.WriteLine("Waiting...");

    t1.Join();
    t2.Join();

    Console.WriteLine("Account A Balance " + accountA.getBalance());        //PRINTS BALANCES
    Console.WriteLine("Account B Balance " + accountB.getBalance());
}

    public static void TransferWithOrderingAndDetection(BankAccount accountA, BankAccount accountB, int amountA, int amountB){      //CALLS FOR TRANSFER AND ORDER DETECTING METHOD IN BANK ACCOUNT CLASS

        Thread t1 = new Thread(() => accountA.TransferWithOrderingAndDetection(accountB, amountA));     //STARTS THREADS SIMULTANEOUSLY
        Thread t2 = new Thread(() => accountB.TransferWithOrderingAndDetection(accountA, amountB));

        t1.Start();
        t2.Start();
    
        Console.WriteLine("Waiting...");

        t1.Join();
        t2.Join();
    
        Console.WriteLine("Account A Balance " + accountA.getBalance());        //PRINTS BALANCES
        Console.WriteLine("Account B Balance " + accountB.getBalance());
    }
}


