import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { shareReplay, tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { Province, Ward } from '../models/location.model';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private provinces$?: Observable<Province[]>;
  private wardsCache = new Map<string, Observable<Ward[]>>();

  constructor(private api: ApiService) {}

  getProvinces(): Observable<Province[]> {
    if (!this.provinces$) {
      this.provinces$ = this.api.get<Province[]>('locations/provinces').pipe(shareReplay(1));
    }
    return this.provinces$;
  }

  getWardsByProvince(provinceCode: string): Observable<Ward[]> {
    if (!provinceCode) return of([]);
    if (!this.wardsCache.has(provinceCode)) {
      this.wardsCache.set(
        provinceCode,
        this.api
          .get<Ward[]>('locations/wards', { provinceCode })
          .pipe(shareReplay(1))
      );
    }
    return this.wardsCache.get(provinceCode)!;
  }
}
