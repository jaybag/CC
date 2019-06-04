Task:
Implement a very basic virtual cash card in C#. 

Requirements:
Can withdraw money if a valid PIN is supplied. The balance on the card needs to adjust accordingly.
Can be topped up any time by an arbitrary amount.
The cash card, being virtual, can be used in many places at the same time.

Principles:
Well tested code (test driven would be best).
Write the code as you would write a part of a production grade system.
Requirements must be met, but please don’t go overboard.

Solution in a nutshell:

- Console application
- Thread safe
- Deadlock safe
- Proper error handling (avoids unnecessary exceptions)
