using CrossCutting;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        private readonly SnapshotStore _snapshots;
        private readonly IPersonRepository _persons;
        private readonly IBbqRepository _repository;

        public RunModerateBbq(IBbqRepository repository, SnapshotStore snapshots, IPersonRepository persons)
        {
            _persons = persons;
            _snapshots = snapshots;
            _repository = repository;
        }

        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            var moderationRequest = await req.Body<ModerateBbqRequest>();

            var bbq = await _repository.GetAsync(id);

            bbq.Apply(new BbqStatusUpdated(moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay));

            if (moderationRequest.GonnaHappen)
                await bbqAccepted(bbq);
            else
                await bbqRejected(bbq);

            await _repository.SaveAsync(bbq);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
        }

        private async Task bbqAccepted(Bbq bbq)
        {
            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            foreach (var personId in lookups.PeopleIds.Except(lookups.ModeratorIds))
            {
                var person = await _persons.GetAsync(personId);
                var @event = new PersonHasBeenInvitedToBbq(bbq.Id, bbq.Date, bbq.Reason);
                person.Apply(@event);
                await _persons.SaveAsync(person);
            }
        }

        private async Task bbqRejected(Bbq bbq)
        {
            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            foreach (var personId in lookups.PeopleIds)
            {
                var person = await _persons.GetAsync(personId);
                var @event = new InviteWasDeclined { InviteId = bbq.Id, PersonId = personId };
                person.Apply(@event);
                await _persons.SaveAsync(person);
            }
        }
    }
}
