# NewAspFtp

This project is an attempt to implement the `NIBLACK.ASPFTP` ActiveX control
from 1998 in a modern language.  *NewAspFtp* is written in C# and is designed
to be a drop-in replacement for the *NIBLACK* module.  This library requires
the .NET Framework 4.5.

You can use this module in "Classic ASP" or ASP.NET code, as well as with any
code that can handle either a .NET assembly or a COM object.  (Instructions for
registering the library as a COM object are given below.)

Under the hood, this library simply wraps calls to the
[System.Net.FtpClient](https://netftp.codeplex.com/) library.

## ActiveX/COM Usage

To use this library as an ActiveX or COM object, you first need to register it.
Signing the assembly using `sn.exe` is also recommended and should be performed
prior to registering, if you opt to build the library from source.

### Registering using RegAsm.exe
* Copy `NewAspFtp.dll` somewhere on your system.  For purposes of this guide,
    assume that you decided to put it in the directory `C:\NewAspFtp`.
* Start cmd.exe (or PowerShell) with adminsitrative privileges.
* Use RegAsm.exe to register the libary.  **NOTE**:  You might need to modify
  the path to the .NET Framework if you have a different patchlevel of 4.0 installed.

  * For 32-bit usage:
	```
    PS> cd C:\NewAspFtp
    PS> C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe NewAspFtp.dll /codebase /tlb:NewAspFtp.lib
	```

  * For 64-bit usage:
	```
    PS> cd C:\NewAspFtp
    PS> C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe NewAspFtp.dll /codebase /tlb:NewAspFtp.lib
	```

  The only difference between the two commands above are the path to RegAsm.exe.  
  You **must** use the correct version of RegAsm.exe for your system and for your usage!

### Unregistering

To unregister the library, use RegAsm.exe with the */unregister* option:

```
PS> cd C:\NewAspFtp
PS> C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe NewAspFtp.dll /unregister
```

(Remember to use the appropriate path to RegAsm.exe. for your system.)

### Usage

The ProgID for the `Crosse.Net.NewAspFtp.ClassicAspFtp` class (which is the only class
visible via COM) is `ClassicAspFtp`.

* In VBScript (say, when used in a "Classic ASP" scenario) you can instantiate the
COM object like this:

 ```
 Set objASPFTP = Server.CreateObject("ClassicAspFTP")
 ```
 
 If you have legacy code that uses the `NIBLACK.ASPFTP` COM object, simply change
 *"NIBLACK.ASPFTP"* to *"ClassicAspFtp"* in your `Server.CreateObject()` lines.  That should
 be the only change you'll have to make.
 

* In PowerShell, you can do the same thing:

 ```
 $aspftp = New-Object -ComObject ClassicAspFtp
 ```
 
 (Why you'd want to do this is left as an exercise for the reader.)
