using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Despensa
{
    public class AdvertenciaItemDespensa : Entity<Guid>
    {
        public Guid ItemDespensaId { get; protected set; }
        public string TipoAdvertencia { get; protected set; }
        public string Mensaje { get; set; }
        public bool Activa { get; set; }

        protected AdvertenciaItemDespensa()
        {
        }

        public AdvertenciaItemDespensa(Guid id, Guid itemDespensaId, string tipoAdvertencia, string mensaje, bool activa)
            : base(id)
        {
            ItemDespensaId = itemDespensaId;
            TipoAdvertencia = tipoAdvertencia;
            Mensaje = mensaje;
            Activa = activa;
        }
        public void Actualizar(string mensaje)
        {
            Mensaje = mensaje;
            Activa = true;
        }

        public void Desactivar()
        {
            Activa = false;
        }
    }
}