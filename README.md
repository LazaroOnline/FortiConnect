
# FortiConnect
This app automates the process of connecting to the Fortinet Client VPN when using email as a 2FA (2 Factor Authentication).  
It works by logging in the installed official FortiClient VPN, then checking in your email for the code and passing it to the VPN client.


## Know issues

### Microsoft Exchange WebService EWS decommission

The official EWS source is in GitHub but has not been updated since 2015:
GitHub: https://github.com/OfficeDev/ews-managed-api  
Docs: https://docs.microsoft.com/en-us/exchange/client-developer/exchange-server-development  
There are community supported versions of EWS to make it work with dotnet core:
https://github.com/sherlock1982/ews-managed-api

EWS will be decommissioned in 2020 as a method to access Office 365 (on-premise servers will continue to work)
as stated [here](http://techgenix.com/ews-no-updates/ ).  
It will be required to migrate to Microsoft Graph API.  

### Microsoft Graph API
The new way to programatically interact with Office is using [the Graph-API](https://docs.microsoft.com/en-us/graph/overview ). 
- Docs for [Outlook interaction](https://docs.microsoft.com/en-us/graph/api/resources/mail-api-overview?view=graph-rest-1.0 ).
- Api example to get emails: `https://graph.microsoft.com/v1.0/me/messages?$search="hello world"`
- Graph Explorer Utility [url](https://developer.microsoft.com/en-us/graph/graph-explorer ). 
- [PowerShel Microsoft Graph tutorial](https://www.youtube.com/watch?v=6CIZWac0TBE ).

Graph API requires you to grant access to the app from your office account,
that may require you to have admin privileges that your company may didn't give you.



### Not connecting to IMAP email:
> Message = A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond. 182.23.143.163:993

### SendKeys function only works in Windows:
> TODO: search for another solutions that are cross-platform.

https://www.reddit.com/r/csharp/comments/9dyf0t/having_my_program_send_an_enter_key_press/
Possible solutions:
- AutoHotkey.Interop: https://github.com/amazing-andrew/AutoHotkey.Interop
- InputSimulator:
```
Install-Package InputSimulator: https://github.com/michaelnoonan/inputsimulator
var sim = new InputSimulator();
sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
```


## Check if your company has enabled email protocols
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
