// https://stackoverflow.com/a/46019852/563532

import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/observable/empty';
import { ToasterService, BodyOutputType } from 'angular2-toaster';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
    constructor(private toasterService: ToasterService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request)
            .catch((err: HttpErrorResponse) => {
                debugger;
                this.toasterService.pop({
                    type: 'error',
                    title: `Error - ${err.status} (${err.statusText})`,
                    body: `Failed to make request to '${request.url}'`,
                    bodyOutputType: BodyOutputType.TrustedHtml,
                    showCloseButton: false
                });

                return Observable.empty<HttpEvent<any>>();
            });
    }
}
