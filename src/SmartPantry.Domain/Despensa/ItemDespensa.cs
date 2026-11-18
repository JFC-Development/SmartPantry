using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Despensa
{
    public class ItemDespensa : FullAuditedAggregateRoot<Guid>
    {
        public Guid PropietarioId { get; protected set; }
        public Guid ProductoId { get; protected set; }
        public string Estado { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal? Cantidad { get; set; }
        public string? Unidad { get; set; }

        protected ItemDespensa()
        {
        }

        public ItemDespensa(Guid id, Guid propietarioId, Guid productoId, string estado, DateTime? fechaVencimiento, decimal? cantidad, string? unidad)
            : base(id)
        {
            PropietarioId = propietarioId;
            ProductoId = productoId;
            Estado = estado;
            FechaVencimiento = fechaVencimiento;
            Cantidad = cantidad;
            Unidad = unidad;
        }

        public void Actualizar(decimal? cantidad, string? unidad, DateTime? fechaVencimiento)
        {
            if (cantidad.HasValue && cantidad.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad no puede ser negativa.");
            }
            Check.Length(unidad, nameof(unidad), ItemDespensaConsts.MaxUnidadLength);

            Cantidad = cantidad;
            Unidad = string.IsNullOrWhiteSpace(unidad) ? null : unidad.Trim();
            FechaVencimiento = fechaVencimiento;
        }

        public void MarcarConsumido()
        {
            Estado = ItemDespensaConsts.EstadoConsumido;
        }
    }
}