using FastEndpoints;

namespace ApiServiceTemplate.Domain
{
    public class TestCaseHandler : Endpoint<EmptyRequest>
    {
        public override void Configure()
        {
            Get("/test");
            AllowAnonymous();
        }
        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            await SendAsync("okay!", cancellation: ct);
        }
    }
}