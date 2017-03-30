# tailwin
Windows program for watching changes appended to files. Great for log files!

## Features:
- Drag-drop files onto window service to start tailing
- Run from command line
- Add optional command line argument for skipping n lines (default 500)

## Installation
- Download the latest [release](https://github.com/sharpguru/tailwin/releases/latest)
- Copy the folder into c:\windows\system32 or another directory in your path.
- Rename the folder "tailwin" 
- Create a shortcut to tailwin.exe on your desktop

## Requirements
Requires .NET Framework 4
Should work out-of-box with Windows 7 or higher 

## Usage

USAGE:
~~~
Command Line: tailwin <filename> <last n lines (optional)>
EXAMPLE: To display last 1000 lines in somefile =>
         tailwin somefile.txt 1000
From Windows: Open Tailwin, drag file onto window
From Tailwin: Open Tailwin, Click on the File -> Open menu, select file to tail
~~~

List entire contents of file then display lines as the file is updated:
~~~
tailwin somefile.txt 0
~~~

List last 50 lines of somefile.txt then display lines as the file is updated:
~~~
tailwin somefile.txt 50
~~~

## Want More
Looking for a command line-only option? Check out [tail](https://github.com/sharpguru/tail)
