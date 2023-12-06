using System.Net;
using Domain.Entities;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetProposedBbqs
    {
        private readonly Person _user;
        private readonly IBbqService _bbqService;
        private readonly IPersonService _personService;
        public RunGetProposedBbqs(IBbqService bbqService, IPersonService personService, Person user)
        {
            _bbqService = bbqService;
            _personService = personService;
            _user = user;
        }

        [Function(nameof(RunGetProposedBbqs))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras")] HttpRequestData req)
        {
            var snapshots = new List<object>();

            var user = await _personService.GetAsync(_user.Id);
            await foreach (var bbq in _bbqService.GetAsync(user.Invites))
                if(bbq.Status != BbqStatus.ItsNotGonnaHappen && bbq?.Date > DateTime.Now) 
                    snapshots.Add(bbq.TakeSnapshot());

            return await req.CreateResponse(HttpStatusCode.Created, snapshots);
        }
    }
}
