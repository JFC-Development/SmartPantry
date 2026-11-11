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
        // Umbral de aviso: se advierte desde 3 días antes, incluyendo el día límite.
        public const int DiasUmbral = 3;

        // Tipo fijo: la clave lógica de una advertencia es (ItemDespensaId, TipoAdvertencia).
        public const string TipoVencimiento = "Vencimiento";

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
        public virtual async Task ProcesarVencimientosAsync(DateTime? ahora = null)
        {
            // Criterio: fecha LOCAL del servidor, comparando solo la parte de día.
            // Las pruebas pasan una fecha fija en `ahora` para no depender del reloj.
            var hoy = (ahora ?? DateTime.Now).Date;
            var limite = hoy.AddDays(DiasUmbral);

            var items = await _itemDespensaRepository.GetListAsync();

            var existentes = (await _advertenciaRepository
                    .GetListAsync(a => a.TipoAdvertencia == TipoVencimiento))
                .ToDictionary(a => a.ItemDespensaId);

            foreach (var item in items)
            {
                var corresponde = item.Estado == "EnStock"
                    && item.FechaVencimiento.HasValue
                    && item.FechaVencimiento.Value.Date <= limite;

                existentes.TryGetValue(item.Id, out var advertencia);

                if (corresponde)
                {
                    var mensaje = item.FechaVencimiento!.Value.Date < hoy
                        ? $"El producto del ítem {item.Id} se encuentra vencido."
                        : $"El producto del ítem {item.Id} está próximo a vencer.";

                    if (advertencia == null)
                    {
                        await _advertenciaRepository.InsertAsync(new AdvertenciaItemDespensa(
                            _guidGenerator.Create(),
                            item.Id,
                            TipoVencimiento,
                            mensaje,
                            activa: true));
                    }
                    else
                    {
                        // Misma fila: actualiza el mensaje y la reactiva si estaba desactivada.
                        advertencia.Actualizar(mensaje);
                        await _advertenciaRepository.UpdateAsync(advertencia);
                    }
                }
                else if (advertencia != null && advertencia.Activa)
                {
                    // Consumido, sin fecha, o fecha fuera del umbral: se desactiva.
                    advertencia.Desactivar();
                    await _advertenciaRepository.UpdateAsync(advertencia);
                }
            }
                        // Producto no disponible (ítem borrado): su advertencia deja de corresponder.
            var idsVigentes = items.Select(i => i.Id).ToHashSet();
            var huerfanas = existentes.Values
                .Where(a => a.Activa && !idsVigentes.Contains(a.ItemDespensaId))
                .ToList();

            foreach (var huerfana in huerfanas)
            {
                huerfana.Desactivar();
                await _advertenciaRepository.UpdateAsync(huerfana);
            }
        }
    }
}