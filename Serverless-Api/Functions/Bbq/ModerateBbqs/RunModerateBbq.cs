using Domain.Entities;
using Domain.Events;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        private readonly IBbqService _bbqService;
        public RunModerateBbq(IBbqService bbqService)
        {
            _bbqService = bbqService;
        }

        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            var moderationRequest = await req.Body<ModerateBbqRequest>();

            var bbq = await _bbqService.GetAsync(id);

            bbq.Apply(new BbqStatusUpdated(moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay));

            await _bbqService.ModerateStatusAsync(bbq);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
        }
    }
}
