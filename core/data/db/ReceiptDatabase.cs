using recibos.features.Receipts.Data.local;
using SQLite;

// Añadimos esta directiva para usar JsonConvert

namespace recibos.core.data.db {
    public class ReceiptDatabase {
        private readonly SQLiteAsyncConnection _database;

        public ReceiptDatabase() {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Receipts.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<ReceiptEntity>().Wait();
        }

        public async Task<List<ReceiptEntity>> GetReceiptsAsync() {
            return await _database.Table<ReceiptEntity>().ToListAsync();
        }

        public async Task<ReceiptEntity> GetReceiptAsync(int id) {
            return await _database.Table<ReceiptEntity>()
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveReceiptAsync(ReceiptEntity receipt) {
            if (receipt.Id != 0) {
                return await _database.UpdateAsync(receipt);
            }
            else {
                return await _database.InsertAsync(receipt);
            }
        }

        public async Task<int> DeleteReceiptAsync(ReceiptEntity receipt) {
            return await _database.DeleteAsync(receipt);
        }
    }
}