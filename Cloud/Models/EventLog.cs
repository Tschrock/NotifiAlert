using System.Text.Json.Serialization;

namespace NotifiAlert.Cloud.Models
{
    public class EventLog
    {
        [JsonPropertyName("DeviceEventId")]
        public int DeviceEventId { get; set; }

        [JsonPropertyName("ImageFile1Name")]
        public string ImageFile1Name { get; set; }

        [JsonPropertyName("ImageFile2Name")]
        public string ImageFile2Name { get; set; }

        [JsonPropertyName("VideoClipName")]
        public string VideoClipName { get; set; }

        [JsonPropertyName("ImageFile1Url")]
        public string ImageFile1Url { get; set; }

        [JsonPropertyName("ImageFile2Url")]
        public string ImageFile2Url { get; set; }

        [JsonPropertyName("VideoClipUrl")]
        public string VideoClipUrl { get; set; }

        [JsonPropertyName("ReadState")]
        public bool ReadState { get; set; }

        [JsonPropertyName("EventTime")]
        public string EventTime { get; set; }

        [JsonPropertyName("EventType")]
        public string EventType { get; set; }

        [JsonPropertyName("EventSource")]
        public string EventSource { get; set; }
    }
}

/*
[
    {
        "DeviceEventId": 59961945,
        "ImageFile1Name": "event59961945_image1.png",
        "ImageFile2Name": "event59961945_image2.png",
        "VideoClipName": null,
        "ImageFile1Url": "https://notifi-backend.azurewebsites.net/content/image/alert?eventId=59961945&imageNo=1",
        "ImageFile2Url": "https://notifi-backend.azurewebsites.net/content/image/alert?eventId=59961945&imageNo=2",
        "VideoClipUrl": null,
        "ReadState": false,
        "EventTime": "2021/06/20 21:45:16",
        "EventType": "Doorbell",
        "EventSource": "Push Button Accessory"
    }
]
*/
