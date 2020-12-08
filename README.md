NotifiAlert
===========

A dotnet program for interacting with the `Heath Zenith Sl-3011-00 Notifi Plug-In Doorbell (Model WLTRX3011)`.

I'm writing this because
1. Their android app is a giant pile of crap.
2. I want to know the security of the things on my network (Hint: It's not good. Welcome to the Internet of S**t).
3. Taking things apart is fun.


Fun fact: This device sells for ~$70 at Lowes and Walmart despite having only a dollar or two's worth of components.

## Device Info

FCC Docs: <https://fccid.io/BJ4-WLTRX3011>

Internal Images:
 - [Board Front](https://cdn.discordapp.com/attachments/383739569356537881/785538075924889600/PXL_20201207_155937072.jpg)
 - [Board Back](https://cdn.discordapp.com/attachments/383739569356537881/785538076416016434/PXL_20201207_160137429.jpg)
 - [Chip Closeup 1](https://cdn.discordapp.com/attachments/383739569356537881/785538076985786418/PXL_20201207_160348249.jpg)
 - [Chip Closeup 2](https://cdn.discordapp.com/attachments/383739569356537881/785538078268719134/PXL_20201207_160559881.jpg)
 - [Chip Closeup 3](https://cdn.discordapp.com/attachments/383739569356537881/785538078798249984/PXL_20201207_160610534.jpg)
 - [Chip Closeup 4](https://cdn.discordapp.com/attachments/383739569356537881/785538079393185792/PXL_20201207_160703703.jpg)

## Communication Protocol

The device communicates on TCP port 12345 using what looks like a custom
protocol.

### Data Packets

Data packets consist of a Command, Command Data, Status, and CRC.

| Field     | Size (bytes) | Description                  |
|-----------|--------------|------------------------------|
| Command   | 1            | The command id               |
| Data Size | 1            | The size of the [Data] field |
| Data      | variable     | The command data             |
| Status    | 1            | The status of the command¹    |
| CRC       | 2            | The CRC-16 of the packet²     |

1. This field is always `0` for commands sent from the app, and appears to
always be `0` in responses.
2. The app doesn't check the CRC of responses, it's unknown if the device
itself does.

### Commands

| Command                 | Id |
|-------------------------|----|
| StartCommunication      | 0  |
| SendWifiSSID            | 16 |
| SendWifiPWD             | 17 |
| SendDeviceName          | 18 |
| SendCloudID             | 19 |
| SendGMT                 | 20 |
| SendCommandType         | 22 |
| SendServer              | 21 |
| GetWifiConnectionStatus | 64 |
| GetScannedWifiSSIDNum   | 65 |
| GetScannedWifiSSID      | 66 |
| EndCommunication        | -1 |
| NoCommand               | -1 |
| GetMacAddress           | 67 |

### Packet Transmission

Packets are encrypted using AES128 (CBC) and then encoded as base64.

### Auth Handshake

1. Client sends a `StartCommunication` command encrypted using a hardcoded AES key and blank IV.
2. Device responds with a new AES key to use for future commands.