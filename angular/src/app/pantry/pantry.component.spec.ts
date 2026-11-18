import { NO_ERRORS_SCHEMA, Component, Pipe, PipeTransform, input, output } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { LocalizationPipe } from '@abp/ng.core';
import {
  ButtonComponent,
  Confirmation,
  ConfirmationService,
  ModalCloseDirective,
  ModalComponent,
} from '@abp/ng.theme.shared';
import { PantryComponent } from './pantry.component';
import { PantryItemService, PantryItemDto } from '../proxy/despensa';

@Pipe({ name: 'abpLocalization' })
class StubLocalizationPipe implements PipeTransform {
  transform(value: string): string {
    return value;
  }
}

@Component({ selector: 'abp-modal', template: '' })
class StubModalComponent {
  visible = input<boolean>(false);
  visibleChange = output<boolean>();
}

const ITEM_COMPLETO: PantryItemDto = {
  id: 'a1',
  productoId: 'p1',
  nombreProducto: 'Leche',
  estado: 'EnStock',
  fechaVencimiento: '2026-12-11T00:00:00',
  cantidad: 5,
  unidad: 'kg',
};

const ITEM_SIN_DATOS: PantryItemDto = {
  id: 'a2',
  productoId: 'p2',
  nombreProducto: 'Nutella',
  estado: 'EnStock',
  fechaVencimiento: null,
  cantidad: null,
  unidad: null,
};

describe('PantryComponent', () => {
  const pantryService = {
    getList: vi.fn(),
    update: vi.fn(),
    markAsConsumed: vi.fn(),
    delete: vi.fn(),
  };
  const confirmation = { warn: vi.fn() };

  async function crear(getList: () => unknown) {
    pantryService.getList.mockReturnValue(getList());
    const fixture = TestBed.createComponent(PantryComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  const texto = (fixture: { nativeElement: HTMLElement }) => fixture.nativeElement.textContent ?? '';

  beforeEach(() => {
    vi.resetAllMocks();
    TestBed.configureTestingModule({
      imports: [PantryComponent],
      providers: [
        { provide: PantryItemService, useValue: pantryService },
        { provide: ConfirmationService, useValue: confirmation },
      ],
    });
    TestBed.overrideComponent(PantryComponent, {
      remove: { imports: [LocalizationPipe, ModalComponent, ModalCloseDirective, ButtonComponent] },
      add: { imports: [StubLocalizationPipe, StubModalComponent], schemas: [NO_ERRORS_SCHEMA] },
    });
  });

  it('muestra el estado de carga y no el mensaje de lista vacía mientras la respuesta está pendiente', () => {
    pantryService.getList.mockReturnValue(new Subject()); // nunca responde
    const fixture = TestBed.createComponent(PantryComponent);
    fixture.detectChanges();

    expect(texto(fixture)).toContain('Pantry:Loading');
    expect(texto(fixture)).not.toContain('Pantry:Empty');
  });

  it('muestra el estado de error con la opción de reintentar', async () => {
    const fixture = await crear(() => throwError(() => new Error('fallo')));
    expect(texto(fixture)).toContain('Pantry:LoadError');
    expect(texto(fixture)).toContain('Pantry:Retry');
    expect(texto(fixture)).not.toContain('Pantry:Empty');
  });

  it('muestra la lista vacía cuando el usuario no tiene ítems', async () => {
    const fixture = await crear(() => of({ items: [], totalCount: 0 }));
    expect(texto(fixture)).toContain('Pantry:Empty');
    expect(texto(fixture)).not.toContain('Pantry:Loading');
  });

  it('muestra los resultados y comunica los datos faltantes sin inventar valores', async () => {
    const fixture = await crear(() =>
      of({ items: [ITEM_COMPLETO, ITEM_SIN_DATOS], totalCount: 2 }),
    );
    const t = texto(fixture);
    expect(t).toContain('Leche');
    expect(t).toContain('Nutella');
    expect(t).toContain('Pantry:NoExpirationDate');
    expect(t).toContain('Pantry:NoQuantity');
    expect(t).toContain('Pantry:NoUnit');
    expect(t).not.toContain('Pantry:Empty');
  });


  it('no llama al servicio si la cantidad es negativa', async () => {
    const fixture = await crear(() => of({ items: [ITEM_COMPLETO], totalCount: 1 }));
    const c = fixture.componentInstance;

    c.openEdit(ITEM_COMPLETO);
    c.form.controls.cantidad.setValue(-1);
    c.save();

    expect(c.form.controls.cantidad.invalid).toBe(true);
    expect(pantryService.update).not.toHaveBeenCalled();
  });

  it('no llama al servicio si la fecha no es válida', async () => {
    const fixture = await crear(() => of({ items: [ITEM_COMPLETO], totalCount: 1 }));
    const c = fixture.componentInstance;

    c.openEdit(ITEM_COMPLETO);
    c.form.controls.fecha.setValue('no-es-una-fecha');
    c.save();

    expect(c.form.controls.fecha.invalid).toBe(true);
    expect(pantryService.update).not.toHaveBeenCalled();
  });

  it('no llama al servicio si la unidad supera los 20 caracteres', async () => {
    const fixture = await crear(() => of({ items: [ITEM_COMPLETO], totalCount: 1 }));
    const c = fixture.componentInstance;

    c.openEdit(ITEM_COMPLETO);
    c.form.controls.unidad.setValue('x'.repeat(21));
    c.save();

    expect(c.form.controls.unidad.invalid).toBe(true);
    expect(pantryService.update).not.toHaveBeenCalled();
  });

  it('envía la actualización cuando los datos son válidos', async () => {
    const fixture = await crear(() => of({ items: [ITEM_COMPLETO], totalCount: 1 }));
    const c = fixture.componentInstance;
    pantryService.update.mockReturnValue(of(ITEM_COMPLETO));

    c.openEdit(ITEM_COMPLETO);
    c.form.controls.cantidad.setValue(3);
    c.form.controls.unidad.setValue(' litros ');
    c.form.controls.fecha.setValue('2027-01-15');
    c.save();

    expect(pantryService.update).toHaveBeenCalledTimes(1);
    expect(pantryService.update).toHaveBeenCalledWith(
      'a1',
      { cantidad: 3, unidad: 'litros', fechaVencimiento: '2027-01-15T00:00:00' },
      { skipHandleError: true },
    );
  });

  it('pide confirmación y elimina solo si el usuario confirma', async () => {
    const fixture = await crear(() => of({ items: [ITEM_COMPLETO], totalCount: 1 }));
    confirmation.warn.mockReturnValue(of(Confirmation.Status.confirm));
    pantryService.delete.mockReturnValue(of(undefined));

    fixture.componentInstance.delete(ITEM_COMPLETO);

    expect(confirmation.warn).toHaveBeenCalledTimes(1);
    expect(pantryService.delete).toHaveBeenCalledWith('a1');
  });

  it('no llama al servicio si el usuario cancela la confirmación', async () => {
    const fixture = await crear(() => of({ items: [ITEM_COMPLETO], totalCount: 1 }));
    confirmation.warn.mockReturnValue(of(Confirmation.Status.reject));

    fixture.componentInstance.delete(ITEM_COMPLETO);

    expect(confirmation.warn).toHaveBeenCalledTimes(1);
    expect(pantryService.delete).not.toHaveBeenCalled();
  });
});