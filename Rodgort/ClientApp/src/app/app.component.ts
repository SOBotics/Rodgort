import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'app';
  public isLoggedIn: Boolean;
  public revision: string;
  public quotaRemaining = 0;

  constructor(
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.httpClient.get('/assets/revision.txt', { responseType: 'text' }).subscribe(a => this.revision = a);

    const quotaRemainingSocket = new WebSocket('wss://localhost:5001/ws/quotaRemaining');
    quotaRemainingSocket.onmessage = event => {
      const payload = JSON.parse(event.data);
      this.quotaRemaining = payload.quotaRemaining;
    };
  }

  public getLoginUrl() {
    return `/api/Authentication/Login?redirect_uri=${window.location}&scope=all`;
  }
}
