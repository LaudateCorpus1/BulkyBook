using AutoMapper;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;

namespace BulkyBook.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<InputModel, ApplicationUser>().ForMember(a => a.UserName, o => o.MapFrom(a => a.Email));
        }
    }
}
