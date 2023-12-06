using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IBbqService : IService<Bbq>
    {
        public IAsyncEnumerable<Bbq> GetAsync(IEnumerable<Invite> invites);
        public Task<IEnumerable<Bbq>> GetDynamicAsync(string personId, string? id = null, Dictionary<string, int>? shoppingList = null);
        public Task CreateAsync(Bbq bbq);
        public Task ModerateStatusAsync(Bbq bbq);
    }
}
