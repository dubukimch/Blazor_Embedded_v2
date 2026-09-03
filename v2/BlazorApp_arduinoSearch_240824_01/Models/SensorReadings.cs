namespace BlazorApp_arduinoSearch_240824_01.Models;

public readonly record struct DhzSensorReading(float Temperature, float Humidity);

public readonly record struct SoilMoistureSensorReading(float Temperature, float Humidity, int SoilMoisture);
