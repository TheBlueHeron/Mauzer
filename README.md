# Mauzer

Automatically minimizing console app that prevents a windows machine from locking or sleeping and prevents the current user from being marked as 'Away' by apps like Teams.
This is done by calling SetThreadExecutionState and performing random mouse moves around a central point (i.e the current mouse position 3 secxonds after starting Mauzer).
