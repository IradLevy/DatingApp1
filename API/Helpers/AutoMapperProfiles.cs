using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() // The auto mapper is used to map from entity to Dto and vice versa.
        {   
            CreateMap<AppUser, MemberDto>() // we want to take the main photo from the AppUser and map it to the PhotoUrl prop in MemberDto
                .ForMember(dest => dest.PhotoUrl, // here we specify the destination which is the PhotoUrl in the MemberDto
                /* line 16: we are mapping from the source which is AppUser and we are going through the photos list 
                and take the main photo url and place it inside the PhotoUrl of the MemberDto */
                opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url)) 
                .ForMember(dest => dest.Age, // here we specify the destination as the Age in MemberDto   
                // line 19: we take the DateOfBirth from the AppUser, calculate the age and place it inside the Age prop of MemberDto
                opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge())); 
            
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderPhotoUrl, 
                           opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, 
                           opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
            
            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? 
                DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
        }
    }
}