
# FortiConnect
This app automates the process of connecting to the Fortinet Client VPN when using email as a 2FA (2 Factor Authentication).  

It gets the VPN code from your recent emails automatically.
Optionally if you are on Windows and have the FortiClient opened, it can also write the credentials and email code in the FortiClient VPN UI.  

# Console Mode
This app can run in console mode if any of the console actions are passed as parameters:
- `GetEmailVpnCode`
- `LoginToVpn`


## Know issues

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
- AutoHotkey.Interop: https://github.com/amazing-andrew/AutoHotkey.Interop
- InputSimulator:
```
Install-Package InputSimulator: https://github.com/michaelnoonan/inputsimulator
var sim = new InputSimulator();
sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
```


## Check what email protocols are enabled by your company
See [using-telnet](https://inthetechpit.com/2019/07/23/telnet-to-test-connection-to-pop3-imap ) and 
[microsoft-docs](https://docs.microsoft.com/en-us/exchange/mail-flow/test-smtp-with-telnet ).  
```ps1
TELNET mail.mycompany.com  25		# SMTP   no-SSL (or Microsoft Exchange).
TELNET mail.mycompany.com 465		# SMTP with SSL.
TELNET mail.mycompany.com 587		# SMTP with TLS.
TELNET imap.mycompany.com 143		# IMAP   no-SSL.
TELNET imap.mycompany.com 993		# IMAP with SSL.
TELNET  pop.mycompany.com 110		# POP3   no-SSL.
TELNET  pop.mycompany.com 995		# POP3 with SSL.
USER my.email@MyCompany.com
PASS 1234
LIST
QUIT
```
