using BrasilCompete.Worker.Domain;

namespace BrasilCompete.Worker.Identity;

/// <summary>Registro encontrado para um participante. Casamento só por nome tem baixa confiança.</summary>
public sealed record IdentityMatch(IdentityRecord Record, Confidence Confidence);
