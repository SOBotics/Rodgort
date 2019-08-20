import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService, ADMIN, TRIAGER } from '../services/auth.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  public statistics: any = {
    requests: {
      total: '?',

      withTrackedTags: '?',
      requireTrackingApproval: '?',

      declined: '?',
      completed: '?',

      completedWithQuestionsLeft: '?',
      noStatusButCompleted: '?',
    },
    tags: {
      total: '?',
      noQuestions: '?',
      synonymised: '?',
      hasQuestionsAndAttachedToCompletedRequest: '?',
      zombieCount: '?'
    },
    admin: {
      unknownDeletions: '?'
    },
    users: {
      totalUsers: '?'
    }
  };

  public loading = false;

  public isAdmin = false;
  public isTriager = true;

  constructor(
    private httpClient: HttpClient,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.loading = true;
    this.httpClient.get('/api/statistics').subscribe(d => {
      this.loading = false;
      this.statistics = d;
    });

    this.authService.GetAuthDetails().subscribe(d => {
      this.isAdmin = d.HasRole(ADMIN);
      this.isTriager = d.HasRole(TRIAGER);
    });
  }

  public shutdown() {
    this.httpClient.post('/api/admin/shutdown', {}).subscribe(d => {
      window.location.reload();
    });
  }
  public downloadBackup() {
    this.httpClient.post('/api/admin/backup', {}, {responseType: 'text'}).subscribe(token => {
      window.open('/api/admin/backup?token=' + token);
    });
  }
}
