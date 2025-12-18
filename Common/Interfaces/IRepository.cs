using System;
using server.Application.Request;

namespace server.Common.Interfaces;

public interface IRepository<T> where T : class
{
    //Task<PaginatedResult<T>> GetAllAsync(PaginationRequest request);

    Task<T> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<bool> UpdateItemAsync(Guid id, T entity);
    Task<bool> DeleteItemAsync(Guid id);
}
