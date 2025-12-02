using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlobalDisasterManagement.Models.DTO
{
    public class UserEditViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int LGAId { get; set; }
        public int CityId { get; set; }
    }
}
