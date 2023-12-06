using System.ComponentModel;
using System.Net;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api.Functions.BuyList.GetBuyList
{
    public class RunGetBuyList
    {
        private readonly Person _user;
        private readonly IBbqRepository _bbqRepository;
        private readonly IPersonRepository _personRepository;
        public RunGetBuyList(IPersonRepository personRepository, IBbqRepository bbqRepository, Person user)
        {
            _user = user;
            _bbqRepository = bbqRepository;
            _personRepository = personRepository;
        }

        [Function(nameof(RunGetBuyList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "listaCompras")] HttpRequestData req)
        {
            var input = await req.Body<GetBuyListRequest>();
            if (input == null)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required");

            var person = await _personRepository.GetAsync(_user.Id);

            if (!person.IsCoOwner)
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            //Pela descrição da tarefa da lista de compras parecem querer avaliar como o candidato realizaria a busca tanto por id
            //quanto por outros parametros (uma consulta com filtros dinamicos),
            //se a busca fosse somente por id não seria tão estranho o código, não entendi se era necessário esse filtro a mais.
            //Eu Também normalmente não colocaria isso aqui por linq e sim na consulta do banco de dados pelo repositorio
            //mas não encontrei uma forma de fazer isso sem passar por cima da interface de IEventStore utilizada na definição das dependências
            //E a forma como o banco fica estruturado também dificulta essa outra abordagem
            var bbqs = new List<Bbq>();

            if (input.Id != null)
                bbqs.Add(await _bbqRepository.GetAsync(input.Id));
            else
                foreach (var invite in person.Invites)
                    bbqs.Add(await _bbqRepository.GetAsync(invite.Id));

            var querrybbqs = bbqs.Where(i => i.Date > DateTime.Now);
            if (input.BuyList != null)
                bbqs = bbqs.Where(i => i.BuyList == input.BuyList).ToList();

            var snapshots = new List<object>();
            foreach (var bbq in querrybbqs)
                snapshots.Add(new
                {
                    Id = bbq.Id,
                    BuyList = bbq.BuyList.Select(x => new KeyValuePair<string, decimal>(x.Key, Math.Round((decimal)x.Value / 1000, 2)))
                });

            return await req.CreateResponse(HttpStatusCode.Created, snapshots);
        }
    }
}
