using System.Globalization;
using System.Web;
using BlazorApp_arduinoSearch_240824_01.Models;

namespace BlazorApp_arduinoSearch_240824_01.Services;

public static class MqttQueryStringParser
{
    public static bool TryParse(
        string uri,
        out MqttConnectionParameters parameters,
        out string errorMessage)
    {
        parameters = MqttConnectionParameters.Empty;
        errorMessage = string.Empty;

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
        {
            errorMessage = "MQTT connection URL is invalid.";
            return false;
        }

        var query = HttpUtility.ParseQueryString(parsedUri.Query);
        var server = query["mqttAddress"]?.Trim() ?? string.Empty;
        var portText = query["mqttPort"]?.Trim() ?? string.Empty;
        var topic = query["mqttTopic"]?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(server))
        {
            errorMessage = "MQTT server address is required.";
            return false;
        }

        if (!int.TryParse(portText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) ||
            port is < 1 or > 65535)
        {
            errorMessage = "MQTT port must be between 1 and 65535.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            errorMessage = "MQTT topic is required.";
            return false;
        }

        parameters = new MqttConnectionParameters(server, port, topic);
        return true;
    }
}
