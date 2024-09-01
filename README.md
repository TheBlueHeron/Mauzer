# Mauzer

Automatically minimizing console app that prevents a windows machine from locking or sleeping and prevents the current user from being marked as 'Away' by apps like Teams.
This is done by calling SetThreadExecutionState and performing random mouse moves around a central point (i.e the current mouse position 3 seconds after starting Mauzer).

## Features

* Moves the mouse randomly around the start position every 5 seconds
* Only moves the mouse when the user has been inactive during the last interval
* Prevents the system from locking or sleeping
* Minimizes to the system tray (double click to show window)
