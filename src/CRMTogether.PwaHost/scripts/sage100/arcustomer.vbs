' arcustomer.vbs - VBScript to open CRM Together entity via protocol handler
' Parameters: entityId
' Usage: cscript arcustomer.vbs ""

' Get command line arguments
Dim entityId, protocolUrl

entityId = AR_Customer_bus_ARDivisionNo & "_" & AR_Customer_bus_CustomerNo

entityId = replace(entityId," ", "%20")

' Construct the protocol URL
' Note: In VBScript, we don't need to encode the parameters as the protocol handler will handle it
protocolUrl = "crmtog://openEntity?entityType=arcustomers&entityId=" & entityId

' Execute the protocol handler
On Error Resume Next
CreateObject("WScript.Shell").Run protocolUrl, 1, False
