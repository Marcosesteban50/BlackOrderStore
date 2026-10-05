namespace Black_order.Services.Files
{
    public interface iFileStorage
    {
        Task<string> Storage(string contenedor, IFormFile archivo);
        Task Delete(string? ruta, string? contenedor);
        async Task<string> Update(string? ruta, string contenedor, IFormFile archivo)
        {
            await Delete(ruta, contenedor);
            return await Storage(contenedor, archivo);
        }
    }
}
