using DrCell_V02.Data.Modelos;

namespace DrCell_V02.Services.Interface
{
    public interface IBateriasService
    {
        //BATERIAS
        Task<List<object>> ObtenerBateriasAsync();
        Task<List<object>> ObtenerBateriasByMarcaAsync(string marca);
        Task<List<string>> ObtenerBateriasByModeloAsync();
        Task<List<object>> ObtenerBateriasByModeloYMarcaAsync(string marca, string modelo);
        Task<Baterias?> GetBateriaByIdAsync(int id);

        Task UpdateAsync(Baterias bateria);
        Task DeleteAsync(int id);
        Task<bool> ExistsBateriaAsync(string marca, string modelo);
        Task<Baterias> AddAsync(Baterias bateria);
    }
}
