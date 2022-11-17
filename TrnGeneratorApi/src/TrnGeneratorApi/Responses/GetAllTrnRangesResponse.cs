namespace TrnGeneratorApi.Responses;

public record GetAllTrnRangesResponse
{
    public required IEnumerable<GetTrnRangeResponseBody> TrnRanges { get; init; }
}

public record GetTrnRangeResponseBody
{
    public required int FromTrn { get; init; }

    public required int ToTrn { get; init; }

    public required int NextTrn { get; init; }

    public required bool IsExhausted { get; init; }
}
