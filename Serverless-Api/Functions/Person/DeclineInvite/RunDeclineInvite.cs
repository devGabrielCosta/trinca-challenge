using Domain;
using Eveneum;
using CrossCutting;
using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using static Domain.ServiceCollectionExtensions;
using static Serverless_Api.RunAcceptInvite;
using System.Net;

namespace Serverless_Api
{
    public partial class RunDeclineInvite
    {
        private readonly Person _user;
        private readonly IPersonRepository _personRepository;
        private readonly IBbqRepository _bbqRepository;

        public RunDeclineInvite(Person user, IPersonRepository personRepository, IBbqRepository bbqRepository)
        {
            _user = user;
            _personRepository = personRepository;
            _bbqRepository = bbqRepository;
        }

        [Function(nameof(RunDeclineInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/decline")] HttpRequestData req, string inviteId)
        {
            var input = await req.Body<InviteAnswer>();
            if (input == null)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");

            var person = await _personRepository.GetAsync(_user.Id);
            if (person == null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            var inviteDeclined = new InviteWasDeclined { InviteId = inviteId, PersonId = person.Id, IsVeg = input.IsVeg};

            if (person.Invites.First(x => x.Id == inviteId).Status == InviteStatus.Accepted)
            {
                var bbq = await _bbqRepository.GetAsync(inviteId);
                bbq.Apply(inviteDeclined);
                await _bbqRepository.SaveAsync(bbq);
            }
            person.Apply(inviteDeclined);
            await _personRepository.SaveAsync(person);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
