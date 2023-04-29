using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next(); // get the result context after the request executed

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;  // checking if the user is authenticated

            var userId = resultContext.HttpContext.User.GetUserId();

            var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); // gets the IUserRepository so we can use it
            var user = await uow.UserRepository.GetUserByIdAsync(userId); // taking the user
            user.LastActive = DateTime.UtcNow; // change the lastActive prop
            await uow.Complete(); // save to db
        }
    }
}