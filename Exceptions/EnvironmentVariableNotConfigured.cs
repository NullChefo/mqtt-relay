using Microsoft.OpenApi.Expressions;

namespace MqttRelay.Exceptions;

public class EnvironmentVariableNotConfigured : Exception
{
   public EnvironmentVariableNotConfigured() : base("Environment variables is not configured."){}
   
   // try to fix with message templates
   public EnvironmentVariableNotConfigured(string variableName) : base($"Environment variable {variableName} is not configured."){}
   
   
}