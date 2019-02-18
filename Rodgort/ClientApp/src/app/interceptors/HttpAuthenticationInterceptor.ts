import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { AuthService } from '../services/auth.service';
import 'rxjs/add/operator/switchMap';
import 'rxjs/add/operator/first';

@Injectable()
export class HttpAuthenticationInterceptor implements HttpInterceptor {
    constructor(private authService: AuthService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return this.authService.GetAuthDetails().first().switchMap(d => {
            const modified =
                d.IsAuthenticated
                    ? request.clone({ setHeaders: { 'Authorization': `Bearer ${d.RawToken}` } })
                    : request.clone();

            return next.handle(modified);
        });
    }
}
