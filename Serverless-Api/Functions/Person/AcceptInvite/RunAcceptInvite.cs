using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Linq;

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        private readonly Person _user;
        private readonly IPersonRepository _personRepository;
        private readonly IBbqRepository _bbqRepository;
        public RunAcceptInvite(IPersonRepository personRepository, IBbqRepository bbqRepository, Person user)
        {
            _user = user;
            _personRepository = personRepository;
            _bbqRepository = bbqRepository;
        }

        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            var answer = await req.Body<InviteAnswer>();

            var person = await _personRepository.GetAsync(_user.Id);

            var inviteAccepted = new InviteWasAccepted { InviteId = inviteId, IsVeg = answer.IsVeg, PersonId = person.Id };
            person.Apply(inviteAccepted);
            await _personRepository.SaveAsync(person);

            var bbq = await _bbqRepository.GetAsync(inviteId);
            bbq.Apply(inviteAccepted);
            await _bbqRepository.SaveAsync(bbq);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
