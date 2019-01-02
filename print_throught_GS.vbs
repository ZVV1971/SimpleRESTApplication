Dim objShell
Set objShell = WScript.CreateObject("WScript.Shell")

Set myArgs = WScript.Arguments.Unnamed
Set myNamedArgs = WScript.Arguments.Named
If myNamedArgs.Exists("printer") Then
   prn = myNamedArgs.Item("printer")
   ghostscriptPath = myNamedArgs.Item("gspath")
Else
	Wscript.echo "No printer provided"
	WScript.Quit -1
End If
For i = 0 To myArgs.Count - 1
	currentPDF = Chr(34) & myArgs.Item(i) & Chr(34)
	GS = Chr(34) & ghostscriptPath & Chr(34)
	objShell.Run GS & " -sDEVICE=mswinpr2 -dBATCH -dNOPAUSE -sOutputFile=%printer%" & _
				 Chr(34) & prn & Chr(34) &" -f " & currentPDF, 0, true
Next