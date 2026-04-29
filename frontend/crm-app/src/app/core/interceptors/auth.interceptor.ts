import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { StorageService } from '../services/storage.service';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(
    private storageService: StorageService,
    private authService: AuthService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Public / anonymous endpoints opt out of token via X-Skip-Auth header.
    if (request.headers.has('X-Skip-Auth')) {
      const cleaned = request.clone({ headers: request.headers.delete('X-Skip-Auth') });
      return next.handle(cleaned);
    }

    // Silent auth: send token if any, but on 401 just propagate the error
    // instead of triggering refresh/logout/redirect (used by public scan page).
    const silent = request.headers.has('X-Silent-Auth');
    const token = this.storageService.getToken();
    if (token) {
      request = this.addToken(request, token);
    }
    if (silent) {
      const cleaned = request.clone({ headers: request.headers.delete('X-Silent-Auth') });
      return next.handle(cleaned);
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        // Skip refresh logic cho login/refresh — login 401 là sai mật khẩu (caller xử lý),
        // refresh 401 nghĩa là refresh token cũng hết hạn → catchError ở handle401Error tự forceLogout.
        const isAuthEndpoint = request.url.includes('auth/login') || request.url.includes('auth/refresh-token');
        if (error.status === 401 && !isAuthEndpoint) {
          return this.handle401Error(request, next);
        }
        return throwError(() => error);
      })
    );
  }

  private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.authService.refreshToken().pipe(
        switchMap((response) => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(response.accessToken);
          return next.handle(this.addToken(request, response.accessToken));
        }),
        catchError((error) => {
          this.isRefreshing = false;
          this.authService.forceLogout();
          return throwError(() => error);
        })
      );
    }

    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => next.handle(this.addToken(request, token!)))
    );
  }
}
