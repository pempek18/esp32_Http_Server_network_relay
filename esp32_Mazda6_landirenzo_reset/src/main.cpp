// Import required libraries
#include "WiFi.h"
#include "ESPAsyncWebServer.h"

#include <Wire.h>

#define LED 2
#define RELAY 4
#define ResetTime 30
#define SerialDebug false

// Set your access point network credentials
const char* ssid = "ESP32-Access-Point";
const char* password = "123456789";

// Create AsyncWebServer object on port 80
AsyncWebServer server(80);

//global variables
bool RelayState = false;
int TimeElapsed = 0;

String relayStatus (bool status, int elapsed)
{
  String msg;
  if (RelayState)
  {
    msg = String("Sterownik gazowy jest w trakcie resetu, pozostalo : ");
    msg = String(msg + elapsed);
    msg = String(msg + "s");
    return msg;
  } else
  {
    return String("Sterownik gazowy jest wlaczony");
  }
}

void setup(){
  // Serial port for debugging purposes
  if (SerialDebug)
  {
    Serial.begin(9600);
    Serial.println();
    // Setting the ESP as an access point
    Serial.print("Setting AP (Access Point)…");
    // Remove the password parameter, if you want the AP (Access Point) to be open
  }
  WiFi.softAP(ssid, password);

  IPAddress IP = WiFi.softAPIP();

    if (SerialDebug)
  {
    Serial.print("AP IP address: ");
    Serial.println(IP);
  }

  //pins
  pinMode(LED, OUTPUT);
  pinMode(RELAY, OUTPUT);

  server.on("/lpgstatus", HTTP_GET, [](AsyncWebServerRequest *request){
    request->send_P(200, "text/plain", relayStatus(RelayState, TimeElapsed).c_str() );
  });

  server.on(
    "/post",
    HTTP_POST,
    [](AsyncWebServerRequest * request){},
    NULL,
    [](AsyncWebServerRequest * request, uint8_t *data, size_t len, size_t index, size_t total) {
      
      char msgRecived[30];
      String msg;
      for (size_t i = 0; i < len; i++) {
        
        msgRecived[i] = data[i];
      }

      msg = msgRecived;

      if (msg == "{'RelayState':true}")
      {
        RelayState = true;
        if (SerialDebug)
        {
          Serial.println("RelayState jest równe TRUE");
        }
      } else
      {
        if (SerialDebug)
        {
          Serial.println(msgRecived);
        }
      }
 
      request->send(200);
  });
  // Start server
  server.begin();
}
 
void loop(){
  if (RelayState)
  {
    digitalWrite(LED,HIGH);
    digitalWrite(RELAY,HIGH);
    for (TimeElapsed = ResetTime ; TimeElapsed >= 1; TimeElapsed-- )
    {
      if (SerialDebug)
      {
        Serial.println(relayStatus(RelayState, TimeElapsed));
      }
      delay(1000);
    }
    digitalWrite(LED,LOW);
    digitalWrite(RELAY,LOW);
    RelayState = false;
  } 
}