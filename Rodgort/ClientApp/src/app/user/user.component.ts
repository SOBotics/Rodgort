import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService, RODGORT_ADMIN } from '../services/auth.service';
import * as moment from 'moment';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.scss']
})
export class UserComponent implements OnInit {
  public isAdmin = false;

  public selectedRole: string;

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
    }[],

    availableRoles: {
      name: string
    }
  };

  constructor(
    private route: ActivatedRoute,
    private httpClient: HttpClient,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.authService.GetAuthDetails().subscribe(d => {
      this.isAdmin = !!d.GetClaim(RODGORT_ADMIN);
    });

    this.route.params.subscribe(params => {
      const id = params['id'];
      if (!isNaN(id)) {
        this.reloadData(id);
      } else {
        this.reloadData(null);
      }
    });
  }

  private reloadData(id?: number) {
    let query: string;
    if (id) {
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
  }

  public addRole() {
    if (this.selectedRole === '') {
      return;
    }

    this.authService.GetAuthDetails().subscribe(d => {
      this.httpClient.post(`api/users/addRole`, { userId: this.userData.userId, roleName: this.selectedRole },
        {
          headers: { 'Authorization': 'Bearer ' + d.RawToken }
        }).subscribe((response) => {
          this.reloadData(this.userData.userId);
          this.selectedRole = '';
        });
    });
  }

  public removeRole(roleName: string) {
    if (roleName === '') {
      return;
    }
    this.authService.GetAuthDetails().subscribe(d => {
      this.httpClient.post(`api/users/removeRole`, { userId: this.userData.userId, roleName },
        {
          headers: { 'Authorization': 'Bearer ' + d.RawToken }
        }).subscribe((response) => {
          this.reloadData(this.userData.userId);
          this.selectedRole = '';
        });
    });
  }
}
