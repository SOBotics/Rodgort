import { Component, OnInit } from '@angular/core';
import { PagingInfo, GetPagingInfo } from '../../utils/PagingHelper';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import * as moment from 'moment';

@Component({
  selector: 'app-tag-tracking-status-audits',
  templateUrl: './tag-tracking-status-audits.component.html',
  styleUrls: ['./tag-tracking-status-audits.component.scss']
})
export class TagTrackingStatusAuditsComponent implements OnInit {

  public pagingInfo: PagingInfo[];
  public filter = {
    userId: String,
    metaQuestionId: String,

    pageNumber: 1,
  };

  public audits: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private httpClient: HttpClient
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.filter.userId = params['userId'] || '';
      this.filter.metaQuestionId = params['metaQuestionId'] || '';
      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.reloadData();
    });
  }


  public reloadData() {
    const query =
      `/api/TagTrackingStatusAudits` +
      `?userId=${this.filter.userId}` +
      `&metaQuestionId=${this.filter.metaQuestionId}` +
      `&page=${this.filter.pageNumber}`;

    this.httpClient.get(query).subscribe((response: any) => {
      if (response.totalPages > 0 && response.pageNumber > response.totalPages) {
        this.filter.pageNumber = 1;
        this.reloadData();
      } else {
        this.audits = response.data.map((item: any) => ({
          ...item,
          localTime: moment.utc(item.timeChanged).local().format('YYYY-MM-DD hh:mm:ss A')
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
    this.router.navigate(['/tag-tracking-status-audits'], { queryParams: this.filter });
  }

}
