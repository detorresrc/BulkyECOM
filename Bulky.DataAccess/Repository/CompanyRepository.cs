using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly ApplicationDbContext _db;
        private ApplicationDbContext Db => _db;
        public CompanyRepository(ApplicationDbContext db) : base(db) => _db = db;

        public void Save()
        {
            Db.SaveChanges();
        }

        public void Update(Company company)
        {
            Db.Companies.Update(company);
        }
    }
}
