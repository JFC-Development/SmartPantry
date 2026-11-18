import { ChangeDetectionStrategy, Component, computed, inject, Injector, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { ListService, LocalizationPipe, PagedResultDto } from '@abp/ng.core';
import {
  ButtonComponent,
  Confirmation,
  ConfirmationService,
  ModalCloseDirective,
  ModalComponent,
} from '@abp/ng.theme.shared';
import { PantryItemService, PantryItemDto } from '../proxy/despensa';

function validDate(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string | null;
  if (!value) {
    return null;
  }
  return isNaN(new Date(`${value}T00:00:00`).getTime()) ? { invalidDate: true } : null;
}

@Component({
  selector: 'app-pantry',
  templateUrl: './pantry.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
    DatePipe,
    ReactiveFormsModule,
    NgbPaginationModule,
    LocalizationPipe,
    ModalComponent,
    ModalCloseDirective,
    ButtonComponent,
  ],
  providers: [ListService],
})
export class PantryComponent {
  public readonly list = inject(ListService);
  private readonly injector = inject(Injector);
  private readonly pantryService = inject(PantryItemService);
  private readonly fb = inject(FormBuilder);
  private readonly confirmation = inject(ConfirmationService);
  readonly isModalOpen = signal(false);
  readonly saving = signal(false);
  readonly saveError = signal(false);
  readonly editing = signal<PantryItemDto | undefined>(undefined);
  readonly form = this.fb.group({
    cantidad: this.fb.control<number | null>(null, [Validators.min(0)]),
    unidad: this.fb.control<string>('', [Validators.maxLength(20)]),
    fecha: this.fb.control<string>('', [validDate]),
  });
  private readonly query = toSignal(this.list.query$, {
    initialValue: { skipCount: 0, maxResultCount: 10, sorting: '' },
    injector: this.injector,
  });

  readonly resource = rxResource<
    PagedResultDto<PantryItemDto>,
    { skipCount: number; maxResultCount: number; sorting: string }
  >({
    params: () => this.query(),
    stream: ({ params }) => this.pantryService.getList(params as any, { skipHandleError: true }),
    defaultValue: { items: [], totalCount: 0 },
    injector: this.injector,
  });

  readonly isLoading = computed(() => {
    const status = this.resource.status();
    return status === 'loading' || status === 'reloading';
  });
  readonly hasError = computed(() => this.resource.status() === 'error');
  readonly items = computed(() => (this.resource.hasValue() ? this.resource.value().items ?? [] : []));
  readonly totalCount = computed(() => (this.resource.hasValue() ? this.resource.value().totalCount ?? 0 : 0));
  readonly isEmpty = computed(() => this.resource.status() === 'resolved' && this.items().length === 0);

  retry() {
    this.list.get();
  }

  readonly pageSize = computed(() => this.list.maxResultCount ?? 10);

  get currentPage(): number {
    return (this.list.page ?? 0) + 1;
  }

  onPageChange(page: number) {
    this.list.page = page - 1;
  }

    openEdit(item: PantryItemDto) {
    this.editing.set(item);
    this.saveError.set(false);
    this.form.reset({
      cantidad: item.cantidad ?? null,
      unidad: item.unidad ?? '',
      // Solo la parte de día (yyyy-MM-dd), sin convertir zona horaria.
      fecha: item.fechaVencimiento ? item.fechaVencimiento.substring(0, 10) : '',
    });
    this.isModalOpen.set(true);
  }

  save() {
    const id = this.editing()?.id;
    if (!id || this.saving()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    this.saveError.set(false);

    this.pantryService
      .update(
        id,
        {
          cantidad: value.cantidad ?? null,
          unidad: value.unidad?.trim() || null,
          fechaVencimiento: value.fecha ? `${value.fecha}T00:00:00` : null,
        },
        { skipHandleError: true },
      )
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.isModalOpen.set(false);
          this.list.get();
        },
        error: () => {
          this.saving.set(false);
          this.saveError.set(true);
        },
      });
  }

  markAsConsumed(item: PantryItemDto) {
    if (!item.id) {
      return;
    }
    this.pantryService.markAsConsumed(item.id).subscribe(() => this.list.get());
  }

  delete(item: PantryItemDto) {
    if (!item.id) {
      return;
    }
    const id = item.id;
    this.confirmation
      .warn('::Pantry:ConfirmDelete', '::Pantry:ConfirmDeleteTitle')
      .subscribe(status => {
        // Si cancela, no se invoca al servidor.
        if (status === Confirmation.Status.confirm) {
          this.pantryService.delete(id).subscribe(() => this.list.get());
        }
      });
  }

}