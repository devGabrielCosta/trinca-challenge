using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IService<T> where T : AggregateRoot
    {
        public Task<T?> GetAsync(string id);
        public Task SaveAsync(T entity);
    }
}
