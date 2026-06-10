using BlazorApp_arduinoSearch_240824_01.Services;
using Xunit;

namespace BlazorApp_arduinoSearch_240824_01.Tests;

public class SensorPayloadParserTests
{
    [Fact]
    public void TryParseSoilMoisture_ParsesSnakeCasePayload()
    {
        const string payload = """
            {
              "temperature": 23.5,
              "humidity": 41.25,
              "soil_moisture": 782
            }
            """;

        var result = SensorPayloadParser.TryParseSoilMoisture(payload, out var reading, out var errorMessage);

        Assert.True(result, errorMessage);
        Assert.Equal(23.5f, reading.Temperature);
        Assert.Equal(41.25f, reading.Humidity);
        Assert.Equal(782, reading.SoilMoisture);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void TryParseSoilMoisture_ParsesAliasesAndStringNumbers()
    {
        const string payload = """
            {
              "temp": "21.5",
              "humid": "44.5",
              "soilMoisture": "615"
            }
            """;

        var result = SensorPayloadParser.TryParseSoilMoisture(payload, out var reading, out var errorMessage);

        Assert.True(result, errorMessage);
        Assert.Equal(21.5f, reading.Temperature);
        Assert.Equal(44.5f, reading.Humidity);
        Assert.Equal(615, reading.SoilMoisture);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void TryParseDhz_ParsesTemperatureAndHumidity()
    {
        const string payload = """
            {
              "temperature": 18.75,
              "humidity": 55.5
            }
            """;

        var result = SensorPayloadParser.TryParseDhz(payload, out var reading, out var errorMessage);

        Assert.True(result, errorMessage);
        Assert.Equal(18.75f, reading.Temperature);
        Assert.Equal(55.5f, reading.Humidity);
        Assert.Empty(errorMessage);
    }

    [Theory]
    [InlineData("", "empty")]
    [InlineData("[]", "object")]
    [InlineData("{not-json}", "valid JSON")]
    [InlineData("""{"temperature": 20}""", "humidity")]
    [InlineData("""{"temperature": 20, "humidity": 40, "soil_moisture": "wet"}""", "soil moisture")]
    public void TryParseSoilMoisture_RejectsInvalidPayloads(string payload, string expectedError)
    {
        var result = SensorPayloadParser.TryParseSoilMoisture(payload, out _, out var errorMessage);

        Assert.False(result);
        Assert.Contains(expectedError, errorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
