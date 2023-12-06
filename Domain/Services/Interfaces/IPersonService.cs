using Domain.Entities;
using Domain.Events;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IPersonService : IService<Person>
    {
        public Task InviteAcceptedAsync(Person person, Events.InviteWasAccepted inviteAccepted);
        public Task InviteDeclinedAsync(Person person, InviteWasDeclined inviteDeclined);
    }
}
