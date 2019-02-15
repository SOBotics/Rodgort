import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import * as moment from 'moment';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {

  public userData: {
    userId: number;
    displayName: string;
    isModerator: boolean;
    numBurninations: number;
    burns: {
      metaQuestionId: number;
      metaQuestionTitle: string;
      startDate: string;
      startDateLocal: string;
      numActions: number;
    }[];
    triageTags: number;
    triageQuestions: number;

    roles: {
      name: string;
      addedBy: string;
      addedById: number;
      dateAdded: string;
      dateAddedLocal: string;
    }[]
  };

  constructor(
    private route: ActivatedRoute,
    private httpClient: HttpClient,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      const id = params['id'];
      let query: string;
      if (!isNaN(id)) {
        query = `/api/users?userId=${id}`;
      } else {
        query = '/api/users/me';
      }

      this.authService.GetAuthDetails().subscribe(d => {
        this.httpClient.get(query,
          {
            headers: { 'Authorization': 'Bearer ' + d.RawToken }
          }).subscribe((response: any) => {
            response.burns = response.burns.map((item: any) => ({
              ...item, startDateLocal: moment.utc(item.startDate).local().format('YYYY-MM-DD hh:mm:ss A')
            }));

            response.roles = response.roles.map((item: any) => ({
              ...item, dateAddedLocal: moment.utc(item.dateAdded).local().format('YYYY-MM-DD hh:mm:ss A')
            }));

            this.userData = response;
          });
      });
    });
  }
}
