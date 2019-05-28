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

    let requiresImmediateRefresh = true;
    this.activatedRoute.queryParams.subscribe((params: Params) => {
      const accessToken = params['access_token'];
      if (accessToken) {
        requiresImmediateRefresh = false;
        this.authService.Login(accessToken);
        this.router.navigate([], { queryParams: { access_token: null }, queryParamsHandling: 'merge' });
      }
    });

    let autoRefreshSetup = false;
    let refreshTimer: NodeJS.Timer;
    this.authService.GetAuthDetails().subscribe(details => {
      this.authService.RawToken = details.RawToken;
      this.isLoggedIn = details.IsAuthenticated;

      if (this.isLoggedIn) {
        this.userName = details.TokenData.unique_name;

        if (!autoRefreshSetup) {
          autoRefreshSetup = true;
          if (requiresImmediateRefresh) {
            this.refreshToken();
          }

          if (refreshTimer === undefined) {
            refreshTimer = setInterval(this.refreshToken, 1000 * 60 * 60);
          }
        }
      } else {
        if (refreshTimer !== undefined) {
          clearInterval(refreshTimer);
        }
        autoRefreshSetup = false;
        requiresImmediateRefresh = true;
      }
    });

    const quotaRemainingSocket = new WebsocketHelper<{ quotaRemaining: number }>('ws/quotaRemaining');
    quotaRemainingSocket.Observable.subscribe(payload => {
      this.quotaRemaining = payload.quotaRemaining;
    });

    const pipelinesSocket = new WebsocketHelper<{ status: string }>('ws/pipelines');
    let currentToast: Toast;
    pipelinesSocket.Observable.subscribe(payload => {
      if (currentToast) {
        this.toasterService.clear(currentToast.toastId, currentToast.toastContainerId);
      }
      if (payload && payload.status) {
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

  private refreshToken() {
    this.httpClient.post(`/api/Authentication/RefreshToken`, {}, { responseType: 'text' })
      .subscribe(token => {
        this.authService.Login(token);
      });
  }

  public onLogoutClicked() {
    this.authService.Logout();
    this.router.navigate(['/']);
  }

  public getLoginUrl() {
    return `/api/Authentication/Login?redirect_uri=${window.location}&scope=all`;
  }
}
