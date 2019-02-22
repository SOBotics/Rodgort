import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService, RODGORT_ADMIN } from '../services/auth.service';

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
    }
  };

  public isAdmin = false;

  constructor(
    private httpClient: HttpClient,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.httpClient.get('/api/statistics').subscribe(d => {
      this.statistics = d;
    });

    this.authService.GetAuthDetails().subscribe(d => {
      this.isAdmin = !!d.GetClaim(RODGORT_ADMIN);
    });
  }
}
