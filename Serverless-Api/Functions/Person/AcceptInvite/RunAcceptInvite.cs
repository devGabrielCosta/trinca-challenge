using Domain.Events;
using Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Domain.Services.Interfaces;
using System.Net;

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        private readonly Person _user;
        private readonly IPersonService _personService;
        public RunAcceptInvite(IPersonService personService, Person user)
        {
            _user = user;
            _personService = personService;
        }

        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            var input = await req.Body<InviteAnswer>();
            if (input == null)
               return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");

            var person = await _personService.GetAsync(_user.Id);
            if (person == null)
               return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            var invite = person.Invites.FirstOrDefault(x => x.Id == inviteId);
            if (invite == null)
               return await req.CreateResponse(HttpStatusCode.BadRequest, "not invited for this bbq");
            if (invite.Status == InviteStatus.Accepted)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "invite already accepted");

            var inviteAccepted = new InviteWasAccepted { InviteId = inviteId, IsVeg = input.IsVeg, PersonId = person.Id };
            await _personService.InviteAcceptedAsync(person, inviteAccepted);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
