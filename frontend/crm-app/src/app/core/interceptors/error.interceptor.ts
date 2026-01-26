import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor() {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'Đã xảy ra lỗi. Vui lòng thử lại.';

        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = error.error.message;
        } else {
          // Server-side error
          switch (error.status) {
            case 400:
              errorMessage = error.error?.message || 'Dữ liệu không hợp lệ.';
              break;
            case 401:
              errorMessage = 'Phiên đăng nhập đã hết hạn.';
              break;
            case 403:
              errorMessage = 'Bạn không có quyền thực hiện thao tác này.';
              break;
            case 404:
              errorMessage = 'Không tìm thấy dữ liệu.';
              break;
            case 500:
              errorMessage = 'Lỗi hệ thống. Vui lòng thử lại sau.';
              break;
            default:
              errorMessage = error.error?.message || 'Đã xảy ra lỗi. Vui lòng thử lại.';
          }
        }

        console.error('HTTP Error:', error);

        // You can integrate with a notification service here
        // this.notificationService.showError(errorMessage);

        return throwError(() => ({ ...error, message: errorMessage }));
      })
    );
  }
}
