using AuthDatabase.Context;
using AuthDomain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthDatabase.Repository
{
    public class RevokedTokenRepository
    {
        private readonly AuthServiceDbContext _context;

        public RevokedTokenRepository(AuthServiceDbContext context)
        {
            _context = context;
        }

        public async Task AddRevokedTokenAsync(RevokedToken token)
        {
            await _context.RevokedTokens.InsertOneAsync(token);
        }

        public async Task<bool> IsTokenRevokedAsync(string jti)
        {
            var result = await _context.RevokedTokens
                .Find(t => t.Jti == jti)
                .FirstOrDefaultAsync();

            return result != null;
        }

        public async Task DeleteRevokedTokenAsync(string jti)
        {
            await _context.RevokedTokens.DeleteOneAsync(t => t.Jti == jti);
        }

        public async Task<List<RevokedToken>> GetAllRevokedTokensAsync()
        {
            return await _context.RevokedTokens.Find(_ => true).ToListAsync();
        }
    }
}
