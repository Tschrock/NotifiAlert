
namespace NotifiAlert
{
    /// <summary>
    /// Information about a WiFi network.
    /// </summary>
    public class WifiInfo
    {
        /// <summary>
        /// The name of the network.
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// The signal strength.
        /// </summary>
        /// <value>An Integer representing the signal strength on a scale of 0 to 100.</value>
        public int SignalPercent { get; set; }

        /// <summary>
        /// If the network is secured.
        /// </summary>
        /// <value>A Boolean indicating if the network is secured.</value>
        public bool IsSecured { get; set; }

        public WifiInfo(string name, int signalPercent, bool isSecured)
        {
            Name = name;
            SignalPercent = signalPercent;
            IsSecured = isSecured;
        }
    }
}