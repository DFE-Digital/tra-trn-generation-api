namespace TrnGeneratorApi.Requests;

public record CreateTrnRangeRequest
{
    public required int FromTrn { get; init; }

    public required int ToTrn { get; init; }
}
