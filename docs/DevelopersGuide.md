
# Developer's Guide

## Developer Software
- Visual Studio 2026 (or higher)
- Windows (on Mac or Linux it may work except for the FortiClient automation part that uses WinForms)
- DotNet 10.0 SDK ([url](https://dotnet.microsoft.com/download))


## Dependencies
- [(Avalonia-UI)](https://avaloniaui.net): Cross platform UI.
- WinForms: to automate the FortiClient UI.


## How to debug
Create a file named `AppSettings.local.json` with your personal information, next to the `AppSettings.json`  
to prevent your credentials from ending up in the git repository. 
This file is excluded from git in `.gitignore`, and can override the configuration from `AppSettings.json`.


## Known issues

### Getting he emails takes more than 1 minute using MS Exchange EWS
Some VPN 2FA email may only last for 1 minute, making the automation fail.


### Microsoft Exchange WebService EWS decommission
The official EWS source is in GitHub but has not been updated since 2015:  
GitHub: https://github.com/OfficeDev/ews-managed-api  
Docs: https://docs.microsoft.com/en-us/exchange/client-developer/exchange-server-development  
There are community supported versions of EWS to make it work with dotnet core:  
https://github.com/sherlock1982/ews-managed-api  

EWS will be decommissioned in 2020 as a method to access Office 365 (on-premise servers will continue to work). 
[See this](http://techgenix.com/ews-no-updates/ ).  
It will be required to migrate to Microsoft Graph API.  

### Microsoft Graph API
The new way to programatically interact with Office is using [the Graph-API](https://docs.microsoft.com/en-us/graph/overview ). 
- Docs for [Outlook interaction](https://docs.microsoft.com/en-us/graph/api/resources/mail-api-overview?view=graph-rest-1.0 ).
- Api example to get emails: `https://graph.microsoft.com/v1.0/me/messages?$search="hello world"`
- Graph Explorer Utility [url](https://developer.microsoft.com/en-us/graph/graph-explorer ). 
- [PowerShel Microsoft Graph tutorial](https://www.youtube.com/watch?v=6CIZWac0TBE ).

Graph API requires you to grant access to the app from your office account,
that may require you to have admin privileges that your company may didn't give you.


### SendKeys function only works in Windows:
https://www.reddit.com/r/csharp/comments/9dyf0t/having_my_program_send_an_enter_key_press/  
Possible solutions:
- AutoHotkey.Interop: [git](https://github.com/amazing-andrew/AutoHotkey.Interop)  
- NuGet package "InputSimulator": [git](https://github.com/michaelnoonan/inputsimulator) Basic.  
- NuGet package "InputSimulatorPlus": [git](https://github.com/TChatzigiannakis/InputSimulatorPlus) Forked from `InputSimulator` adds scan codes.  
- NuGet package "InputSimulatorStandard": [git](https://github.com/GregsStack/InputSimulatorStandard) Forked from `InputSimulatorPlus` converted to dotnet standard 2.0.  
- NuGet package "H.InputSimulator": [git](https://github.com/HavenDV/H.InputSimulator) Based on `InputSimulatorStandard` converted to dotnet standard 2.0 or dotnet 8 for AOT and trimming.  

In order of creation: `InputSimulator < InputSimulatorPlus < InputSimulatorStandard < H.InputSimulator`.  
So the best option at the moment is `H.InputSimulator`.  
