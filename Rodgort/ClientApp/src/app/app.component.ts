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

  constructor(
    private http: HttpClient
  ) {}

  ngOnInit() {
    this.http.get('/assets/revision.txt', { responseType: 'text' }).subscribe(a => this.revision = a);
  }

  public getLoginUrl() {
    return `/api/Authentication/Login?redirect_uri=${window.location}&scope=all`;
  }
}
