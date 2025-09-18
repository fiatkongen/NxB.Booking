using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class MasterRadioRepository : IMasterRadioRepository
    {
        private readonly ITConRepository _tconRepository;
        private readonly MasterRadioFactory _masterRadioFactory;
        private readonly AppTallyDbContext _appTallyDbContext;

        public MasterRadioRepository(ITConRepository tconRepository, MasterRadioFactory masterRadioFactory, AppTallyDbContext appTallyDbContext)
        {
            _tconRepository = tconRepository;
            _masterRadioFactory = masterRadioFactory;
            _appTallyDbContext = appTallyDbContext;
        }

        public async Task<List<MasterRadio>> FindAllMasterRadios()
        {
            var tconMasterRadios = await _tconRepository.FindAllTConMasterRadios();
            var masterRadios = tconMasterRadios.Select(x => _masterRadioFactory.Create(x)).ToList();
            return masterRadios;
        }

        public async Task PublishUpdate()
        {
            var masterRadios = await FindAllMasterRadios();
            masterRadios.First().PublishUpdate();
            await _appTallyDbContext.SaveChangesAsync();
        }

        public async Task Update(MasterRadio masterRadio)
        {
            _appTallyDbContext.Update(masterRadio.GetTConEntity());
            await _appTallyDbContext.SaveChangesAsync();
        }
    }
}