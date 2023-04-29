using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUnitOfWork _uow;
        public LikesController(IUnitOfWork uow)
        {
            _uow = uow;

        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _uow.UserRepository.GetUserByUsernameAsync(username); // get the user you want to like to
            var sourceUser = await _uow.LikesRepository.GetUserWithLikes(sourceUserId); // get the source user who want to add like to someone

            // if the user you want to like to is not exist
            if (likedUser == null) return NotFound(); 

            // if the user you want to like to is yourself
            if (sourceUser.UserName == username) return BadRequest("You cannot like yourself");

            // get the row from db which contains SourceUserId = sourceUserId and TargetUserId = likedUser.Id
            var userLike = await _uow.LikesRepository.GetUserLike(sourceUserId, likedUser.Id); 

            // if the row exists it means that you already liked this user.
            if (userLike != null) return BadRequest("You already like this user");

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _uow.Complete()) return Ok();

            return BadRequest("Failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _uow.LikesRepository.GetUserLikes(likesParams);
            
            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages
                ));
                
            return Ok(users);
        }
    }
}