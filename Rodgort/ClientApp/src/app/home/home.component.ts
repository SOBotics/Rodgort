import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  public statistics: any = {
    requests: {
      total: 658,
      declined: 53,
      completed: 123,

      completedWithQuestionsLeft: 86,
      noStatusButCompleted: 12,
    },
    tags: {
      total: 2018,
      noQuestions: 28,
      synonymised: 38,
      hasQuestionsAndAttachedToCompletedRequest: 32
    }
  };

  constructor(
  ) { }

  ngOnInit() {

  }
}
