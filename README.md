# esp32_Http_Server_network_relay

using hardware : esp32 devkit v1

to connect :

connect to WiFi ssid
to check HTTP POST use powershell :
Invoke-WebRequest -UseBasicParsing http://192.168.4.1/post -ContentType "application/json" -Method POST -Body "{""RelayState"":true}"
to check HTTP GET :
http://192.168.4.1/lpgstatus
or 
Invoke-WebRequest -uri http://192.168.4.1/lpgstatus -Method Get

Helpful websites :
https://randomnerdtutorials.com/esp32-client-server-wi-fi/