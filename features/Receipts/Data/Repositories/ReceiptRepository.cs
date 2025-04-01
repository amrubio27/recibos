using System.Diagnostics;
using FluentResults;
using recibos.core.data.db;
using recibos.features.Receipts.Data.local;
using recibos.features.Receipts.Domain.Interfaces;
using recibos.features.Receipts.Domain.Models;

namespace recibos.features.Receipts.Data {
    public class ReceiptRepository : IReceiptRepository {
        private readonly ReceiptDatabase _database;

        public ReceiptRepository(ReceiptDatabase database) {
            _database = database;
        }

        public async Task<Result<List<Receipt>>> GetReceiptsAsync() {
            try {
                var entities = await _database.GetReceiptsAsync();
                var models = entities.Select(ReceiptEntityMapper.ToModel).ToList();
                return Result.Ok(models);
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al obtener recibos: {ex.Message}");
                return Result.Fail($"Error de base de datos al obtener recibos: {ex.Message}");
            }
        }

        public async Task<Result<Receipt>> GetReceiptAsync(int id) {
            try {
                var entity = await _database.GetReceiptAsync(id);
                if (entity == null) {
                    return Result.Fail($"No se encontró el recibo con ID: {id}");
                }

                var model = ReceiptEntityMapper.ToModel(entity);
                return Result.Ok(model);
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al obtener recibo {id}: {ex.Message}");
                return Result.Fail($"Error de base de datos al obtener recibo {id}: {ex.Message}");
            }
        }

        public async Task<Result<Receipt>> AddReceiptAsync(Receipt receipt) {
            try {
                var entity = ReceiptEntityMapper.ToEntity(receipt);
                await _database.SaveReceiptAsync(entity);
                receipt.Id = entity.Id;
                return Result.Ok(receipt);
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al añadir recibo: {ex.Message}");
                return Result.Fail($"Error de base de datos al añadir recibo: {ex.Message}");
            }
        }

        public async Task<Result<Receipt>> UpdateReceiptAsync(Receipt receipt) {
            try {
                var entity = ReceiptEntityMapper.ToEntity(receipt);
                await _database.SaveReceiptAsync(entity);
                return Result.Ok(receipt);
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al actualizar recibo {receipt.Id}: {ex.Message}");
                return Result.Fail($"Error de base de datos al actualizar recibo {receipt.Id}: {ex.Message}");
            }
        }

        public async Task<Result> DeleteReceiptAsync(int id) {
            try {
                var entity = await _database.GetReceiptAsync(id);
                if (entity == null) {
                    return Result.Fail($"No se encontró el recibo con ID: {id} para eliminar.");
                }

                var result = await _database.DeleteReceiptAsync(entity);
                if (result > 0) {
                    return Result.Ok();
                }
                else {
                    return Result.Fail($"No se pudo eliminar el recibo con ID: {id}.");
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error al eliminar recibo {id}: {ex.Message}");
                return Result.Fail($"Error de base de datos al eliminar recibo {id}: {ex.Message}");
            }
        }
    }
}