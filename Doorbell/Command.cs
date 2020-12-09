namespace NotifiAlert.Doorbell
{
    /// <summary>
    /// The device command.
    /// NOTE: The app source has the id for 'SendCommandType' and 'GetMacAddress' swapped when reading.
    /// </summary>
    public enum Command
    {
        StartCommunication = 0,
        SendWifiSSID = 16,
        SendWifiPWD = 17,
        SendDeviceName = 18,
        SendCloudID = 19,
        SendGMT = 20,
        SendCommandType = 22,
        SendServer = 21,
        GetWifiConnectionStatus = 64,
        GetScannedWifiSSIDNum = 65,
        GetScannedWifiSSID = 66,
        EndCommunication = -1,
        NoCommand = -1,
        GetMacAddress = 67
    }
}
