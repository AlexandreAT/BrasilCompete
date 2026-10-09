namespace BrasilCompete.Worker.Commands;

/// <summary>Erro de uso na linha de comando, mostrado ao usuário junto com a ajuda.</summary>
public sealed class CommandLineException(string message) : Exception(message);
