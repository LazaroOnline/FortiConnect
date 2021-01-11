
# User's Guide

## Requirements
- DotNet 5.0 Runtime ([url](https://dotnet.microsoft.com/download))
- Windows (on Mac or Linux some functionality like  may still work).


## Configuration
First you need to configure the application with your user information.  
The configuration is stored in a file named `AppSettings.json` placed next to the `FortiConnect.exe`.  
Another config file is `AppSettings.AutoSave.json` which contains only the part of the config that can be edited from the UI,
and overrides the other config files when a value is present.

 Config parameter                     | Description
--------------------------------------|---------------------------------------------------------------
 FortiClient > ExeFullPath            | Path to the installed FortiClient.
 FortiClient > ProcessName            | Name of the FortiClient executable.
 Vpn > Password                       | Your personal VPN user password.
 Vpn > LoginPasswordFocusSequence     | Keystroke sequence\* used after launching FotiClient, to set the password textbox into focus. It can be used to change the VPN configuration dropdown by using for example `"LoginPasswordFocusSequence": "{TAB}{TAB}{DOWN}{TAB}{TAB}"`, or set/change the username with `"{TAB}{TAB}{TAB}MyUserName{TAB}"`.
 Vpn > LoginVerificationFocusSequence | Keystroke sequence\* used after entering the vpn password, to set the confirmation code textbox into focus.
 EmailServer > Protocol               | Protocol used to connect to your email.
 EmailServer > Server                 | Name of the server or url used to connect, ie: `imap.mycompany.com`, `pop.mycompany.com` or Exchange URL in case your company is not using the default `https://mail.{companyDomain}/EWS/Exchange.asmx`.
 EmailServer > Port                   | Port used to connect to the email.
 EmailAccount > Email                 | Email account ie: your-email@company.com
 EmailAccount > Password              | Email password.
 EmailAccount > MarkVpnEmailAsRead    | Set to true to mark the vpn email as "read" in your mailbox.
 DelayToSpawnFortiClientProcess       | Time in milliseconds to wait for a newly created FortiClient process to load it's UI before sending keystrokes to write to it.
 DelayToShowVpnClient                 | Time in milliseconds to wait for the Operating System to show and bring to the front the FortiClient window before sending keystrokes to write to it.

> \* **Keystroke sequences**:  
> Leave not set or null to use the default sequence corresponding to your installed FortiClient version.  
> To configure the keystroke sequences follow `SendKeys` 
> [Microsoft's documentation](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys).

## Execution

- `Get VPN Email Code` button:  
 It will just get the VPN email code and copy it into your clipboard. 
 This feature is there to provide some minimum level of functionality in case the full automation to connect to the VPN doesn't work.

- `Connect to VPN` button:  
It will open a FortiClient window, or reuse the existing one if already open,
then it will bring the window to the front active window and automatically enter your VPN password. 
The VPN user's name is not entered because FortiClient can be set to remember it.  
After that it will search your emails looking for the 2-Factor-Authentication email with the one-time VPN code and write it back in the FortiClient app.

> It is important not to interact with the computer while this app is connecting to the VPN or otherwise it may not work.  
> This is due to the nature of the technology used to automate the connection,
> that requires the VPN Client window to be active while sending the keystrokes.
> Otherwise if you accidentally change the active window during the process,
> the other application will receive the keyboard inputs.


### Console Mode
This app can run in console mode if any of the console actions are passed as parameters:
- `FortiConnect.exe -GetEmailVpnCode`
- `FortiConnect.exe -LoginToVpn`


## Check available email protocols in your company
To configure the app properly you need to select an email protocol that must be enable by your email provider.  
If your company is using an on-premise Outlook, chances are that `Exchange` protocol will be available.  
To check what protocols are enabled see
[using-telnet](https://inthetechpit.com/2019/07/23/telnet-to-test-connection-to-pop3-imap ) and 
[Microsoft-docs](https://docs.microsoft.com/en-us/exchange/mail-flow/test-smtp-with-telnet ):  
```ps1
TELNET mail.mycompany.com  25		# SMTP   no-SSL (or Microsoft Exchange).
TELNET mail.mycompany.com 465		# SMTP with SSL.
TELNET mail.mycompany.com 587		# SMTP with TLS.
TELNET imap.mycompany.com 143		# IMAP   no-SSL.
TELNET imap.mycompany.com 993		# IMAP with SSL.
TELNET  pop.mycompany.com 110		# POP3   no-SSL.
TELNET  pop.mycompany.com 995		# POP3 with SSL.
USER my.email@MyCompany.com
PASS yourEmailP4$w0rd
LIST
QUIT
```
