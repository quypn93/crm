import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string, params?: HttpParams | { [key: string]: string | number | boolean }): Observable<T> {
    const httpParams = params instanceof HttpParams ? params : new HttpParams({ fromObject: params as any });
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, { params: httpParams }).pipe(
      map(response => response.data)
    );
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body).pipe(
      map(response => response.data)
    );
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body).pipe(
      map(response => response.data)
    );
  }

  patch<T>(endpoint: string, body: any): Observable<T> {
    return this.http.patch<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, body).pipe(
      map(response => response.data)
    );
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`).pipe(
      map(response => response.data)
    );
  }

  // Helper method to build query params from filter object
  buildParams(filter: { [key: string]: any }): HttpParams {
    let params = new HttpParams();

    Object.keys(filter).forEach(key => {
      const value = filter[key];
      if (value !== null && value !== undefined && value !== '') {
        if (value instanceof Date) {
          params = params.set(key, value.toISOString());
        } else {
          params = params.set(key, value.toString());
        }
      }
    });

    return params;
  }
}
