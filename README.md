Language: c#
Focuses: Thread Syncronization & Shared Resouce protextion

This program simulates a simple banking application with a focus on threads sharing access to 
certain resources. It can perform all the usual banking app functions such as depositing, 
withdrawals, and transfers. With functionality like deadlock detection and mutex locks, this 
program serves as a proof of concept for something to be made on a larger scale

SETUP:
Close this repository to you local machine...

git clone https://github.com/JJohnson-png/OS_Project

Open the project in your IDE of choice

Build and run the program

Done!

Example output: 
Welcome to the bank of Operating Systems
Waiting...
Transferred 100 successfully.
Transferred 500 successfully.
Transferred 100 successfully.
Final Account 1 Balance: 1200
Final Account 2 Balance: 900

Note: To alter what threads do what, you can change the code in the thread operations section
by creating your own or just commenting out what is already there.
It will always print out the final balance for each account at the end.
I have included both Deadlock and deadlock detection methods but they are not used
by the threads currently. The deadlock method will stop your program from completing and
is only there as part of a demonstration.

