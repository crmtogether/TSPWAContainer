' arcustomer.vbs - VBScript to open CRM Together entity via protocol handler
' Parameters: entityId
' Usage: cscript arcustomerorders.vbs ""

' Get command line arguments
Dim entityId, protocolUrl

entityId = SO_SalesOrder_bus_SalesOrderNo

entityId = replace(entityId," ", "%20")

' Construct the protocol URL
' Note: In VBScript, we don't need to encode the parameters as the protocol handler will handle it
protocolUrl = "crmtog://openEntity?entityType=arcustomerorders&entityId=" & entityId

' Execute the protocol handler
On Error Resume Next
CreateObject("WScript.Shell").Run protocolUrl, 1, False
