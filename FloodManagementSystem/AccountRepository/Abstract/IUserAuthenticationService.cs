using GlobalDisasterManagement.Models.DTO;

namespace GlobalDisasterManagement.AccountRepository.Abstract
{
    public interface IUserAuthenticationService
    {
        Task<Status> LoginAsync(LoginModel model);
        Task<Status> RegistrationAdminAsync(RegistrationModel model);
        Task LogoutAsync();
    }
}
