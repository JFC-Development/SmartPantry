import type { EntityDto } from '@abp/ng.core';

export interface PantryItemDto extends EntityDto<string> {
  productoId?: string;
  nombreProducto?: string;
  estado?: string;
  fechaVencimiento?: string | null;
  cantidad?: number | null;
  unidad?: string | null;
}

export interface UpdatePantryItemDto {
  cantidad?: number | null;
  unidad?: string | null;
  fechaVencimiento?: string | null;
}
