using System.Net;
using Domain.Entities;
using Domain.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api.Functions.BuyList.GetBuyList
{
    public class RunGetBuyList
    {
        private readonly Person _user;
        private readonly IBbqService _bbqService;
        private readonly IPersonService _personService;
        public RunGetBuyList(IBbqService bbqService, IPersonService personService, Person user)
        {
            _user = user;
            _bbqService = bbqService;
            _personService = personService;
        }

        [Function(nameof(RunGetBuyList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "listaCompras")] HttpRequestData req)
        {
            var input = await req.Body<GetBuyListRequest>();
            if (input == null)
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required");

            var person = await _personService.GetAsync(_user.Id);

            if (!person.IsCoOwner)
                return req.CreateResponse(HttpStatusCode.Unauthorized);

            var bbqs = await _bbqService.GetDynamicAsync(person.Id, input.Id, input.BuyList);
            return await req.CreateResponse(HttpStatusCode.Created, ConvertBuyListWeigth(bbqs));
        }

        private IEnumerable<object> ConvertBuyListWeigth(IEnumerable<Bbq> bbqs)
        {
            foreach (var bbq in bbqs)
                yield return new {
                    Id = bbq.Id,
                    BuyList = bbq.BuyList
                        .Select(
                            x => new {
                                x.Key,
                                Value = Math.Round((decimal)x.Value / 1000, 2)
                            }
                        ).ToDictionary(x => x.Key, x => x.Value)              
                };
        }
    }
}
