using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _uow;
        public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await _uow.UserRepository.GetUserGender(User.GetUserName());
            userParams.CurrentUsername = User.GetUserName();

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await _uow.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _uow.UserRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) 
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName());

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file) // the function gets a file from the request body
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName()); // getting the user

            if (user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file); // add the photo using the PhotoService we created that returns ImageUploadResult

            if(result.Error != null) return BadRequest(result.Error.Message); // if error occurred

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) photo.IsMain = true;  // if this is the first photo make it main

            user.Photos.Add(photo); // add photo so that entity framework is listening to it

            if (await _uow.Complete()) // if the saving to db succeeded
            {
                return CreatedAtAction
                (
                    nameof(GetUser), 
                    new {username = user.UserName}, 
                    _mapper.Map<PhotoDto>(photo)
                );
                // _mapper.Map<PhotoDto>(photo); // this line is to return photoDto and not Photo
                // CreatedAtAction is for returning 201 status code and we give the user to tell where the new photo created
            }

            return BadRequest("Problem adding photo"); // bad request if the saving to db didn't succeed
        } 

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName());

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            // Get the user from db
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUserName());
            // Get the photo the user want to delete
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            // if the photo is null
            if (photo == null) return NotFound();
            // if the photo is the main the user cannot delete it
            if (photo.IsMain) return BadRequest("You cant delete your main photo");

            if (photo.PublicId != null) {
                // Delete the photo from cloudinary using the photoService we built via publicId
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                // if an error occurred when trying to delete
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            // remove the photo from entity framework
            user.Photos.Remove(photo);

            // remove the photo from db
            if (await _uow.Complete()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}