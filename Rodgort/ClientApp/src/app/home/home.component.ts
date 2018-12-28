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
      total: 0,

      withTrackedTags: 0,
      requireTrackingApproval: 0,

      declined: 0,
      completed: 0,

      completedWithQuestionsLeft: 0,
      noStatusButCompleted: 0,
    },
    tags: {
      total: 0,
      noQuestions: 0,
      synonymised: 0,
      hasQuestionsAndAttachedToCompletedRequest: 0
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
