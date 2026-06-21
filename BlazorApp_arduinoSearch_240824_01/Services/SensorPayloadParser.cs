using System.Globalization;
using System.Text.Json;
using BlazorApp_arduinoSearch_240824_01.Models;

namespace BlazorApp_arduinoSearch_240824_01.Services;

public static class SensorPayloadParser
{
    private static readonly HashSet<string> TemperatureNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "temperature",
        "temp"
    };

    private static readonly HashSet<string> HumidityNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "humidity",
        "humid"
    };

    private static readonly HashSet<string> SoilMoistureNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "soil_moisture",
        "soilMoisture",
        "moisture"
    };

    public static bool TryParseDhz(
        string payload,
        out DhzSensorReading reading,
        out string errorMessage)
    {
        reading = default;

        if (!TryParseObject(payload, out var root, out errorMessage))
        {
            return false;
        }

        using var document = root;
        var rootElement = document.RootElement;

        if (!TryReadSingle(rootElement, TemperatureNames, "temperature", out var temperature, out errorMessage) ||
            !TryReadSingle(rootElement, HumidityNames, "humidity", out var humidity, out errorMessage))
        {
            return false;
        }

        reading = new DhzSensorReading(temperature, humidity);
        return true;
    }

    public static bool TryParseSoilMoisture(
        string payload,
        out SoilMoistureSensorReading reading,
        out string errorMessage)
    {
        reading = default;

        if (!TryParseObject(payload, out var root, out errorMessage))
        {
            return false;
        }

        using var document = root;
        var rootElement = document.RootElement;

        if (!TryReadSingle(rootElement, TemperatureNames, "temperature", out var temperature, out errorMessage) ||
            !TryReadSingle(rootElement, HumidityNames, "humidity", out var humidity, out errorMessage) ||
            !TryReadInt32(rootElement, SoilMoistureNames, "soil moisture", out var soilMoisture, out errorMessage))
        {
            return false;
        }

        reading = new SoilMoistureSensorReading(temperature, humidity, soilMoisture);
        return true;
    }

    private static bool TryParseObject(
        string payload,
        out JsonDocument document,
        out string errorMessage)
    {
        document = null!;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(payload))
        {
            errorMessage = "Sensor payload is empty.";
            return false;
        }

        try
        {
            document = JsonDocument.Parse(payload);

            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                return true;
            }

            document.Dispose();
            errorMessage = "Sensor payload must be a JSON object.";
            return false;
        }
        catch (JsonException ex)
        {
            errorMessage = $"Sensor payload is not valid JSON: {ex.Message}";
            return false;
        }
    }

    private static bool TryReadSingle(
        JsonElement root,
        IReadOnlySet<string> names,
        string displayName,
        out float value,
        out string errorMessage)
    {
        value = 0;
        errorMessage = string.Empty;

        if (!TryFindProperty(root, names, out var property))
        {
            errorMessage = $"Sensor payload is missing {displayName}.";
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number &&
            property.TryGetSingle(out value) &&
            float.IsFinite(value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.String &&
            float.TryParse(property.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
            float.IsFinite(value))
        {
            return true;
        }

        errorMessage = $"Sensor payload has an invalid {displayName} value.";
        return false;
    }

    private static bool TryReadInt32(
        JsonElement root,
        IReadOnlySet<string> names,
        string displayName,
        out int value,
        out string errorMessage)
    {
        value = 0;
        errorMessage = string.Empty;

        if (!TryFindProperty(root, names, out var property))
        {
            errorMessage = $"Sensor payload is missing {displayName}.";
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.Number &&
            property.TryGetDouble(out var doubleValue) &&
            TryConvertWholeNumber(doubleValue, out value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            var text = property.GetString();

            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return true;
            }

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble) &&
                TryConvertWholeNumber(parsedDouble, out value))
            {
                return true;
            }
        }

        errorMessage = $"Sensor payload has an invalid {displayName} value.";
        return false;
    }

    private static bool TryConvertWholeNumber(double doubleValue, out int value)
    {
        value = 0;

        if (!double.IsFinite(doubleValue) ||
            doubleValue < int.MinValue ||
            doubleValue > int.MaxValue ||
            Math.Abs(doubleValue - Math.Round(doubleValue)) >= 0.0001)
        {
            return false;
        }

        value = (int)Math.Round(doubleValue);
        return true;
    }

    private static bool TryFindProperty(
        JsonElement root,
        IReadOnlySet<string> names,
        out JsonElement value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (names.Contains(property.Name))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
