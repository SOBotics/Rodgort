import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  public statistics: any = {
    requests: {
      total: 0,

      withApprovedTags: 0,
      requireApproval: 0,
      
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

  constructor(
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.httpClient.get('/api/statistics').subscribe(d => {
      this.statistics = d;
    });
  }
}
