# Mauzer

Tray icon app that prevents a windows machine from locking or sleeping and prevents the current user from being marked as 'Away' by apps like Teams.
This is basically achieved by calling SetThreadExecutionState and performing random mouse moves around a central point (i.e the current mouse position 5 seconds after starting Mauzer).

## Features

* Moves the mouse randomly around the start position every 5 seconds
* Only moves the mouse when the user has been inactive during the last interval
* Prevents the system from locking or sleeping
* Only visible in the system tray (double click for instructions, right-click to open menu and close the app)

## Prerequisites

* Windows 10/11 x64
* .NET 10 Runtime installed (download it [here](https://dotnet.microsoft.com/en-us/download/dotnet/10.0 ))
