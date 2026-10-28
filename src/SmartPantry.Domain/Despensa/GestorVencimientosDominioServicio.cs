using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace SmartPantry.Despensa
{
    public class GestorVencimientosDominioServicio : DomainService
    {
        private readonly IRepository<ItemDespensa, Guid> _itemDespensaRepository;
        private readonly IRepository<AdvertenciaItemDespensa, Guid> _advertenciaRepository;
        private readonly IGuidGenerator _guidGenerator;

        public GestorVencimientosDominioServicio(
            IRepository<ItemDespensa, Guid> itemDespensaRepository,
            IRepository<AdvertenciaItemDespensa, Guid> advertenciaRepository,
            IGuidGenerator guidGenerator)
        {
            _itemDespensaRepository = itemDespensaRepository;
            _advertenciaRepository = advertenciaRepository;
            _guidGenerator = guidGenerator;
        }

        [UnitOfWork]
        public virtual async Task ProcesarVencimientosAsync()
        {
            var fechaHoy = DateTime.Now.Date;
            var fechaLimiteAdvertencia = fechaHoy.AddDays(3);

            var itemsAVencer = await _itemDespensaRepository.GetListAsync(
                x => x.FechaVencimiento.HasValue && x.Estado == "EnStock"
            );

            foreach (var item in itemsAVencer)
            {
                if (!item.FechaVencimiento.HasValue)
                {
                    continue;
                }

                string? tipoAdvertencia = null;
                string? mensaje = null;

                if (item.FechaVencimiento.Value.Date < fechaHoy)
                {
                    tipoAdvertencia = "Vencido";
                    mensaje = $"El producto del ítem {item.Id} se encuentra vencido.";
                }
                else if (item.FechaVencimiento.Value.Date <= fechaLimiteAdvertencia)
                {
                    tipoAdvertencia = "ProximoAVencer";
                    mensaje = $"El producto del ítem {item.Id} está próximo a vencer.";
                }

                if (tipoAdvertencia != null && mensaje != null)
                {
                    var existeAdvertencia = await _advertenciaRepository.AnyAsync(a => 
                        a.ItemDespensaId == item.Id && a.TipoAdvertencia == tipoAdvertencia && a.Activa);

                    if (!existeAdvertencia)
                    {
                        var nuevaAdvertencia = new AdvertenciaItemDespensa(
                            _guidGenerator.Create(),
                            item.Id,
                            tipoAdvertencia,
                            mensaje,
                            activa: true
                        );

                        await _advertenciaRepository.InsertAsync(nuevaAdvertencia, autoSave: true);
                    }
                }
            }
        }
    }
}