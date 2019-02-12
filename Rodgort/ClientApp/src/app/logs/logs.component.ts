import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';
import * as moment from 'moment';

@Component({
  selector: 'app-logs',
  templateUrl: './logs.component.html',
  styleUrls: ['./logs.component.scss']
})
export class LogsComponent implements OnInit {

  public pagingInfo: PagingInfo[];
  public filter = {
    search: String,
    level: String,

    pageNumber: 1,
  };

  public logs: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.search = params['search'] || '';
      this.filter.level = params['level'] || '';
      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.reloadData();
    });
  }


  public reloadData() {
    const query =
      `/api/logs` +
      `?search=${this.filter.search}` +
      `&level=${this.filter.level}` +
      `&page=${this.filter.pageNumber}`;

    this.httpClient.get(query).subscribe((response: any) => {
      if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
        this.filter.pageNumber = 1;
        this.reloadData();
      } else {
        this.logs = response.data.map(d => ({
          ...d, localTime: moment.utc(d.timeLogged).local().format('YYYY-MM-DD hh:mm:ss A')
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
    this.router.navigate(['/logs'], { queryParams: this.filter });
  }
}
