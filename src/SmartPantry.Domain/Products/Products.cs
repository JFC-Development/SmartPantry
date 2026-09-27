using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using SmartPantry.Domain.Products;



namespace SmartPantry.Domain.Products
{
    public class Product : AggregateRoot<Guid>
    {
        public string CodigoBarras { get; private set; }
        public string Nombre { get; private set; }
        public string Marca { get; private set; }
        public string Ingredientes { get; private set; }
        public string Alergenos { get; private set; }

        protected Product() { }

        public Product(
            Guid id, 
            string codigoBarras, 
            string nombre, 
            string marca = null, 
            string ingredientes = null, 
            string alergenos = null) : base(id)
        {
            SetCodigoBarras(codigoBarras);
            SetNombre(nombre);
            SetMarca(marca);
            SetIngredientes(ingredientes);
            SetAlergenos(alergenos);
        }

        public void SetCodigoBarras(string codigoBarras)
        {
            Check.NotNullOrWhiteSpace(codigoBarras, nameof(codigoBarras), ProductConsts.MaxCodigoBarrasLength);
            CodigoBarras = codigoBarras.Trim().Replace(" ", ""); 
        }

        public void SetNombre(string nombre)
        {
            Check.NotNullOrWhiteSpace(nombre, nameof(nombre), ProductConsts.MaxNombreLength);
            Nombre = nombre.Trim(); 
        }

        public void SetMarca(string marca)
        {
            Check.Length(marca, nameof(marca), ProductConsts.MaxMarcaLength);
            Marca = marca?.Trim();
        }

        public void SetIngredientes(string ingredientes)
        {
            Check.Length(ingredientes, nameof(ingredientes), ProductConsts.MaxIngredientesLength);
            Ingredientes = ingredientes?.Trim();
        }

        public void SetAlergenos(string alergenos)
        {
            Check.Length(alergenos, nameof(alergenos), ProductConsts.MaxAlergenosLength);
            Alergenos = alergenos?.Trim();
        }
    }
}