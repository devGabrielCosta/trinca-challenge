using System.Net;
using Domain.Events;
using Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Services.Interfaces;

namespace Serverless_Api
{
    public partial class RunCreateNewBbq
    {
        private readonly IBbqService _bbqService;
        public RunCreateNewBbq(IBbqService bbqService)
        {
            _bbqService = bbqService;
        }

        [Function(nameof(RunCreateNewBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "churras")] HttpRequestData req)
        {
            var input = await req.Body<NewBbqRequest>();

            if (input == null)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");

            var churras = new Bbq();
            churras.Apply(new ThereIsSomeoneElseInTheMood(Guid.NewGuid(), input.Date, input.Reason, input.IsTrincasPaying));

            await _bbqService.CreateAsync(churras);

            return await req.CreateResponse(HttpStatusCode.Created, churras.TakeSnapshot());
        }
    }
}
