* This library requires the .Net Framework 4.0.
* The ProgID for the Crosse.Net.NewAspFtp.ClassicAspFtp class is "ClassicAspFtp"
	* For instance in VBScript you can instantiate the COM object like this:
		Set objASPFTP = Server.CreateObject("ClassicAspFTP")
	* In PowerShell, you can do the same thing like so (for testing):
		$aspftp = New-Object -ComObject ClassicAspFtp


To REGISTER this library, start cmd.exe (or PowerShell) with adminsitrative
privileges, and then...

* For 32-bit usage:
	cd path\to\dll
	C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe NewAspFtp.dll /codebase /tlb:NewAspFtp.lib

* For 64-bit usage:
	cd path\to\dll
	C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe NewAspFtp.dll /codebase /tlb:NewAspFtp.lib


NOTE:  You might need to modify the path to the .NET Framework if you have a
       different patchlevel of 4.0 installed.


To UNREGISTER the library:
	cd path\to\dll
	C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe NewAspFtp.dll /unregister
	C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe NewAspFtp.dll /unregister


Here is a full example using PowerShell:

	$f = "C:\Users\wrightst\code\NewAspFtp\NewAspFtp\bin\Debug\NewAspFtp.dll"
	C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe $f /unregister
	C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe $f /unregister
	C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe $f /codebase /tlb:NewAspFtp.lib
	C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe $f /codebase
