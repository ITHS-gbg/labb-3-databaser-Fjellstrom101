using System.Collections.Generic;
using MongoDB.Bson;

namespace DataAccess
{
    public interface IRepository<T>
    {
        void Add(T item);

        IEnumerable<T> GetAll();
        T Get(ObjectId id);

        void Update(T item);
        void Delete(T item);
    }
}