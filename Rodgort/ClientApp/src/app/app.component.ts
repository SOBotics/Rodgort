import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { AuthService } from './services/auth.service';
import { WebsocketHelper } from '../utils/WebsocketHelper';
import { ToasterService, BodyOutputType, Toast } from 'angular2-toaster';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'app';
  public isLoggedIn: Boolean;
  public userName: string;
  public revision: string;
  public quotaRemaining = 0;

  constructor(
    private httpClient: HttpClient,
    private router: Router,
    private authService: AuthService,
    private activatedRoute: ActivatedRoute,
    private toasterService: ToasterService
  ) { }

  ngOnInit() {
    this.httpClient.get('/assets/revision.txt', { responseType: 'text' }).subscribe(a => this.revision = a);

    this.activatedRoute.queryParams.subscribe((params: Params) => {
      const accessToken = params['access_token'];
      if (accessToken) {
        this.authService.Login(accessToken);
        const fixedUrl = this.router.url.substr(0, this.router.url.indexOf('?'));
        this.router.navigateByUrl(fixedUrl);
      }
    });

    this.authService.GetAuthDetails().subscribe(details => {
      this.authService.RawToken = details.RawToken;
      this.isLoggedIn = details.IsAuthenticated;
      if (this.isLoggedIn) {
        this.userName = details.TokenData.unique_name;
      }
    });

    const quotaRemainingSocket = new WebsocketHelper<{ quotaRemaining: number }>('ws/quotaRemaining');
    quotaRemainingSocket.Observable.subscribe(payload => {
      this.quotaRemaining = payload.quotaRemaining;
    });

    const pipelinesSocket = new WebsocketHelper<{ status: string }>('ws/pipelines');
    let currentToast: Toast;
    pipelinesSocket.Observable.subscribe(payload => {
      const status = payload.status;
      if (status === 'running') {
        if (currentToast) {
          this.toasterService.clear(currentToast.toastId, currentToast.toastContainerId);
        }
        currentToast = this.toasterService.pop({
          type: 'info',
          title: 'New version',
          body: 'A new version of Rodgort is being <a class="deploy-link" href="https://gitlab.com/rjrudman/Rodgort/pipelines">deployed</a>.',
          bodyOutputType: BodyOutputType.TrustedHtml,
          timeout: 0,
          showCloseButton: false
        });
      }
    });
  }

  public onLogoutClicked() {
    this.authService.Logout();
  }

  public getLoginUrl() {
    return `/api/Authentication/Login?redirect_uri=${window.location}&scope=all`;
  }
}
