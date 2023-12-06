using CrossCutting;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class BbqService : IBbqService
    {
        private readonly SnapshotStore _snapshots;
        private readonly IBbqRepository _bbqRepository;
        private readonly Lazy<IPersonService> _personService;
        public BbqService(IBbqRepository bbqRepository, Lazy<IPersonService> personService, SnapshotStore snapshots)
        {
            _snapshots = snapshots;
            _bbqRepository = bbqRepository;
            _personService = personService;
        }

        //Pela descrição da tarefa da lista de compras parecem querer avaliar como o candidato realizaria a busca tanto por id
        //quanto por outros parametros (uma consulta com filtros dinamicos),
        //se a busca fosse somente por id não seria tão estranho o código para pegar a lista de compras, não entendi se era necessário esse filtro a mais.
        //Eu também normalmente não colocaria isso aqui por linq e sim na consulta do banco de dados pelo repositorio
        //mas não encontrei uma forma de fazer isso sem passar por cima da interface de IEventStore utilizada na definição das dependências
        //E a forma como o banco fica estruturado também dificulta essa outra abordagem
        public async Task<IEnumerable<Bbq>> GetDynamicAsync(string personId, string? id = null, Dictionary<string, int>? shoppingList = null)
        {        
            var bbqs = new List<Bbq>();

            if (id != null)
            {
                var bbq = await _bbqRepository.GetAsync(id);
                if (bbq != null) bbqs.Add(bbq);
            }
            else
            {
                var person = await _personService.Value.GetAsync(personId);

                foreach (var invite in person?.Invites)
                {
                    var bbq = await _bbqRepository.GetAsync(invite.Id);
                    if (bbq != null) bbqs.Add(bbq);
                }
            }
               
            var filtredBbqs = bbqs.Where(i => i.Date > DateTime.Now && i.Status != BbqStatus.ItsNotGonnaHappen);
            if (shoppingList != null)
                filtredBbqs = filtredBbqs.Where(
                    i => i.ShoppingList.Keys.All(
                        key => shoppingList.ContainsKey(key) ? shoppingList[key] == i.ShoppingList[key] : true
                    )
                );

            return filtredBbqs;
        }

        public async Task<Bbq?> GetAsync(string id)
        {
            return await _bbqRepository.GetAsync(id);
        }

        public async IAsyncEnumerable<Bbq> GetAsync(IEnumerable<Invite> invites)
        {
            foreach (Invite invite in invites)
            {
                var bbq = await _bbqRepository.GetAsync(invite.Id);
                if(bbq != null)
                    yield return bbq;
            }
        }

        public async Task ModerateStatusAsync(Bbq bbq)
        {   
            if (bbq.Status == BbqStatus.PendingConfirmations)
                await bbqAccepted(bbq);
            if (bbq.Status == BbqStatus.ItsNotGonnaHappen)
                await bbqRejected(bbq);
            
            await SaveAsync(bbq);
        }

        private async Task bbqAccepted(Bbq bbq)
        {
            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            var @event = new PersonHasBeenInvitedToBbq(bbq.Id, bbq.Date, bbq.Reason);
            foreach (var personId in lookups.PeopleIds.Except(lookups.ModeratorIds))
            {
                var person = await _personService.Value.GetAsync(personId);
                person?.Apply(@event);
                await _personService.Value.SaveAsync(person);
            }
        }

        private async Task bbqRejected(Bbq bbq)
        {
            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            foreach (var personId in lookups.PeopleIds)
            {
                var person = await _personService.Value.GetAsync(personId);
                var @event = new InviteWasDeclined { InviteId = bbq.Id, PersonId = personId };
                person?.Apply(@event);
                await _personService.Value.SaveAsync(person);
            }
        }

        public async Task CreateAsync(Bbq bbq)
        {
            await SaveAsync(bbq);

            var Lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();
            foreach (var personId in Lookups.ModeratorIds)
            {
                var person = await _personService.Value.GetAsync(personId);
                if (person == null) continue;

                person.Apply(new PersonHasBeenInvitedToBbq(bbq.Id, bbq.Date, bbq.Reason));

                await _personService.Value.SaveAsync(person);
            }
        }

        public async Task SaveAsync(Bbq bbq)
        {
            await _bbqRepository.SaveAsync(bbq);
        }
    }
}
