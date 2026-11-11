namespace Black_order.Services.UserServices
{
    public static class UserServiceExtensions
    {
        /// <summary>
        /// Returns the authenticated user id or throws <see cref="UnauthorizedAccessException"/>
        /// when the request has no usable identity. The global exception handler maps it to 401.
        /// </summary>
        public static async Task<string> RequireUserId(this IUserService userService)
        {
            var userId = await userService.GetUserId();

            return userId ?? throw new UnauthorizedAccessException("User is not authenticated.");
        }
    }
}
