using System.Diagnostics;
using recibos.core.data.db;
using recibos.features.Receipts.Data.local;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Data
{
    public class ReceiptService : IReceiptService
    {
        private readonly ReceiptDatabase _database;

        public ReceiptService(ReceiptDatabase database)
        {
            _database = database;
        }

        public async Task<List<Receipt>> GetReceiptsAsync()
        {
            try
            {
                var entities = await _database.GetReceiptsAsync();
                return entities.Select(ReceiptEntityMapper.ToModel).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener recibos: {ex.Message}");
                return new List<Receipt>();
            }
        }

        public async Task<Receipt> GetReceiptAsync(int id)
        {
            try
            {
                var entity = await _database.GetReceiptAsync(id);
                return ReceiptEntityMapper.ToModel(entity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener recibo {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<Receipt> AddReceiptAsync(Receipt receipt)
        {
            try
            {
                var entity = ReceiptEntityMapper.ToEntity(receipt);
                await _database.SaveReceiptAsync(entity);
                receipt.Id = entity.Id;
                return receipt;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al añadir recibo: {ex.Message}");
                return null;
            }
        }

        public async Task<Receipt> UpdateReceiptAsync(Receipt receipt)
        {
            try
            {
                var entity = ReceiptEntityMapper.ToEntity(receipt);
                await _database.SaveReceiptAsync(entity);
                return receipt;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al actualizar recibo {receipt.Id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteReceiptAsync(int id)
        {
            try
            {
                var entity = await _database.GetReceiptAsync(id);
                if (entity == null)
                    return false;

                var result = await _database.DeleteReceiptAsync(entity);
                return result > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al eliminar recibo {id}: {ex.Message}");
                return false;
            }
        }
    }
}
