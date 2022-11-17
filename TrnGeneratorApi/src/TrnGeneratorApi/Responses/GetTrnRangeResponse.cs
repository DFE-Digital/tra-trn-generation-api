namespace TrnGeneratorApi.Responses;

public record GetTrnRangeResponse
{
    public required int FromTrn { get; init; }

    public required int ToTrn { get; init; }

    public required int NextTrn { get; init; }

    public required bool IsExhausted { get; init; }
}
