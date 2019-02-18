import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-manual-question-processing',
  templateUrl: './manual-question-processing.component.html',
  styleUrls: ['./manual-question-processing.component.scss']
})
export class ManualQuestionProcessingComponent implements OnInit {

  public request = {
    questionIds: '',
    following: '',
    roomId: '',
  };

  public isLoading = false;

  constructor(private httpClient: HttpClient) {
  }

  ngOnInit() {
  }

  public process() {
    const roomId = parseInt(this.request.roomId, 10);
    const followingId = parseInt(this.request.following, 10);
    const questionIds = this.request.questionIds.split(';')
      .filter(v => v !== '')
      .map(v => parseInt(v, 10));

    if (isNaN(roomId)) { return; }
    if (isNaN(followingId)) { return; }

    this.isLoading = true;

    this.httpClient.post('/api/admin/ManuallyProcessQuestions', {
      roomId,
      followingId,
      questionIds
    }).subscribe(_ => {
      this.isLoading = false;
    });
  }
}
