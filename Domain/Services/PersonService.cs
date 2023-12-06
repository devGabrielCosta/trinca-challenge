using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Domain.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services
{
    internal class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly Lazy<IBbqService> _bbqService;
        public PersonService(IPersonRepository personRepository, Lazy<IBbqService> bbqService)
        {
            _personRepository = personRepository;
            _bbqService = bbqService;
        }
        public async Task<Person?> GetAsync(string id)
        {
            return await _personRepository.GetAsync(id);
        }

        public async Task SaveAsync(Person person)
        {
            await _personRepository.SaveAsync(person);
        }
        public async Task InviteAcceptedAsync(Person person, InviteWasAccepted inviteResponse)
        {
            var bbq = await _bbqService.Value.GetAsync(inviteResponse.InviteId);
            if (bbq?.Status == BbqStatus.ItsNotGonnaHappen)
                return;

            person.Apply(inviteResponse);
            await SaveAsync(person);

            bbq?.Apply(inviteResponse);
            await _bbqService.Value.SaveAsync(bbq);
        }

        public async Task InviteDeclinedAsync(Person person, InviteWasDeclined inviteResponse)
        {
            if (person.Invites.First(x => x.Id == inviteResponse.InviteId).Status == InviteStatus.Accepted)
            {
                var bbq = await _bbqService.Value.GetAsync(inviteResponse.InviteId);
                bbq?.Apply(inviteResponse);
                await _bbqService.Value.SaveAsync(bbq);
            }
            person.Apply(inviteResponse);
            await SaveAsync(person);
        }
    }
}
