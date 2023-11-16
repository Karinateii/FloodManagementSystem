using NewLagosFloodDetectionSystem.Models;
using NewLagosFloodDetectionSystem.Models.DTO;

namespace NewLagosFloodDetectionSystem.AccountRepository.Abstract
{
    public interface IUserAuthenticationService
    {
        Task<Status> LoginAsync(LoginModel model);
        //Task<Status> LoginAsync(LoginModel model);
        //Task<Status> RegistrationAsync(RegistrationModel model);
        Task<Status> RegistrationAdminAsync(RegistrationModel model);
        Task LogoutAsync();
    }
}
