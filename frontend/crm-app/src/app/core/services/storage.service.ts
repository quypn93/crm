import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StorageService {
  private readonly TOKEN_KEY = 'crm_access_token';
  private readonly REFRESH_TOKEN_KEY = 'crm_refresh_token';
  private readonly USER_KEY = 'crm_user';

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  setRefreshToken(token: string): void {
    localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
  }

  getUser<T>(): T | null {
    const user = localStorage.getItem(this.USER_KEY);
    return user ? JSON.parse(user) : null;
  }

  setUser<T>(user: T): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  clear(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  // Session storage methods
  getSessionItem<T>(key: string): T | null {
    const item = sessionStorage.getItem(key);
    return item ? JSON.parse(item) : null;
  }

  setSessionItem<T>(key: string, value: T): void {
    sessionStorage.setItem(key, JSON.stringify(value));
  }

  removeSessionItem(key: string): void {
    sessionStorage.removeItem(key);
  }
}
