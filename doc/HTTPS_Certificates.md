# Enabling HTTPS

To run the server and tests with HTTPS endpoints locally, you must trust the ASP.NET Core development certificate on your machine. 

1. **Trust the ASP.NET Core development certificate (localhost):**

This allows your server to listen on `https://localhost` without browser or client warnings (For Local Development).

	Open a terminal and run:

	```sh
	dotnet dev-certs https --trust
	```

	- On Windows, this will prompt you to trust the certificate. Click 'Yes' to confirm.
	- The certificate is stored in your user profile's certificate store (not as a file in your project).
	- You can view it in the Windows Certificate Manager under `Current User > Personal > Certificates` as `ASP.NET Core HTTPS development certificate` (certmgr.msc ).   

	By default, ASP.NET Core uses this certificate for HTTPS on localhost. 

	Access your API using https://localhost (not an IP address or other hostname).

The certificate is only valid for localhost, not for 127.0.0.1 or other names.

2. **Use HTTPS with a custom hostname (e.g., mydns.local):**

   1. Generate a self-signed certificate for your hostname:
Open PowerShell as Administrator and run:

```sh
$cert = New-SelfSignedCertificate -DnsName "mydns.local" -CertStoreLocation "cert:\LocalMachine\My" -TextExtension @("2.5.29.17={text}dns=mydns.local&ipaddress=192.168.10.1&ipaddress=fd00:10::1")
$password = ConvertTo-SecureString -String "P@ssw0rd!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "C:\mydns.local.pfx" -Password $password
```

   2. Trust the certificate:

      Open mmc.exe, add the “Certificates” snap-in for “Local Computer”.
Import the generated .pfx into “Trusted Root Certification Authorities”.

or via powershell

```sh
Export-Certificate -Cert $cert -FilePath "C:\mydns.local.cer"
Import-Certificate -FilePath "C:\mydns.local.cer" -CertStoreLocation "cert:\LocalMachine\Root"
```

   3. Add the hostname to your hosts file:

      On Windows, edit 'C:\Windows\System32\drivers\etc\hosts' and add:
127.0.0.1 mydns.local
```

   4. Start the server with arguments specifying the certificate path and password:

```sh
DualstackDnsServer.exe --cert "C:\mydns.local.pfx" --certPassw "P@ssw0rd!"
```