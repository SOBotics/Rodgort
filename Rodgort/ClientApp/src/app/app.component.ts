import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { AuthService } from './services/auth.service';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material';
import { NewVersionDeployingComponent } from './snackbar/new-version-deploying/new-version-deploying.component';

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
    private authService: AuthService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar
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

    const quotaRemainingSocket = new WebSocket(`wss://${location.host}/ws/quotaRemaining`);
    quotaRemainingSocket.onmessage = event => {
      const payload = JSON.parse(event.data);
      this.quotaRemaining = payload.quotaRemaining;
    };
    this.authService.GetAuthDetails().subscribe(details => {
      this.authService.RawToken = details.RawToken;
      this.isLoggedIn = details.IsAuthenticated;
      if (this.isLoggedIn) {
        this.userName = details.TokenData.unique_name;
      }
    });

    const pipelinesStatus = new WebSocket(`wss://${location.host}/ws/pipelines`);
    pipelinesStatus.onmessage = event => {
      const payload = JSON.parse(event.data);
      const status = payload.status;
      if (status !== 'failed' && status !== 'success') {
        this.snackBar.openFromComponent(NewVersionDeployingComponent, {
          verticalPosition: 'top'
        });
      } else {
        this.snackBar.dismiss();
      }
    };

    pipelinesStatus.onclose = () => {
      this.snackBar.dismiss();
    };
  }

  public onLogoutClicked() {
    this.authService.Logout();
  }

  public getLoginUrl() {
    return `/api/Authentication/Login?redirect_uri=${window.location}&scope=all`;
  }
}
