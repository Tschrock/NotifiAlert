namespace NotifiAlert.Doorbell
{
    /// <summary>
    /// The device command.
    /// NOTE: The app source has the id for 'SendCommandType' and 'GetMacAddress' swapped when reading.
    /// </summary>
    public enum Command
    {
        StartCommunication = 0, // StartCommunication
        SendWifiSSID = 16, // SendWifiSSID
        SendWifiPWD = 17, // SendWifiPWD
        SendDeviceName = 18, // SendDeviceName
        SendCloudID = 19, // SendCloudID
        SendGMT = 20, // SendTimeZone
        SendServer = 21, // SendDebugMode
        SendCommandType = 22,
        GetWifiConnectionStatus = 64, // GetWifiConnectStatus
        GetScannedWifiSSIDNum = 65, // GetScannedWifiSsidNum
        GetScannedWifiSSID = 66, // GetScannedWifiSsid
        GetMacAddress = 67 // GetMacAddress
        EndCommunication = -1, // EndCommunication
        NoCommand = -1,
    }
}
