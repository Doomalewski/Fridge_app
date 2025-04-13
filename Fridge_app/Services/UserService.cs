using Fridge_app.Models;
using Fridge_app.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdWithIncludesAsync(
            userId,
            query => query
                .Include(u => u.Fridge)
                    .ThenInclude(sp => sp.Product)
                .Include(u => u.Diet)
        );
    }
    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByConditionAsync(
            u => u.Email == email,
            u => u.Fridge,
            u => u.Diet
        );
    }
}