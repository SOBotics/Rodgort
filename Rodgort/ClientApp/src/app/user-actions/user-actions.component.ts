import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import * as moment from 'moment';

@Component({
  selector: 'app-user-actions',
  templateUrl: './user-actions.component.html',
  styleUrls: ['./user-actions.component.scss']
})
export class UserActionsComponent implements OnInit {
  public actions: any[];

  public loading = true;

  public pagingInfo: PagingInfo[];
  public filter = {
    tag: String,
    type: String,

    pageNumber: 1,
  };

  public id: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.id = +params['id'];
      this.route.queryParams.subscribe(qParams => {
        this.filter.tag = qParams['tag'] || '';
        this.filter.type = qParams['type'] || '';
        this.filter.pageNumber = +qParams['pageNumber'] || 1;
        this.reloadData();
      });
    });
  }

  private reloadData() {
    this.loading = true;
    this.httpClient.get(`/api/users/actions?userId=${this.id}` +
      `&tag=${this.filter.tag}` +
      `&type=${this.filter.type}` +
      `&pageNumber=${this.filter.pageNumber}`)
      .subscribe((response: any) => {
        this.loading = false;
        if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
          this.filter.pageNumber = 1;
          this.reloadData();
        } else {
          this.actions = response.data.map(d => ({
            ...d, localTime: moment.utc(d.time).local().format('YYYY-MM-DD hh:mm:ss A')
          }));
          this.pagingInfo = GetPagingInfo(response);
        }
      });
  }

  public loadPage(pageNumber: number) {
    this.filter.pageNumber = pageNumber;
    this.applyFilter();
  }

  public applyFilter() {
    this.router.navigate(['/user-actions', this.id], { queryParams: this.filter });
  }
}
