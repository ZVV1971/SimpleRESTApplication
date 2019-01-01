Dim objShell
Set objShell = WScript.CreateObject("WScript.Shell")

Set myArgs = WScript.Arguments.Unnamed
Set myNamedArgs = WScript.Arguments.Named
If myNamedArgs.Exists("printcommand") Then
   prnCommand = myNamedArgs.Item("printcommand")
Else
	Wscript.echo "No printing command provided"
	WScript.Quit -1
End If
For i = 0 To myArgs.Count - 1
	replacestring = ".exe" & Chr(34)
	printCurrentPDF = LCase(Replace(prnCommand, "%1", Chr(34) & myArgs.Item(i) & Chr(34)))
	printCurrentPDF = Chr(34) & Replace(printCurrentPDF, ".exe", replacestring, vbTextCompare)
	objShell.Run printCurrentPDF, 0, true
Next