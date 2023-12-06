using Domain.Events;
using Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using Domain.Services.Interfaces;
using static Serverless_Api.RunAcceptInvite;

namespace Serverless_Api
{
    public partial class RunDeclineInvite
    {
        private readonly Person _user;
        private readonly IPersonService _personService;
        public RunDeclineInvite(IPersonService personService, Person user)
        {
            _user = user;
            _personService = personService;
        }

        [Function(nameof(RunDeclineInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/decline")] HttpRequestData req, string inviteId)
        {
            var input = await req.Body<InviteAnswer>();
            if (input == null)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");

            var person = await _personService.GetAsync(_user.Id);
            if (person == null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            if (person.Invites.FirstOrDefault(x => x.Id == inviteId) == null)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "not invited for this bbq");

            var inviteDeclined = new InviteWasDeclined { InviteId = inviteId, PersonId = person.Id, IsVeg = input.IsVeg };
            await _personService.InviteDeclinedAsync(person, inviteDeclined);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
