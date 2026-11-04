using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;
using SmartPantry.Despensa;

namespace SmartPantry.EntityFrameworkCore.Despensa
{
    public class GestorVencimientosDominioServicio_Reglas_Tests
    : SmartPantryEntityFrameworkCoreTestBase
    {
        // Fecha controlada (criterio: fecha local, solo parte de día). Umbral = 3 días.
        private static readonly DateTime Hoy = new DateTime(2026, 11, 4);

        private readonly IRepository<ItemDespensa, Guid> _items;
        private readonly IRepository<AdvertenciaItemDespensa, Guid> _advertencias;
        private readonly GestorVencimientosDominioServicio _gestor;

        public GestorVencimientosDominioServicio_Reglas_Tests()
        {
            _items = GetRequiredService<IRepository<ItemDespensa, Guid>>();
            _advertencias = GetRequiredService<IRepository<AdvertenciaItemDespensa, Guid>>();
            _gestor = GetRequiredService<GestorVencimientosDominioServicio>();
        }

        private async Task<Guid> CrearItemAsync(DateTime? vencimiento, string estado = "EnStock")
        {
            var id = Guid.NewGuid();
            await WithUnitOfWorkAsync(async () =>
            {
                await _items.InsertAsync(new ItemDespensa(
                    id, Guid.NewGuid(), Guid.NewGuid(), estado, vencimiento, 1, "u"));
            });
            return id;
        }

        private Task ProcesarAsync(DateTime? ahora = null) =>
            WithUnitOfWorkAsync(() => _gestor.ProcesarVencimientosAsync(ahora ?? Hoy));

        private Task<System.Collections.Generic.List<AdvertenciaItemDespensa>> AdvertenciasDeAsync(Guid itemId) =>
            WithUnitOfWorkAsync(() => _advertencias.GetListAsync(a => a.ItemDespensaId == itemId));

        [Fact]
        public async Task Dentro_Del_Umbral_Crea_Advertencia_Activa()
        {
            var id = await CrearItemAsync(Hoy.AddDays(2));
            await ProcesarAsync();

            var lista = await AdvertenciasDeAsync(id);
            lista.Count.ShouldBe(1);
            lista[0].Activa.ShouldBeTrue();
        }

        [Fact]
        public async Task Dia_Limite_Inclusive_Crea_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(GestorVencimientosDominioServicio.DiasUmbral));
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).Count.ShouldBe(1);
        }

        [Fact]
        public async Task Fuera_Del_Umbral_No_Crea_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(GestorVencimientosDominioServicio.DiasUmbral + 1));
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).ShouldBeEmpty();
        }

        [Fact]
        public async Task Vencido_Crea_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(-1));
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).Count.ShouldBe(1);
        }

        [Fact]
        public async Task Sin_Fecha_No_Crea_Advertencia()
        {
            var id = await CrearItemAsync(null);
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).ShouldBeEmpty();
        }

        [Fact]
        public async Task Consumido_No_Crea_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(1), estado: "Consumido");
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).ShouldBeEmpty();
        }

        [Fact]
        public async Task Dos_Ejecuciones_Dejan_Una_Sola_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(1));

            await ProcesarAsync();
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).Count.ShouldBe(1);
        }

        [Fact]
        public async Task Cambiar_Fecha_Desactiva_Y_Luego_Reactiva_La_Misma_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(1));
            await ProcesarAsync();
            var idAdvertencia = (await AdvertenciasDeAsync(id)).Single().Id;

            // La fecha se aleja: sale del umbral -> se desactiva.
            await WithUnitOfWorkAsync(async () =>
            {
                var item = await _items.GetAsync(id);
                item.FechaVencimiento = Hoy.AddDays(30);
                await _items.UpdateAsync(item);
            });
            await ProcesarAsync();

            var desactivada = (await AdvertenciasDeAsync(id)).Single();
            desactivada.Id.ShouldBe(idAdvertencia);
            desactivada.Activa.ShouldBeFalse();

            // La fecha vuelve a entrar al umbral -> se reactiva la MISMA fila.
            await WithUnitOfWorkAsync(async () =>
            {
                var item = await _items.GetAsync(id);
                item.FechaVencimiento = Hoy.AddDays(2);
                await _items.UpdateAsync(item);
            });
            await ProcesarAsync();

            var reactivada = (await AdvertenciasDeAsync(id)).Single();
            reactivada.Id.ShouldBe(idAdvertencia);
            reactivada.Activa.ShouldBeTrue();
        }

        [Fact]
        public async Task Consumir_El_Item_Desactiva_La_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(1));
            await ProcesarAsync();

            await WithUnitOfWorkAsync(async () =>
            {
                var item = await _items.GetAsync(id);
                item.Estado = "Consumido";
                await _items.UpdateAsync(item);
            });
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).Single().Activa.ShouldBeFalse();
        }

        [Fact]
        public async Task Item_Borrado_Desactiva_La_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(1));
            await ProcesarAsync();

            await WithUnitOfWorkAsync(() => _items.DeleteAsync(id));
            await ProcesarAsync();

            (await AdvertenciasDeAsync(id)).Single().Activa.ShouldBeFalse();
        }

        [Fact]
        public async Task Indice_Unico_Rechaza_Una_Segunda_Advertencia_Del_Mismo_Item_Y_Tipo()
        {
            var id = await CrearItemAsync(Hoy.AddDays(1));
            await ProcesarAsync();

            await Should.ThrowAsync<Exception>(async () =>
            {
                await WithUnitOfWorkAsync(async () =>
                {
                    await _advertencias.InsertAsync(new AdvertenciaItemDespensa(
                        Guid.NewGuid(), id, GestorVencimientosDominioServicio.TipoVencimiento,
                        "duplicada", activa: true), autoSave: true);
                });
            });

            (await AdvertenciasDeAsync(id)).Count.ShouldBe(1);
        }

        [Fact]
        public async Task Ejecuciones_Concurrentes_Dejan_Una_Sola_Advertencia()
        {
            var id = await CrearItemAsync(Hoy.AddDays(1));

            // Dos ejecuciones lanzadas a la vez. Si una choca con el índice único,
            // puede fallar (el worker real captura y loguea ese error); lo que importa
            // es el estado final: una sola advertencia.
            var tareas = new[] { IntentarProcesarAsync(), IntentarProcesarAsync() };
            await Task.WhenAll(tareas);

            (await AdvertenciasDeAsync(id)).Count.ShouldBe(1);
        }

        private async Task IntentarProcesarAsync()
        {
            try { await ProcesarAsync(); }
            catch (Exception) { /* la carrera perdida es aceptable */ }
        }
    }
}