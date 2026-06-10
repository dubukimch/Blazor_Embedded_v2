using BlazorApp_arduinoSearch_240824_01.Services;
using Xunit;

namespace BlazorApp_arduinoSearch_240824_01.Tests;

public class MqttQueryStringParserTests
{
    [Fact]
    public void TryParse_ReturnsDecodedConnectionParameters()
    {
        var uri = "https://localhost/soilMoistureChart?mqttAddress=broker.local&mqttPort=1883&mqttTopic=sensors%2Fsoil";

        var result = MqttQueryStringParser.TryParse(uri, out var parameters, out var errorMessage);

        Assert.True(result, errorMessage);
        Assert.Equal("broker.local", parameters.Server);
        Assert.Equal(1883, parameters.Port);
        Assert.Equal("sensors/soil", parameters.Topic);
        Assert.Empty(errorMessage);
    }

    [Theory]
    [InlineData("https://localhost/chart?mqttPort=1883&mqttTopic=sensors/soil", "서버")]
    [InlineData("https://localhost/chart?mqttAddress=broker.local&mqttPort=0&mqttTopic=sensors/soil", "포트")]
    [InlineData("https://localhost/chart?mqttAddress=broker.local&mqttPort=70000&mqttTopic=sensors/soil", "포트")]
    [InlineData("https://localhost/chart?mqttAddress=broker.local&mqttPort=1883", "토픽")]
    public void TryParse_RejectsMissingOrInvalidParameters(string uri, string expectedError)
    {
        var result = MqttQueryStringParser.TryParse(uri, out var parameters, out var errorMessage);

        Assert.False(result);
        Assert.Contains(expectedError, errorMessage);
        Assert.Equal(string.Empty, parameters.Server);
        Assert.Equal(0, parameters.Port);
        Assert.Equal(string.Empty, parameters.Topic);
    }
}
