namespace Black_order.Services.UserServices
{
    public interface IUserService
    {
        /// <summary>
        /// Returns the authenticated user id, or null when the current request
        /// has no email claim or the identity user no longer exists.
        /// </summary>
        Task<string?> GetUserId();
    }
}
