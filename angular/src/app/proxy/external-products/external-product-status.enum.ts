import { mapEnumToOptions } from '@abp/ng.core';

export enum ExternalProductStatus {
  Found = 0,
  NotFound = 1,
  RateLimited = 2,
  Unavailable = 3,
}

export const externalProductStatusOptions = mapEnumToOptions(ExternalProductStatus);
