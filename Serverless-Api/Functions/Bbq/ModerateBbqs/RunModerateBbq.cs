using Domain.Entities;
using Domain.Events;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
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
            if (bbq == null)
                return req.CreateResponse(HttpStatusCode.NoContent);
            if (bbq.Status == BbqStatus.ItsNotGonnaHappen)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "event already cancelled");
            if ((bbq.Status == BbqStatus.PendingConfirmations || bbq.Status == BbqStatus.Confirmed) && moderationRequest.GonnaHappen != false)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "event already accepted");

            bbq.Apply(new BbqStatusUpdated(moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay));

            await _bbqService.ModerateStatusAsync(bbq);

            return await req.CreateResponse(HttpStatusCode.OK, bbq.TakeSnapshot());
        }
    }
}
