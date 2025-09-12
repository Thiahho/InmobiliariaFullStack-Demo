using DrCell_V02.Data.Modelos;

namespace DrCell_V02.Services.Interface
{
    public interface IPinesService
    {
        //PINES
        Task<List<object>> ObtenerPinesAsync();
        Task<List<object>> ObtenerPinesByMarcaAsync(string marca);
        Task<List<string>> ObtenerPinesByModeloAsync();
        Task<List<object>> ObtenerPinesByModeloYMarcaAsync(string marca, string modelo);

        Task<Pines?> GetPinByIdAsync(int id);
        Task UpdateAsync(Pines pin);
        Task DeleteAsync(int id);
        Task<bool> ExistsPinAsync(string marca, string modelo);
        Task<Pines> AddAsync(Pines pin);
    }
}
