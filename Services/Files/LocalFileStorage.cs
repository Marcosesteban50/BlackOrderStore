namespace Black_order.Services.Files
{
    public class LocalFileStorage : iFileStorage
    {
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;
        public LocalFileStorage(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
        }

        // Delete a file from the local storage
        public Task Delete(string? path, string? container)
        {

            // If the path is null or empty, return a completed task
            if (string.IsNullOrWhiteSpace(path))
            {
                // If the path is null or empty, return a completed task
                return Task.CompletedTask;
            }
            // Get the file name from the path and combine it with the web root path and container to get the full directory path
            var fileName = Path.GetFileName(path);

            // Combine the web root path, container, and file name to get the full directory path
            var directoryPath = Path.Combine(env.WebRootPath, container ?? "", fileName);

            // If the file exists at the directory path, delete it
            if (File.Exists(directoryPath))
            {
             
                File.Delete(directoryPath);
            }
            // Return a completed task
            return Task.CompletedTask;
        }

        // Store a file in the local storage and return its URL
        public async Task<string> Storage(string container, IFormFile file)
        {
            // Get the file extension and create a unique file name using a GUID
            var extension = Path.GetExtension(file.FileName);
            // Create a unique file name using a GUID and the file extension
            var fileName = $"{Guid.NewGuid()}{extension}";
            // Combine the web root path and container to get the folder path
            var folder = Path.Combine(env.WebRootPath, container);
            // If the folder does not exist, create it
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            // Combine the folder path and file name to get the full file path
            var path = Path.Combine(folder, fileName);
            // Create a new file stream and copy the contents of the uploaded file to it
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                // Copy the contents of the uploaded file to the file stream asynchronously
                await file.CopyToAsync(fileStream);
            }
            // Get the current request's scheme and host to construct the full URL of the stored file
            var CurrentUrl = $"{httpContextAccessor.HttpContext?.Request.Scheme}://{httpContextAccessor.HttpContext?.Request.Host}";
            // Combine the current URL, container, and file name to create the URL for the stored file
            var DataBaseURL = Path.Combine(CurrentUrl, container, fileName).Replace("\\", "/");
            return DataBaseURL;
        }
    }
}