# tailwin
Windows program for tailing files. Similar to unix tail command, but runs in Windows

## Features:
- Drag-drop files onto window service to start tailing
- Run from command line
- Add optional command line argument for skipping n lines (default 500)

## Installation
Build, then copy tailwin\bin\release folder into c:\windows\system32 or another directory in your path.
Rename your copy of the release folder "Tailwin" 
Create a shortcut to tailwin.exe on your desktop

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

Display usage help text:
~~~
Tail
~~~

List entire contents of file then display lines as the file is updated:
~~~
tailwin somefile.txt 0
~~~

List last 50 lines of somefile.txt then display lines as the file is updated:
~~~
tailwin somefile.txt 50
~~~
